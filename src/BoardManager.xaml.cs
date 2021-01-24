using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;

namespace Cutting_Optimizer
{
    public partial class BoardManager : Window
    {
        private ObservableCollection<Board> Board_List = new ObservableCollection<Board> { };

        // TwoDInput says if 1D(false) or 2D(true) input in active
        private bool TwoDInput;


        private string orderBy;
        private string select;

        private bool widthUD = true;
        private bool HeightUD = true;
        private bool thickUD = true;
        private bool amountUD = true;
        private bool priceUD = true;
        private bool barUD = true;


        public BoardManager()
        {
            InitializeComponent();
            board_box.ItemsSource = Board_List;
            TwoDInput = true;
            filterDropBox.SelectedIndex = 0;
            bxbox.Focus();
            BarDropField.ToolTip = HelpTexts.BarFilterTT;


            // Fill Board-Boax with values from stock database in the order saved in 'saveorder'.
            select = "SELECT s.* FROM stock AS s " +
                     "JOIN saveorder AS o " +
                     "ON s.ID = o.BoardIndex ";
            orderBy = "ORDER BY o.ID ";
            var command = select +  orderBy + ";";
            WriteBoardList(true, command);
        }


        void WriteBoardList(bool message, string command)
        {
             using (SQLiteConnection con = new SQLiteConnection("data source=Boards.db"))
             {
                  using (SQLiteCommand com = new SQLiteCommand(con))
                  {
                      con.Open();
                      com.CommandText = command;
                      using (SQLiteDataReader Reader = com.ExecuteReader())
                      {                          
                          //try
                          //{
                              while (Reader.Read())
                              {
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
                                        Bar = Reader.GetString(7),
                                        Thickness = Reader.GetInt32(3)
                                    });
                              }
                         /* }
                          catch
                          {
                              if (message)
                              {
                                  infobox.Text = "SQL Error! \n" +
                                                  "Cound not load database.";
                                  add_board_btn.Visibility = Visibility.Collapsed;
                              }
                              else
                              {
                                  infobox.Text = "Filter: Invalid Input.";
                              }
                          }*/
                      }
                      con.Close();
                  }
             }
            return;
        }




        // Sort Board-Box by column.
        // To do this the list gets cleared and than filled again 
        // from stock database in correct order, using sql 'order by' statement.
        private void SetOrder(object sender, RoutedEventArgs e)
        {
            bool upDown = true;
            string updown = "";
            string colm = "";
            
            if (sender.Equals(Width))
            {
                colm = "Width ";
                widthUD = !widthUD;
                upDown = widthUD;
                Width.Content = (upDown) ? "    x  " + (char)'\x25B2' : "    x  " + (char)'\x25BC';
            }
            else if (sender.Equals(Height))
            {
                colm = "Height ";
                HeightUD = !HeightUD;
                upDown = HeightUD;
                Height.Content = (upDown) ? "     y  " + (char)'\x25B2' : "     y  " + (char)'\x25BC';
            }
            else if (sender.Equals(Thickness))
            {
                colm = "Thickness ";
                thickUD = !thickUD;
                upDown = thickUD;
                Thickness.Content = (upDown) ? "     T  " + (char)'\x25B2' : "     T  " + (char)'\x25BC';
            }
            else if (sender.Equals(Amount))
            {
                colm = "Unlimited ";
                amountUD = !amountUD;
                upDown = amountUD;
                Amount.Content = (upDown) ? "     A  " + (char)'\x25B2' : "     A  " + (char)'\x25BC';
            }
            else if (sender.Equals(Price))
            {
                colm = "Price ";
                priceUD = !priceUD;
                upDown = priceUD;
                Price.Content = (upDown) ? " Price" + (char)'\x25B2' : " Price" + (char)'\x25BC';
            }
            else if (sender.Equals(Bar))
            {
                colm = "Bar ";
                barUD = !barUD;
                upDown = barUD;
                Bar.Content = (upDown) ? "   Bar " + (char)'\x25B2' : "   Bar " + (char)'\x25BC';
            }


            if (sender.Equals(Amount))
            {
                updown = (upDown) ? "ASC, Amount ASC" : "DESC, Amount DESC";
            }
            else
            {
                updown = (upDown) ? "ASC" : "DESC";
            }


            select = "SELECT s.* FROM stock  AS s ";
            orderBy = "ORDER BY " + colm + updown;
            var command = select + orderBy + ";";
            Board_List.Clear();
            WriteBoardList(true, command);
        }



        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Q:
                    SetTo1D();
                    break;
                case Key.W:
                    SetTo2D();
                    break;
            }
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
            BoardYText.Visibility = Visibility.Collapsed;
            bybox.Visibility = Visibility.Collapsed;
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
            BoardYText.Visibility = Visibility.Visible;
            bybox.Visibility = Visibility.Visible;
            TwoDInput = true;
            bxbox.Focus();
        }


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
                infobox.Text = "Error: Empty field.";
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
                infobox.Text = "Error: Invalid " + type + "size.";
                bxbox.Focus();
                return;
            }

            bool set_new = true;

            // Sum Boards if equal
            if (Options.SumBoardsStock)
            {
                foreach (Board board in Board_List)
                {
                    if (((board.Width == x && board.Height == y) ||
                        (board.Width == y && board.Height == x)) &&
                        board.Price == price &&
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

                        var command = " UPDATE stock " +
                                              " SET Amount = " + board.Amount_string +
                                              " WHERE ID = " + board.SqlIndex.ToString() +
                                              " ;";
                        SendSql(command);

                        break;
                    }
                }
            }
            if (set_new)
            {
                string a_string = (unlimited) ? "*" : a.ToString();
                string p_string = "";
                for (int i = 0; i < price_string.Length; i++)
                {
                    if (price_string[i] == ',') p_string += ".";
                    else p_string += price_string[i];
                }


                //Add new Board to Sql Database
                var bar_string = (bar == " ") ? "' '" : "'x'";
                var command = "INSERT INTO stock (Width, Height, Thickness, Price, Amount, Unlimited, Bar)" +
                              "VALUES (" + x.ToString() + ", " + y.ToString() + ", " + t.ToString() +
                              ", " + p_string + ", " + a.ToString() + ", " + unlimited.ToString() + ", " + bar_string + ");";
                SendSql(command);


                int index = 0;
                using (SQLiteConnection con = new SQLiteConnection("data source=Boards.db"))
                {
                    using (SQLiteCommand com = new SQLiteCommand(con))
                    {
                        con.Open();
                        com.CommandText = "SELECT MAX(ID) FROM stock";
                        index = Int32.Parse(com.ExecuteScalar().ToString());
                        con.Close();
                    }
                }


                command = "INSERT INTO saveorder (BoardIndex) " +
                          "Values (" + index.ToString() + ");";
                SendSql(command);

                // Add new Board to list
                Board_List.Add(new Board
                {
                    Width = x,
                    Height = y,
                    Amount = a,
                    Amount_string = a_string,
                    Price = price,
                    Price_string = price_string,
                    Unlimited = unlimited,
                    Bar = bar,
                    Thickness = t,
                    SqlIndex = index
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


        private void SendSql(string command)
        {
            using (SQLiteConnection con = new SQLiteConnection("data source=Boards.db"))
            {
                using (SQLiteCommand com = new SQLiteCommand(con))
                {
                    con.Open();
                    com.CommandText = command;
                    com.ExecuteNonQuery();
                    con.Close();
                }
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

                var command = " UPDATE stock " +
                              " SET Amount = " + Board_List[index].Amount_string +
                              " WHERE ID = " + Board_List[index].SqlIndex.ToString() +
                              " ;";
                SendSql(command);
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

                var command = " UPDATE stock " +
                              " SET Amount = " + Board_List[index].Amount_string +
                              " WHERE ID = " + Board_List[index].SqlIndex.ToString() +
                              " ;";
                SendSql(command);
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




        // Delete single Board.
        private void ClickDelBoard(object sender, RoutedEventArgs e)
        {
            try
            {
                var pos = board_box.SelectedIndex;
                var index = Board_List[pos].SqlIndex.ToString();
                Board_List.RemoveAt(pos);

                var command = "DELETE FROM stock " +
                              "WHERE ID = " + index +
                              ";";
                SendSql(command);

                command = "DELETE FROM saveorder " +
                          "WHERE BoardIndex = " + index +
                          ";";
                SendSql(command);

                infobox.Text = "Board Removed.";
            }
            catch
            {
                infobox.Text = "Error: No Board Selected.";
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
                var command = "DELETE FROM stock;";
                SendSql(command);
                command = "DELETE FROM saveorder;";
                SendSql(command);
            }
        }

        private void ClickAddToMain(object sender, RoutedEventArgs e)
        {
            try
            {
                var pos = board_box.SelectedIndex;
                if (MessageBoxReturn.BoardsReturn == null)
                    MessageBoxReturn.BoardsReturn = new List<Board> { new Board(Board_List[pos]) };
                else
                    MessageBoxReturn.BoardsReturn.Add(new Board(Board_List[pos]));
                infobox.Text = "Board will be added to Main Window \n" + 
                               "after closing this window.";
            }
            catch
            {
                infobox.Text = "Error: No Board Selected.";
            }
        }

        private void ClickClose(object sender, RoutedEventArgs e)
        {            
            this.Close();
        }



        // Handle which inputs in TextBoxes.
        // Only int allowed in most TextBoxes.
        // "," or "." possible in price, 2 decimal places.
        // "*" possible in board/bar amount.
        private void PreviewText(object sender, TextCompositionEventArgs e)
        {
            TextBox srcTextbox = e.Source as TextBox;
            srcTextbox.SelectedText = "";
            if (srcTextbox.Name == "filterBox")
            {
                filterBlock.Visibility = Visibility.Collapsed;
            }
            else if (((e.Text != "," && e.Text != "*") && !Tools.IsNumber(e.Text)) || srcTextbox.Text.Contains("*"))
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

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            filterBlock.Visibility = (filterBox.Text == "") ? Visibility.Visible : Visibility.Collapsed;
            string command;
            if (filterBox.Text == "")
            {
                command = select + orderBy + ";";
            }
            else if (filterDropBox.Text == "Bar" &&
                     (filterBox.Text == "board" ||
                      filterBox.Text == "Board" ||
                      filterBox.Text == "y" ||
                      filterBox.Text == "-"))
            {
                command = select +
                          "WHERE NOT s." +
                          filterDropBox.Text + " = 'x' " + orderBy + ";";
            }

            else if (filterDropBox.Text == "Bar" &&
                     (filterBox.Text == "bar" ||
                      filterBox.Text == "Bar" ||
                      filterBox.Text == "x"))
            {
                command = select +
                          "WHERE s." +
                          filterDropBox.Text + " = 'x' " + orderBy + ";";
            }
            else if (filterDropBox.Text == "Amount" && filterBox.Text == "*")
            {
                command = select +
                          "WHERE s.Unlimited = 1 " +
                          orderBy + ";";
            }
            else
            {
                command = select +
                          "WHERE s." +
                          filterDropBox.Text + " = " + filterBox.Text +
                          " " + orderBy + ";";
            }

            Board_List.Clear();
            WriteBoardList(false, command);
        }



        // Select all text, when click in TextBox
        private void KeySelectedBox(object sender, RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            tb.SelectAll();
        }


        // Save order to 'saveorder' when closing window, if nothing is filtered. 
        private void CloseWindow(object sender, CancelEventArgs e)
        {
            if (filterBox.Text != "") return;

            var command = "DELETE FROM saveorder;";
            SendSql(command);

            foreach (Board board in Board_List)
            {
                command = "INSERT INTO saveorder (BoardIndex) " +
                          "Values (" + board.SqlIndex.ToString() + ");";
                SendSql(command);
            }
        }
    }
}
