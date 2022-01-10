using System;
using System.Collections.Generic;

namespace PhotoArchive.Domain
{
    public class ImageMetaData
    {
        public static string CurrentVersion = "2";

        //version 1 properties
        public string ISOSpeedRatings { get; set; }
        public string PixelXDimension { get; set; }
        public string PixelYDimension { get; set; }
        public string GPSAltitude { get; set; }
        public string GPSLatitude { get; set; }
        public string GPSLongitude { get; set; }
        public string ExifDateTime { get; set; }
        public string Image { get; set; }
        public string Path { get; set; }
        public DateTime FileDateTime { get; set; }
        public long Width { get; set; }
        public long Height { get; set; }
        public int LeftCrop { get; set; }
        public int RightCrop { get; set; }
        public int TopCrop { get; set; }
        public int BottomCrop { get; set; }
        public int Rotate { get; set; }

        //version 2 properties
        public string Version { get; set; }
        public List<ImageCoords> SubImages{get;set;}
    }
}