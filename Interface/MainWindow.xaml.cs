using System.Formats.Tar;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GeneticLibViewModel;
using System.IO;
using System.Text.Json;
using System;

namespace Interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class RunInfo
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public RunInfo(string name, string filename) {
            Name = name;
            FileName = filename;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            RunInfo objAsPart = obj as RunInfo;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode() + FileName.GetHashCode();
        }
        public bool Equals(RunInfo other)
        {
            if (other == null) return false;
            return (this.Name.Equals(other.Name));
        }
    }
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            double doubleValue = System.Convert.ToDouble(value);

            return String.Format("{0:F}", doubleValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            double? result = null;

            try
            {
                result = System.Convert.ToDouble(value);
            }
            catch
            {
            }

            return result.HasValue ? (object)result.Value : DependencyProperty.UnsetValue;
        }
    }
    public partial class MainWindow : Window, IUIServices
    {
        public const string RUNSJSON = "runs.json";
        public class Matchup
        {
            public int Round { get; set; }
            public Dictionary<int, string> CourtMatches { get; set; }

            public Matchup(int round, Dictionary<int, string> matches)
            {
                Round = round;
                CourtMatches = matches;
            }
        }
        public void ErrorReport(string message)
        {
            MessageBox.Show(message);
        }
        private void Result_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowTable(((MainViewModel)DataContext).BestTable, ((MainViewModel)DataContext).Courts);
        }
        public void ShowTable(Dictionary<int, SortedSet<int>>[] table, int num_courts)
        {
            List<Matchup> matchups = new List<Matchup>();
            for (int i = 0; i < table.Length; i++)
            {
                var matchup = new Dictionary<int, string>();
                foreach (var match in table[i])
                {
                    int[] players = match.Value.ToArray();
                    matchup.Add(match.Key, $"{players[0] + 1} : {players[1] + 1}");
                }
                matchups.Add(new Matchup(i + 1, matchup)); 
            }

            GenerateDataGridColumns(num_courts);

            Table.ItemsSource = matchups;
        }

        private void GenerateDataGridColumns(int numberOfCourts)
        {
            Table.Columns.Clear();
            Table.Columns.Add(new DataGridTextColumn
            {
                Header = "Тур",
                Binding = new Binding("Round")
            });

            for (int i = 1; i <= numberOfCourts; i++)
            {
                Table.Columns.Add(new DataGridTextColumn
                {
                    Header = $"Площадка {i}",
                    Binding = new Binding($"CourtMatches[{i}]")
                });
            }
        }
        public MainWindow()
        {
            DataContext = new MainViewModel(this);
            InitializeComponent();
            if (!File.Exists(RUNSJSON))
            {
                try
                {
                    File.Create(RUNSJSON);
                    File.WriteAllText(RUNSJSON, "[]");
                }
                catch (Exception ex) 
                {
                    MessageBox.Show($"{RUNSJSON} initialization failed: \n{ex.Message}");
                }
            }
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            string TMPRUNSJSON = $"tmp_{RUNSJSON}"; // Временный файл для хранения обновленного списка экспериментов
            string state = ((MainViewModel)DataContext).SaveState();
            RunInfo Experiment = new RunInfo(ExperimentName.Text,  $"{ExperimentName.Text}-{ExperimentName.Text.GetHashCode()}.json");
            List<RunInfo> Runs;
            try // Загружаем данные экпериментов
            {
                Runs = JsonSerializer.Deserialize<List<RunInfo>>(File.ReadAllText(RUNSJSON));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occured while reading from {RUNSJSON}:\n{ex.Message}");
                return;
            }
            if (Runs.Contains(Experiment)) // Если эксперимент с таким именем уже есть, то возвращаем ошибку
            {
                MessageBox.Show($"Experiment with name: \"{Experiment.Name}\" already exists.");
                return;
            }
            try // Сохраняем эксперимент
            {
                File.WriteAllText(Experiment.FileName, state);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occured while writing file:\n{ex.Message}");
                if (File.Exists(Experiment.FileName)) // Удаляем файл если он был создан но не записан
                {
                    File.Delete(Experiment.FileName); // И в принипе при любых ошибках удаляем этот файл
                }
                return;
            }
            Runs.Add(Experiment);
            try // Сохраняем обновленный список экспериментов 
            {
                File.WriteAllText(TMPRUNSJSON, JsonSerializer.Serialize(Runs));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occured while writing in {RUNSJSON}:\n{ex.Message}");
                if (File.Exists(TMPRUNSJSON))
                {
                    File.Delete(TMPRUNSJSON);
                }
                File.Delete(Experiment.FileName);
                return;
            }
            try // Перезаписываем файл с информацией об экспериментах
            {
                File.Move(TMPRUNSJSON, RUNSJSON, true);
            }
            catch (Exception ex) 
            { 
                MessageBox.Show($"Unable to rewrite {RUNSJSON}:\n{ex.Message}");
                File.Delete(Experiment.FileName);
                return; 
            }
            finally
            {
                File.Delete(TMPRUNSJSON); // Временный файл должен быть удален в любом случае
            }
            MessageBox.Show("Saved.");
        }

        private void LoadClick(object sender, RoutedEventArgs e)
        {
            SaveWindow DialogWindow = new SaveWindow();
            DialogWindow.ShowDialog();
            if (!(bool)DialogWindow.DialogResult)
            {
                return;
            }
            try
            {
                string file_content = File.ReadAllText(DialogWindow.RunFileName);
                ((MainViewModel)DataContext).LoadState(file_content);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occured while reading data: {ex.Message}");
            }
        }
    }
}