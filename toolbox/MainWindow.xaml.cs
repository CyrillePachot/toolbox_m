using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace toolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private int _counter;
        private string _logs = "";
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFiles_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog to select multiple files
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Filter = "Text files (*.json)|*.json|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() != true) return;
            // For each selected file, fill the list on top, create a new tab and load the file content
            foreach (var file in openFileDialog.FileNames)
            {
                var fileName = System.IO.Path.GetFileName(file);
                var fileContent = System.IO.File.ReadAllText(file);
                // Create a new tab and load the file content 
                TabCtrlFiles.Items.Add(new TabItem
                {
                    Header = fileName,
                    Content = new TextBox
                    {
                        Text = fileContent,
                        AcceptsReturn = true,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                    }
                });
            }
        }

        private void LogDifferences(string fileNameOne, string fileNameTwo, dynamic? dataOne, dynamic? dataTwo, string path)
        {
            // Log in terminal the path and the differences
            
            // Part path
            _logs += "Différence found in " + path + ": \n";

            var sizeNameOne = fileNameOne.Length;
            var sizeNameTwo = fileNameTwo.Length;
            var sizeDataOne = dataOne?.ToString().Length;
            var sizeDataTwo = dataTwo?.ToString().Length;
            
            // Part names
            _logs += "| " + fileNameOne;
            if (sizeDataOne > sizeNameOne)
            {
                for (var i = 0; i < (sizeDataOne - sizeNameOne); i++)
                {
                    _logs += " ";
                }
            } 
            _logs += " | " + fileNameTwo;
            if (sizeDataTwo > sizeNameTwo)
            {
                for (var i = 0; i < (sizeDataTwo - sizeNameTwo); i++)
                {
                    _logs += " ";
                }
            }
            _logs += " |\n";
            
            // Part data
            _logs += "| " + dataOne?.ToString();
            if (sizeNameOne > sizeDataOne)
            {
                for (var i = 0; i < (sizeNameOne - sizeDataOne); i++)
                {
                    _logs += " ";
                }
            }
            _logs += " | " + dataTwo?.ToString();
            if (sizeNameTwo > sizeDataTwo)
            {
                for (var i = 0; i < (sizeNameTwo - sizeDataTwo); i++)
                {
                    _logs += " ";
                }
            }
            _logs += " |";
        }
        
        private void CompareNestedDictionnary(string fileNameOne, string fileNameTwo, dynamic? dataOne, dynamic? dataTwo,
            String path)
        {
            if (JToken.DeepEquals(dataOne, dataTwo)) return;

            if (dataOne is JObject jObjectOne && dataTwo is JObject jObjectTwo)
            {
                var keys = new HashSet<string>(jObjectOne.Properties().Select(p => p.Name));
                keys.UnionWith(jObjectTwo.Properties().Select(p => p.Name));

                foreach (var key in keys)
                {
                    var newPath = string.IsNullOrEmpty(path) ? key : $"{path}/{key}";
                    CompareNestedDictionnary(fileNameOne, fileNameTwo, jObjectOne[key], jObjectTwo[key], newPath);
                }
            }
            else if (dataOne is JArray && dataTwo is JArray)
            {
                var size = Math.Min(dataOne.Count, dataTwo.Count);

                for (int i = 0; i < size; i++)
                {
                    var endPath = "";
                    if (dataOne[i] is JObject subData1)
                    {
                        var foundKeyName = false;
                        foreach (var property in subData1.Properties())
                        {
                            if (property.Name != "OptionKey" && property.Name != "Key" && property.Name != "Name")
                                continue;
                            endPath = property.Value.ToString();
                            foundKeyName = true;
                            break;
                        }
                        if (!foundKeyName)
                        {
                            endPath = i.ToString();
                        }
                    }
                    var newPath = string.IsNullOrEmpty(path) ? endPath : $"{path}/{endPath}";
                    CompareNestedDictionnary(fileNameOne, fileNameTwo, dataOne[i], dataTwo[i], newPath);
                }
            }
            else
            {
                _counter++;
                LogDifferences(fileNameOne, fileNameTwo, dataOne, dataTwo, path);
            }
        }
        
        private void CompareFiles_Click(object sender, RoutedEventArgs e)
        {
            // Check if we have at least two files to compare
            if (TabCtrlFiles.Items.Count < 2)
            {
                MessageBox.Show("Please open at least two files to compare.");
                return;
            }
            // For each files in TabCtrlFiles, compare the content and show the result in TxtBxResult
            var results = new StringBuilder();
            
            for (var i = 0; i < TabCtrlFiles.Items.Count; i++)
            {
                for (var j = i+1; j < TabCtrlFiles.Items.Count; j++)
                {
                    var tabItemOne = TabCtrlFiles.Items[i] as TabItem;
                    var tabItemTwo = TabCtrlFiles.Items[j] as TabItem;

                    if (tabItemOne == null || tabItemTwo == null) continue;

                    var textBoxOne = tabItemOne.Content as TextBox;
                    var textBoxTwo = tabItemTwo.Content as TextBox;

                    if (textBoxOne == null || textBoxTwo == null) continue;

                    try
                    {
                        var fileOne = JObject.Parse(textBoxOne.Text);
                        var fileTwo = JObject.Parse(textBoxTwo.Text);

                        _logs = "";
                        CompareNestedDictionnary(tabItemOne.Header.ToString()!, tabItemTwo.Header.ToString()!, fileOne, fileTwo, "");
                        results.AppendLine("--------------------------------------------------------------------------------------");
                        results.AppendLine("Results between " + tabItemOne.Header + " and " + tabItemTwo.Header + ":");
                        results.AppendLine(_logs);
                        results.AppendLine("Count: " + _counter);
                        results.AppendLine();
                    }
                    catch (JsonException ex)
                    {
                        results.AppendLine($"Error during reading file {tabItemOne.Header} or {tabItemTwo.Header} : {ex.Message}");
                    }
                }
            }
            TxtBxResult.Text = results.ToString();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // Close the selected tab
            if (TabCtrlFiles.SelectedItem is TabItem selectedTab)
            {
                TabCtrlFiles.Items.Remove(selectedTab);
            }
        }
        
        private void Save_Click(object sender, RoutedEventArgs e)
        {
        }
        
        
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
        }
        
        private void Export_Click(object sender, RoutedEventArgs e)
        {
        }
        
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
        }
        
        private void Reduce_Click(object sender, RoutedEventArgs e)
        {
            BtnReduce.Visibility = Visibility.Collapsed;
            BtnExpand.Visibility = Visibility.Visible;
            TxtBxResult.Visibility = Visibility.Collapsed;
        }
        
        private void Expand_Click(object sender, RoutedEventArgs e)
        {
            BtnReduce.Visibility = Visibility.Visible;
            BtnExpand.Visibility = Visibility.Collapsed;
            TxtBxResult.Visibility = Visibility.Visible;
        }
        
    }
}