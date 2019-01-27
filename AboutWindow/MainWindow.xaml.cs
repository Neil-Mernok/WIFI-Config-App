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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AboutWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ApplicationName.Content = "Commander1xx Interface Utility";
            ApplicationVersion.Content = "Version 6";

            Application_Description.Text = "This Application is intended for changing Commander1xx parameter values. Other functions of this application include: \n"+
                                            "\n   1.    Resetting these values to default."+
                                            "\n   2.    Changing the date of the commander to that of the system date."+
                                            "\n   3.    Exporting the current parameter values to a .mer file.";

        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }

        private void button_Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }


}
