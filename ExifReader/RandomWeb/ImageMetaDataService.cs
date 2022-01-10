﻿using ExifLibrary;
using Imageflow.Fluent;
using Newtonsoft.Json;
using PhotoArchive.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RandomWeb
{
    public class ImageMetaDataService
    {
        private string[] folders;

        public ImageMetaDataService(string[] folders)
        {
            this.folders = folders;
        }

        internal void UpdateCrop(string image, ImageCoords newCrop)
        {


            int folderId = int.Parse(image.Split('-')[0]);
            int imageId = int.Parse(image.Split('-')[1]);

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

            fileMetaData.BottomCrop = newCrop.BottomCrop;
            fileMetaData.LeftCrop = newCrop.LeftCrop;
            fileMetaData.RightCrop = newCrop.RightCrop;
            fileMetaData.TopCrop = newCrop.TopCrop;
            fileMetaData.Rotate = newCrop.Rotate;

            System.IO.File.WriteAllText(jsonPath, JsonConvert.SerializeObject(fileMetaData, Formatting.Indented));

        }

        internal ImageMetaData GetImageMetaData(string id)
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
            var jsonPath = System.IO.Path.Combine(folders[folderId], ".meta", filename + ".json");

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

                if (fileMetaData.Version != ImageMetaData.CurrentVersion)
                {
                    metaDataCurrent = false;
                }

                //convert v1 to v2
                if (string.IsNullOrEmpty(fileMetaData.Version))
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

            if (imageMetaData.BottomCrop == 0)
            {
                imageMetaData.BottomCrop = (int)imageMetaData.Height;
            }


            //create file if it doesn't exist
            // if(!metaDataCurrent)
            {
                System.IO.File.WriteAllText(jsonPath, JsonConvert.SerializeObject(imageMetaData, Formatting.Indented));
            }

            return imageMetaData;
        }

        internal byte[] GetTransformedImage(string id, int maxWidth, int maxHeight, int rotate = 0, int leftCrop = -1, int topCrop = -1, int rightCrop = -1, int bottomCrop = -1)
        {
            int folderId = int.Parse(id.Split('-')[0]);
            int imageId = int.Parse(id.Split('-')[1]);

            string folder = folders[folderId];
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

                        if ((leftCrop == 0 &&
                           topCrop == 0 &&
                           rightCrop == info.ImageWidth &&
                           bottomCrop == info.ImageHeight)
                           || (leftCrop == -1 &&
                           topCrop == -1 &&
                           rightCrop == -1 &&
                           bottomCrop == -1))
                        {
                            //dont need to crop
                        }
                        else
                        {
                            if (!reverseCrop)
                            {
                                buildNode = buildNode.Crop(leftCrop, topCrop, rightCrop, bottomCrop);
                            }
                            else
                            {
                                buildNode = buildNode.Region(leftCrop, topCrop, rightCrop, bottomCrop, AnyColor.Black);
                            }
                        }

                        switch (rotate)
                        {
                            case 90:
                                buildNode = buildNode.Rotate90();
                                break;
                            case 180:
                                buildNode = buildNode.Rotate180();
                                break;
                            case 270:
                                buildNode = buildNode.Rotate270();
                                break;
                        }


                        if (maxWidth > 0 && maxHeight > 0)
                        {
                            buildNode = buildNode.ResizerCommands("width=" + maxWidth + "&height=" + maxHeight + "&mode=max&scale=down");
                        }

                        var r = buildNode
                        .EncodeToBytes(new MozJpegEncoder(100, true))
                        .Finish().InProcessAsync().Result;

                        imageBytes = r.TryGet(1).TryGetBytes().Value.ToArray();
                        notloaded = false;
                    }
                }
                catch (Exception e)
                {
                    //sometimes this library gets the coordinates mixed up..
                    // doing a region on the second try seems to make it happy
                    if (e.Message.Contains("Crop coordinates"))
                    {
                        reverseCrop = !reverseCrop;
                    }

                    if (e.Message.Contains("ImageMalformed: NoEnabledDecoderFound"))
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

            return imageBytes;
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
            var exceptions = new List<string>() { ".json", ".aae", ".mov" };
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
            foreach (var property in file.Properties)
            {
                if (property.Tag == tag)
                {
                    if (tag == ExifTag.GPSLatitude
                        || tag == ExifTag.GPSLongitude)
                    {
                        return ((GPSLatitudeLongitude)property).Degrees.Numerator.ToString() + "° " +
                            ((GPSLatitudeLongitude)property).Minutes.Numerator.ToString() + "' " +
                            ((GPSLatitudeLongitude)property).Seconds.Numerator.ToString();
                    }

                    if (tag == ExifTag.GPSAltitude)
                    {
                        return (((((MathEx.UFraction32)property.Value).Numerator * 100.0) / ((MathEx.UFraction32)property.Value).Denominator) / 100.0).ToString("N2");
                    }

                    return property.Value.ToString();
                }
            }

            return "";
        }


    }
}
