using Latoken.Api.Client.Library.Utils.Configuration;
using Latoken.Api.Client.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using Newtonsoft.Json;
using static LatokenAPIWpfClient.MainWindow;
using System.ComponentModel;

namespace LatokenAPIWpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; set; } = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            if(ViewModel.IsFirstLaunch)
            {
                this.TabControl.SelectedItem = this.AccountSettingsTabItem;
            }
        }
    }
}
