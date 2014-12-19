using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using QuanLyThuVien_N8.Models;
using System.IO;
using PagedList;
using System.Security.Cryptography;
using System.Text;
using System.Data.Objects;
using Quartz;

namespace QuanLyThuVien_N8.Models
{
    public class EmailJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            DateTime currentDate = DateTime.Now;
            var dsNhacNho = (from ct in data.ChiTietPhieuMuons
                             from pm in data.PhieuMuons
                             from s in data.Saches
                             from nd in data.NguoiDungs
                             where pm.NguoiMuon == nd.MaNguoiDung && ct.MaSach == s.MaSach && ct.MaPhieuMuon == pm.MaPhieuMuon && EntityFunctions.DiffDays(ct.NgayMuon, currentDate) <= 14 && EntityFunctions.DiffDays(ct.NgayMuon, currentDate) >= 11
                             select new RemindMail()
                             {
                                 MaPhieuMuon = ct.MaPhieuMuon,
                                 MaChiTiet = ct.MaChiTietPhieuMuon,
                                 MaNguoiDung = nd.MaNguoiDung,
                                 HoTen = nd.HoTen,
                                 Email = nd.Email,
                                 NgayMuon = ct.NgayMuon,
                                 SoNgayToiHan = 14 - EntityFunctions.DiffDays(ct.NgayMuon, DateTime.Now),
                                 TenSach = s.TenSach
                             });
            var dsNguoidung = (from x in dsNhacNho select x.MaNguoiDung).Distinct().ToList();
            foreach (var MaNguoiDung in dsNguoidung)
            {
                List<String> tenSach = new List<String>();
                List<DateTime?> ngayMuon = new List<DateTime?>();
                int?[] soluong = new int?[5];
                int i = 0;
                string emailDocGia = "";
                string tenDocGia = "";
                int?[] maPhieuMuon = new int?[5];

                var ds = (from x in dsNhacNho where x.MaNguoiDung == MaNguoiDung select x).ToList();
                foreach (var a in ds)
                {
                    tenSach.Add(a.TenSach);
                    emailDocGia = a.Email;
                    tenDocGia = a.HoTen;
                    soluong[i] = a.SoNgayToiHan;
                    maPhieuMuon[i] = a.MaPhieuMuon;
                    ngayMuon.Add(a.NgayMuon);
                    i = i + 1;
                }
                QuanLyThuVien_N8.Models.EmailService mailService = new EmailService();
                string smtpUserName = "hotro.khtn@gmail.com";
                string smtpPassword = "nhom8httt";
                string smtpHost = "smtp.gmail.com";
                int smtpPort = 25;
                string emailTo = emailDocGia;
                string subject = "[Thư viện KHTN] - Nhắc nhở hạn trả sách - " + tenDocGia;
                string body = string.Format("Xin chào độc giả {0} !<br><br>Thư viện trường ĐH KHTN xin gửi đến bạn danh sách các ấn phẩm bạn đã mượn sắp đến hạn trả: <br><br>", tenDocGia);
                for (int j = 0; j < i; j++)
                {
                    body += string.Format("{0}. Cuốn sách: <b>{1}</b> mượn ngày: <b>{2}</b> thuộc phiếu mượn có mã: <b>{3}</b> còn <b>{4} ngày</b> sẽ đến hạn trả.<br>", j + 1, tenSach[j], ngayMuon[j], maPhieuMuon[j], soluong[j]);
                }
                body = body + "<br>Mong bạn sắp xếp thời gian để thực hiện trả sách đúng thời hạn. Đây là email gửi tự động, vui lòng không trả lời!";
                bool kq = mailService.Send(smtpUserName, smtpPassword, smtpHost, smtpPort, emailTo, subject, body);
            }
        }
    }
}