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
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos",
            @"C:\Users\foldi\Dropbox\1-FilesToSort"};

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string id)
        {
            Random random = new Random();
            ImageMetaData imageMetaData = new ImageMetaData();
            int imageId = 0;
            int folderId = 0;
            var loaded = false;
            ImageFile exifData = null;
            string[] pictures = null;
            string imagePath = null;

            if (!string.IsNullOrEmpty(id))
            {

                folderId = int.Parse(id.Split('-')[0]);
                imageId = int.Parse(id.Split('-')[1]);

                string folder = folders[folderId];
                pictures = System.IO.Directory.GetFiles(folder);

                imagePath = pictures[imageId]; //validate the path for security or use other means to generate the path.

            }
            else
            {
                //load a random picture
                while (!loaded)
                {
                    try
                    {
                        folderId = random.Next(0, folders.Count());

                        pictures = System.IO.Directory.GetFiles(folders[folderId]);

                        imageId = random.Next(0, pictures.Count());

                        imagePath = pictures[imageId];

                        bool invalid = IsFileInvalid(imagePath);

                        if (!invalid)
                        {

                            loaded = true;
                        }
                    }
                    catch
                    { }
                }
            }

            //load json meta data
            var filename = System.IO.Path.GetFileName(imagePath);
            var jsonPath = System.IO.Path.Combine(folders[folderId],".meta", filename + ".json");

            try
            {
                exifData = ImageFile.FromFile(imagePath);
            }
            catch
            {
                
            }
            bool metaDataCurrent = true;
            bool metaDataExists = System.IO.File.Exists(jsonPath);
            if (metaDataExists)
            {
                var metaData = System.IO.File.ReadAllText(jsonPath);

                ImageMetaData fileMetaData = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageMetaData>(metaData);

                if(fileMetaData.Version != ImageMetaData.CurrentVersion)
                {
                    metaDataCurrent = false;
                }

                //convert v1 to v2
                if(string.IsNullOrEmpty(fileMetaData.Version))
                {
                    var subimage = new ImageCoords()
                    {
                        BottomCrop = fileMetaData.BottomCrop,
                        LeftCrop = fileMetaData.LeftCrop,
                        RightCrop = fileMetaData.RightCrop,
                        TopCrop = fileMetaData.TopCrop,
                        Rotate = fileMetaData.Rotate
                    };

                    fileMetaData.Version = "2";
                    fileMetaData.SubImages = new List<ImageCoords>();
                    fileMetaData.SubImages.Add(subimage);
                }

                imageMetaData = fileMetaData;
            }
            if (exifData != null)
            {
                string exifJson = JsonConvert.SerializeObject(exifData, Newtonsoft.Json.Formatting.Indented);

                imageMetaData.ISOSpeedRatings = GetPropertyBruteForce(exifData, ExifTag.ISOSpeedRatings);
                imageMetaData.PixelXDimension = GetPropertyBruteForce(exifData, ExifTag.PixelXDimension);
                imageMetaData.PixelYDimension = GetPropertyBruteForce(exifData, ExifTag.PixelYDimension);
                imageMetaData.GPSAltitude = GetPropertyBruteForce(exifData, ExifTag.GPSAltitude) + "m " + GetPropertyBruteForce(exifData, ExifTag.GPSAltitudeRef);
                imageMetaData.GPSLatitude = GetPropertyBruteForce(exifData, ExifTag.GPSLatitude) + " " + GetPropertyBruteForce(exifData, ExifTag.GPSLatitudeRef);
                imageMetaData.GPSLongitude = GetPropertyBruteForce(exifData, ExifTag.GPSLongitude) + " " + GetPropertyBruteForce(exifData, ExifTag.GPSLongitudeRef);
                imageMetaData.ExifDateTime = GetPropertyBruteForce(exifData, ExifTag.DateTime);
            }
            imageMetaData.Image = $"{folderId}-{imageId}";

            imageMetaData.Path = imagePath;

            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            var info = ImageJob.GetImageInfo(new BytesSource(imageBytes)).Result;

            imageMetaData.FileDateTime = System.IO.File.GetCreationTime(imagePath);

            imageMetaData.Width = info.ImageWidth;
            imageMetaData.Height = info.ImageHeight;

            

            //default crops if they don't exist
            if (imageMetaData.RightCrop == 0)
            {
                imageMetaData.RightCrop = (int)imageMetaData.Width;
            }

            if(imageMetaData.BottomCrop == 0)
            {
                imageMetaData.BottomCrop = (int)imageMetaData.Height;
            }

            //AIzaSyCEwQziJOjmBPsI9Z6E40snOVWf3OBpsCc - MAPS key

            //create file if it doesn't exist
           // if(!metaDataCurrent)
            {
                System.IO.File.WriteAllText(jsonPath, JsonConvert.SerializeObject(imageMetaData,Formatting.Indented));
            }

            return View(imageMetaData);
        }

        private bool IsFileInvalid(string imagePath)
        {
            var imgEx = System.IO.Path.GetExtension(imagePath);

            foreach (string extention in GetInvalidExtentions())
            {
                if (imgEx.ToLower() == extention.ToLower())
                {
                    return true;
                }
            }

            return false;
        }

        private List<string> GetInvalidExtentions()
        {
            var exceptions = new List<string>() { ".json",".aae",".mov"};
            //var expath = @"C:\Users\foldi\Dropbox\1-FilesToSort\exceptions.txt";
            //_readWriteLock.EnterReadLock();
            //try
            //{
            //    if (System.IO.File.Exists(expath))
            //    {
            //        exceptions.AddRange(System.IO.File.ReadAllLines(expath));
            //    }
            //}
            //finally
            //{
            //    _readWriteLock.ExitReadLock();
            //}
            return exceptions;
        }

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        private void SaveInvalidExtentions(List<string> extentions)
        {
            //_readWriteLock.EnterWriteLock();
            //try
            //{
            //    var expath = @"C:\Users\foldi\Dropbox\1-FilesToSort\exceptions.txt";
            //    System.IO.File.WriteAllLines(expath, extentions);
            //}
            //finally {
            //    _readWriteLock.ExitWriteLock();
            //}
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

        private FileResult GetImage(string id, int width, int height)
        { 
            if(string.IsNullOrEmpty(id))
            {
                return null;
            }

            var querystring = Request.Query;

            int folderId = int.Parse(id.Split('-')[0]);
            int imageId = int.Parse(id.Split('-')[1]);

            string folder =folders[folderId];
            var pictures = System.IO.Directory.GetFiles(folder);

            var path = pictures[imageId]; //validate the path for security or use other means to generate the path.

            byte[] imageBytes = System.IO.File.ReadAllBytes(path);


            var notloaded = true;
            var reverseCrop = false;

            while (notloaded)
            {
                try
                {
                    var info = ImageJob.GetImageInfo(new BytesSource(imageBytes)).Result;
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
                                    if (coords[0] == 0 &&
                                       coords[1] == 0 &&
                                       coords[2] == info.ImageWidth &&
                                       coords[3] == info.ImageHeight)
                                    {
                                        //dont need to crop
                                    }
                                    else
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

                        if (width > 0 && height > 0)
                        {
                            buildNode = buildNode.ResizerCommands("width=" + width + "&height=" + height + "&mode=max&scale=down");
                        }

                    var r = buildNode
                    .EncodeToBytes(new MozJpegEncoder(100, true))
                    .Finish().InProcessAsync().Result;

                        imageBytes = r.TryGet(1).TryGetBytes().Value.ToArray();
                        notloaded = false;
                    }
                }
                catch(Exception e)
                {
                    //sometimes this library gets the coordinates mixed up..
                    // doing a region on the second try seems to make it happy
                    if(e.Message.Contains("Crop coordinates"))
                    {
                        reverseCrop = !reverseCrop;
                    }

                    if(e.Message.Contains("ImageMalformed: NoEnabledDecoderFound"))
                    {
                        var exceptions = GetInvalidExtentions();

                        var extension = System.IO.Path.GetExtension(path);
                        

                        if (!exceptions.Contains(extension))
                        {
                            exceptions.Add(extension);
                        }

                        SaveInvalidExtentions(exceptions);

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
            var imagePath = pictures[imageId];
            var filename = System.IO.Path.GetFileName(imagePath);
            var jsonPath = System.IO.Path.Combine(folders[folderId], ".meta", filename + ".json");
           
            ImageMetaData fileMetaData = new ImageMetaData();

            if (System.IO.File.Exists(jsonPath))
            {
                var metaData = System.IO.File.ReadAllText(jsonPath);

                fileMetaData = Newtonsoft.Json.JsonConvert.DeserializeObject<ImageMetaData>(metaData);

            }

            fileMetaData.BottomCrop = data.BottomCrop;
            fileMetaData.LeftCrop = data.LeftCrop;
            fileMetaData.RightCrop = data.RightCrop;
            fileMetaData.TopCrop = data.TopCrop;
            fileMetaData.Rotate = data.Rotate;

            System.IO.File.WriteAllText(jsonPath,JsonConvert.SerializeObject(fileMetaData,Formatting.Indented));

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
