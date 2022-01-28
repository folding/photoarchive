using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoArchive.Services.Impl;

namespace RandomWeb.Controllers
{
    public class BaseController : Controller
    {
        protected ImageMetaDataService MetaDataService { get; set; }

        private string[] folders = { @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\101APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\102APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\103APPLE",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\1960s",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\gigi and rayray's wedding",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos\UTO 4th of July",
            @"C:\Users\foldi\Dropbox\1-FilesToSort\photos",
            @"C:\Users\foldi\Dropbox\1-FilesToSort",
            @"C:\Users\foldi\Dropbox\Family History\Family Photo Archives\To Sort\Rastall"};

        private static string queueFolder = @"C:\Users\foldi\Dropbox\John's Docs\Code Projects\photoarchivequeues";

        public BaseController()
        {
            MetaDataService = new ImageMetaDataService(folders,queueFolder);
        }

    }
}
