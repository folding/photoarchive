using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExifLibrary;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RandomWeb.Models;
using Imageflow.Fluent;

namespace RandomWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {

            string folder = @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\103APPLE";
            var pictures = System.IO.Directory.GetFiles(folder);

            Random random = new Random();
            int imageId = 0;
            var loaded = false;
            ImageFile file = null;

            while (!loaded)
            {
                try
                {
                    imageId = random.Next(0, pictures.Count());

                    file = ImageFile.FromFile(pictures[imageId]);

                    loaded = true;
                }
                catch
                { }
            }

            string json = JsonConvert.SerializeObject(file, Newtonsoft.Json.Formatting.Indented);

                ViewBag.ISOSpeedRatings = GetPropertyBruteForce(file,ExifTag.ISOSpeedRatings);
            ViewBag.PixelXDimension = GetPropertyBruteForce(file,ExifTag.PixelXDimension);
            ViewBag.PixelYDimension = GetPropertyBruteForce(file,ExifTag.PixelYDimension);
            ViewBag.GPSAltitude =     GetPropertyBruteForce(file,ExifTag.GPSAltitude);
            ViewBag.GPSLatitude =     GetPropertyBruteForce(file,ExifTag.GPSLatitude);
            ViewBag.GPSLongitude = GetPropertyBruteForce(file, ExifTag.GPSLongitude);

            ViewBag.Image = imageId;

            //AIzaSyCEwQziJOjmBPsI9Z6E40snOVWf3OBpsCc


            return View();
        }

        private string GetPropertyBruteForce(ImageFile file, ExifTag tag)
        {
            foreach(var property in file.Properties)
            {
                if(property.Tag == tag)
                {
                    if(tag == ExifTag.GPSLatitude
                        || tag == ExifTag.GPSLongitude)
                    {
                        return ((GPSLatitudeLongitude)property).Degrees.Numerator.ToString() + "° " +
                            ((GPSLatitudeLongitude)property).Minutes.Numerator.ToString() + "' " +
                            ((GPSLatitudeLongitude)property).Seconds.Numerator.ToString();
                    }

                    return property.Value.ToString();
                }
            }

            return "";
        }


        public FileResult Image(int id)
        {
            string folder = @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\103APPLE";
            var pictures = System.IO.Directory.GetFiles(folder);

            var path = pictures[id]; //validate the path for security or use other means to generate the path.

            byte[] imageBytes = System.IO.File.ReadAllBytes(path);
            using (var b = new ImageJob())
            {
                var r =  b.Decode(imageBytes)
            .ResizerCommands("width=400&height=400&mode=max&scale=down")
            .EncodeToBytes(new MozJpegEncoder(80, true))
            .Finish().InProcessAsync().Result;

                imageBytes = r.TryGet(1).TryGetBytes().Value.ToArray();
            }

            return base.File(imageBytes, "image/jpeg");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
