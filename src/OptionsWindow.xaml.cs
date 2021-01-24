using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.SQLite;
using System.IO;

namespace Cutting_Optimizer
{
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
            box.Text = Options.MaxStage.ToString();
            showTablesbox.IsChecked = Options.ShowParttables;
            showPricesbox.IsChecked = Options.ShowPrices;
            sumPartsbox.IsChecked = Options.SumParts;
            sumBoardsbox.IsChecked = Options.SumBoards;
            sumManagerbox.IsChecked = Options.SumBoardsStock;
            shiftbox.IsChecked = Options.Shift;
            if (Options.CloseHolesEvery)
            {
                closeHolesBox.SelectedIndex = 0;
            }
            else if (Options.CloseHolesEnd)
            {
                closeHolesBox.SelectedIndex = 1;
            }
            else
            {
                closeHolesBox.SelectedIndex = 2;
            }
            simplebox.IsChecked = Options.SimpleMode;
            sortPartsbox.IsChecked = Options.SortParts;
            stockFirstbox.IsChecked = Options.StockFirst;
        }

        private void ClickSave(object sender, RoutedEventArgs e)
        {
            try
            {
                Options.MaxStage = Int32.Parse(box.Text);
            }
            catch { }

            if (Options.MaxStage >= 10000)
            {
                MessageBoxReturn.SetStrings("More Recursion Steps as recommended.\n" + "Continue?", "OK", "Cancel");
                CustomMessageBox customMessageBox = new CustomMessageBox();
                customMessageBox.Width += 50;
                customMessageBox.MinWidth += 50;
                customMessageBox.MaxWidth += 50;
                customMessageBox.Button1.Width +=25;
                customMessageBox.Button2.Width += 25;
                customMessageBox.ShowDialog();

                if (!MessageBoxReturn.Return)
                {
                    return;
                }
            }

            Options.ShowParttables = (showTablesbox.IsChecked == true);
            Options.ShowPrices = (showPricesbox.IsChecked == true);
            Options.SumParts = (sumPartsbox.IsChecked == true);
            Options.SumBoards = (sumBoardsbox.IsChecked == true);
            Options.SumBoardsStock = (sumManagerbox.IsChecked == true);
            Options.Shift = (shiftbox.IsChecked == true);
            switch (closeHolesBox.SelectedIndex)
            {
                case 0:
                    Options.CloseHolesEvery = true;
                    Options.CloseHolesEnd = false;
                    break;
                case 1:
                    Options.CloseHolesEvery = false;
                    Options.CloseHolesEnd = true;
                    break;
                case 2:
                    Options.CloseHolesEvery = false;
                    Options.CloseHolesEnd = false;
                    break;
            }
            Options.SimpleMode = (simplebox.IsChecked == true);
            Options.SortParts = (sortPartsbox.IsChecked == true);
            Options.StockFirst = (stockFirstbox.IsChecked == true);

            try
            {
                using (SQLiteConnection con = new SQLiteConnection("data source=Boards.db"))
                {
                    using (SQLiteCommand com = new SQLiteCommand(con))
                    {
                        con.Open();
                        com.CommandText = "DELETE FROM options; ";
                        com.ExecuteNonQuery();

                        com.CommandText = "INSERT INTO options " +
                                              "VALUES (";


                        com.CommandText += box.Text;
                        string c = (Options.ShowParttables) ? ", 1" : ", 0";
                        com.CommandText += c;
                        c = (Options.ShowPrices) ? ", 1" : ", 0";
                        com.CommandText += c;
                        c = (Options.SumParts) ? ", 1" : ", 0";
                        com.CommandText += c;
                        c = (Options.SumBoards) ? ", 1" : ", 0";
                        com.CommandText += c;
                        c = (Options.SumBoardsStock) ? ", 1" : ", 0";
                        com.CommandText += c;
                        c = (Options.Shift) ? ", 1" : ", 0";
                        com.CommandText += c;
                        c = (Options.CloseHolesEvery) ? ", 1" : ", 0";
                        com.CommandText += c;
                        c = (Options.CloseHolesEnd) ? ", 1" : ", 0";
                        com.CommandText += c;
                        c = (Options.SimpleMode) ? ", 1" : ", 0";
                        com.CommandText += c;
                        c = (Options.StockFirst) ? ", 1" : ", 0";
                        com.CommandText += c;
                        c = (Options.SortParts) ? ", 1" : ", 0";
                        com.CommandText += c;


                        com.CommandText += ", 0); ";

                        com.ExecuteNonQuery();
                        con.Close();
                    }
                }
            }
            catch
            {
                var text = "Sql Error. \n" + 
                           "Could not save Options.";
                MessageBoxReturn.SetStrings(text, "OK");
                CustomMessageBox customMessageBox = new CustomMessageBox();
                customMessageBox.Button2.Visibility = Visibility.Collapsed;
                customMessageBox.Buffer.Width = 35;
                customMessageBox.ShowDialog();
            }
            
            this.Close();
        }



        // Select all Text when go to TextBox.
        private void KeySelectedBox(object sender, RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            tb.SelectAll();
        }



        // Only int to TextBox.
        private void PreviewText(object sender, TextCompositionEventArgs e)
        {
            if (!Tools.IsNumber(e.Text))
            {
                e.Handled = true;
            }            
        }
    }
}
