using System.Windows;
using System.Windows.Controls;

namespace Cutting_Optimizer
{
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox()
        {
            InitializeComponent();
            text.Text = MessageBoxReturn.Text;
            Button1.Content = MessageBoxReturn.Button1;
            Button2.Content = MessageBoxReturn.Button2;
            MessageBoxReturn.Return = false;
            Button1.Focus();
        }

        private void ClickButton(object sender, RoutedEventArgs e)
        {
            Button src = e.Source as Button;
            if (src.Name == "Button1")
            {
                MessageBoxReturn.Return = true;
            }
            this.Close();
        }
    }
}
