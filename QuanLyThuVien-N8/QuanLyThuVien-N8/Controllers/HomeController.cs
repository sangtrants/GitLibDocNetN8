using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyThuVien_N8.Models;
using System.IO;
using PagedList;
using System.Security.Cryptography;
using System.Text;
using System.Data.Objects;

namespace QuanLyThuVien_N8.Controllers
{
    public class HomeController : Controller
    {
        const int SOSACHDUOCMUON = 20; // Số sách được mượn tối đa tại 1 thời điểm 
        const int SONGAYDUOCMUON = 15; // Số ngày được mượn cho 1 lần mượn của 1 cuốn sách
        const int SONGAYDUOCGIAHAN = 5; // Số ngày được mượn cho 1 lần gia hạn của 1 cuốn sách
        const int SOLANGIAHAN = 2; //  Số lần được gia hạn cho 1 cuốn sách
        QuanLyThuVienEntities data = new QuanLyThuVienEntities();
        //
        // GET: /Home/

        public ActionResult Index()
        {         
            return View();
        }
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
        //Kiểm tra login (Từ)
        [HttpPost]
        public ActionResult LoginValid(LoginModel model)
        {
            MD5 md5Hash = MD5.Create();
            string password = GetMd5Hash(md5Hash, model.Password);            
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            NguoiDung user = (from use in data.NguoiDungs
                              where use.TenDangNhap == model.UserName && use.MatKhau == password
                              select use).FirstOrDefault();
            if (user == null)
            {
                NhanVien em = (from use in data.NhanViens
                               where use.TenDangNhap == model.UserName && use.MatKhau == password
                               select use).FirstOrDefault();
                if (em != null)
                {
                    Session["user"] = em.TenDangNhap;
                    Session["type"] = "ThuThu";
                    return RedirectToAction("Index", "ThuThu");
                }
            }

            if (user != null)
            {
                model.SoLuongDenHan = 0;
                List<PhieuMuon> Muon = (from use in data.PhieuMuons
                                        where use.NguoiMuon == user.MaNguoiDung
                                        select use).Distinct().ToList();
                for (int i = 0; i < Muon.Count; i++)
                {
                    if (Muon[i].NgayTra <= DateTime.Today)
                        model.SoLuongDenHan++;
                }
                Session["user"] = user.TenDangNhap;
                Session["userid"] = user.MaNguoiDung;
                Session["type"] = "DocGia";
                Session["SoLanDenHan"] = model.SoLuongDenHan;
                if (model.RememberMe == true)
                {
                    HttpCookie myCookie = new HttpCookie("UserLogin");
                    myCookie["username"] = model.UserName;
                    myCookie["pass"] = model.Password;
                    myCookie.Expires = DateTime.Now.AddDays(1d);
                    Response.Cookies.Add(myCookie);
                }
                return View("~/Views/DocGia/Index.cshtml", model);
            }
            else
            {
                Session["error"] = "wrong";
                return RedirectToAction("Login");
            }
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Logout()
        {
            Session["user"] = null;
            Session["SoLanDenHan"] = null;
            Session["type"] = null;
            return View("~/Views/Home/Index.cshtml");
        }
        public ActionResult Demo()
        {
            return View();
        }

        //Lấy thông tin đọc giả cho phần cập nhật dashboard (Từ)
        public ActionResult suadg(string username)
        {
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            NguoiDung ngd = (from ngds in data.NguoiDungs
                             where ngds.TenDangNhap == username
                             select ngds).FirstOrDefault();
            NhanVien nv = (from ngds in data.NhanViens
                           where ngds.TenDangNhap == username
                           select ngds).FirstOrDefault();
            if (ngd != null)
                return View("~/Views/DocGia/SuaDocGia.cshtml", ngd);
            else if (nv != null)
                return View("~/Views/Home/SuaNhanVien.cshtml", nv);
            else
                return View("~/Views/Home/Index.cshtml"); ;
        }

        //Cập nhật thông tin đọc giả (Từ)
        public ActionResult SuaDocGia(FormCollection f)
        {
            int MaNguoiDung = int.Parse(f["MaNguoiDung"]);
            DateTime NgaySinh;
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            NguoiDung ngd = (from ngds in data.NguoiDungs
                             where ngds.MaNguoiDung == MaNguoiDung
                             select ngds).First();
            ngd.HoTen = f["HoTen"];
            ngd.SoCMND = f["SoCMND"];
            ngd.MatKhau = f["MatKhau"];
            if (DateTime.TryParse(f["NgaySinh"], out NgaySinh))
            {
                ngd.NgaySinh = DateTime.Parse(f["NgaySinh"]);
            }

            data.SaveChanges();
            return RedirectToAction("suadg", "Home", new { username = f["TenDangNhap"] });
        }

        //Cập nhật thông tin nhân viên (Từ)
        public ActionResult SuaNhanVien(FormCollection f)
        {
            int MaNguoiDung = int.Parse(f["MaNhanVien"]);
            DateTime NgaySinh;
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            NhanVien ngd = (from ngds in data.NhanViens
                            where ngds.MaNhanVien == MaNguoiDung
                            select ngds).First();
            ngd.HoTen = f["HoTen"];
            ngd.CMND = f["CMND"];
            ngd.TenDangNhap = f["TenDangNhap"];
            if (DateTime.TryParse(f["NgaySinh"], out NgaySinh))
            {
                ngd.NgaySinh = DateTime.Parse(f["NgaySinh"]);
            }
            ngd.MatKhau = f["MatKhau"];
            data.SaveChanges();
            return RedirectToAction("suadg", "Home", new { username = f["TenDangNhap"] });
        }

        //Cập nhật ảnh của người dùng (Từ)
        public ActionResult capnhatAnh(string username)
        {
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            NguoiDung ngd = (from ngds in data.NguoiDungs
                             where ngds.TenDangNhap == username
                             select ngds).First();
            return View("~/Views/DocGia/CapNhatAnh.cshtml", ngd);
        }

        [HttpPost]
        public ActionResult cnAnh(HttpPostedFileBase file, FormCollection f)
        {
            string mand = f["TenDangNhap"];
            if (ModelState.IsValid)
            {
                string temp = "";
                if (file != null)
                {
                    var tenAnh = mand + ".png";
                    var path = Path.Combine(Server.MapPath("~/Images"), tenAnh);
                    file.SaveAs(path);
                    temp = tenAnh;
                }
            }
            return RedirectToAction("suadg", "Home", new { username = mand });
        }

        // Tim kiem
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
                          (sach.NhaXuatBan == NhaXuatBan || NhaXuatBan == 0)
                          select sach);

            if (LoaiTimKiem == "TieuDe")
                result = (from sach in data.Saches
                          where ((sach.TenSach.Contains(TuKhoa) && GioiHan == "Chua") || (sach.TenSach == TuKhoa && GioiHan == "ChinhXac")) &&
                          (sach.NhaXuatBan == NhaXuatBan || NhaXuatBan == 0)
                          select sach);

            if (LoaiTimKiem == "TacGia")
                result = (from sach in data.Saches
                          from tacgia in data.TacGias
                          where (tacgia.MaTacGia == sach.TacGia &&
                          ((tacgia.TenTacGia.Contains(TuKhoa) && GioiHan == "Chua") || (tacgia.TenTacGia == TuKhoa && GioiHan == "ChinhXac"))) &&
                          (sach.NhaXuatBan == NhaXuatBan || NhaXuatBan == 0)
                          select sach);
            ViewBag.TuKhoa = TuKhoa;
            ViewBag.LoaiTimKiem = LoaiTimKiem;
            ViewBag.GioiHan = GioiHan;
            ViewBag.NhaXuatBan = NhaXuatBan;
            return PartialView("XemSach", result.ToList().Distinct().ToPagedList(page ?? 1, 2));
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

        // Quý - email
        public ActionResult GopY()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GopY(string tenDocGia, string emailDocGia,string tieude, string noidung)
        {
            QuanLyThuVien_N8.Models.EmailService mailService = new EmailService();
            string smtpUserName = "hotro.khtn@gmail.com";
            string smtpPassword = "nhom8httt";
            string smtpHost = "smtp.gmail.com";
            int smtpPort = 25;
            string emailTo = "thuvien.khtn@gmail.com";
            string subject = "Góp ý thư viện KHTN - " +tenDocGia + "-" + tieude;
            string body = string.Format("Bạn vừa nhận được liên hệ từ đọc giả <b>{0}</b> có email là: <b>{1} <br><br>  Nội dung:</b><br> <br> {2}", tenDocGia, emailDocGia, noidung);
            bool kq = mailService.Send(smtpUserName, smtpPassword, smtpHost, smtpPort, emailTo, subject, body);
            if (kq)
                ViewBag.Result = "OK";
            else
                ViewBag.Result = "Fail";
            QuanLyThuVienEntities data = new QuanLyThuVienEntities();
            GopY gy = new GopY();
            gy.NguoiGopY = tenDocGia;           
            gy.NoiDungGopY = noidung;
            gy.TimeGopY = DateTime.Now;
            gy.Email = emailDocGia;
            if (Session["user"] != null)
            {
                smtpUserName = Session["user"].ToString();
                NguoiDung user = (from use in data.NguoiDungs
                                  where use.TenDangNhap == smtpUserName
                                  select use).FirstOrDefault();
                gy.NguoiDungGopY = user.MaNguoiDung;
            }
            gy.DaXem = 0;
            gy.DeleteFlag = 0;
            data.Gopies.Add(gy);
            data.SaveChanges();
            return View(ViewBag);
        }

    }
}
