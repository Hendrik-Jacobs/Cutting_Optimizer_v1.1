using System;

namespace Cutting_Optimizer
{
    [Serializable]
    public class Part
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Thickness { get; set; }
        public int XPosition { get; set; }
        public int YPosition { get; set; }
        public int Amount { get; set; }
        public bool Rotate { get; set; }
        public string Bar { get; set; }

        public Part() {}

        public Part(Part other)
        {
            this.Width = other.Width;
            this.Height = other.Height;
            this.XPosition = other.XPosition;
            this.YPosition = other.YPosition;
            this.Amount = other.Amount;
            this.Rotate = other.Rotate;
            this.Bar = other.Bar;
            this.Thickness = other.Thickness;
        }
    }

}
