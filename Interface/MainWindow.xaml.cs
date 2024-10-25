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

namespace Interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
        }
    }
}