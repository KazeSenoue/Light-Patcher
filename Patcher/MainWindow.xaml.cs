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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //Loads settings
            Settings set = new Settings();
            Settings settings = set.ReturnSettings();

            //Prompts user to select pso2_dir
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();

            //If the folder is "pso2_bin", saves it to settings.json. If not, tells the user they fucked up.
            var pso2_dir = dialog.FileName;
            Console.WriteLine(pso2_dir);
            if (pso2_dir.EndsWith("pso2_bin"))
            {
                settings.PSO2 = pso2_dir;
                string output = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText("settings.json", output);
                MessageBox.Show(String.Format("Saved!\nSelected folder: {0}", settings.PSO2));
            }
            else
            {
                MessageBox.Show(String.Format("{0} is not a valid PSO2 folder. Please try again.", pso2_dir));
            }
        }
    }
}
