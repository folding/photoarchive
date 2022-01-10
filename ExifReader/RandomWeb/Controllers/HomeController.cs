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
using Microsoft.AspNetCore.Hosting;
using System.Threading;
using PhotoArchive.Domain;

namespace RandomWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ImageMetaDataService MetaDataService { get; set; }

        private string[] folders = { @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\101APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\102APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\103APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\1960s",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\gigi and rayray's wedding",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\UTO 4th of July",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos",
            @"C:\Users\foldi\Dropbox\1-FilesToSort"};

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            MetaDataService = new ImageMetaDataService(folders);
        }

        public IActionResult Index(string id)
        {
            //AIzaSyCEwQziJOjmBPsI9Z6E40snOVWf3OBpsCc - MAPS key

            ImageMetaData imageMetaData = MetaDataService.GetImageMetaData(id);

            return View(imageMetaData);
        }


        public FileResult Image(string id)
        {
            return GetImage(id, 400, 400);
        }

        public FileResult ImageMobile(string id)
        {
            return GetImage(id, 300, 300);
        }
        public FileResult ImageFull(string id)
        {
            return GetImage(id, 0, 0);
        }
        public FileResult ImageThumb(string id)
        {
            return GetImage(id, 100, 100);
        }

        private FileResult GetImage(string id, int maxWidth, int maxHeight)
        { 
            if(string.IsNullOrEmpty(id))
            {
                return null;
            }

            var querystring = Request.Query;

            int leftCrop = -1
               , rightCrop = -1, topCrop = -1, bottomCrop=-1, rotate=0;

            foreach(var q in querystring)
            {
                if (q.Key == "rot")
                {
                    int tmp = 0;
                    if (int.TryParse(q.Value, out tmp))
                    {
                        rotate = tmp;
                    }
                }
                else if (q.Key == "crop")
                {
                    int[] coords = q.Value.ToString().Split(',').Select(x => int.Parse(x)).ToArray();

                    if(coords.Length == 4)
                    {
                        leftCrop = coords[0];
                        topCrop = coords[1];
                        rightCrop = coords[2];
                        bottomCrop = coords[3];
                    }
                }
            }

            byte[] imageBytes = MetaDataService.GetTransformedImage(id,maxWidth,maxHeight,rotate,leftCrop,topCrop,rightCrop,bottomCrop);

            return base.File(imageBytes, "image/jpeg");
        }

        [HttpPost]
        public JsonResult UpdateImage([FromBody]ImageCropData data)
        {
            ImageCoords newCrop = new ImageCoords
            {
                BottomCrop = data.BottomCrop,
                LeftCrop = data.LeftCrop,
                RightCrop = data.RightCrop,
                TopCrop = data.TopCrop,
                Rotate = data.Rotate
            };

            MetaDataService.UpdateCrop(data.Image, newCrop);

            return new JsonResult("yay");
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
