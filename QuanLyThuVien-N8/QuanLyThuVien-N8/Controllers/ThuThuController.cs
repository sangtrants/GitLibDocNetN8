using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyThuVien_N8.Models;
using PagedList;
using System.Drawing;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using DotNet.Highcharts.Attributes;
using Point = DotNet.Highcharts.Options.Point;
using System.IO;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyThuVien_N8.Controllers
{
    public class ThuThuController : Controller
    {
        const int SOSACHDUOCMUON = 20; // Số sách được mượn tối đa tại 1 thời điểm 
        const int SONGAYDUOCMUON = 15; // Số ngày được mượn cho 1 lần mượn của 1 cuốn sách
        const int SONGAYDUOCGIAHAN = 5; // Số ngày được mượn cho 1 lần gia hạn của 1 cuốn sách
        const int SOLANGIAHAN = 2; //  Số lần được gia hạn cho 1 cuốn sách
        QuanLyThuVienEntities data = new QuanLyThuVienEntities();
        // GET: /ThuThu/
        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }

        public ActionResult Index()
        {
            if (Session["type"] != "ThuThu")
            {
                return View("~/Views/Home/Login.cshtml");
            }
            return View();
        }
        public ActionResult Logout()
        {
            Session["user"] = null;
            Session["SoLanDenHan"] = null;
            Session["type"] = null;
            return View("~/Views/Home/Index.cshtml");
        }
        public ActionResult GetAllNguoiDung(int? page)
        {

            QuanLyThuVienEntities ql = new QuanLyThuVienEntities();

            var listUser = from nd in ql.NguoiDungs select nd;
            listUser = listUser.OrderBy(s => s.MaNguoiDung);

            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(listUser.ToPagedList(pageNumber, pageSize));
        }

        public JsonResult GetNguoiDungWithParameter(String prefix)
        {
            List<NguoiDung> listUser = new List<NguoiDung>();

            using (QuanLyThuVienEntities ql = new QuanLyThuVienEntities())
            {
                listUser = ql.NguoiDungs.Where(a => a.HoTen.Contains(prefix)).ToList();
            }
            return new JsonResult { Data = listUser, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        /// QL DOC GIA -- TAN
        /// //Goi view Quan Ly Doc Gia: co chuc nang tim kiem doc gia
        public ActionResult QLDGia(int? page)
        {
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            var kq = from ngd in data.NguoiDungs
                     where (ngd.DeleteFlag != 1 || ngd.DeleteFlag == null)
                     select ngd;
            kq = kq.OrderBy(s => s.MaNguoiDung);
            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(kq.ToPagedList(pageNumber, pageSize));
        }

        //[ChildActionOnly] xu ly tim kiem doc gia
        public ActionResult KQTKiem(string TuKhoa, string KieuTim, int? page)
        {
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();


            var kq = from ngd in data.NguoiDungs
                     where ((ngd.DeleteFlag != 1 || ngd.DeleteFlag == null))
                     select ngd;


            if (KieuTim == "TenDG")
            {
                kq = from ngd in data.NguoiDungs
                     where (ngd.HoTen.Contains(TuKhoa) && (ngd.DeleteFlag != 1 || ngd.DeleteFlag == null))
                     select ngd;
            }

            if (KieuTim == "MaDG")
            {
                kq = from ngd in data.NguoiDungs
                     where (ngd.TenDangNhap.Contains(TuKhoa) && (ngd.DeleteFlag != 1 || ngd.DeleteFlag == null))
                     select ngd;
            }
            ViewBag.TuKhoa = TuKhoa;
            ViewBag.KieuTim = KieuTim;
            kq = kq.OrderBy(s => s.MaNguoiDung);
            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View("KQTKiem", kq.ToList().ToPagedList(pageNumber, pageSize));
        }

        // Goi view Them Doc Gia
        public ActionResult themDGia()
        {
            return View();
        }

        // Xu ly them doc gia roi goi view Quan Ly Doc Gia
        public ActionResult themdg(string hoten, string email, string cmnd, DateTime ngsinh, string mssv, DateTime ngayhh, string loaingdung, string tendn, string mkhau)
        {
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            MD5 md5Hash = MD5.Create();
            string password = GetMd5Hash(md5Hash, mkhau);  
            NguoiDung dg = new NguoiDung();
            dg.HoTen = hoten;
            dg.SoCMND = cmnd;
            dg.NgaySinh = ngsinh;
            dg.MSSV = mssv;
            dg.Email = email;
            dg.NgayHetHan = ngayhh;
            dg.LoaiND = loaingdung;
            dg.TenDangNhap = tendn;
            dg.MatKhau = password;
            dg.NgayDangKy = DateTime.Today;
            dg.DeleteFlag = 0;
            data.NguoiDungs.Add(dg);
            data.SaveChanges();
            return RedirectToAction("QLDGia");
        }

        // Goi view Cap Nhat Doc Gia
        public ActionResult suadg(int id)
        {
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            NguoiDung ngd = (from ngds in data.NguoiDungs
                             where ngds.MaNguoiDung == id
                             select ngds).First();
            return View("suaDGia", ngd);
        }

        //Xu ly cap nhat doc gia
        public ActionResult suaDGia(FormCollection f)
        {
            int id = int.Parse(f["MaNguoiDung"]);
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            NguoiDung ngd = (from ngds in data.NguoiDungs
                             where ngds.MaNguoiDung == id
                             select ngds).First();
            ngd.HoTen = f["HoTen"];
            ngd.SoCMND = f["SoCMND"];
            ngd.NgaySinh = DateTime.Parse(f["NgaySinh"]);
            ngd.MSSV = f["MSSV"];
            ngd.Email = f["Email"];
            ngd.NgayHetHan = DateTime.Parse(f["NgayHetHan"]);
            ngd.LoaiND = f["LoaiND"];
            ngd.TenDangNhap = f["TenDangNhap"];
            //ngd.MatKhau = f["MatKhau"];
            data.SaveChanges();
            return RedirectToAction("chitietDGia/" + ngd.MaNguoiDung);
        }

        //Xu ly xoa doc gia
        public ActionResult xoaDGia(int id)
        {
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            NguoiDung ngd = (from ngds in data.NguoiDungs
                             where ngds.MaNguoiDung == id
                             select ngds).First();
            ngd.DeleteFlag = 1;
            data.SaveChanges();
            return RedirectToAction("QLDGia");
        }

        //Goi view Chi Tiet Doc Gia
        public ActionResult chitietDGia(int id)
        {
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            NguoiDung ngd = (from ngds in data.NguoiDungs
                             where ngds.MaNguoiDung == id
                             select ngds).First();
            return View("chitietDGia", ngd);
        }


        public ActionResult capnhatAnh(string id)
        {
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            NguoiDung ngd = (from ngds in data.NguoiDungs
                             where (ngds.TenDangNhap == id)
                             select ngds).First();
            return View("capnhatAnh", ngd);
        }

        [HttpPost]
        public ActionResult cnAnh(HttpPostedFileBase file, FormCollection f)
        {
            string mand = f["TenDangNhap"];
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            NguoiDung ngd = (from ngds in data.NguoiDungs
                             where ngds.TenDangNhap == mand
                             select ngds).First();
            if (ModelState.IsValid)
            {
                string temp = "";

                if (file != null)
                {
                    var tenAnh = mand + ".png";
                    var path = Path.Combine(Server.MapPath("~/Images"), tenAnh);
                    file.SaveAs(path);
                    temp = tenAnh;
                    ngd.AnhDaiDien = "/Images/" + tenAnh;
                    data.SaveChanges();
                }
            }
            return RedirectToAction("chitietDGia/" + ngd.MaNguoiDung);
        }

        public ActionResult qlHinhAnh(int? page)
        {
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            var kq = from ngd in data.NguoiDungs
                     where (ngd.DeleteFlag != 1 || ngd.DeleteFlag == null)
                     select ngd;
            kq = kq.OrderBy(s => s.MaNguoiDung);
            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(kq.ToPagedList(pageNumber, pageSize));
        }

        // nguyên cục của anh
        public ActionResult getViewPieChart()
        {
            return View();
        }
        public ActionResult getdata()
        {
            return Json(getListdata(), JsonRequestBehavior.AllowGet);
        }
        private IEnumerable<TypeUserM> getListdata()
        {
            List<TypeUserM> listUser = new List<TypeUserM>();

            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            var kq = from nd in data.NguoiDungs
                     group nd by nd.LoaiND into newgroup
                     select new { LoaiNguoiDung = newgroup.Key, SoLuong = newgroup.Count() };
            
            foreach (var st in kq)
            {
                TypeUserM user = new TypeUserM();
                user.LoaiNguoiDung = st.LoaiNguoiDung;
                user.SoLuong = st.SoLuong;
                listUser.Add(user);
            }
            return listUser;
        }
        /* Tín */
        
        // Quản lý sách

        public ActionResult XemToanBoSach(int? page)
        {
            var sach = from s in data.Saches where s.DeleteFlag == 0 select s;
            sach = sach.OrderBy(s => s.MaSach);
            return View(sach.ToList().ToPagedList(page ?? 1, 2));
        }

        public ActionResult XemChiTietSach(int id = 0)
        {
            Sach sach = data.Saches.Find(id);
            if (sach == null)
            {
                return HttpNotFound();
            }
            return View(sach);
        }

        public ActionResult CapNhatSach(int id)
        {
            Sach result = data.Saches.Find(id);
            if (result == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaDanhMuc = new SelectList(data.DanhMucs, "MaDanhMuc", "TenDanhMuc", result.MaDanhMuc);
            ViewBag.NhaXuatBan = new SelectList(data.NhaXuatBans, "MaNhaXuatBan", "TenNhaXuatBan", result.NhaXuatBan);
            ViewBag.TacGia = new SelectList(data.TacGias, "MaTacGia", "TenTacGia", result.TacGia);
            return View(result);
        }

        [HttpPost]
        public ActionResult CapNhatSach(Sach sach, HttpPostedFileBase file, FormCollection f)
        {
            string mand = sach.MaSach.ToString();
            if (ModelState.IsValid)
            {
                string temp = "";
                if (file != null)
                {
                    var tenAnh = mand + ".jpg";
                    var path = Path.Combine(Server.MapPath("~/BookImg"), tenAnh);
                    file.SaveAs(path);
                    temp = tenAnh;
                }
                data.Entry(sach).State = EntityState.Modified;
                data.SaveChanges();
                return RedirectToAction("XemChiTietSach", new { id = sach.MaSach });
            }
            ViewBag.MaDanhMuc = new SelectList(data.DanhMucs, "MaDanhMuc", "TenDanhMuc", sach.MaDanhMuc);
            ViewBag.NhaXuatBan = new SelectList(data.NhaXuatBans, "MaNhaXuatBan", "TenNhaXuatBan", sach.NhaXuatBan);
            ViewBag.TacGia = new SelectList(data.TacGias, "MaTacGia", "TenTacGia", sach.TacGia);
            return RedirectToAction("XemToanBoSach");
        }

        public ActionResult ThemMoiSach()
        {
            ViewBag.MaDanhMuc = new SelectList(data.DanhMucs, "MaDanhMuc", "TenDanhMuc");
            ViewBag.NhaXuatBan = new SelectList(data.NhaXuatBans, "MaNhaXuatBan", "TenNhaXuatBan");
            ViewBag.TacGia = new SelectList(data.TacGias, "MaTacGia", "TenTacGia");
            return View();
        }

        [HttpPost]
        public ActionResult ThemMoiSach(Sach sach, HttpPostedFileBase file, FormCollection f)
        {
            try { sach.MaSach = data.Saches.Max(u => u.MaSach) + 1; }
            catch { sach.MaSach = 1; }
            sach.TinhTrangMuon = 0;
            sach.DeleteFlag = 0;
            string mand = sach.MaSach.ToString();
            if (ModelState.IsValid)
            {
                string temp = "";
                if (file != null)
                {
                    var tenAnh = mand + ".jpg";
                    var path = Path.Combine(Server.MapPath("~/BookImg"), tenAnh);
                    file.SaveAs(path);
                    temp = tenAnh;
                    // cẩn thận đườn dẫn, sửa theo từng máy
                    mand = @"C:\Users\HoDuyTin\Documents\Visual Studio 2012\Projects\TV-MVC-N8\QuanLyThuVien-N8\QuanLyThuVien-N8\BookImg\" + sach.MaSach.ToString();
                    if (!Directory.Exists(mand))
                        Directory.CreateDirectory(mand);
                }
                data.Saches.Add(sach);
                data.SaveChanges();
                return RedirectToAction("XemChiTietSach", new { id = sach.MaSach });
            }
            ViewBag.MaDanhMuc = new SelectList(data.DanhMucs, "MaDanhMuc", "TenDanhMuc", sach.MaDanhMuc);
            ViewBag.NhaXuatBan = new SelectList(data.NhaXuatBans, "MaNhaXuatBan", "TenNhaXuatBan", sach.NhaXuatBan);
            ViewBag.TacGia = new SelectList(data.TacGias, "MaTacGia", "TenTacGia", sach.TacGia);
            return View(sach);
        }

        public ActionResult XoaSach(int id)
        {
            var result = (from ct in data.ChiTietPhieuMuons where ct.MaSach == id && ct.Sach.DeleteFlag == 0 select ct);
            if (result != null)
            {
                @ViewBag.Message = "Quyển " + id.ToString() + " đang có người mượn, không thể xoá!";
                return View("KqXoaDoiTuong");
            }
            Sach sach = data.Saches.Find(id);
            sach.DeleteFlag = 1;
            if (ModelState.IsValid)
            {
                data.Entry(sach).State = EntityState.Modified;
                data.SaveChanges();
            }
            @ViewBag.Message = "Quyển sách " + id.ToString() + " đã được xoá.";
            return View("KqXoaDoiTuong");
        }

        public ActionResult TimKiemSach()
        {
            var result = (from nxb in data.NhaXuatBans select nxb).ToList();
            return View(result);
        }

        public ActionResult KqTimKiemSach(string TuKhoa, string LoaiTimKiem, string GioiHan, int NhaXuatBan, int? page)
        {
            //if (LoaiTimKiem == "TatCa")
            var result = (from sach in data.Saches
                          from tacgia in data.TacGias
                          where ((tacgia.MaTacGia == sach.TacGia &&
                          ((tacgia.TenTacGia.Contains(TuKhoa) && GioiHan == "Chua") || (tacgia.TenTacGia == TuKhoa && GioiHan == "ChinhXac"))) ||
                          ((sach.TenSach.Contains(TuKhoa) && GioiHan == "Chua") || (sach.TenSach == TuKhoa && GioiHan == "ChinhXac"))) &&
                          (sach.NhaXuatBan == NhaXuatBan || NhaXuatBan == 0) && sach.DeleteFlag == 0
                          select sach);

            if (LoaiTimKiem == "TieuDe")
                result = (from sach in data.Saches
                          where ((sach.TenSach.Contains(TuKhoa) && GioiHan == "Chua") || (sach.TenSach == TuKhoa && GioiHan == "ChinhXac")) &&
                          (sach.NhaXuatBan == NhaXuatBan || NhaXuatBan == 0) && sach.DeleteFlag == 0
                          select sach);

            if (LoaiTimKiem == "TacGia")
                result = (from sach in data.Saches
                          from tacgia in data.TacGias
                          where (tacgia.MaTacGia == sach.TacGia &&
                          ((tacgia.TenTacGia.Contains(TuKhoa) && GioiHan == "Chua") || (tacgia.TenTacGia == TuKhoa && GioiHan == "ChinhXac"))) &&
                          (sach.NhaXuatBan == NhaXuatBan || NhaXuatBan == 0) && sach.DeleteFlag == 0
                          select sach);
            ViewBag.TuKhoa = TuKhoa;
            ViewBag.LoaiTimKiem = LoaiTimKiem;
            ViewBag.GioiHan = GioiHan;
            ViewBag.NhaXuatBan = NhaXuatBan;
            return PartialView("XemSach", result.ToList().Distinct().ToPagedList(page ?? 1, 2));
        }

        public ActionResult UploadNoiDungSach(int id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        public ActionResult UploadNoiDungSach(int id, HttpPostedFileBase[] files)
        {
            string s = "~/BookImg/" + id.ToString();

            foreach (HttpPostedFileBase file in files)
            {
                string path = System.IO.Path.Combine(Server.MapPath(s), System.IO.Path.GetFileName(file.FileName));
                file.SaveAs(path);
            }
            ViewBag.Message = "File(s) uploaded successfully";
            return RedirectToAction("XemChiTietSach", new { id = id});
        }

        // Quản lý tác giả

        public ActionResult XemToanBoTacGia(int? page)
        {
            var tacgia = from s in data.TacGias select s;
            tacgia = tacgia.OrderBy(s => s.MaTacGia);
            return View(tacgia.ToList().ToPagedList(page ?? 1, 2));
        }

        public ActionResult CapNhatTacGia(int id)
        {
            TacGia result = data.TacGias.Find(id);
            if (result == null)
            {
                return HttpNotFound();
            }
            return View(result);
        }


        [HttpPost]
        public ActionResult CapNhatTacGia(TacGia tacgia)
        {
            if (ModelState.IsValid)
            {
                data.Entry(tacgia).State = EntityState.Modified;
                data.SaveChanges();
                return RedirectToAction("XemToanBoTacGia");
            }

            return Redirect("Index");
        }

        public ActionResult ThemMoiTacGia()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ThemMoiTacGia(TacGia tacgia)
        {
            try { tacgia.MaTacGia = data.TacGias.Max(u => u.MaTacGia) + 1; }
            catch { tacgia.MaTacGia = 1; }
            if (ModelState.IsValid)
            {
                data.TacGias.Add(tacgia);
                data.SaveChanges();
                return RedirectToAction("XemToanBoTacGia");
            }
            return Redirect("Index");
        }

        public ActionResult XoaTacGia(int id)
        {
            var result = (from s in data.Saches where s.TacGia == id && s.DeleteFlag == 0 select s);
            if (result != null)
            {
                @ViewBag.Message = "Tác giả " + id.ToString() + " đang có sách lưu trong thư viện, không thể xoá!";
                return View("KqXoaDoiTuong");
            }
            TacGia tg = data.TacGias.Find(id);
            tg.DeleteFlag = 1;
            if (ModelState.IsValid)
            {
                data.Entry(tg).State = EntityState.Modified;
                data.SaveChanges();
            }
            @ViewBag.Message = "Tác giả " + id.ToString() + " đã được xoá.";
            return View("KqXoaDoiTuong");
        }

        public ActionResult TimKiemTacGia()
        {
            return View();
        }

        public ActionResult KqTimKiemTacGia(string TuKhoa, string LoaiTimKiem, string GioiHan, int? page)
        {
            //if (LoaiTimKiem == "TatCa")
            var result = (from tacgia in data.TacGias
                          where (tacgia.TenTacGia.Contains(TuKhoa) && GioiHan == "Chua") || (tacgia.TenTacGia == TuKhoa && GioiHan == "ChinhXac") ||
                                (tacgia.TenVietTat.Contains(TuKhoa) && GioiHan == "Chua") || (tacgia.TenVietTat == TuKhoa && GioiHan == "ChinhXac")
                          select tacgia);

            if (LoaiTimKiem == "TenTacGia")
                result = (from tacgia in data.TacGias
                          where (tacgia.TenTacGia.Contains(TuKhoa) && GioiHan == "Chua") || (tacgia.TenTacGia == TuKhoa && GioiHan == "ChinhXac")
                          select tacgia);

            if (LoaiTimKiem == "TenVietTat")
                result = (from tacgia in data.TacGias
                          where (tacgia.TenVietTat.Contains(TuKhoa) && GioiHan == "Chua") || (tacgia.TenVietTat == TuKhoa && GioiHan == "ChinhXac")
                          select tacgia);
            ViewBag.TuKhoa = TuKhoa;
            ViewBag.LoaiTimKiem = LoaiTimKiem;
            ViewBag.GioiHan = GioiHan;
            return PartialView("XemTacGia", result.ToList().Distinct().ToPagedList(page ?? 1, 2));
        }

        // Quản lý nhà xuất bản

        public ActionResult XemToanBoNXB(int? page)
        {
            var nxb = from s in data.NhaXuatBans select s;
            nxb = nxb.OrderBy(s => s.MaNhaXuatBan);
            return View(nxb.ToList().ToPagedList(page ?? 1, 2));
        }

        public ActionResult CapNhatNXB(int id)
        {
            NhaXuatBan result = data.NhaXuatBans.Find(id);
            if (result == null)
            {
                return HttpNotFound();
            }
            return View(result);
        }


        [HttpPost]
        public ActionResult CapNhatNXB(NhaXuatBan nxb)
        {
            if (ModelState.IsValid)
            {
                data.Entry(nxb).State = EntityState.Modified;
                data.SaveChanges();
                return RedirectToAction("XemToanBoNXB");
            }

            return Redirect("Index");
        }

        public ActionResult ThemMoiNXB()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ThemMoiNXB(NhaXuatBan nxb)
        {
            try { nxb.MaNhaXuatBan = data.NhaXuatBans.Max(u => u.MaNhaXuatBan) + 1; }
            catch { nxb.MaNhaXuatBan = 1; }
            if (ModelState.IsValid)
            {
                data.NhaXuatBans.Add(nxb);
                data.SaveChanges();
                return RedirectToAction("XemToanBoNXB");
            }
            return View(nxb);
        }

        public ActionResult XoaNXB(int id)
        {
            var result = (from s in data.Saches where s.NhaXuatBan == id && s.DeleteFlag == 0 select s);
            if (result != null)
            {
                @ViewBag.Message = "Nhà xuất bản " + id.ToString() + " đang có sách lưu trong thư viện, không thể xoá!";
                return View("KqXoaDoiTuong");
            }
            TacGia tg = data.TacGias.Find(id);
            tg.DeleteFlag = 1;
            if (ModelState.IsValid)
            {
                data.Entry(tg).State = EntityState.Modified;
                data.SaveChanges();
            }
            @ViewBag.Message = "Nhà xuất bản " + id.ToString() + " đã được xoá.";
            return View("KqXoaDoiTuong");
        }

        public ActionResult TimKiemNXB()
        {
            return View();
        }

        public ActionResult KqTimKiemNXB(string TuKhoa, string GioiHan, int? page)
        {
            var result = (from nxb in data.NhaXuatBans
                          where (nxb.TenNhaXuatBan.Contains(TuKhoa) && GioiHan == "Chua") || (nxb.TenNhaXuatBan == TuKhoa && GioiHan == "ChinhXac")
                          select nxb);
            ViewBag.TuKhoa = TuKhoa;
            ViewBag.GioiHan = GioiHan;
            return PartialView("XemNXB", result.ToList().Distinct().ToPagedList(page ?? 1, 2));
        }

        // Xử lý mượn Sách

        public ActionResult QLMuonSach()
        {
            return View();
        }

        public ActionResult XacNhanDocGiaMuonSach(int MaDocGia)
        {
            var result = (from dg in data.NguoiDungs where dg.MaNguoiDung == MaDocGia select dg);
            if (!result.Any())
            {
                ViewBag.DeleteFlag = 0;
                return PartialView("XacNhanDocGiaMuonSach");
            }
            // if (result.Any())
            int SoSachDangMuon = (from pm in data.PhieuMuons
                                    from ct in data.ChiTietPhieuMuons
                                    where pm.NguoiMuon == MaDocGia && pm.MaPhieuMuon == ct.MaPhieuMuon && ct.NgayTra == null
                                    select ct).Count();
            if (SoSachDangMuon >= SOSACHDUOCMUON)
            {
                ViewBag.DeleteFlag = 1;
                return PartialView("XacNhanDocGiaMuonSach");
            }
            // if (SoSachDangMuon < SOSACHDUOCMUON))
            ViewBag.SoSach = SOSACHDUOCMUON - SoSachDangMuon;
            ViewBag.MaDocGia = MaDocGia;
            ViewBag.DeleteFlag = 100;
            return PartialView("XacNhanDocGiaMuonSach");
        }

        public ActionResult ThemSachMuon1(int MaSach, int MaDocGia, int SoSach)
        {
            ViewBag.SoSach = SoSach;
            ViewBag.MaDocGia = MaDocGia;
            string ListMaSach = "";
            var result = (from sach in data.Saches where sach.MaSach == MaSach select sach);
            if (result == null)
            {
                ViewBag.DeleteFlag = 0;
                return PartialView("ThemSachMuon");
            }
            // if (result.Any())
            int TinhTrang = (from sach in data.Saches where sach.MaSach == MaSach && sach.TinhTrangMuon == 0 select sach).Count();
            if (TinhTrang == 0)
            {
                ViewBag.DeleteFlag = 1;
                return PartialView("ThemSachMuon");
            }
            ViewBag.MaSach = MaSach;
            ListMaSach = MaSach.ToString();
            ViewBag.ListMaSach = ListMaSach;
            SoSach--;
            ViewBag.SoSach = SoSach;
            return PartialView("ThemSachMuon", result.ToList());
        }

        public ActionResult ThemSachMuon2(int MaSach, string ListMaSach, int MaDocGia, int SoSach)
        {
            ViewBag.SoSach = SoSach;
            ViewBag.MaDocGia = MaDocGia;
            ViewBag.ListMaSach = ListMaSach;
            List<int> a = StringToListInt(ListMaSach);
            var result = (from sach in data.Saches where a.Contains(sach.MaSach) select sach);
            var test = data.Saches.Find(MaSach);
            if (test == null)
            {
                ViewBag.DeleteFlag = 0;
                return PartialView("ThemSachMuon", result.ToList());
            }
            // if (result.Any())
            int TinhTrang = (from sach in data.Saches where sach.MaSach == MaSach && sach.TinhTrangMuon == 0 select sach).Count();
            if (TinhTrang == 0)
            {
                ViewBag.DeleteFlag = 1;
                return PartialView("ThemSachMuon", result.ToList());
            }
            if (a.Contains(MaSach))
            {
                ViewBag.DeleteFlag = 2;
                return PartialView("ThemSachMuon", result.ToList());
            }
            ListMaSach += '|';
            ListMaSach += MaSach.ToString();
            ViewBag.ListMaSach = ListMaSach;
            a = StringToListInt(ListMaSach);
            result = (from sach in data.Saches where a.Contains(sach.MaSach) select sach);
            SoSach--;
            ViewBag.SoSach = SoSach;
            return PartialView("ThemSachMuon",result.ToList());
        }

        public ActionResult ChoMuonSach(string ListMaSach, int MaDocGia)
        {
            List<int> a = StringToListInt(ListMaSach);
            // Thêm phiếu mượn
            PhieuMuon pm = new PhieuMuon();
            try { pm.MaPhieuMuon = data.PhieuMuons.Max(u => u.MaPhieuMuon) + 1; }
            catch { pm.MaPhieuMuon = 1; }
            pm.NgayMuon = DateTime.Today;
            pm.NguoiMuon = MaDocGia;
            if (ModelState.IsValid)
            {
                data.PhieuMuons.Add(pm);
                data.SaveChanges();
            }
            for (int i = 0; i < a.Count(); i++)
            {
                // Thêm các chi tiết phiếu mượn
                ChiTietPhieuMuon ct = new ChiTietPhieuMuon();
                try { ct.MaChiTietPhieuMuon = data.ChiTietPhieuMuons.Max(u => u.MaChiTietPhieuMuon) + 1; }
                catch { ct.MaChiTietPhieuMuon = 1; }
                ct.MaSach = a[i];
                ct.NgayMuon = DateTime.Today;
                ct.MaPhieuMuon = pm.MaPhieuMuon;
                ct.LanMuon = 0;
                if (ModelState.IsValid)
                {
                    data.ChiTietPhieuMuons.Add(ct);
                }
                //Cập nhật trạng thái mượn cho sách
                Sach sach = data.Saches.Find(a[i]);
                sach.TinhTrangMuon = 1;
                if (ModelState.IsValid)
                {
                    data.Entry(sach).State = EntityState.Modified;
                    data.SaveChanges();
                }
            }
            var result = from sach in data.Saches where a.Contains(sach.MaSach) select sach;
            ViewBag.SoLuongSachMuon = result.Count();
            ViewBag.MaDocGia = MaDocGia;
            ViewBag.NgayMuon = DateTime.Today.ToString();
            return View(result.ToList());
        }

        public List<int> StringToListInt(string s)
        {
            List<int> kq = new List<int>();
            string[] st = s.Split('|');
            int i;
            foreach (string stem in st)
            {
                int.TryParse(stem, out i);
                kq.Add(i);
            }
            return kq;
        }

        // Xử lý trả Sách

        public ActionResult QLTraSach()
        {
            return View();
        }

        public ActionResult XacNhanDocGiaTraSach(int MaDocGia)
        {
            ViewBag.MaDocGia = MaDocGia;
            var test = (from dg in data.NguoiDungs where dg.MaNguoiDung == MaDocGia select dg);
            if (!test.Any())
            {
                ViewBag.DeleteFlag = 0;
                return PartialView("XacNhanDocGiaTraSach");
            }
            var result = from pm in data.PhieuMuons
                         from ct in data.ChiTietPhieuMuons
                         where pm.NguoiMuon == MaDocGia && pm.MaPhieuMuon == ct.MaPhieuMuon && ct.NgayTra == null
                         select ct;
            int SoSachMuon = result.Count();
            if (SoSachMuon == 0)
            {
                ViewBag.DeleteFlag = 1;
                return PartialView("XacNhanDocGiaTraSach");
            }
            return PartialView("XacNhanDocGiaTraSach", result.ToList());
        }

        [HttpPost]
        public ActionResult NhanTraSach(IEnumerable<int> DSMaCT, int MaDocGia)
        {
            // Cập nhật ngày mượn cho chi tiết phiếu mượn
            var result1 = (from ct in data.ChiTietPhieuMuons where DSMaCT.Contains(ct.MaChiTietPhieuMuon) select ct).ToList();
            int temp;
            foreach (ChiTietPhieuMuon ct in result1)
            {
                ct.NgayTra = DateTime.Today;
                if (ModelState.IsValid)
                {
                    data.Entry(ct).State = EntityState.Modified;
                    data.SaveChanges();
                }
                temp = ((DateTime)ct.NgayTra).Subtract((DateTime)ct.NgayMuon).Days;
                if (temp > SONGAYDUOCMUON + SONGAYDUOCGIAHAN * ct.LanMuon)
                {
                    ct.SoNgayQuaHan = temp - (int)ct.LanMuon * SONGAYDUOCGIAHAN - SONGAYDUOCMUON;
                }
                else
                    ct.SoNgayQuaHan = 0;
            }
            // Cập nhật tình trạng mượn cho sách
            var result2 = (from ct in data.ChiTietPhieuMuons
                          from sach in data.Saches
                          where ct.MaSach == sach.MaSach && DSMaCT.Contains(ct.MaChiTietPhieuMuon)
                          select sach).ToList();
            foreach (Sach sach in result2)
            {
                sach.TinhTrangMuon = 0;
                if (ModelState.IsValid)
                {
                    data.Entry(sach).State = EntityState.Modified;
                    data.SaveChanges();
                }
            }
            var result3 = (from ct in data.ChiTietPhieuMuons where DSMaCT.Contains(ct.MaChiTietPhieuMuon) select ct).ToList();
            ViewBag.MaDocGia = MaDocGia;
            ViewBag.SoLuongSachTra = result3.Count();
            ViewBag.NgayTra = DateTime.Today.ToString();
            return View(result3);
        }

     
    }
    
}
