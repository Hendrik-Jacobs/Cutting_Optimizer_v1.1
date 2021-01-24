// If adding Option also update:
//          - SQL Database (boards_db.options)
//          - Reading values when starting application
//          - OptionsWindow (xaml)
//          - Set Value when staring OptionsWindow
//          - Saving values when closing OptionsWindow
//          - Help Text


namespace Cutting_Optimizer
{
    static class Options
    {
        public static int MaxStage { get; set; }
        public static bool ShowParttables { get; set; }
        public static bool ShowPrices { get; set; }
        public static bool SumParts { get; set; }
        public static bool SumBoards { get; set; }
        public static bool SumBoardsStock { get; set; }
        public static bool Shift { get; set; }
        public static bool CloseHolesEnd { get; set; }
        public static bool CloseHolesEvery { get; set; }
        public static bool SimpleMode { get; set; }
        public static bool StockFirst { get; set; }
        public static bool SortParts { get; set; }
    }
}
