using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace QuanLyThuVien_N8.Controllers
{
    public class BarcodeController : Controller
    {
        //
        // GET: /Barcode/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetStatus()
        {
            return View();
        }

        public ActionResult CreateBarCodeView()
        {
            string data = Request["txtBarcode"];
            string path = Path.Combine( HttpRuntime.AppDomainAppPath ,"BarCode\\Created\\" , data + ".jpg");
            QuanLyThuVien_N8.Models.BarCode b = new Models.BarCode(data,path);
            b.CreateBarCode(data, path);
            return View(b);
        }
        public ActionResult GetBarCodeView()
        {
            return View();
        }
        public ActionResult GetBarCodeView_Camera()
        {
            return View();
        }
        public ActionResult GetBarCodeView_File()
        {
            return View();
        }
        [HttpPost]
        public JsonResult SaveImage()
        {
            if (Request.InputStream.Length > 0)
            {
                using (StreamReader reader = new StreamReader(Request.InputStream))
                {
                    string hexString = Server.UrlEncode(reader.ReadToEnd());
                    string imageName = DateTime.Now.ToString("dd-MM-yy hh-mm-ss");
                    string imagePath = string.Format("/BarCode/Captured/{0}.png", imageName);
                    string imageFullPath = Server.MapPath(imagePath);
                    MakeSureDirectoryExists(Path.GetDirectoryName(imageFullPath));
                    System.IO.File.WriteAllBytes(Server.MapPath(imagePath), ConvertHexToBytes(hexString));
                    //string[] result = BarcodeLib.BarcodeReader.BarcodeReader.read(imageFullPath, BarcodeLib.BarcodeReader.BarcodeReader.CODABAR);
                    //string kq = result[0];
                }
            }

            return Json(new { success = true });
        }

        private static void MakeSureDirectoryExists(string directory)
        {
            if (Directory.Exists(directory)) { return; }

            Directory.CreateDirectory(directory);
        }

        private static byte[] ConvertHexToBytes(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

    }
}
