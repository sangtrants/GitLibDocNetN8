using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyThuVien_N8.Models;
using System.IO;
using PagedList;
using System.Data;

namespace QuanLyThuVien_N8.Controllers
{
    public class DocGiaController : Controller
    {
        const int SOSACHDUOCMUON = 20; // Số sách được mượn tối đa tại 1 thời điểm 
        const int SONGAYDUOCMUON = 15; // Số ngày được mượn cho 1 lần mượn của 1 cuốn sách
        const int SONGAYDUOCGIAHAN = 5; // Số ngày được mượn cho 1 lần gia hạn của 1 cuốn sách
        const int SOLANGIAHAN = 2; //  Số lần được gia hạn cho 1 cuốn sách
        QuanLyThuVienEntities data = new QuanLyThuVienEntities();

        // Quản lý mượn trả
        public ActionResult QLMuonTra(int? page)
        {
            int id = 1;
            var kq = (from ctpmuon in data.ChiTietPhieuMuons
                      from pmuon in data.PhieuMuons
                      from ngd in data.NguoiDungs
                      from s in data.Saches
                      where ((ngd.MaNguoiDung == id) && (ngd.MaNguoiDung == pmuon.NguoiMuon) && (pmuon.MaPhieuMuon == ctpmuon.MaPhieuMuon)
                              && (ctpmuon.MaSach == s.MaSach))
                      select ctpmuon);
            kq = kq.OrderBy(ctpmuon => ctpmuon.MaChiTietPhieuMuon);
            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(kq.ToPagedList(pageNumber, pageSize));
        }

        // Gia hạn sách
        public ActionResult GiaHanSach(int id)
        {
            int SoLanGiaHan = (int)(from ct in data.ChiTietPhieuMuons where ct.MaChiTietPhieuMuon == id select ct.LanMuon).First();
            if (SoLanGiaHan >= SOLANGIAHAN)
            {
                @ViewBag.Message = "Bạn không thể tiếp tục gia hạn cho quyển sách này!";
                return View("KqGiaHanSach");
            }
            ChiTietPhieuMuon chitiet = data.ChiTietPhieuMuons.Find(id);
            chitiet.LanMuon++;
            if (ModelState.IsValid)
            {
                data.Entry(chitiet).State = EntityState.Modified;
                data.SaveChanges();
            }
            @ViewBag.Message = "Sách được gia hạn thành công! Lần gia hạn thứ " + chitiet.LanMuon.ToString();
            return View("KqGiaHanSach");
        }
       
    }
}
