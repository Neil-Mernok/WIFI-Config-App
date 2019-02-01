using CommanderParameters;
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

namespace WIFI_Config_App
{
    /// <summary>
    /// Interaction logic for ConfigureView.xaml
    /// </summary>
    public partial class ConfigureView : UserControl
    {
        public static CommanderParameterFile CommanderParameterFile = new CommanderParameterFile();


        public ConfigureView()
        {
            InitializeComponent();

 //           CommanderParameterFile = CommanderParameterManager.ReadCommanderParameterFile();

 //           CommanderParametersGrid.ItemsSource = CommanderParameterFile.CommanderParameterList;
//            WiFimessages.ParameterListsize = CommanderParameterFile.CommanderParameterList.Count * 4;

        }

        private void ViewChangeBack(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Startup;
        }
    }
}
