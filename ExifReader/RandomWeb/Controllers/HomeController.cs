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

        private string[] folders = { @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\101APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\102APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\103APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\1960s",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\gigi and rayray's wedding",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\UTO 4th of July",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos"};

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            Random random = new Random();
            ImageMetaData imageMetaData = new ImageMetaData();
            int imageId = 0;
            int folderId = 0;
            var loaded = false;
            ImageFile file = null;
            string[] pictures = null;

            while (!loaded)
            {
                try
                {
                    folderId = random.Next(0, folders.Count());

                    pictures = System.IO.Directory.GetFiles(folders[folderId]);

                    imageId = random.Next(0, pictures.Count());

                    file = ImageFile.FromFile(pictures[imageId]);

                    loaded = true;
                }
                catch
                { }
            }

            string json = JsonConvert.SerializeObject(file, Newtonsoft.Json.Formatting.Indented);

            imageMetaData.ISOSpeedRatings = GetPropertyBruteForce(file,ExifTag.ISOSpeedRatings);
            imageMetaData.PixelXDimension = GetPropertyBruteForce(file,ExifTag.PixelXDimension);
            imageMetaData.PixelYDimension = GetPropertyBruteForce(file,ExifTag.PixelYDimension);
            imageMetaData.GPSAltitude =     GetPropertyBruteForce(file,ExifTag.GPSAltitude)+"m "+ GetPropertyBruteForce(file, ExifTag.GPSAltitudeRef);
            imageMetaData.GPSLatitude =     GetPropertyBruteForce(file,ExifTag.GPSLatitude)+" "+ GetPropertyBruteForce(file, ExifTag.GPSLatitudeRef);
            imageMetaData.GPSLongitude = GetPropertyBruteForce(file, ExifTag.GPSLongitude)+" "+ GetPropertyBruteForce(file, ExifTag.GPSLongitudeRef);
            imageMetaData.ExifDateTime = GetPropertyBruteForce(file, ExifTag.DateTime);

            imageMetaData.Image = $"{folderId}-{imageId}";

            var path = pictures[imageId];
            imageMetaData.Path = path;

            byte[] imageBytes = System.IO.File.ReadAllBytes(path);
            var info = ImageJob.GetImageInfo(new BytesSource(imageBytes)).Result;

            imageMetaData.FileDateTime = System.IO.File.GetCreationTime(path);

            imageMetaData.Width = info.ImageWidth;
            imageMetaData.Height = info.ImageHeight;


            //load json meta data
            var jsonPath = path + ".json";
            var metadataexists = System.IO.File.Exists(jsonPath);
            if (metadataexists)
            {
                var metaData = System.IO.File.ReadAllText(jsonPath);

                ImageMetaData fileMetaData = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageMetaData>(metaData);

                imageMetaData.BottomCrop = fileMetaData.BottomCrop;
                imageMetaData.LeftCrop = fileMetaData.LeftCrop;
                imageMetaData.RightCrop = fileMetaData.RightCrop;
                imageMetaData.TopCrop = fileMetaData.TopCrop;
                imageMetaData.Rotate = fileMetaData.Rotate;
            }

            //default crops if they don't exist
            if(imageMetaData.RightCrop == 0)
            {
                imageMetaData.RightCrop = (int)imageMetaData.Width;
            }

            if(imageMetaData.BottomCrop == 0)
            {
                imageMetaData.BottomCrop = (int)imageMetaData.Height;
            }

            //AIzaSyCEwQziJOjmBPsI9Z6E40snOVWf3OBpsCc
            if(!metadataexists)
            {
                System.IO.File.WriteAllText(jsonPath, JsonConvert.SerializeObject(imageMetaData));
            }

            return View(imageMetaData);
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

                    if(tag == ExifTag.GPSAltitude)
                    {
                        return (((((MathEx.UFraction32)property.Value).Numerator * 100.0) / ((MathEx.UFraction32)property.Value).Denominator)/100.0).ToString("N2");
                    }

                    return property.Value.ToString();
                }
            }

            return "";
        }


        public FileResult Image(string id)
        {
            var querystring = Request.Query;

            int folderId = int.Parse(id.Split('-')[0]);
            int imageId = int.Parse(id.Split('-')[1]);

            string folder =folders[folderId];
            var pictures = System.IO.Directory.GetFiles(folder);

            var path = pictures[imageId]; //validate the path for security or use other means to generate the path.

            byte[] imageBytes = System.IO.File.ReadAllBytes(path);

            var info = ImageJob.GetImageInfo(new BytesSource(imageBytes)).Result;

            var notloaded = true;
            var reverseCrop = false;

            while (notloaded)
            {
                try
                {
                    using (var b = new ImageJob())
                    {
                        var buildNode = b.Decode(imageBytes);

                        foreach (var q in querystring)
                        {
                            if (q.Key == "crop")
                            {
                                int[] coords = q.Value.ToString().Split(',').Select(x => int.Parse(x)).ToArray();
                                if (coords.Count() == 4)
                                {
                                    if (!reverseCrop)
                                    {
                                        buildNode = buildNode.Crop(coords[0], coords[1], coords[2], coords[3]);
                                    }
                                    else
                                    {
                                        buildNode = buildNode.Region(coords[0], coords[1], coords[2], coords[3], AnyColor.Black);
                                       // buildNode = buildNode.Crop(coords[1], coords[0], coords[3], coords[2]);
                                    }
                                }
                            }

                            if (q.Key == "rot")
                            {
                                switch (q.Value.ToString())
                                {
                                    case "90":
                                        buildNode = buildNode.Rotate90();
                                        break;
                                    case "180":
                                        buildNode = buildNode.Rotate180();
                                        break;
                                    case "270":
                                        buildNode = buildNode.Rotate270();
                                        break;
                                }

                            }
                        }



                        var r = buildNode
                    .ResizerCommands("width=400&height=400&mode=max&scale=down")
                    .EncodeToBytes(new MozJpegEncoder(100, true))
                    .Finish().InProcessAsync().Result;

                        imageBytes = r.TryGet(1).TryGetBytes().Value.ToArray();
                        notloaded = false;
                    }
                }
                catch(Exception e)
                {
                    if(e.Message.Contains("Crop coordinates"))
                    {
                        reverseCrop = !reverseCrop;
                    }
                }
            }
            //var info = ImageJob.GetImageInfo(new BytesSource(imageBytes)).Result;
            //var mimeType = info.PreferredMimeType;

            //return base.File(imageBytes, mimeType);
            return base.File(imageBytes, "image/jpeg");
        }

        [HttpPost]
        public JsonResult UpdateImage([FromBody]ImageCropData data)
        {
            int folderId = int.Parse(data.Image.Split('-')[0]);
            int imageId = int.Parse(data.Image.Split('-')[1]);

            string folder = folders[folderId];
            var pictures = System.IO.Directory.GetFiles(folder);

            var path = pictures[imageId]+".json"; //validate the path for security or use other means to generate the path.

            ImageMetaData fileMetaData = new ImageMetaData();

            if (System.IO.File.Exists(path))
            {
                var metaData = System.IO.File.ReadAllText(path);

                fileMetaData = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageMetaData>(metaData);

            }

            fileMetaData.BottomCrop = data.BottomCrop;
            fileMetaData.LeftCrop = data.LeftCrop;
            fileMetaData.RightCrop = data.RightCrop;
            fileMetaData.TopCrop = data.TopCrop;
            fileMetaData.Rotate = data.Rotate;

            System.IO.File.WriteAllText(path,JsonConvert.SerializeObject(fileMetaData));

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
