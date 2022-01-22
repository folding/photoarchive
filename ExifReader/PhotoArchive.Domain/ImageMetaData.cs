using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PhotoArchive.Domain
{
    public class ImageMetaData
    {
        public static string CurrentVersion = "4";

        //version 1 properties
        public string ISOSpeedRatings { get; set; }
        public string PixelXDimension { get; set; }
        public string PixelYDimension { get; set; }
        public string GPSAltitude { get; set; }
        public string GPSLatitude { get; set; }
        public string GPSLongitude { get; set; }
        public string ExifDateTime { get; set; }
        public string ImagePrev { get; set; }//not saved
        public string Image { get; set; }
        public string ImageNext { get; set; }//not saved
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

        //version 3 properties
        public double Rotate2 { get; set; }

        //version 4 properties
        public List<ImageComment> WhoWhat { get; set; }
        public List<ImageComment> When { get; set; }
        public List<ImageComment> Where { get; set; }
        public List<ImageComment> WhyHow { get; set; }
        [JsonIgnore]
        public string Transformation { 
            get
            {
                return "crop=" + LeftCrop + "," + TopCrop + "," + RightCrop + "," + BottomCrop + "&rot=" + Rotate + "&rot2=" + Rotate2;
            }
        }

        public ImageMetaData()
        {
            WhoWhat = new List<ImageComment>();
            When = new List<ImageComment>();
            Where = new List<ImageComment>();
            WhyHow = new List<ImageComment>();
        }
    }

}