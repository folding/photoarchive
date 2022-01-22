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
    public class ImageController : Controller
    {
        private readonly ILogger<ImageController> _logger;
        private ImageMetaDataService MetaDataService { get; set; }

        private string[] folders = { @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\101APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\102APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\103APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\1960s",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\gigi and rayray's wedding",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\UTO 4th of July",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos",
            @"C:\Users\foldi\Dropbox\1-FilesToSort",
            @"C:\Users\foldi\Dropbox\Family History\Family Photo Archives\To Sort\Rastall"};

        public ImageController(ILogger<ImageController> logger)
        {
            _logger = logger;
            MetaDataService = new ImageMetaDataService(folders);
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

        public FileResult Full(string id)
        {
            return GetImage(id, 0, 0);
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
            double rotate2 = 0.0;
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

            byte[] imageBytes = MetaDataService.GetTransformedImage(id,maxWidth,maxHeight,rotate,rotate2,leftCrop,topCrop,rightCrop,bottomCrop);

            if(imageBytes == null)
            {
                return null;
            }

            return base.File(imageBytes, "image/jpeg");
        }

    }
}
