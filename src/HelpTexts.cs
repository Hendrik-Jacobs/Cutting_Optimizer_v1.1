namespace Cutting_Optimizer
{
    // Texts shown in 'Help'-Window
    class HelpTexts
    {
        public static string Boards = "1D objects are called BARS in this program.\n" +
                                      "2D objects are called BOARDS in this program.\n" +
                                      "Every BAR has the following properties:\n" +
                                      "     - LENGTH\n" +
                                      "     - THICKNESS\n" +
                                      "     - AMOUNT\n" +
                                      "     - PRICE\n\n" +
                                      "Every BOARD has the following properties:\n" +
                                      "     - WIDTH\n" +
                                      "     - HEIGHT\n" +
                                      "     - THICKNESS\n" +
                                      "     - AMOUNT\n" +
                                      "     - PRICE\n\n" +
                                      "WIDTH, HEIGHT, LENGTH, THICKNESS, PRICE can not be 0.\n" + 
                                      "Amount can be a number or * for unlimited amount.\n" +
                                      "PRICE can be 0.";


        public static string BoardManager = "- Add Boards to the list like in the Main Window. \n" +
                                            "- This list gets saved in a SQL database. \n" + 
                                            "- If you change the order this gets saved, too. \n" + 
                                            "- If you click the 'Add to Main Window'-button \n" + 
                                            "  the selected Board appears in the Main Window \n" + 
                                            "  Board-List, when you close the Board Manager \n" + 
                                            "  Window. \n" +
                                            "- Shortcuts: \n" + 
                                            "       Q - Switch to 1D-Input\n" +
                                            "       W - Switch to 2D-Input\n" +
                                            "\n\n" +
                                            "In Main Window: \n" +
                                            "       - 'Board Manager'-Button opens Board Manager. \n" +
                                            "       - 'Use Stock'-Checkbox: Add Boards/Bars with \n" +
                                            "         correct Thickness to Main Window from stock \n" +
                                            "         automatically when clicking 'Run'. \n" +
                                            "         The Boards will be in the same order like \n" +
                                            "         you set it in the Board Manager. \n" +
                                            "         No effect to manually added Boards. \n" +
                                            "       - 'Delete used Boards from stock'-button: \n" +
                                            "         Delete the Boards from the SQL database, \n" +
                                            "         which where actually used in your last \n" +
                                            "         calculation. \n" + 
                                            "\n" +
                                            "To filter Boards type 'board', 'y' or '-'. \n" +
                                            "To filter Bars type 'bar' or 'x'";


        public static string Parts = "Same properties like Boards/Bars, except of Price.\n" + 
                                      "No unlimited amount possible.\n" + 
                                      "THICKNESS can be 0. In this case 0 means that this part \n" +
                                      "can be used with every board (thickness).";


        public static string Space = "Space needed between parts.\n" + 
                                     "Can be 0.";


        public static string Run = "Sort your Boards and Parts before running the calculation:\n" +
                                   "\n" +
                                   "        - Start with the smallest Boards and put the biggest\n" +
                                   "          to the end of the list.\n" +
                                   "\n" +
                                   "        - Start with the biggest parts and put the smallest\n" +
                                   "          to the end of the list. (Auto-Sort avaible.)\n" +
                                   "\n" +
                                   "(You don't have to do this, but it's recommended.)\n" +
                                   "\n" +
                                   "Tipp: Change options (recursion steps, shift parts, close holes\n" +
                                   "simple mode) and RUN again to find the best solution.\n" +
                                   "\n" +
                                   "Searching a solution keeps longer, when there are more (small)\n" +
                                   "parts. In this cases try simple mode first or recursion with a\n" +
                                   "low number of recursion steps.";


        public static string Options = "Max Recursion Steps:\n" +
                                       "        This value tells the program how long it should\n" +
                                       "        search for solutions.\n" +
                                       "        It's recommended to use no values over 10000.\n\n" +
                                       "Shift Parts in Recursion:\n" +
                                       "        More flexible search for part positions (2D).\n" +
                                       "Where close Holes:\n" +
                                       "        Try to fit unused in free spaces in a solution\n" +
                                       "        found with recursion (2D). Options:\n" +
                                       "        - For every solution found with recursion\n" +
                                       "        - Only for the best solution found\n" +
                                       "        - Off\n" +
                                       "\n" +
                                       "Simple Mode:\n" +
                                       "        In this mode no recursion is used (2D).\n" +
                                       "        The algorithm scrolls through all poosible\n" +
                                       "        coordinates one time and sets the biggest\n" +
                                       "        possible part, if it finds a free space.\n" +
                                       "        \n" +
                                       "        This is much faster then recursion, but in\n" +
                                       "        some cases you will not find the best solution.\n" +
                                       "        In some cases you will find a better solution\n" +
                                       "        with this method, than with recursion.\n" +
                                       "\n" +
                                       "Stock First:\n" +
                                       "        Use Boards from stock (Board Manager) or from \n" +
                                       "        Main Window List first.\n" +
                                       "\n" +
                                       "Sort Parts: \n" +
                                       "        Automaticly sort Parts from big to small.\n" +
                                       "\n" +
                                       "You have to create a folder for saving the\n" +
                                       "Options manually: c:/Cutting_Optimizer_Options\n\n";


        public static string Shortcuts = "You can access these functions via shortcuts (single Key):\n\n" +
                                         "      S - Save\n" +
                                         "      L - Load\n" +
                                         "      I - Save Image\n" +
                                         "      R - Run\n" +
                                         "      O - Open Options Window\n" +
                                         "      H - Open Help Window\n" +
                                         "      Q - Switch to 1D-Input\n" +
                                         "      W - Switch to 2D-Input\n" +
                                         "      B - Jump to Board Input\n" +
                                         "      P - Jump to Part Input\n" +
                                         "      S - Jump to Space Input\n" +
                                         "      M - Open Board Manager \n" + 
                                         "      U - Check/Uncheck 'Use Stock' Checkbox";



        public static string About = "Cutting Optimizer v1.1 \n" + 
                                     "Hendrik Jacobs \n" + 
                                     "2021";


        public static string BarFilterTT = "To filter Boards type 'board', 'y' or '-'. \n" + 
                                           "To filter Bars type 'bar' or 'x'";





        public static string CreateDBCommand = "create table stock \n" +
                                               "( \n" +
                                               "     ID integer primary key autoincrement, \n" +
                                               "     Width int not null, \n" +
                                               "     Height int not null, \n" +
                                               "     Thickness int not null, \n" +
                                               "     Price decimal(10, 2) not null, \n" +
                                               "     Amount int not null, \n" +
                                               "     Unlimited bool not null, \n" +
                                               "     Bar char(1) not null \n" +
                                               "  ); \n" +
                                               " \n" +
                                               "  create table options \n" +
                                               "  ( \n" +
                                               "     MaxStage int, \n" +
                                               "     ShowPartTable bool, \n" +
                                               "     ShowPrices bool, \n" +
                                               "     SumParts bool, \n" +
                                               "     SumBoards bool, \n" +
                                               "     SumBoardsStock bool, \n" +
                                               "     Shift bool, \n" +
                                               "     CloseHolesEnd bool, \n" +
                                               "     CloseHolesEvery bool, \n" +
                                               "     SimpleMode bool, \n" +
                                               "     StockFirst bool, \n" +
                                               "     SortParts bool, \n" +
                                               "     UseStock bool \n" +
                                               "  ); \n" +
                                               " \n" +
                                               " insert into options \n" +
                                               " values (1000, true, true, false, false, false, false, false, true, false, false, true, false); \n" +
                                               " \n" +
                                               " create table saveOrder \n" +
                                               " ( \n" +
                                               "     ID integer primary key autoincrement, \n" +
                                               "     BoardIndex int \n" +
                                               " );";
    }
}
