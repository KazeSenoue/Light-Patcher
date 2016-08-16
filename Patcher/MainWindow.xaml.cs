using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Patcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public void runHelper()
        {
            //Loads settings
            Settings settings = new Settings().ReturnSettings();

            if (settings.PSO2 == "")
            {
                MessageBox.Show("Hi there! This appears to be your first time running Meme Patcher." + 
                    " Let's get you all set up, shall we? \nFirst, select your pso2_bin folder.", "Setup");
                settings.SavePSO2Dir();
            }
            else if (!settings.PSO2.EndsWith("pso2_bin"))
            {
                MessageBox.Show("Your PSO2 directory seems to be invalid. Let's re-do the setup process.");
                settings.SavePSO2Dir();
            }
        }


        public MainWindow()
        {
            runHelper();
            InitializeComponent();
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            //Loads settings
            Settings settings = new Settings().ReturnSettings();
            var command = "/C " + settings.PSO2;
            System.Diagnostics.Process.Start("EnglishPatchInstaller.exe", command);
        }
    }
}
