using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyThuVien_N8.Models
{
    public class RemindMail
    {
        public int? MaPhieuMuon {get; set; } 
        public int MaChiTiet {get; set; }
        public int MaNguoiDung {get; set; }
        public string HoTen {get; set; }
        public string Email {get; set; }
        public DateTime? NgayMuon {get; set; }
        public int? SoNgayToiHan {get; set; }
        public string TenSach {get; set; }                
    }
}