namespace RandomWeb.Models
{
    public class ImageCropData
    {
        public string Image { get; set; }
        public int LeftCrop { get; set; }
        public int RightCrop { get; set; }
        public int TopCrop { get; set; }
        public int BottomCrop { get; set; }
        public int Rotate { get; set; }
        public double Rotate2 { get; set; }
    }
}