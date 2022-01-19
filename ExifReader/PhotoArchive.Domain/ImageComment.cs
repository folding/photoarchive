using System;

namespace PhotoArchive.Domain
{
    public class ImageComment
    {
        public string Comment { get; set; }
        public string Commenter { get; set; }
        public DateTime CommentDateTime { get; set; }
    }
}