using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;


namespace Cutting_Optimizer
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Part> Part_List = new ObservableCollection<Part> { };
        private ObservableCollection<Board> Board_List = new ObservableCollection<Board> { };
        private Calc calc;
        private bool dataToSave = false;
        private int imgXMax = 0, imgYMax = 0;
        public ScaleTransform st = new ScaleTransform();
        private int space, spaceBar;
        private List<Board> SummedBoards;
        private List<Part> SummedParts;
        private List<int> Used = new List<int>();

        // TwoDInput says if 1D(false) or 2D(true) input in active
        private bool TwoDInput;


        private bool boardwidthUD = true;
        private bool boardHeightUD = true;
        private bool boardthickUD = true;
        private bool boardamountUD = true;
        private bool boardpriceUD = true;
        private bool boardbarUD = true;
        private bool partwidthUD = true;
        private bool partHeightUD = true;
        private bool partthickUD = true;
        private bool partamountUD = true;
        private bool partbarUD = true;



        public MainWindow()
        {
            InitializeComponent();

            // Data Binding for ListView
            part_box.ItemsSource = Part_List;
            board_box.ItemsSource = Board_List;
            delUsed_btn.Visibility = Visibility.Collapsed;

            // Load Options or set to default values
            try
            {
                if (!File.Exists("Boards.db"))
                    CreateDB();
                else
                {
                    using (SQLiteConnection con = new SQLiteConnection("data source=Boards.db"))
                    {
                        using (SQLiteCommand com = new SQLiteCommand(con))
                        {
                            con.Open();
                            com.CommandText = "SELECT * FROM options;";
                            using (SQLiteDataReader Reader = com.ExecuteReader())
                            {    
                                Reader.Read();
                                Options.MaxStage = Reader.GetInt32(0);
                                Options.ShowParttables = Reader.GetBoolean(1);
                                Options.ShowPrices = Reader.GetBoolean(2);
                                Options.SumParts = Reader.GetBoolean(3);
                                Options.SumBoards = Reader.GetBoolean(4);
                                Options.SumBoardsStock = Reader.GetBoolean(5);
                                Options.Shift = Reader.GetBoolean(6);
                                Options.CloseHolesEvery = Reader.GetBoolean(7);
                                Options.CloseHolesEnd = Reader.GetBoolean(8);
                                Options.SimpleMode = Reader.GetBoolean(9);
                                Options.StockFirst = Reader.GetBoolean(10);
                                Options.SortParts = Reader.GetBoolean(11);

                                useStockbox.IsChecked = Reader.GetBoolean(12);
                            }
                            con.Close();
                        }
                    }
                }
            }
            catch
            {
                Options.MaxStage = 1000;
                Options.ShowParttables = true;
                Options.ShowPrices = true;
                Options.SumParts = false;
                Options.SumBoards = false;
                Options.SumBoardsStock = false;
                Options.Shift = false;
                Options.CloseHolesEvery = false;
                Options.CloseHolesEnd = true;
                Options.SimpleMode = false;
                Options.StockFirst = false;
                Options.SortParts = true;
                infobox.Text = "Failed loading Options.";
            }

            // Zoom for canvas with mousewheel
            canvas.RenderTransform = st;
            canvas.MouseWheel += (sender, e) =>
            {
                if (e.Delta > 0)
                {
                    st.ScaleX *= 1.2;
                    st.ScaleY *= 1.2;
                }
                else
                {
                    if (st.ScaleX > 1)
                    {
                        st.ScaleX /= 1.2;
                        st.ScaleY /= 1.2;
                    }
                }
                UpdateCanvasSize();
            };

            TitleText.ToolTip = HelpTexts.About;
            TwoDInput = true;
            bxbox.Focus();
            SpaceText.ToolTip = "Space between Parts. \n" + "Key: D";
        }


        private void CreateDB()
        {
            SQLiteConnection.CreateFile("Boards.db");

            using (SQLiteConnection con = new SQLiteConnection("data source=Boards.db"))
            {
                using (SQLiteCommand com = new SQLiteCommand(con))
                {
                    con.Open();

                    com.CommandText = HelpTexts.CreateDBCommand;
                    Trace.WriteLine("here");
                    com.ExecuteNonQuery();
                    Trace.WriteLine("after");
                    con.Close();
                }
            }
        }



        // Event to update canvas size and redraw canvas-children when window size changes
        private void UpdateSize(object sender, RoutedEventArgs e)
        {
            UpdateCanvasSize();
            if (calc != null) ShowOnCanvas();
        }


        private void Click1D(object sender, RoutedEventArgs e)
        {
            SetTo1D();
        }

        // Change UI for 1D input
        private void SetTo1D()
        {
            if (!TwoDInput) return;

            BoardXText.Text = "Bar X:";
            PartXText.Text = "Part X:";
            BoardYText.Visibility = Visibility.Collapsed;
            bybox.Visibility = Visibility.Collapsed;
            PartYText.Visibility = Visibility.Collapsed;
            ybox.Visibility = Visibility.Collapsed;
            TwoDInput = false;
            bxbox.Focus();
        }



        private void Click2D(object sender, RoutedEventArgs e)
        {
            SetTo2D();
        }


        // Change UI for 2D input
        private void SetTo2D()
        {
            if (TwoDInput) return;

            BoardXText.Text = "Board X:";
            PartXText.Text = "Part X:";
            BoardYText.Visibility = Visibility.Visible;
            bybox.Visibility = Visibility.Visible;
            PartYText.Visibility = Visibility.Visible;
            ybox.Visibility = Visibility.Visible;
            TwoDInput = true;
            bxbox.Focus();
        }


        // Shortkeys
        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.S:
                    Save();
                    break;
                case Key.L:
                    Load();
                    break;
                case Key.I:
                    SaveImg();
                    break;
                case Key.R:
                    Run();
                    break;
                case Key.B:
                    bxbox.Focus();
                    break;
                case Key.P:
                    xbox.Focus();
                    break;
                case Key.H:
                    OpenHelpWindow();
                    break;
                case Key.O:
                    OpenOptionsWindow();
                    break;
                case Key.Q:
                    SetTo1D();
                    break;
                case Key.W:
                    SetTo2D();
                    break;
                case Key.D:
                    s1dbox.Focus();
                    break;
                case Key.M:
                    OpenBoardManager();
                    break;
                case Key.U:
                    CheckUseBox();
                    break;
            };
        }



        void CheckUseBox()
        {
            useStockbox.IsChecked = !useStockbox.IsChecked;
        }


        // Set new canvas size
        private void UpdateCanvasSize()
        {
            canvas.Width = (grid.ActualWidth - 265) * st.ScaleX;
            var y_canvas = grid.ActualHeight - 70;
            canvas.Height = (y_canvas > imgYMax) ? y_canvas : imgYMax;
            canvas.Height *= st.ScaleY;
        }



        // Select all text, when click in TextBox
        private void KeySelectedBox(object sender, RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            tb.SelectAll();
        }


        // Click Close Button
        private void ClickClose(object sender, RoutedEventArgs e)
        {
            MessageBoxReturn.SetStrings("Close?", "OK", "Cancel");
            CustomMessageBox customMessageBox = new CustomMessageBox();
            customMessageBox.ShowDialog();

            if (MessageBoxReturn.Return) this.Close();
        }


        // Open Options window
        private void ClickOptions(object sender, RoutedEventArgs e)
        {
            OpenOptionsWindow();
        }

        private void OpenOptionsWindow()
        {
            OptionsWindow optionswindow = new OptionsWindow();
            optionswindow.ShowDialog();
            if (calc != null) ShowOnCanvas();
        }

        // Open Board Manager
        private void ClickBoardManager(object sender, RoutedEventArgs e)
        {
            OpenBoardManager();
        }

        private void OpenBoardManager()
        {
            BoardManager boardManager = new BoardManager();
            boardManager.ShowDialog();
            if (MessageBoxReturn.BoardsReturn != null)
            {
                foreach (Board board in MessageBoxReturn.BoardsReturn)
                {
                    Board_List.Add(new Board(board));
                }
                MessageBoxReturn.BoardsReturn.Clear();
            }
        }



        // Open Help window
        private void ClickHelp(object sender, RoutedEventArgs e)
        {
            OpenHelpWindow();
        }


        private void OpenHelpWindow()
        {
            HelpWindow helpwindow = new HelpWindow();
            helpwindow.ShowDialog();
        }


        // Close-Event to ask if changes should be saved.
        // Save in Sql database, if 'Use Stock' box is checked or unchecked.
        private void CloseWindow(object sender, CancelEventArgs e)
        {
            using (SQLiteConnection con = new SQLiteConnection("data source=Boards.db"))
            {
                using (SQLiteCommand com = new SQLiteCommand(con))
                {
                    con.Open();

                    com.CommandText = "UPDATE options " +
                                      "SET UseStock = " + useStockbox.IsChecked.ToString() +
                                      "; ";

                    com.ExecuteNonQuery();
                    con.Close();
                }
            }

            // If there is new data to save.
            if (dataToSave)
            {
                MessageBoxReturn.SetStrings("Save before Exit?", "Yes", "No");
                CustomMessageBox customMessageBox = new CustomMessageBox();
                customMessageBox.ShowDialog();

                if (MessageBoxReturn.Return) Save();
            }
        }


        // Handle which inputs in TextBoxes.
        // Only int allowed in most TextBoxes.
        // "," or "." possible in price, 2 decimal places.
        // "*" possible in board/bar amount.
        private void PreviewText(object sender, TextCompositionEventArgs e)
        {
            TextBox srcTextbox = e.Source as TextBox;
            srcTextbox.SelectedText = "";

            if (((e.Text != "," && e.Text != "*") && !Tools.IsNumber(e.Text)) || srcTextbox.Text.Contains("*"))
            {
                e.Handled = true;
            }
            else if (e.Text == "," && srcTextbox.Name == "pbox")
            {
                if (srcTextbox.Text.IndexOf(",") > -1)
                {
                    e.Handled = true;
                }
            }
            else if (e.Text == "*" && srcTextbox.Name == "babox")
            {
                if (srcTextbox.Text.IndexOf(e.Text) > -1 || srcTextbox.Text.Length > 0)
                {
                    e.Handled = true;
                }
            }
            else if (e.Text == "," || e.Text == "*")
            {
                e.Handled = true;
            }
            else if (srcTextbox.Text.IndexOf(",") != -1 && srcTextbox.Text.IndexOf(",") < srcTextbox.Text.Count() - 2 && srcTextbox.CaretIndex > srcTextbox.Text.Count() - 2)
            {
                e.Handled = true;
            }
        }



        // Increase amount of selected element in board list
        private void ClickBoardAdd(object sender, RoutedEventArgs e)
        {
            var index = board_box.SelectedIndex;

            // Return if no element selceted.
            if (index == -1) return;

            // Increase if board has no unlimited amount
            if (!Board_List[index].Unlimited)
            {
                Board_List[index].Amount++;
                Board_List[index].Amount_string = Board_List[index].Amount.ToString();
                board_box.Items.Refresh();
            }
        }


        // Decrease amount of selected element in board list
        private void ClickBoardSub(object sender, RoutedEventArgs e)
        {
            var index = board_box.SelectedIndex;
            if (index == -1) return;

            // Decrease if board has no unlimited amount
            if (!Board_List[index].Unlimited && Board_List[index].Amount > 0)
            {
                Board_List[index].Amount--;
                Board_List[index].Amount_string = Board_List[index].Amount.ToString();
                board_box.Items.Refresh();
            }
        }


        // Move board up in list
        private void ClickBoardUp(object sender, RoutedEventArgs e)
        {
            var index = board_box.SelectedIndex;
            if (index == -1) return;
            if (index != 0) Board_List.Move(index, index - 1);
        }


        // Move baord down in list
        private void ClickBoardDown(object sender, RoutedEventArgs e)
        {
            var index = board_box.SelectedIndex;
            if (index == -1) return;
            if (index + 1 < Board_List.Count)
                Board_List.Move(index, index + 1);
        }


        // Increase amount of selected element in part list
        private void ClickPartAdd(object sender, RoutedEventArgs e)
        {
            var index = part_box.SelectedIndex;
            if (index == -1) return;

            Part_List[index].Amount++;
            part_box.Items.Refresh();
        }



        // Decrease amount of selected element in part list
        private void ClickPartSub(object sender, RoutedEventArgs e)
        {
            var index = part_box.SelectedIndex;
            if (index == -1) return;
            if (Part_List[index].Amount > 0)
            {
                Part_List[index].Amount--;
                part_box.Items.Refresh();
            }
        }


        // Move part up in list
        private void ClickPartUp(object sender, RoutedEventArgs e)
        {
            var index = part_box.SelectedIndex;
            if (index == -1) return;
            if (index != 0) Part_List.Move(index, index - 1);
        }



        // Move part down in list
        private void ClickPartDown(object sender, RoutedEventArgs e)
        {
            var index = part_box.SelectedIndex;
            if (index == -1) return;
            if (index + 1 < Part_List.Count) Part_List.Move(index, index + 1);
        }

        /**************************************************************************************************************************/


        // Add new board to list.
        private void ClickAddBoard(object sender, RoutedEventArgs e)
        {
            int a, x, y, t;
            float price;
            bool unlimited = false;

            string type = (TwoDInput) ? "Board" : "Bar";
            string bar = (TwoDInput) ? " " : "x";

            try
            {
                x = Int32.Parse(bxbox.Text);
                y = Int32.Parse(bybox.Text);
                t = Int32.Parse(btbox.Text);
            }
            catch
            {
                var text = "Error: Empty field.";
                MessageBoxReturn.SetStrings(text, "OK");
                CustomMessageBox customMessageBox = new CustomMessageBox();
                customMessageBox.Button2.Visibility = Visibility.Collapsed;
                customMessageBox.Buffer.Width = 35;
                customMessageBox.ShowDialog();

                infobox.Text = text;
                bxbox.Focus();
                return;
            }

            try
            {
                a = Int32.Parse(babox.Text);
            }
            catch
            {
                // Unlimited amount when input is "*"
                a = 1;
                unlimited = true;
            }

            try
            {
                price = float.Parse(pbox.Text);
            }
            catch
            {
                price = 0;
            }

            string price_string = String.Format("{0:0.00}", price);

            // Cancel when width, height/ length or thickness is 0
            if (((x == 0 || y == 0 || t == 0) && TwoDInput) ||
                ((x == 0 || t == 0) && !TwoDInput))
            {
                var text = "Error: Invalid " + type + "size.";
                MessageBoxReturn.SetStrings(text, "OK");
                CustomMessageBox customMessageBox = new CustomMessageBox();
                customMessageBox.Button2.Visibility = Visibility.Collapsed;
                customMessageBox.Buffer.Width = 35;
                customMessageBox.ShowDialog();

                infobox.Text = text;                
                bxbox.Focus();
                return;
            }

            bool set_new = true;

            // Sum Boards if equal
            if (Options.SumBoards)
            {
                foreach (Board board in Board_List)
                {
                    if (((board.Width == x && board.Height == y) ||
                        (board.Width == y && board.Height == x)) &&
                        board.Price == price &&
                        board.Bar == bar &&
                        board.Thickness == t)
                    {
                        if (board.Unlimited || unlimited)
                        {
                            board.Amount = 1;
                            board.Amount_string = "*";
                            board.Unlimited = true;
                        }
                        else
                        {
                            board.Amount += a;
                            board.Amount_string = board.Amount.ToString();
                        }
                        set_new = false;
                        board_box.Items.Refresh();
                        break;
                    }
                }
            }
            if (set_new)
            {
                string a_string = (unlimited) ? "*" : a.ToString();
                // Add new Board to list
                Board_List.Add(new Board
                {
                    SqlIndex = -1,
                    Width = x,
                    Height = y,
                    Amount = a,
                    Amount_string = a_string,
                    Price = price,
                    Price_string = price_string,
                    Unlimited = unlimited,
                    Bar = bar,
                    Thickness = t
                });
            }
            // Reset TextBoxes
            infobox.Text = "Part added.";
            bxbox.Text = "0";
            bybox.Text = "0";
            babox.Text = "0";
            pbox.Text = "0,00";
            btbox.Text = "0";

            bxbox.Focus();
        }




        // Check if Board list contains Boards/Bars
        private bool CheckIfBoardsInList(string bar)
        {
            bool found = false;
            foreach (Board board in Board_List)
            {
                if (board.Bar == bar)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }




        // Check if a compatible board is in stock database when adding part.
        // (If 'Use Stock')-box is checked.)
        private bool CheckStock(int width, int height, int thickness)
        {            
            using (SQLiteConnection con = new SQLiteConnection("data source=Boards.db"))
            {
                using (SQLiteCommand com = new SQLiteCommand(con))
                {
                    con.Open();
                    com.CommandText = "SELECT * FROM stock;";
                    using (SQLiteDataReader Reader = com.ExecuteReader())
                    {
                        while (Reader.Read())
                        {
                            if ((thickness == Reader.GetInt32(3) || thickness == 0) &&
                                ((height == 0 && Reader.GetInt32(2) == 0) ||
                                  (height != 0 && Reader.GetInt32(2) != 0)) &&
                                  (width <= Reader.GetInt32(1) && height <= Reader.GetInt32(2)) ||
                                  (width <= Reader.GetInt32(2) && height <= Reader.GetInt32(1)))
                            {
                                con.Close();
                                return true;
                            }
                        }
                    }
                    con.Close();
                }
            }
            return false;
        }


        // Add new Part to list.
        private void ClickAddPart(object sender, RoutedEventArgs e)
        {
            string type = (TwoDInput) ? "Board" : "Bar";
            string type2 = (TwoDInput) ? "Bar": "Board";
            string bar = (TwoDInput) ? " " : "x";
            int x, y, a, t;

            

            try
            {
                x = Int32.Parse(xbox.Text);
                y = Int32.Parse(ybox.Text);
                a = Int32.Parse(abox.Text);
                t = Int32.Parse(tbox.Text);
            }
            catch 
            {
                var text = "Error: Empty field.";
                MessageBoxReturn.SetStrings(text, "OK");
                CustomMessageBox customMessageBox = new CustomMessageBox();
                customMessageBox.Button2.Visibility = Visibility.Collapsed;
                customMessageBox.Buffer.Width = 35;
                customMessageBox.ShowDialog();

                infobox.Text = text;
                xbox.Focus();
                return;
            }

            if (!TwoDInput) y = 0;

           


            if ((x > 0 && y > 0 && a > 0 && TwoDInput) || (x > 0 && a > 0 && !TwoDInput))
            {
                if (useStockbox.IsChecked == false || !CheckStock(x, y, t))
                {
                    if (Board_List.Count == 0)
                    {
                        var text = "Error: Add " + type + " first.";
                        MessageBoxReturn.SetStrings(text, "OK");
                        CustomMessageBox customMessageBox = new CustomMessageBox();
                        customMessageBox.Button2.Visibility = Visibility.Collapsed;
                        customMessageBox.Buffer.Width = 35;
                        customMessageBox.ShowDialog();

                        infobox.Text = text;
                        return;
                    }

                    bool found = CheckIfBoardsInList(bar);
                    if (!found)
                    {
                        var text = "Error: Add " + type + " first. \n" + "Only " + type2 + " in List.";
                        MessageBoxReturn.SetStrings(text, "OK");
                        CustomMessageBox customMessageBox = new CustomMessageBox();
                        customMessageBox.Button2.Visibility = Visibility.Collapsed;
                        customMessageBox.Buffer.Width = 35;
                        customMessageBox.ShowDialog();

                        infobox.Text = text;
                        xbox.Focus();
                        return;
                    }

                    bool run = false;
                    foreach (Board board in Board_List)
                    {
                        if ((board.Width >= x || board.Height >= y) &&
                            (board.Width >= y || board.Height >= x) &&
                            TwoDInput && board.Bar == " " &&
                            (t == 0 || t == board.Thickness))
                        {
                            run = true;
                            break;
                        }
                        else if (board.Width > x &&
                                 (board.Thickness == t || t == 0) &&
                                 !TwoDInput && board.Bar == "x")
                        {
                            run = true;
                            break;
                        }
                    }

                    if (!run)
                    {
                        infobox.Text = "Part to big. \n" + "Add bigger " + type + ".";
                        return;
                    }
                }
                bool set_new = true;
                if (Options.SumParts)
                {
                    foreach (Part part in Part_List)
                    {
                        if (Tools.CheckIfPartsEqual(part.Width, part.Height, part.Thickness, x, y, t) && part.Bar == bar)
                        {
                            part.Amount += a;
                            set_new = false;
                            part_box.Items.Refresh();
                            break;
                        }
                    }
                }

                if (set_new)
                {
                    Part_List.Add(new Part { Width = x, Height = y, Amount = a, Bar = bar, Thickness = t });
                }
                infobox.Text = "Part added.";
                xbox.Text = "0";
                ybox.Text = "0";
                abox.Text = "0";
                tbox.Text = "0";
                    
                    
                xbox.Focus();
            }
            else
            {
                var text = "Error: Invalid Part Size.";
                MessageBoxReturn.SetStrings(text, "OK");
                CustomMessageBox customMessageBox = new CustomMessageBox();
                customMessageBox.Button2.Visibility = Visibility.Collapsed;
                customMessageBox.Buffer.Width = 35;
                customMessageBox.ShowDialog();

                infobox.Text = text;
                xbox.Focus();
            }
        }


        // Delete single part from list
        private void ClickDelPart(object sender, RoutedEventArgs e)
        {
            try
            {
                int pos = part_box.SelectedIndex;
                Part_List.RemoveAt(pos);
                infobox.Text = "Part Removed.";
            }
            catch
            {
                var text = "Error: No Part selected.";
                MessageBoxReturn.SetStrings(text, "OK");
                CustomMessageBox customMessageBox = new CustomMessageBox();
                customMessageBox.Button2.Visibility = Visibility.Collapsed;
                customMessageBox.Buffer.Width = 35;
                customMessageBox.ShowDialog();

                infobox.Text = text;
            }
        }



        // Delete all Parts from list.
        // Open OK-Cancel Messagebox before.
        private void ClickDelAllParts(object sender, RoutedEventArgs e)
        {
            // Return when ne elements in list.
            if (Part_List.Count == 0) return;

            MessageBoxReturn.SetStrings("Remove all parts?", "OK", "Cancel");
            CustomMessageBox customMessageBox = new CustomMessageBox();
            customMessageBox.ShowDialog();

            if (MessageBoxReturn.Return) Part_List.Clear();
        }


        // Delete single Board.
        private void ClickDelBoard(object sender, RoutedEventArgs e)
        {
            try
            {
                int pos = board_box.SelectedIndex;

                if (Board_List[pos].SqlIndex != -1)
                    Used.RemoveAt(Used.IndexOf(Board_List[pos].SqlIndex));

                Board_List.RemoveAt(pos);
                infobox.Text = "Board Removed.";
            }
            catch
            {
                var text = "Error: No Boards selected.";
                MessageBoxReturn.SetStrings(text, "OK");
                CustomMessageBox customMessageBox = new CustomMessageBox();
                customMessageBox.Button2.Visibility = Visibility.Collapsed;
                customMessageBox.Buffer.Width = 35;
                customMessageBox.ShowDialog();

                infobox.Text = text;
            }
        }



        // Delete all Boards from list.
        // Open OK-Cancel Messagebox before.
        private void ClickDelAllBoards(object sender, RoutedEventArgs e)
        {
            // Return when ne elements in list.
            if (Board_List.Count == 0) return;

            MessageBoxReturn.SetStrings("Remove all boards?", "OK", "Cancel");
            CustomMessageBox customMessageBox = new CustomMessageBox();
            customMessageBox.ShowDialog();

            if (MessageBoxReturn.Return)
            {
                Board_List.Clear();
                Used.Clear();
            }
        }





        /**************************************************************************************************************************/




        private void ClickSaveButton(object sender, RoutedEventArgs e)
        {
            Save();
        }


        // Save class object calc to file (*.cuo).
        private void Save()
        {
            if (calc == null)
                infobox.Text = "Nothing to save. \n" + "Click Run-Button first.";
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Cutting Optimizer File (*.cuo)|*.cuo";
                if (saveFileDialog.ShowDialog() == true)
                {
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new FileStream(saveFileDialog.FileName,
                                                   FileMode.Create,
                                                   FileAccess.Write);
                    formatter.Serialize(stream, calc);
                    stream.Close();
                    infobox.Text = "Saved.";
                    dataToSave = false;
                }
            }
        }

        private void ClickSaveImageButton(object sender, RoutedEventArgs e)
        {
            SaveImg();
        }



        // Save canvas as PNG-image.
        private void SaveImg()
        {
            // Return if canvas is empty.
            if (canvas.Children.Count == 0)
            {
                infobox.Text = "Nothing to save. \n" + "Click Run-Button first.";
                return;
            }

            // Save actual scale and scroll values.
            var scaleX = st.ScaleX;
            var scaley = st.ScaleY;
            var scrollOffset = scroll.VerticalOffset;

            // Set scale an scroll values for saving image.
            scroll.ScrollToVerticalOffset(0);
            st.ScaleX = 1;
            st.ScaleY = 1;

            // Save to file using FileDialog.
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG (*.png)|*.png";
            if (saveFileDialog.ShowDialog() == true)
            {
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width,
                                                                (int)canvas.RenderSize.Height,
                                                                96d, 96d,
                                                                PixelFormats.Default);
                rtb.Render(canvas);

                var crop = new CroppedBitmap(rtb, new Int32Rect(0, 0, imgXMax, imgYMax));
                BitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(crop));

                using (var path = File.OpenWrite(saveFileDialog.FileName))
                {
                    pngEncoder.Save(path);
                }

                infobox.Text = "Image saved.";
            }
            
            // Restore scale an scroll values
            st.ScaleX = scaleX;
            st.ScaleY = scaley;
            scroll.ScrollToVerticalOffset(scrollOffset);
        }




        private void ClickLoadButton(object sender, RoutedEventArgs e)
        {
            Load(); 
        }


        // Load from *.cuo file (in calc)
        private void Load()
        {
            if (dataToSave)
            {
                string text = "Load file? \n" + "Any unsaved changes will be lost.";
                MessageBoxReturn.SetStrings(text, "OK", "Cancel");
                CustomMessageBox customMessageBox = new CustomMessageBox();
                customMessageBox.ShowDialog();

                if (!MessageBoxReturn.Return) return;
            }


            // Load file using FileDialog.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Cutting Optimizer File (*.cuo)|*.cuo";
            if (openFileDialog.ShowDialog() == true)
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                calc = (Calc)formatter.Deserialize(stream);
                ShowOnCanvas();
                UpdateTextboxes();
                infobox.Text = "Loaded.";
            }            
        }




        // Update Lists for Listviews after loading from file.
        private void UpdateTextboxes()
        {
            sbox.Text = calc.Space.ToString();
            s1dbox.Text = calc.SpaceBar.ToString();

            Board_List.Clear();
            foreach (Board board in calc.BoardsSave)
                Board_List.Add(new Board(board));

            Part_List.Clear();
            foreach (Part part in calc.PartsSave)
                Part_List.Add(new Part (part));
        }
        



        /**************************************************************************************************************************/


        // Click RUN-button -> start calculation.
        private void ClickRun(object sender, RoutedEventArgs e)
        {
            Run();
        }

        private void Run()
        {
            // Open InProgrss-window in the thread.
            Thread viewerThread = new Thread(delegate ()
            {
                InProgress inProgress = new InProgress();
                inProgress.Show();
            });
            viewerThread.SetApartmentState(ApartmentState.STA);
            viewerThread.Start();

            // Add Boards from Stock database.
            if (useStockbox.IsChecked == true) AddFromStock();

            // Check if there is a compatible board for ever part.
            if (!RunPossible()) return;


            ReadSpaceInput();               

            // Create the new Calc object.
            calc = new Calc
            {
                Parts = MultParts(" "),
                PartsBar = MultParts("x"),
                PartsSave = Part_List,
                BoardList = MultBoards(" "),
                BarList = MultBoards("x"),
                BoardsSave = Board_List,
                Space = space,
                SpaceBar = spaceBar
            };


            // Run calculation.
            if (calc.Make())
            {
                // If no solution found (2D).
                infobox.Text = "Fail: Increase max. Recursion Steps.";
            }
            else
            {
                // Show graphics.
                ShowOnCanvas();
                infobox.Text = "Finished.";
                dataToSave = true;
            }

            // Show 'Delete Used Boards from stock'-button, if boards from stock were used.
            if (calc.Used.Count > 0)
                delUsed_btn.Visibility = Visibility.Visible;
        }



        // Check if Boards/Bars avaible.
        // Check if Parts avaible.
        // Check if there is a compatible board/bar for every part.
        private bool RunPossible()
        {
            if (Board_List.Count == 0)
            {
                var text = "Error: No Boards.";
                MessageBoxReturn.SetStrings(text, "OK");
                CustomMessageBox customMessageBox = new CustomMessageBox();
                customMessageBox.Button2.Visibility = Visibility.Collapsed;
                customMessageBox.Buffer.Width = 35;
                customMessageBox.ShowDialog();

                infobox.Text = text;
                bxbox.Focus();
                return false;
            }

            if (Part_List.Count > 0)
            {
                foreach (Part part in Part_List)
                {
                    bool run = false;
                    foreach (Board board in Board_List)
                    {
                        if ((board.Width >= part.Width || board.Height >= part.Height) &&
                            (board.Width >= part.Height || board.Height >= part.Width) &&
                            (board.Thickness == part.Thickness || part.Thickness == 0))
                        {
                            run = true;
                            break;
                        }
                    }
                    if (!run)
                    {
                        var text = "Error: Part too big. \n" + "Board too small.";
                        MessageBoxReturn.SetStrings(text, "OK");
                        CustomMessageBox customMessageBox = new CustomMessageBox();
                        customMessageBox.Button2.Visibility = Visibility.Collapsed;
                        customMessageBox.Buffer.Width = 35;
                        customMessageBox.ShowDialog();

                        infobox.Text = text;
                        bxbox.Focus();
                        return false;
                    }
                }

            }
            else
            {
                var text = "Error: No Parts.";
                MessageBoxReturn.SetStrings(text, "OK");
                CustomMessageBox customMessageBox = new CustomMessageBox();
                customMessageBox.Button2.Visibility = Visibility.Collapsed;
                customMessageBox.Buffer.Width = 35;
                customMessageBox.ShowDialog();

                infobox.Text = text;
                xbox.Focus();
                return false;
            }
            return true;
        }


        // Read input from TextBoxes for 1D and 2D Space.
        private void ReadSpaceInput()
        { 
                try
                {
                    space = Int32.Parse(sbox.Text);
                }
                catch
                {
                    space = 0;
                }

                try
                {
                    spaceBar = Int32.Parse(s1dbox.Text);
                }
                catch
                {
                    spaceBar = 0;
                }
        }





        // "Splitt" Boards, so that the amount of ever position in list is 1.
        // Needed for simple deleting in Calc.Recursion().
        private List<Board> MultBoards(string bar)
        {
            List<Board> Boards = new List<Board>();
            foreach (Board board in Board_List)
            {
                for (int i = 0; i < board.Amount; i++)
                {
                    if (board.Bar == bar)
                    {
                        Boards.Add(new Board(board));
                        Boards[Boards.Count - 1].Amount = 1;
                        Boards[Boards.Count - 1].Amount_string = "1";
                    }
                }
            }
            return Boards;
        }



        // Add Boards from Stock database to Main Window Board Box,
        // when clicking 'Run'.
        private void AddFromStock()
        {
            var countBefore = Board_List.Count;
            using (SQLiteConnection con = new SQLiteConnection("data source=Boards.db"))
            {
                using (SQLiteCommand com = new SQLiteCommand(con))
                {
                    con.Open();
                    com.CommandText = "SELECT s.* FROM stock AS s " +
                                      "JOIN saveorder AS o " +
                                      "ON s.ID = o.BoardIndex " +
                                      "ORDER BY o.ID;"; ;
                    using (SQLiteDataReader Reader = com.ExecuteReader())
                    {
                        while (Reader.Read())
                        {
                            foreach (Part part in Part_List)
                            {
                                if (part.Thickness == Reader.GetInt32(3) &&
                                    ((part.Height == 0 && Reader.GetInt32(2) == 0) ||
                                      part.Height != 0 && Reader.GetInt32(2) != 0))
                                {
                                    if (!Used.Contains(Reader.GetInt32(0)))
                                    {
                                        string c = (Reader.GetString(7) == "x") ? "x" : " ";
                                        Board_List.Add(new Board
                                        {
                                            SqlIndex = Reader.GetInt32(0),
                                            Width = Reader.GetInt32(1),
                                            Height = Reader.GetInt32(2),
                                            Amount = Reader.GetInt32(5),
                                            Amount_string = (Reader.GetBoolean(6)) ? "*" : Reader.GetInt32(5).ToString(),
                                            Price = Reader.GetFloat(4),
                                            Price_string = Reader.GetFloat(4).ToString(),
                                            Unlimited = Reader.GetBoolean(6),
                                            Bar = c,
                                            Thickness = Reader.GetInt32(3)
                                        });
                                        Used.Add(Reader.GetInt32(0));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    con.Close();
                }
            }

            if (Options.StockFirst)
            {
                var countAfter = Board_List.Count - 1;
                for (var i = 0; i < countBefore; i++)
                    Board_List.Move(0, countAfter);
            }

        }


        // Delete the boards which were actually used in the last calculation.
        // The boards are also getting deleted from Main Window Board Box.
        private void ClickDelUsed(object sender, RoutedEventArgs e)
        {
            MessageBoxReturn.SetStrings("Remove Boards?", "OK", "Cancel");
            CustomMessageBox customMessageBox = new CustomMessageBox();
            customMessageBox.ShowDialog();
            if (!MessageBoxReturn.Return) return;
                        
            List<Board> Boards = new List<Board>();
            using (SQLiteConnection con = new SQLiteConnection("data source=Boards.db"))
            {
                using (SQLiteCommand com = new SQLiteCommand(con))
                {
                    con.Open();
                    com.CommandText = "SELECT * from stock;"; ;
                    using (SQLiteDataReader Reader = com.ExecuteReader())
                    {
                        // Create a List with Sql-indexes and amount values of all baords in stock.
                        // This List will be used to check, if a board gets deleted from stock database 
                        // or if the amount value gets updated.
                        while (Reader.Read())
                        {
                            Boards.Add(new Board
                            {
                                SqlIndex = Reader.GetInt32(0),
                                Amount = Reader.GetInt32(5),
                            });
                        }
                    }
                    con.Close();

                    con.Open();
                    foreach (int i in calc.Used)
                    {
                        //var index = Boards.Where(board => board.SqlIndex == i);
                        int index = 0;
                        int index2 = 0;
                        int j = 0;
                        foreach (Board board in Boards)
                        {
                            if (board.SqlIndex == i)
                            {
                                index = j;
                                break;
                            }
                            j++;
                        }

                        j = 0;
                        foreach (Board board in Board_List)
                        {
                            if (board.SqlIndex == i)
                            {
                                index2 = j;
                                break;
                            }
                            j++;
                        }

                        if (Boards[index].Amount == 1 && !Boards[index].Unlimited)
                        {
                            com.CommandText = "DELETE FROM stock " +
                                                  "WHERE ID = " + i.ToString() + ";";
                            com.ExecuteNonQuery();
                            com.CommandText = "DELETE FROM saveorder " +
                                                  "WHERE BoardIndex = " + i.ToString() + ";";
                            Board_List.RemoveAt(index2);
                        }
                        else
                        {
                            Boards[index].Amount--;
                            Board_List[index2].Amount--;
                            Board_List[index2].Amount_string = Board_List[index2].Amount.ToString();
                            com.CommandText = "UPDATE stock " +
                                                  "SET Amount = " + Boards[index].Amount.ToString() +
                                                  " WHERE ID = " + i.ToString() + ";";
                        }
                        com.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }
            delUsed_btn.Visibility = Visibility.Collapsed;

            board_box.Items.Refresh();
        }



        // Like MultBoards
        private List<Part> MultParts(string bar)
        {
            List<Part> Parts = new List<Part>();
            foreach (Part part in Part_List)
            {
                for (int i = 0; i < part.Amount; i++)
                {
                    if (part.Bar == bar)
                    {
                        Parts.Add(new Part(part));
                        Parts[Parts.Count - 1].Amount = 1;
                    }
                }
            }
            if (Options.SortParts)
            {
                Parts = SortParts(Parts);
            }
            return Parts;
        }


        // Sort Board- and Part-Box by column.
        private void SetOrder(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Board> Boards = new ObservableCollection<Board>();
            ObservableCollection<Part> Parts = new ObservableCollection<Part>();
            bool part = false;
            if (sender.Equals(BoardWidth) && boardwidthUD)
            {
                Boards = new ObservableCollection<Board>(Board_List.OrderBy(b => b.Width));
                boardwidthUD = false;
                BoardWidth.Content = " x  \x25B2";
            }
            else if (sender.Equals(BoardWidth) && !boardwidthUD)
            {
                Boards = new ObservableCollection<Board>(Board_List.OrderBy(b => b.Width).Reverse());
                boardwidthUD = true;
                BoardWidth.Content = " x  \x25BC";
            }
            else if (sender.Equals(BoardHeight) && boardHeightUD)
            {
                Boards = new ObservableCollection<Board>(Board_List.OrderBy(b => b.Height));
                boardHeightUD = false;
                BoardHeight.Content = "  y  \x25B2";
            }
            else if (sender.Equals(BoardHeight) && !boardHeightUD)
            {
                Boards = new ObservableCollection<Board>(Board_List.OrderBy(b => b.Height).Reverse());
                boardHeightUD = true;
                BoardHeight.Content = "  y  \x25BC";
            }
            else if (sender.Equals(BoardThickness) && boardthickUD)
            {
                Boards = new ObservableCollection<Board>(Board_List.OrderBy(b => b.Thickness));
                boardthickUD = false;
                BoardThickness.Content = "  T  \x25B2";
            }
            else if (sender.Equals(BoardThickness) && !boardthickUD)
            {
                Boards = new ObservableCollection<Board>(Board_List.OrderBy(b => b.Thickness).Reverse());
                boardthickUD = true;
                BoardThickness.Content = "  T  \x25BC";
            }
            else if (sender.Equals(BoardAmount) && boardamountUD)
            {
                Boards = new ObservableCollection<Board>(Board_List.OrderBy(b => b.Amount_string));
                boardamountUD = false;
                BoardAmount.Content = " A  \x25B2";
            }
            else if (sender.Equals(BoardAmount) && !boardamountUD)
            {
                Boards = new ObservableCollection<Board>(Board_List.OrderBy(b => b.Amount_string).Reverse());
                boardamountUD = true;
                BoardAmount.Content = " A  \x25BC";
            }
            else if (sender.Equals(BoardPrice) && boardpriceUD)
            {
                Boards = new ObservableCollection<Board>(Board_List.OrderBy(b => b.Price_string));
                boardpriceUD = false;
                BoardPrice.Content = "Price\x25B2";
            }
            else if (sender.Equals(BoardPrice) && !boardpriceUD)
            {
                Boards = new ObservableCollection<Board>(Board_List.OrderBy(b => b.Price_string).Reverse());
                boardpriceUD = true;
                BoardPrice.Content = "Price\x25BC";
            }
            else if (sender.Equals(BoardBar) && boardbarUD)
            {
                Boards = new ObservableCollection<Board>(Board_List.OrderBy(b => b.Bar));
                boardbarUD = false;
                BoardBar.Content = "Bar \x25B2";
            }
            else if (sender.Equals(BoardBar) && !boardbarUD)
            {
                Boards = new ObservableCollection<Board>(Board_List.OrderBy(b => b.Bar).Reverse());
                boardbarUD = true;
                BoardBar.Content = "Bar \x25BC";
            }            
            else if (sender.Equals(PartWidth) && partwidthUD)
            {
                Parts = new ObservableCollection<Part>(Part_List.OrderBy(p => p.Width));
                partwidthUD = false;
                PartWidth.Content = " x  \x25B2";
                part = true;
            }
            else if (sender.Equals(PartWidth) && !partwidthUD)
            {
                Parts = new ObservableCollection<Part>(Part_List.OrderBy(p => p.Width).Reverse());
                partwidthUD = true;
                PartWidth.Content = " x  \x25BC"; 
                part = true;
            }
            else if (sender.Equals(PartHeight) && partHeightUD)
            {
                Parts = new ObservableCollection<Part>(Part_List.OrderBy(p => p.Height));
                partHeightUD = false;
                PartHeight.Content = "  y  \x25B2";
                part = true;
            }
            else if (sender.Equals(PartHeight) && !partHeightUD)
            {
                Parts = new ObservableCollection<Part>(Part_List.OrderBy(p => p.Height).Reverse());
                partHeightUD = true;
                PartHeight.Content = "  y  \x25BC";
                part = true;
            }
            else if (sender.Equals(PartThickness) && partthickUD)
            {
                Parts = new ObservableCollection<Part>(Part_List.OrderBy(p => p.Thickness));
                partthickUD = false;
                PartThickness.Content = "  T  \x25B2";
                part = true;
            }
            else if (sender.Equals(PartThickness) && !partthickUD)
            {
                Parts = new ObservableCollection<Part>(Part_List.OrderBy(p => p.Thickness).Reverse());
                partthickUD = true;
                PartThickness.Content = "  T  \x25BC";
                part = true;
            }
            else if (sender.Equals(PartAmount) && partamountUD)
            {
                Parts = new ObservableCollection<Part>(Part_List.OrderBy(p => p.Amount));
                partamountUD = false;
                PartAmount.Content = " A  \x25B2";
                part = true;
            }
            else if (sender.Equals(PartAmount) && !partamountUD)
            {
                Parts = new ObservableCollection<Part>(Part_List.OrderBy(p => p.Amount).Reverse());
                partamountUD = true;
                PartAmount.Content = " A  \x25BC";
                part = true;
            }
            else if (sender.Equals(PartBar) && partbarUD)
            {
                Parts = new ObservableCollection<Part>(Part_List.OrderBy(p => p.Bar));
                partbarUD = false;
                PartBar.Content = "Bar \x25B2";
                part = true;
            }
            else if (sender.Equals(PartBar) && !partbarUD)
            {
                Parts = new ObservableCollection<Part>(Part_List.OrderBy(p => p.Bar).Reverse());
                partbarUD = true;
                PartBar.Content = "Bar \x25BC";
                part = true;
            }


            if (part) 
            {
                Part_List.Clear();
                foreach (Part p in Parts)
                    Part_List.Add(p);
                part_box.Items.Refresh();
            }
            else
            {
                Board_List.Clear();
                foreach (Board board in Boards)
                    Board_List.Add(board);
                board_box.Items.Refresh();
            }
            
        }



        // Sort part from big to small
        private List<Part> SortParts(List<Part> Parts)
        {
            if (Parts.Count <= 1) return Parts;
            
            if (Parts[0].Bar == "x")
                return Parts.OrderBy(r => r.Width).ToList();
            else
            {
                var index = 0;
                List<int[]> order = new List<int[]>();
                foreach (Part part in Parts)
                {
                    int[] temp = new int[2];
                    temp[0] = (part.Width > part.Height) ? part.Width : part.Height;
                    temp[1] = index;
                    order.Add(temp);
                    index++;
                }
                order = order.OrderBy(r => r[0]).Reverse().ToList();

                List<Part> newParts = new List<Part>();

                foreach (int[] part in order)
                    newParts.Add(Parts[part[1]]);

                return newParts;
            }
        }


        // Set zoom/scale value if a board is to big.
        private int CalcZoom(int x, int y)
        {
            int zoom = (x > y) ? x: y;
            zoom /= 500;
            zoom = (zoom == 0) ? zoom + 1 : zoom;
            return zoom;
        }



        // Draw all avaible elements to canvas.
        private void ShowOnCanvas()
        {
            canvas.Children.Clear();

            imgYMax = 10;

            if (calc.Boards.Count > 0) ShowBoards();

            if (calc.Bars.Count > 0) ShowBars();

            if (calc.Parts.Count > 0 || calc.PartsBar.Count > 0) ShowRestPartTable();

            if (Options.ShowPrices) ShowPrices();

            if (canvas.Height < imgYMax) canvas.Height = imgYMax;
            UpdateCanvasSize();
        }




        // Draw Bars incl. part (and PartTable)
        private void ShowBars()
        {
            foreach (Board bar in calc.Bars)
            {
                var zoomOut = CalcZoom(bar.Width, 0);
                var zoomIn = (bar.Width > 250) ? 1 : 2;
                var height = (bar.Thickness > 20) ? bar.Thickness : bar.Thickness + 20;

                MakeRectangle(10,
                              imgYMax,
                              (bar.Width / zoomOut) * zoomIn,
                              height,
                              "length: " + bar.Width.ToString() + " thickness: " + bar.Thickness.ToString(),
                              Colors.White);

                var shift = 10;
                foreach (Part part in bar.Parts)
                {
                    MakeRectangle(shift,
                                  imgYMax,
                                  (part.Width / zoomOut) * zoomIn,
                                  height,
                                  "length: " + part.Width.ToString() + " thickness: " + part.Thickness.ToString(),
                                  Colors.LightBlue);

                    shift += ((part.Width + calc.SpaceBar) / zoomOut) * zoomIn;
                }

                imgYMax += height + 20;
                if (imgXMax < (bar.Width / zoomOut) * zoomIn + 20)
                    imgXMax = (bar.Width / zoomOut) * zoomIn + 20;

                if (Options.ShowParttables)
                    imgYMax = ShowPartTable(bar, 10, imgYMax);
            }
        }



        // Draw boards (and PartTables).
        private void ShowBoards()
        {
            int shift_x = 10, shift_y = 10;
            int end = calc.Boards.Count;
            int count = 0;
            int new_shift_y = 0;

            var zoom = CalcZoom(calc.Boards[0].Width, calc.Boards[0].Height);
            imgXMax = shift_x * 2 + calc.Boards[0].Width / zoom;
            imgYMax = shift_y * 2 + calc.Boards[0].Height / zoom;

            foreach (Board board in calc.Boards)
            {
                if (board.Parts.Count == 0) continue;
                zoom = CalcZoom(board.Width, board.Height);

                MakeRectangle(shift_x,
                              shift_y,
                              board.Width / zoom,
                              board.Height / zoom,
                              "x: " + board.Width.ToString() + " y: " + board.Height.ToString(),
                              Colors.White);

                foreach (Part part in board.Parts)
                {
                    int xZoom = 0, yZoom = 0;
                    if (zoom > 1 && part.XPosition % 2 != 0) xZoom++;
                    if (zoom > 1 && part.YPosition % 2 != 0) yZoom++;

                    if (!part.Rotate)
                        MakeRectangle(shift_x + part.XPosition / zoom + xZoom,
                                      shift_y + part.YPosition / zoom + yZoom,
                                      part.Width / zoom,
                                      part.Height / zoom,
                                      "x: " + part.Width.ToString() + " y: " + part.Height.ToString(),
                                      Colors.LightBlue);
                    else
                        MakeRectangle(shift_x + part.XPosition / zoom + xZoom,
                                      shift_y + part.YPosition / zoom + yZoom,
                                      part.Height / zoom,
                                      part.Width / zoom,
                                      "x: " + part.Width.ToString() + " y: " + part.Height.ToString(),
                                      Colors.LightBlue);
                }


                // Find highest y (height) value.
                int high_y;
                if (Options.ShowParttables)
                    high_y = ShowPartTable(board, shift_x, 
                                           shift_y + board.Height / zoom + 10);
                else
                    high_y = shift_y + board.Height / zoom + 20;
                if (high_y > new_shift_y) new_shift_y = high_y;

                count++;
                // If more boards to plot.
                if (count < end)
                {
                    // Calculate new Width including next board.
                    int new_width = shift_x + 20 + calc.Boards[count].Width;
                    new_width += (board.Width / zoom > 193) ? board.Width / zoom : 193;

                    // Set new shift_x value.
                    // Or Jump to next row.
                    if (new_width < canvas.Width)
                    {
                        imgXMax = (new_width + 10 > imgXMax) ? new_width + 10 : imgXMax;
                        var temp = board.Width / zoom + 20;
                        shift_x += (board.Width / zoom > 193) ? temp : 193;
                    }
                    else
                    {
                        shift_x = 10;
                        shift_y = new_shift_y;
                        imgYMax = shift_y + board.Height / zoom + 10;
                    }
                }

            }
            imgYMax = new_shift_y + 10;
        }



        // Combine calc.Bars and calc.Boards to one List.
        private List<Board> SumBoards()
        {
            List<Board> Combo = new List<Board>(calc.Boards);
            Combo.AddRange(calc.Bars);

            SummedBoards = new List<Board> { };

            foreach (Board board in Combo)
                if (!SumBoardsIteration(board)) 
                    SummedBoards.Add(new Board (board));

            return SummedBoards;
        }



        // Increase Amount if a identical board is already in list.
        private bool SumBoardsIteration(Board board)
        {
            foreach (Board b in SummedBoards)
            {
                if (Tools.CheckIfBoardsEqual(board, b))
                {
                    b.Amount++;
                    b.Amount_string = b.Amount.ToString();
                    return true;
                }
            }
            return false;
        }





        // Sum identical, used parts to show in one line in tables.
        private List<Part> SumParts(List<Part> Parts)
        {
            SummedParts = new List<Part> { };
            foreach (Part part in Parts)
            {
                if (!SumPartsIteration(part))
                {
                    part.Amount = 1;
                    SummedParts.Add(new Part(part));
                }
            }
            return SummedParts;
        }





        // Increase Amount if a identical part is already in list.
        private bool SumPartsIteration(Part part)
        {
            foreach (Part p in SummedParts)
            {
                if (Tools.CheckIfPartsEqual(part, p))
                {
                    p.Amount++;
                    return true;
                }
            }
            return false;
        }




        // Draw PriceTable on canvas.
        private void ShowPrices()
        {
            List<Board> SummedBoards = SumBoards();
            float price = 0;

            DrawTextBlock("Board", 12, imgYMax, 79, HorizontalAlignment.Center);
            DrawTextBlock("Amount", 91, imgYMax, 79, HorizontalAlignment.Center);
            DrawTextBlock("Price", 170, imgYMax, 79, HorizontalAlignment.Center);
            imgYMax++;
            foreach (Board board in SummedBoards)
            {
                price += board.Price * board.Amount;
                imgYMax += 18;
                if (board.Bar == " ")
                    DrawTextBlock(board.Width.ToString() + " x " + board.Height.ToString(), 12, imgYMax, 79, HorizontalAlignment.Center);
                else
                    DrawTextBlock(board.Width.ToString(), 12, imgYMax, 79, HorizontalAlignment.Center);

                DrawTextBlock(board.Amount.ToString() + " ", 91, imgYMax, 79, HorizontalAlignment.Right);
                DrawTextBlock(board.Price_string + " ", 170, imgYMax, 79, HorizontalAlignment.Right);
            }
            imgYMax += 18;

            DrawTextBlock("Total:", 91, imgYMax, 79, HorizontalAlignment.Center);
            DrawTextBlock(String.Format("{0:0.00}", price) + " ", 170, imgYMax, 79, HorizontalAlignment.Right);
            imgYMax += 58;

            if (imgXMax < 260) imgXMax = 260;
        }



        // Draw PartTable on canvas.
        private int ShowPartTable(Board board, int shift_x, int shift_y)
        {
            List<Part> SummedParts = SumParts(board.Parts);
            shift_x += 2;

            string text, text2;
            if (board.Bar == "x")
            {
                text = " Bar:";
                text2 = " Length: " + board.Width.ToString();
            }
            else
            {
                text = " Board:";
                text2 = " Width: " + board.Width.ToString() + " Height: " + board.Height.ToString();
            }
            DrawTextBlock(text, shift_x, shift_y, 177, HorizontalAlignment.Left);
            shift_y += 18;
            DrawTextBlock(text2, shift_x, shift_y, 177, HorizontalAlignment.Left);
            shift_y += 18;
            DrawTextBlock(" Thickness: ", shift_x, shift_y, 60, HorizontalAlignment.Left);
            DrawTextBlock(board.Thickness.ToString(), shift_x + 59, shift_y, 118, HorizontalAlignment.Center);


            shift_y += 18;
            var usedShift = shift_y;
            shift_y += 19;
            DrawTextBlock("X", shift_x, shift_y, 59, HorizontalAlignment.Center);
            DrawTextBlock("Y", shift_x+59, shift_y, 59, HorizontalAlignment.Center);
            DrawTextBlock("Amount", shift_x+118, shift_y, 59, HorizontalAlignment.Center);

            float percent, percentUsed;
            if (board.Bar == " ")
            {
                percent = (board.Width * board.Height) / 100;
                percentUsed = board.Width * board.Height;
            }
            else
            {
                percent = board.Width / 100;
                percentUsed = board.Width;
            }

            shift_y++;
            foreach (Part part in SummedParts)
            {
                shift_y += 18;
                DrawPartLine(part, shift_x, shift_y, false);
                if (board.Bar == " ")
                    percentUsed -= (part.Width * part.Height) * part.Amount;
                else
                    percentUsed -= (part.Width * part.Amount);
            }
            
            percentUsed = (percentUsed / percent - 100) * (-1);
            
            DrawTextBlock(" Used:", shift_x, usedShift, 60, HorizontalAlignment.Left);
            DrawTextBlock(percentUsed.ToString() + " % ", shift_x + 59, usedShift, 118, HorizontalAlignment.Center);
            shift_y += 38;
            if (imgXMax < shift_x + 187) imgXMax = shift_x + 187;
            return shift_y;
        }



        // Draw RestPartTable on canvas.
        private void ShowRestPartTable()
        {
            List<Part> SummedParts = SumParts(calc.Parts);
            var shift_x = 12;

            DrawTextBlock(" Unused Parts: ", shift_x, imgYMax, 265, HorizontalAlignment.Left);


            imgYMax += 19;

            DrawTextBlock("X", shift_x, imgYMax, 59, HorizontalAlignment.Center);
            DrawTextBlock("Y", shift_x + 59, imgYMax, 59, HorizontalAlignment.Center);
            DrawTextBlock("Amount", shift_x + 118, imgYMax, 59, HorizontalAlignment.Center);
            DrawTextBlock("Thickness", shift_x + 177, imgYMax, 59, HorizontalAlignment.Center);
            DrawTextBlock("Bar", shift_x + 236, imgYMax, 29, HorizontalAlignment.Center);

            imgYMax++;
            foreach (Part part in SummedParts)
            {
                imgYMax += 18;
                DrawPartLine(part, shift_x, imgYMax, true);
            }

            SummedParts = SumParts(calc.PartsBar);
            foreach (Part part in SummedParts)
            {
                imgYMax += 18;
                DrawPartLine(part, shift_x, imgYMax, true);
            }


            imgYMax += 38;
            if (imgXMax < shift_x + 275) imgXMax = shift_x + 275;
        }





        // Draw hole line of Parttable and RestPartTable.
        private void DrawPartLine(Part part, int shift_x, int shift_y, bool rest)
        {
            DrawTextBlock(part.Width.ToString() + " ", shift_x, shift_y, 59, HorizontalAlignment.Right);
            // No output in y-cell for bars.
            var text = (part.Bar == " ") ? part.Height.ToString() + " " : "- ";
            DrawTextBlock(text, shift_x + 59, shift_y, 59, HorizontalAlignment.Right);
            DrawTextBlock(part.Amount.ToString() + " ", shift_x + 118, shift_y, 59, HorizontalAlignment.Right);
            if (rest)
            {
                DrawTextBlock(part.Thickness.ToString() + " ", shift_x + 177, shift_y, 59, HorizontalAlignment.Right);
                DrawTextBlock(part.Bar, shift_x + 236, shift_y, 29, HorizontalAlignment.Center);
            }
        }




        // Draw a cell of a table.
        private void DrawTextBlock(string text, int x, int y, int width, HorizontalAlignment ha)
        {
            Border border = new Border();
            border.BorderThickness = new Thickness(1);
            border.BorderBrush = Brushes.Black;
            border.Width = width;
            border.Background = Brushes.White;

            TextBlock tb = new TextBlock();
            tb.Text = text;
            tb.Height = 17;
            tb.HorizontalAlignment = ha;

            border.Child = tb;

            Canvas.SetLeft(border, x);
            Canvas.SetTop(border, y);
            canvas.Children.Add(border);
        }




        // Plot a single rectangle.
        // Used for boards/bars and parts.
        private void MakeRectangle(int x, int y, int width, int height, string tooltip, Color color)
        {
            Rectangle rect = new Rectangle
            {
                Width = width,
                Height = height,
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.Black),
                ToolTip = tooltip,
                Fill = new SolidColorBrush(color)
            };

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            canvas.Children.Add(rect);
        }
    }
}


