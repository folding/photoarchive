using ExifLibrary;
using Imageflow.Fluent;
using ImageMagick;
using Newtonsoft.Json;
using PhotoArchive.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoArchive.Services.Impl
{
    public class ImageMetaDataService
    {
        private string[] folders;

        public ImageMetaDataService(string[] folders)
        {
            this.folders = folders;
        }

        public void UpdateCrop(string image, ImageCoords newCrop)
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
            fileMetaData.Rotate2 = newCrop.Rotate2;

            System.IO.File.WriteAllText(jsonPath, JsonConvert.SerializeObject(fileMetaData, Formatting.Indented));

        }

        public ImageMetaData GetRandomImageMetaData()
        {
            int imageId = 0;
            int folderId = 0;
            Random random = new Random();
            bool loaded = false;
            //load a random picture
            while (!loaded)
            {
                try
                {

                    Dictionary<string, Tuple<int, int>> allfiles = new Dictionary<string, Tuple<int, int>>();

                    int foId = 0;
                    foreach (var f in folders)
                    {

                        List<string> folderfiles = System.IO.Directory.GetFiles(f).ToList();

                        int fiId = 0;
                        foreach (var ff in folderfiles)
                        {
                            allfiles.Add(ff, new Tuple<int, int>(foId, fiId));
                            fiId++;
                        }
                        foId++;
                    }

                    var rnd = random.Next(0, allfiles.Count);

                    var ras = allfiles.ToArray()[rnd];

                    folderId = ras.Value.Item1;
                    imageId = ras.Value.Item2;

                    string imagePath = ras.Key;

                    bool invalid = IsFileInvalid(imagePath);

                    if (!invalid)
                    {

                        loaded = true;
                    }
                }
                catch
                { }
            }

            return GetImageMetaData(folderId + "-" + imageId);
        }
        public ImageMetaData GetImageMetaData(string id)
        {
            ImageMetaData imageMetaData = new ImageMetaData();
            int imageId = 0;
            int folderId = 0;
            ImageFile exifData = null;
            string[] pictures = null;
            if (string.IsNullOrEmpty(id))
            {
                throw new Exception();
            }

            folderId = int.Parse(id.Split('-')[0]);
            imageId = int.Parse(id.Split('-')[1]);

            string folder = folders[folderId];
            pictures = System.IO.Directory.GetFiles(folder);

            string imagePath = pictures[imageId];




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
                    fileMetaData.Version = "2";

                    var subimage = new ImageCoords()
                    {
                        BottomCrop = fileMetaData.BottomCrop,
                        LeftCrop = fileMetaData.LeftCrop,
                        RightCrop = fileMetaData.RightCrop,
                        TopCrop = fileMetaData.TopCrop,
                        Rotate = fileMetaData.Rotate
                    };

                    fileMetaData.SubImages = new List<ImageCoords>();
                    fileMetaData.SubImages.Add(subimage);
                }

                //convert version 2 to 3
                if (fileMetaData.Version == "2")
                {
                    //This version added Rotate2 which defaults to zero...
                    fileMetaData.Version = "3";
                }

                //convert 3 to 4
                if (fileMetaData.Version == "3")
                {
                    //Adds WhoWhat, When, Where, WhyHow which default just fine too..
                    fileMetaData.Version = "4";
                }

                imageMetaData = fileMetaData;
            }
            if (exifData != null)
            {
                string exifJson = JsonConvert.SerializeObject(exifData, Newtonsoft.Json.Formatting.Indented);

                imageMetaData.ISOSpeedRatings = GetPropertyBruteForce(exifData, ExifLibrary.ExifTag.ISOSpeedRatings);
                imageMetaData.PixelXDimension = GetPropertyBruteForce(exifData, ExifLibrary.ExifTag.PixelXDimension);
                imageMetaData.PixelYDimension = GetPropertyBruteForce(exifData, ExifLibrary.ExifTag.PixelYDimension);
                imageMetaData.GPSAltitude = GetPropertyBruteForce(exifData, ExifLibrary.ExifTag.GPSAltitude) + "m " + GetPropertyBruteForce(exifData, ExifLibrary.ExifTag.GPSAltitudeRef);
                imageMetaData.GPSLatitude = GetPropertyBruteForce(exifData, ExifLibrary.ExifTag.GPSLatitude) + " " + GetPropertyBruteForce(exifData, ExifLibrary.ExifTag.GPSLatitudeRef);
                imageMetaData.GPSLongitude = GetPropertyBruteForce(exifData, ExifLibrary.ExifTag.GPSLongitude) + " " + GetPropertyBruteForce(exifData, ExifLibrary.ExifTag.GPSLongitudeRef);
                imageMetaData.ExifDateTime = GetPropertyBruteForce(exifData, ExifLibrary.ExifTag.DateTime);
            }

            //find the previous and next ids without going outside the size of the array
            int previousImageId = imageId == 0 ? pictures.Count() : imageId - 1;
            int nextImageId = imageId == pictures.Count() ? 0 : imageId + 1;

            //make sure we get a vaild picture type
            while(GetInvalidExtentions().Contains(System.IO.Path.GetExtension(pictures[previousImageId])))
            {
                previousImageId--;
            }
            while (GetInvalidExtentions().Contains(System.IO.Path.GetExtension(pictures[nextImageId])))
            {
                nextImageId++;
            }


            imageMetaData.ImagePrev = $"{folderId}-{imageId-1}";
            imageMetaData.Image = $"{folderId}-{imageId}";
            imageMetaData.ImageNext = $"{folderId}-{imageId+1}";

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

            //default location to Lat/Long if they exist
            if(imageMetaData.Where.Count == 0 &&
                !string.IsNullOrWhiteSpace(imageMetaData.GPSLatitude) &&
                !string.IsNullOrWhiteSpace(imageMetaData.GPSLongitude))
            {
                imageMetaData.Where.Add(new ImageComment
                {
                    Comment = imageMetaData.GPSLatitude + Environment.NewLine + imageMetaData.GPSLongitude + Environment.NewLine + imageMetaData.GPSAltitude,
                    CommentDateTime = DateTime.Now,
                    Commenter = "from exif"
                });

                imageMetaData.Where.Add(new ImageComment
                {
                    Comment = GetDecimalDegreesFromDegreesMinutesSeconds(imageMetaData.GPSLatitude) + "," + GetDecimalDegreesFromDegreesMinutesSeconds(imageMetaData.GPSLongitude) + Environment.NewLine + imageMetaData.GPSAltitude,
                    CommentDateTime = DateTime.Now,
                    Commenter = "converted from exif"
                });
            }

            //default time to Exif time if it exists
            if(imageMetaData.When.Count == 0 && !string.IsNullOrWhiteSpace(imageMetaData.ExifDateTime))
            {
                imageMetaData.When.Add(new ImageComment
                {
                    Comment = imageMetaData.ExifDateTime,
                    CommentDateTime = DateTime.Now,
                    Commenter = "from exif"
                });
            }


            //create file if it doesn't exist
            if(!System.IO.File.Exists(jsonPath) || !metaDataCurrent)
            {
                System.IO.File.WriteAllText(jsonPath, JsonConvert.SerializeObject(imageMetaData, Formatting.Indented));
            }

            return imageMetaData;
        }

        public ImageCoords GetImageCoords(string image)
        {
            ImageMetaData data = GetImageMetaData(image);

            return new ImageCoords
            {
                BottomCrop = data.BottomCrop,
                LeftCrop = data.LeftCrop,
                RightCrop = data.RightCrop,
                TopCrop = data.TopCrop,
                Rotate = data.Rotate,
                Rotate2 = data.Rotate2
            };

        }

        private string GetDecimalDegreesFromDegreesMinutesSeconds(string gPSLatitude)
        {
            if(string.IsNullOrWhiteSpace(gPSLatitude))
            {
                return "";
            }

            var parts = gPSLatitude.Split(' ');//61° 22' 2407 North;
            double deg = double.Parse(parts[0].TrimEnd('°'));
            double min = double.Parse(parts[1].TrimEnd('\''));
            double sec = double.Parse(parts[2]);

            double dmin = (min / 60.0);
            double dsec = (sec / 3600.0);

            string sign = parts[3].Contains("North") || parts[3].Contains("East") ? "" : "-";
            string degrees = sign + (deg + dmin + dsec).ToString("0.00000000");

            return degrees;
        }

        public void UpdateComment(string image, string type, ImageComment comment)
        {
            var metadata = GetImageMetaData(image);

            if (image != metadata.Image)
            {
                throw new Exception();
            }

            bool addedComment = false;
            switch (type)
            {
                case "WhoWhat":
                    if (NewCommentIsDifferent(metadata.WhoWhat, comment))
                    {
                        metadata.WhoWhat.Add(comment);
                        addedComment = true;
                    }
                    break;
                case "When":
                    if (NewCommentIsDifferent(metadata.When, comment))
                    {
                        metadata.When.Add(comment);
                        addedComment = true;
                    }
                    break;
                case "Where":
                    if (NewCommentIsDifferent(metadata.Where, comment))
                    {
                        metadata.Where.Add(comment);
                        addedComment = true;
                    }
                    break;
                case "WhyHow":
                    if (NewCommentIsDifferent(metadata.WhyHow, comment))
                    {
                        metadata.WhyHow.Add(comment);
                        addedComment = true;
                    }
                    break;
                default:
                    throw new Exception();
            }

            if (addedComment)
            {
                SaveImageMetaData(metadata);
            }
        }

        private bool NewCommentIsDifferent(List<ImageComment> whoWhat, ImageComment comment)
        {
            if (whoWhat.Count > 0)
            {
                var lastComment = whoWhat.OrderByDescending(x => x.CommentDateTime).Select(x => x.Comment).FirstOrDefault();

                return comment.Comment != lastComment;
            }

            return true;
        }

        private void SaveImageMetaData(ImageMetaData metadata)
        {
            int folderId = int.Parse(metadata.Image.Split('-')[0]);
            int imageId = int.Parse(metadata.Image.Split('-')[1]);

            string folder = folders[folderId];
            var pictures = System.IO.Directory.GetFiles(folder);
            var imagePath = pictures[imageId];

            //make sure we are looking at the right file
            if (imagePath != metadata.Path)
            {
                throw new Exception();
            }

            var filename = System.IO.Path.GetFileName(metadata.Path);
            var jsonPath = System.IO.Path.Combine(folders[folderId], ".meta", filename + ".json");



            System.IO.File.WriteAllText(jsonPath, JsonConvert.SerializeObject(metadata, Formatting.Indented));

        }

        public byte[] GetTransformedImage(string id, int maxWidth, int maxHeight, int rotate1 = 0, double rotate2 = 0.0, int leftCrop = -1, int topCrop = -1, int rightCrop = -1, int bottomCrop = -1)
        {
            int folderId = int.Parse(id.Split('-')[0]);
            int imageId = int.Parse(id.Split('-')[1]);

            string folder = folders[folderId];
            var pictures = System.IO.Directory.GetFiles(folder);

            var path = pictures[imageId]; //validate the path for security or use other means to generate the path.

            var ext = System.IO.Path.GetExtension(path);

            if(GetInvalidExtentions().Contains(ext.ToLower()))
            {
                return null;
            }

            byte[] imageBytes = System.IO.File.ReadAllBytes(path);


            var notloaded = true;
            var reverseCrop = false;

            var info = ImageJob.GetImageInfo(new BytesSource(imageBytes)).Result;

            while (notloaded)
            {
                try
                {
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

                        switch (rotate1)
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

            if (rotate2 != 0.0)
            {
                using (var image = new MagickImage(imageBytes))
                {
                    //https://legacy.imagemagick.org/Usage/photos/#rotation
                    //calculate the distortion so:
                    // image is the same size afterwards
                    // image is zoomed so there isn't any dead space in the corners
                    var w = image.Width;
                    var h = image.Height;

                    double angle = rotate2; //negative is counter-clockwise
                    var aa = angle * Math.PI / 180;//convert to radians

                    //TODO: This is dumb.  Depending on rotation (0,90,180,270) I think we need to handle only one
                    // of these next calculations.. one ends up like 1.02 and the other 1.3!  1.3 is tooo much so 
                    // I'm just taking the smaller one for now until I'm smarter...sigh
                    var scaleFactor1 = (w * Math.Abs(Math.Sin(aa)) + h * Math.Abs(Math.Cos(aa))) / Math.Min(w, h);
                    var scaleFactor2 = (w * Math.Abs(Math.Cos(aa)) + h * Math.Abs(Math.Sin(aa))) / Math.Min(w, h);
                    var scaleFactor = Math.Min(scaleFactor1, scaleFactor2);

                    image.Distort(DistortMethod.ScaleRotateTranslate, new double[] { scaleFactor, angle });

                    // Save the result
                    imageBytes = image.ToByteArray();
                }
            }

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
            var exceptions = new List<string>() { ".json",
                ".aae", //https://discussions.apple.com/thread/7810994
                ".mov", ".pdf", ".bmp" };
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

        private string GetPropertyBruteForce(ImageFile file, ExifLibrary.ExifTag tag)
        {
            foreach (var property in file.Properties)
            {
                if (property.Tag == tag)
                {
                    if (tag == ExifLibrary.ExifTag.GPSLatitude
                        || tag == ExifLibrary.ExifTag.GPSLongitude)
                    {
                        return (((GPSLatitudeLongitude)property).Degrees.Numerator * 100.0 / (((GPSLatitudeLongitude)property).Degrees.Denominator) / 100.0).ToString() + "° " +
                            (((GPSLatitudeLongitude)property).Minutes.Numerator * 100.0 / ((GPSLatitudeLongitude)property).Minutes.Denominator / 100.0).ToString() + "' " +
                            (((GPSLatitudeLongitude)property).Seconds.Numerator * 100.0 / ((GPSLatitudeLongitude)property).Seconds.Denominator / 100.0).ToString();
                    }

                    if (tag == ExifLibrary.ExifTag.GPSAltitude)
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
