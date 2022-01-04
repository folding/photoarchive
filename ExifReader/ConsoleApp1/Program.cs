using ExifLibrary;
using Newtonsoft.Json;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //get files in folder
            //string folder = @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\1960s";
            string folder = @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\103APPLE";
            foreach (var file in System.IO.Directory.GetFiles(folder))
            {
                //read each file for exif data
                var picture = ImageFile.FromFile(file);
                //save exif data to json

                //foreach (var property in picture.Properties)
                //{
                //    Console.WriteLine(property.Name+":"+property.Value);
                //}

                string json = JsonConvert.SerializeObject(picture, Newtonsoft.Json.Formatting.Indented);

                Console.WriteLine(json);

                var x = ExifTag.ISOSpeedRatings;
                //var x = ExifTag.PixelXDimension;
                //var x = ExifTag.PixelYDimension;
                //var x = ExifTag.GPSAltitude;
                //var x = ExifTag.GPSLatitude;
                //var x = ExifTag.GPSLongitude;


            }



        }
    }
}
