using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace Interface
{
    public partial class SaveWindow : Window
    {
        public List<RunInfo> Runs { get; set; }
        public string RunFileName { get; set; } = string.Empty;
        public void InitComboBox()
        {
            
            Runs = JsonSerializer.Deserialize<List<RunInfo>>(File.ReadAllText("runs.json"));
            ExpName.ItemsSource = Runs.Select(x => x.Name);
        }
        public SaveWindow()
        {
            InitializeComponent();
            InitComboBox();
        }

        private void Button_Click_Load(object sender, RoutedEventArgs e)
        {
            this.RunFileName = Runs[ExpName.SelectedIndex].FileName;
            this.DialogResult = true;
        }
    }
}
