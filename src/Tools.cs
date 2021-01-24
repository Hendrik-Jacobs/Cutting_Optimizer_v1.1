namespace Cutting_Optimizer
{
    public static class Tools
    {
        public static bool CheckIfPartsEqual(Part part, Part part2)
        {
            return CheckIfPartsEqual(part.Width, part.Height, part.Thickness,
                                     part2.Width, part2.Height, part2.Thickness);
        }


        public static bool CheckIfPartsEqual(int width, int height, int thickness, int width2, int height2, int thickness2)
        {
            if (((width == width2 && height == height2) ||
                (width == height2 && height == width2)) &&
                 thickness == thickness2)
            {
                return true;
            }
            return false;
        }



        public static bool CheckIfBoardsEqual(Board board, Board board2)
        {
            if (((board.Width == board2.Width && board.Height == board2.Height) ||
                (board.Width == board2.Height && board.Height == board2.Width)) &&
                board.Price == board2.Price)
            {
                return true;
            }
            return false;
        }



        public static bool IsNumber(string text)
        {
            return int.TryParse(text, out int output);
        }
    }
}
