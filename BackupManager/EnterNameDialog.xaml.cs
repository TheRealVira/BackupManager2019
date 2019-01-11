using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BackupManager
{
    /// <summary>
    /// Interaction logic for EnterNameDialog.xaml
    /// </summary>
    public partial class EnterNameDialog : Window
    {
        public EnterNameDialog()
        {
            InitializeComponent();
        }

        public string ResponseText {
            get => ResponseTextBox.Text;
            set => ResponseTextBox.Text = value;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ResponseTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            btn_ok.Visibility = ResponseText.Equals(string.Empty) ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
