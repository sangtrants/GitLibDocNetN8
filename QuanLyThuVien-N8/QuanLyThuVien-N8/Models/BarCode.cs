using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyThuVien_N8.Models
{
    public class BarCode
    {
        public string Data;
        public string FullPath;
        public BarCode(string data, string path)
        {
            this.Data = data;
            this.FullPath = path;
        }
        public string CreateBarCode(string data, string path)
        {
            string kq = "Thanh Cong";
            BarcodeLib.Barcode.Linear codabar = new BarcodeLib.Barcode.Linear();
            codabar.Type = BarcodeLib.Barcode.BarcodeType.CODABAR;
            codabar.Data = data;
            string imageFullPath = path;
            this.FullPath = imageFullPath;
            codabar.UOM = BarcodeLib.Barcode.UnitOfMeasure.PIXEL;
            codabar.BarColor = System.Drawing.Color.Blue;
            codabar.BarWidth = 2;
            codabar.LeftMargin = 12;
            codabar.RightMargin = 12;
            codabar.drawBarcode(imageFullPath);
            string kt = "";
            try
            {
                string[] result = BarcodeLib.BarcodeReader.BarcodeReader.read(imageFullPath, BarcodeLib.BarcodeReader.BarcodeReader.CODABAR);
                kt = result[0];
                kq = imageFullPath;
                return kq;
            }
            catch (IndexOutOfRangeException ex)
            {
                kq = "Wrong input";
                return kq;
            }
        }
        public string ReadBarCode(string path)
        {
            string kq = "";
            try
            {
                string[] result = BarcodeLib.BarcodeReader.BarcodeReader.read(path, BarcodeLib.BarcodeReader.BarcodeReader.CODABAR);
                kq = result[0];
                return kq.Substring(1, kq.Length - 2);
            }
            catch (IndexOutOfRangeException ex)
            {
                return kq;
            }
        }
    }
}