using System;
using System.Net;
using System.Diagnostics;
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
        public void RunHelper()
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

        public void CheckForUpdates()
        {
            //Reads current version from Sega's servers
            WebRequest request = WebRequest.Create("http://download.pso2.jp/patch_prod/patches/version.ver");
            ((HttpWebRequest)request).UserAgent = "AQUA_HTTP";

            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string serverVersion = reader.ReadToEnd();

            reader.Close();
            response.Close();

            //Reads current stored version
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                using (StreamReader stream = new StreamReader(path + "SEGA\\PHANTASYSTARONLINE2"))
                {
                    string currentVersion = stream.ReadToEnd();

                    //Compares both versions
                    if (currentVersion != serverVersion)
                    {
                        Process.Start("Modules\\Updater.exe");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        public MainWindow()
        {
            RunHelper();
            CheckForUpdates();
            InitializeComponent();
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            //Loads settings
            Settings settings = new Settings().ReturnSettings();
            var command = "/C " + settings.PSO2;
            Process.Start("Modules\\EnglishPatchInstaller.exe", command);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //Loads settings
            Settings settings = new Settings().ReturnSettings();

            if (File.Exists(settings.PSO2 + "/pso2.exe"))
            {
                var info = new ProcessStartInfo(settings.PSO2 + "/pso2.exe");
                info.EnvironmentVariables.Add("-pso2", "+0x01e3f1e9");
                info.Arguments = "+0x33aca2b9";
                info.UseShellExecute = false;

                var process = new Process();
                process.StartInfo = info;
                process.Start();
            }
            else
            {
                MessageBox.Show("Nope");
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            var arguments = "/C \"{0}\" \"update\"";
            Process.Start(@"Modules/Updater.exe", arguments);
        }
    }
}
