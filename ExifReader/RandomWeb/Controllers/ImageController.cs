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
    public class ImageController : BaseController
    {
        private readonly ILogger<ImageController> _logger;

        public ImageController(ILogger<ImageController> logger)
        {
            _logger = logger;
        }

        public FileResult Thumb(string id)
        {
            return GetImage(id, 150, 100);
        }

        public FileResult Small(string id)
        {
            return GetImage(id, 300, 300);
        }

        public FileResult Medium(string id)
        {
            return GetImage(id, 400, 400);
        }

        public FileResult Large(string id)
        {
            return GetImage(id, 600, 600);
        }

        public FileResult Full(string id)
        {
            return GetImage(id);
        }
        public FileResult Full60(string id)
        {
            return GetImage(id,800,800,60);
        }

        private FileResult GetImage(string id, int maxWidth = 0, int maxHeight = 0, int quality=100)
        { 
            if(string.IsNullOrEmpty(id))
            {
                return null;
            }

            var querystring = Request.Query;

            int leftCrop = -1
               , rightCrop = -1, topCrop = -1, bottomCrop=-1, rotate=0;
            double rotate2 = 0.0;
            foreach(var q in querystring)
            {
                if (q.Key == "rot")
                {
                    int tmp = 0;
                    if (int.TryParse(q.Value.Last(), out tmp))//take last rot value (if there is more than one in the query string)
                    {
                        rotate = tmp;
                    }
                }
                else if (q.Key == "rot2")
                {
                    double tmp = 0;
                    if (double.TryParse(q.Value, out tmp))
                    {
                        rotate2 = tmp;
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

            byte[] imageBytes = MetaDataService.GetTransformedImage(id,maxWidth,maxHeight,rotate,rotate2,leftCrop,topCrop,rightCrop,bottomCrop,quality);

            if(imageBytes == null)
            {
                return null;
            }

            return base.File(imageBytes, "image/jpeg");
        }

    }
}
