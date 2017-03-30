using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.IO.Compression;
using SharpCompress.Archives;
using Newtonsoft.Json;
using System.Net.Http;
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
using SharpCompress.Readers;

namespace Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Settings settings = new Settings().ReturnSettings();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void launch_button_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(settings.pso2_path + "/pso2.exe"))
            {
                var info = new ProcessStartInfo(settings.pso2_path + "/pso2.exe");
                info.EnvironmentVariables.Add("-pso2", "+0x01e3f1e9");
                info.Arguments = "+0x33aca2b9";
                info.UseShellExecute = false;

                var process = new Process();
                process.StartInfo = info;
                process.Start();
            }
        }

        public async Task<Dictionary<string, Dictionary<string, string>[]>> GetDictAsync(string uri)
        {
            using (HttpClient client = new HttpClient())
            {
                var content = await client.GetStringAsync(uri);
                return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>[]>>(content);
            }
        }

        public async Task GetFileAsync(string uri, string location)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                Directory.CreateDirectory(location);
                using (FileStream fileStream = new FileStream(Path.Combine(location, Path.GetFileName(uri)), FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }
        }

        private async void enpatch_button_Click(object sender, RoutedEventArgs e)
        {
            pb_label.Content = "Installing english patch...";

            string url = "https://pitchblack.arghlex.net/pso2/?sort=modtime&order=desc&json";
            string baseUrl = "https://pso2.arghlex.net/pso2/";

            var json = await GetDictAsync(url);
            var file = json["files"][0]["name"];

            string fileUrl = baseUrl + file;
            await GetFileAsync(fileUrl, "Temp");

            using (var archive = SharpCompress.Archives.Zip.ZipArchive.Open(Path.Combine("Temp/", file)))
            {
                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                {
                    entry.WriteToDirectory(Path.Combine(settings.pso2Path, @"data/win32"), new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }

            Directory.Delete("Temp", true);
            pb_label.Content = "Success!";
        }
    }
}
