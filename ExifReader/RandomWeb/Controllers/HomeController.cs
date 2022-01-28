using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoArchive.Domain;
using PhotoArchive.Services.Impl;
using RandomWeb.Models;
using System;
using System.Diagnostics;
using System.Linq;

namespace RandomWeb.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string id)
        {
            //AIzaSyCEwQziJOjmBPsI9Z6E40snOVWf3OBpsCc - MAPS key
            ImageMetaData imageMetaData;
            if (string.IsNullOrEmpty(id))
            {
                imageMetaData = MetaDataService.GetRandomImageMetaData();
            }
            else
            {
                imageMetaData = MetaDataService.GetImageMetaData(id);
            }

            ImageModel model = new ImageModel
            {
                //version 1 properties
                ISOSpeedRatings = imageMetaData.ISOSpeedRatings,
                PixelXDimension = imageMetaData.PixelXDimension,
                PixelYDimension = imageMetaData.PixelYDimension,
                GPSAltitude = imageMetaData.GPSAltitude,
                GPSLatitude = imageMetaData.GPSLatitude,
                GPSLongitude = imageMetaData.GPSLongitude,
                ExifDateTime = imageMetaData.ExifDateTime,
                ImagePrev = imageMetaData.ImagePrev,
                Image = imageMetaData.Image,
                ImageNext = imageMetaData.ImageNext,
                Path = imageMetaData.Path,
                FileDateTime = imageMetaData.FileDateTime,
                Width = imageMetaData.Width,
                Height = imageMetaData.Height,
                LeftCrop = imageMetaData.LeftCrop,
                RightCrop = imageMetaData.RightCrop,
                TopCrop = imageMetaData.TopCrop,
                BottomCrop = imageMetaData.BottomCrop,
                Rotate = imageMetaData.Rotate,

                //version 2 properties
                Version = imageMetaData.Version,
                SubImages = imageMetaData.SubImages,

                //version 3 properties
                Rotate2 = imageMetaData.Rotate2,

                //version 4 properties
                WhoWhat = imageMetaData.WhoWhat.OrderByDescending(x => x.CommentDateTime).Select(x => x.Comment).FirstOrDefault(),
                When = imageMetaData.When.OrderByDescending(x => x.CommentDateTime).Select(x => x.Comment).FirstOrDefault(),
                Where = imageMetaData.Where.OrderByDescending(x => x.CommentDateTime).Select(x => x.Comment).FirstOrDefault(),
                WhyHow = imageMetaData.WhyHow.OrderByDescending(x => x.CommentDateTime).Select(x => x.Comment).FirstOrDefault(),

            };

            return View(model);
        }

        public IActionResult Rotate(string id)
        {
            //AIzaSyCEwQziJOjmBPsI9Z6E40snOVWf3OBpsCc - MAPS key
            ImageMetaData imageMetaData;
            if (string.IsNullOrEmpty(id))
            {
                var path = MetaDataService.QueuePeek("needs-rotation");

                if (!string.IsNullOrEmpty(path))
                {
                    imageMetaData = MetaDataService.GetImageMetaDataByPath(path);
                }
                else
                {
                    imageMetaData = MetaDataService.GetRandomImageMetaData();
                }
            }
            else
            {
                imageMetaData = MetaDataService.GetImageMetaData(id);
            }

            ImageRotateModel model = new ImageRotateModel
            {

                Image = imageMetaData.Image,
                Path = imageMetaData.Path,
                Rotate = imageMetaData.Rotate,
                Transformation = imageMetaData.Transformation
            };

            return View(model);
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
                Rotate = data.Rotate,
                Rotate2 = data.Rotate2
            };

            MetaDataService.UpdateCrop(data.Image, newCrop);

            return new JsonResult("yay");
        }

        public JsonResult UpdateRotation([FromBody] ImageRotateData data)
        {
            //get current
            ImageCoords tranformation = MetaDataService.GetImageCoords(data.Image);

            //update rotation
            tranformation.Rotate = data.Rotate;

            //save
            MetaDataService.UpdateCrop(data.Image, tranformation);

            return new JsonResult("yay");
        }

        public JsonResult ConfirmRotation([FromBody] ImageRotateData data)
        {
            var peek = MetaDataService.QueuePeek("needs-rotation");
            var md = MetaDataService.GetImageMetaDataByPath(peek);

            if (data.Image == md.Image)
            {

                //move queues
                var item = MetaDataService.Dequeue("needs-rotation");

                MetaDataService.Queue("has-rotation", item);

                return new JsonResult("yay");
            }
            else
            {
                throw new Exception("wrong imageid");
            }
        }



        [HttpPost]
        public JsonResult UpdateComment([FromBody] ImageCommentModel data)
        {
            ImageComment comment = new ImageComment
            {
                Comment = data.Comment,
                CommentDateTime = DateTime.Now,
                Commenter = "jjc"
            };

            MetaDataService.UpdateComment(data.Image, data.Type, comment);

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
