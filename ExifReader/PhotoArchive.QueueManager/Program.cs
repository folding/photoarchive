using PhotoArchive.Domain;
using PhotoArchive.Services.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace PhotoArchive.QueueManager
{
    class Program
    {
        private static string[] folders = { @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\101APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\102APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\103APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\1960s",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\gigi and rayray's wedding",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\UTO 4th of July",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos",
            @"C:\Users\foldi\Dropbox\1-FilesToSort",
            @"C:\Users\foldi\Dropbox\Family History\Family Photo Archives\To Sort\Rastall"};


        private static string queueFolder = @"C:\Users\foldi\Dropbox\John's Docs\Code Projects\photoarchivequeues";
        static void Main(string[] args)
        {
            ImageMetaDataService dataService = new ImageMetaDataService(folders, queueFolder);

            //SeedRotationQueues(dataService);

            DoDuplicates(dataService);
        }

        private static void DoDuplicates(ImageMetaDataService dataService)
        {

            List<string> files = new List<string>();

            //get all folders/files
            foreach (var folder in folders)
            {
                files.AddRange(System.IO.Directory.GetFiles(folder));
            }

            //var hasRot = dataService.QueueContents("has-rotation");
            //var needsRot = dataService.QueueContents("needs-rotation");

            //files = files.Where(x => !hasRot.Contains(x)).Where(x => !needsRot.Contains(x)).ToList();

            Dictionary<string, ImageMetaData> dic = new Dictionary<string, ImageMetaData>();
            List<string> hashes = new List<string>();
            List<Tuple<ImageMetaData, ImageMetaData>> dups = new List<Tuple<ImageMetaData, ImageMetaData>>();

            //get meta data for each
            foreach (var file in files)
            {
                bool shouldHash = true;
                //skip invalid types
                if (dataService.IsFileInvalid(file))
                {
                    shouldHash = false;
                }

                if (shouldHash)
                {
                    var meta = dataService.GetImageMetaDataByPath(file);

                    if (!hashes.Contains(meta.Hash))
                    {
                        hashes.Add(meta.Hash);
                        dic.Add(meta.Hash, meta);
                    }
                    else//duplicate!
                    {
                        Console.WriteLine("hash-" + meta.Hash);
                        Console.WriteLine(file + " and "+ dic[meta.Hash].Path);
                        dups.Add(new Tuple<ImageMetaData, ImageMetaData>(meta, dic[meta.Hash]));
                    }


                }
            }

            foreach(var dup in dups)
            {
                var one = dup.Item1;
                var two = dup.Item2;

                dataService.Queue("duplicates", one.Path + ";" + two.Path);
            }

        }


        private static void SeedRotationQueues(ImageMetaDataService dataService)
        {

            List<string> files = new List<string>();

            //get all folders/files
            foreach (var folder in folders)
            {
                files.AddRange(System.IO.Directory.GetFiles(folder));
            }

            var hasRot = dataService.QueueContents("has-rotation");
            var needsRot = dataService.QueueContents("needs-rotation");

            files = files.Where(x => !hasRot.Contains(x)).Where(x => !needsRot.Contains(x)).ToList();

            //get meta data for each
            foreach (var file in files)
            {
                Console.WriteLine(file);

                bool shouldQueue = true;
                bool hasRotation = false;
                //skip invalid types
                if (dataService.IsFileInvalid(file))
                {
                    shouldQueue = false;
                }

                if (shouldQueue)
                {
                    var meta = dataService.GetImageMetaDataByPath(file);
                    Console.WriteLine("rot-" + meta.Rotate);

                    if (meta.Rotate > 0 || !string.IsNullOrWhiteSpace(meta.GPSLatitude))
                    {
                        hasRotation = true;
                    }
                }

                //if has rotation put it in one list
                if (hasRotation)
                {
                    dataService.Queue("has-rotation", file);
                }
                else if (!hasRotation && shouldQueue) //if not put in other
                {
                    dataService.Queue("needs-rotation", file);
                }
            }
        }
    }
}
