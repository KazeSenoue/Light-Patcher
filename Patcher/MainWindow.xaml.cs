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
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SEGA\PHANTASYSTARONLINE2\version.ver";
                using (StreamReader stream = new StreamReader(path))
                {
                    string currentVersion = stream.ReadToEnd();

                    //Compares both versions
                    if (currentVersion != serverVersion)
                    {
                        //Loads settings
                        Settings settings = new Settings().ReturnSettings();

                        string args = String.Format("\"{0}\"", settings.PSO2);
                        Process.Start("Modules\\Updater.exe", args);
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

        private void button_Click_2(object sender, RoutedEventArgs e)
        {
            //Loads settings
            Settings settings = new Settings().ReturnSettings();
            var command = String.Format("\"{0}\"", settings.PSO2);
            Process.Start(@"InstallEnglishPatch.exe", command);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //Loads settings
            Settings settings = new Settings().ReturnSettings();

            if (File.Exists(settings.PSO2 + "/pso2.exe"))
            {
                //Downloads ddraw.dll and translation files
                using (var client = new WebClient())
                {
                    client.DownloadFile("http://kazesenoue.moe/pso2/ddraw.dll", settings.PSO2 + "ddraw.dll");
                    client.DownloadFile("http://108.61.203.33/freedom/translation.bin", settings.PSO2 + "translation.bin");
                    client.DownloadFile("http://108.61.203.33/freedom/translator.dll", settings.PSO2 + @"\plugins\translator.dll");
                }

                var info = new ProcessStartInfo(settings.PSO2 + "/pso2.exe");
                info.EnvironmentVariables.Add("-pso2", "+0x01e3f1e9");
                info.Arguments = "+0x33aca2b9";
                info.UseShellExecute = false;

                var process = new Process();
                process.StartInfo = info;
                process.Start();

                //Deletes ddraw.dll after it's launched
                while (true)
                {
                    Process[] processes = Process.GetProcessesByName("pso2");
                    if (processes.Length > 0){
                        if (processes[0].MainWindowTitle == "Phantasy Star Online 2" && processes[0].MainModule.ModuleName == "pso2.exe")
                        {
                            File.Delete(settings.PSO2 + "ddraw.dll");
                            break;
                        }
                    }
                    
                }
            }
            else
            {
                MessageBox.Show("Nope");
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            //Loads settings
            Settings settings = new Settings().ReturnSettings();

            string args = String.Format("\"{0}\"", settings.PSO2);
            Process.Start(@"Modules/FixInstall.exe", args);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var PSO2Settings = new Window1();
            PSO2Settings.Show();
        }
    }
}
