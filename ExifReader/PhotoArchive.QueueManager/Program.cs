using PhotoArchive.Services.Impl;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    Console.WriteLine("rot-"+meta.Rotate);

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
                    else if(!hasRotation && shouldQueue) //if not put in other
                    {
                        dataService.Queue("needs-rotation", file);
                    }
            }
        }
    }
}
