using System;
using System.Collections.Generic;

namespace Cutting_Optimizer
{
    [Serializable]
    public class Board
    {
        public List<Part> Parts = new List<Part> { };
        public int Width { get; set; }
        public int Height { get; set; }
        public int Thickness { get; set; }
        public float Price { get; set; }
        public string Price_string { get; set; }
        public float Amount { get; set; }
        public string Amount_string { get; set; }
        public bool Unlimited { get; set; }
        public string Bar { get; set; }
        public int SqlIndex { get; set; }


        public Board() { }        

        public Board(Board other)
        {
            this.Parts = other.Parts;
            this.Width = other.Width;
            this.Height = other.Height;
            this.Price = other.Price;
            this.Price_string = other.Price_string;
            this.Amount = other.Amount;
            this.Amount_string = other.Amount_string;
            this.Unlimited = other.Unlimited;
            this.Bar = other.Bar;
            this.Thickness = other.Thickness;
            this.SqlIndex = other.SqlIndex;
        }
    }
}
