using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using SrtReformatterCore.Models;

namespace SrtReformatterCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration Configuration;

        public static List<string> pathList { get; set; }

        public HomeController(IConfigurationRoot iConfig)
        {
            Configuration = iConfig;
            pathList = this.Configuration.GetSection("srtPath").Value.Split(',').ToList();
        }

        public IActionResult Index()
        {
            var myfiles = ConvertFiles();
            return this.View(myfiles);
        }

        public static List<string> ConvertFiles()
        {
            List<string> srtFiles = new List<string>();
            foreach (var path in pathList)
            {
                srtFiles.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(s => s.Contains(".srt") && !Equals(GetEncoding(s), Encoding.UTF8)));
            }
            AnsiConvert(srtFiles);
            return srtFiles.ToList();
        }

        public static void AnsiConvert(IEnumerable<string> fileList)
        {
            Parallel.ForEach(fileList, f => { ConvertFileEncoding(f, Encoding.UTF8); });
        }

        /// <summary>
        /// Converts a file from one encoding to another.
        /// </summary>
        /// <param name="path">the file to convert</param>
        /// <param name="destPath">the destination for the converted file</param>
        /// <param name="sourceEncoding">the original file encoding</param>
        /// <param name="destEncoding">the encoding to which the contents should be converted</param>
        public static void ConvertFileEncoding(string path, Encoding destEncoding)
        {
            try
            {
                //if (Equals(GetEncoding(path), Encoding.UTF8)) return;

                Encoding fileEncode = Encoding.GetEncoding("ISO-8859-9");

                string stt = System.IO.File.ReadAllText(path, fileEncode);

                byte[] bytes = fileEncode.GetBytes(stt);

                byte[] utfBytes = Encoding.Convert(fileEncode, Encoding.UTF8, bytes);

                var utf8_String = Encoding.UTF8.GetString(utfBytes);

                System.IO.File.Delete(path);

                System.IO.File.WriteAllText(path, utf8_String, Encoding.UTF8);
            }
            catch (Exception)
            {

            }

        }

        public static Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.ASCII;
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
