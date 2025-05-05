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
                var tabItem = new TabItem
                {
                    Header = fileName,
                    Content = new ScrollViewer
                    {
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, 
                        Content = new TextBox
                        {
                            Text = fileContent
                        }
                    }
                };
                
                // Add the new tab to the TabCtrlFiles
                TabCtrlFiles.Items.Add(tabItem);
            }
        }

        private void LogDifferences(string fileNameOne, string fileNameTwo, string dataOneStg, string dataTwoStg, string path)
        {
            // Log in terminal the path and the differences
            
            // Part path
            _logs += "Différence found in " + path + ": \n";

            var sizeNameOne = fileNameOne.Length;
            var sizeNameTwo = fileNameTwo.Length;
            var sizeDataOne = dataOneStg.Length;
            var sizeDataTwo = dataTwoStg.Length;
            
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
            _logs += "| " + dataOneStg;
            if (sizeNameOne > sizeDataOne)
            {
                for (var i = 0; i < (sizeNameOne - sizeDataOne); i++)
                {
                    _logs += " ";
                }
            }
            _logs += " | " + dataTwoStg;
            if (sizeNameTwo > sizeDataTwo)
            {
                for (var i = 0; i < (sizeNameTwo - sizeDataTwo); i++)
                {
                    _logs += " ";
                }
            }
            _logs += " |";
        }

        private void FillTabResults(string path, string dataOne, string dataTwo, Grid resultGrid)
        {
            // Create a new row in the result grid
            resultGrid.RowDefinitions.Add(new RowDefinition());
            
            // Get the row index
            var rowIndex = resultGrid.RowDefinitions.Count;
            
            // Create a new text block for each column
            var textBlockPath = new TextBlock
            {
                Text = path,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var textBlockDataOne = new TextBlock
            {
                Text = dataOne,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var textBlockDataTwo = new TextBlock
            {
                Text = dataTwo,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            
            // Set the position of the text blocks in the grid
            Grid.SetRow(textBlockPath, rowIndex);
            Grid.SetRow(textBlockDataOne, rowIndex);
            Grid.SetRow(textBlockDataTwo, rowIndex);
            Grid.SetColumn(textBlockPath, 0);
            Grid.SetColumn(textBlockDataOne, 1);
            Grid.SetColumn(textBlockDataTwo, 2);
        }
        
        private void CompareNestedDictionary(string fileNameOne, string fileNameTwo, dynamic? dataOne, dynamic? dataTwo,
            string path, Grid resultGrid)
        {
            // If equals don't need to continue
            if (JToken.DeepEquals(dataOne, dataTwo)) return;

            // If both is JSON objects, compare the properties
            if (dataOne is JObject jObjectOne && dataTwo is JObject jObjectTwo)
            {
                // Get the properties of both objects
                var keys = new HashSet<string>(jObjectOne.Properties().Select(p => p.Name));
                // Link the properties of the second object
                keys.UnionWith(jObjectTwo.Properties().Select(p => p.Name));

                // Compare each property & change the path
                foreach (var key in keys)
                {
                    var newPath = string.IsNullOrEmpty(path) ? key : $"{path}/{key}";
                    CompareNestedDictionary(fileNameOne, fileNameTwo, jObjectOne[key], jObjectTwo[key], newPath, resultGrid);
                }
            }
            
            // Else if both is JSON arrays, compare the elements
            else if (dataOne is JArray && dataTwo is JArray)
            {
                var size = Math.Min(dataOne.Count, dataTwo.Count);

                // Compare each element in both list & change the path
                for (var i = 0; i < size; i++)
                {
                    var endPath = "";
                    if (dataOne[i] is JObject subData1)
                    {
                        var foundKeyName = false;
                        foreach (var property in subData1.Properties())
                        {
                            // Skip the properties that are not needed because can be different in value
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
                    CompareNestedDictionary(fileNameOne, fileNameTwo, dataOne[i], dataTwo[i], newPath, resultGrid);
                }
            }
            // Else they are different 
            else
            {
                _counter++;
                var dateOneStg = dataOne?.ToString() ?? "null";
                var dateTwoStg = dataTwo?.ToString() ?? "null";
                LogDifferences(fileNameOne, fileNameTwo, dateOneStg, dateTwoStg, path);
                FillTabResults(path, dateOneStg, dateTwoStg, resultGrid);
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
                    
                    var scrollViewerOne = tabItemOne?.Content as ScrollViewer;
                    var scrollViewerTwo = tabItemTwo?.Content as ScrollViewer;
                    
                    var textBoxOne = scrollViewerOne?.Content as TextBox;
                    var textBoxTwo = scrollViewerTwo?.Content as TextBox;

                    if (tabItemOne == null || tabItemTwo == null) continue;

                    if (textBoxOne == null || textBoxTwo == null) continue;
                    
                    // Reset logs
                    _logs = "";
                    
                    try
                    {
                        // Get the file names and content
                        var fileOneHeader = tabItemOne.Header.ToString();
                        var fileTwoHeader = tabItemTwo.Header.ToString();
                        
                        var fileOne = JObject.Parse(textBoxOne.Text);
                        var fileTwo = JObject.Parse(textBoxTwo.Text);
                        
                        // Create a new grid for the result
                        var resultGrid = new Grid
                        {
                            Margin = new Thickness(5),
                            // Grid layout
                            ColumnDefinitions =
                            {
                                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                            }, 
                            // Column titles
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = "PATH",
                                    FontSize = 24,
                                    FontWeight = FontWeights.Bold,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                            
                                },
                                new TextBlock
                                {
                                    Text = fileOneHeader,
                                    FontSize = 24,
                                    FontWeight = FontWeights.Bold,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                },
                                new TextBlock
                                {
                                    Text = fileTwoHeader,
                                    FontSize = 24,
                                    FontWeight = FontWeights.Bold,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                },
                            }
                        };
                        
                        // -- Set position of the column titles
                        Grid.SetColumn(resultGrid.Children[0], 0);
                        Grid.SetColumn(resultGrid.Children[1], 1);
                        Grid.SetColumn(resultGrid.Children[2], 2);
                        
                        // Compare the files
                        CompareNestedDictionary(tabItemOne.Header.ToString()!, tabItemTwo.Header.ToString()!, fileOne, fileTwo, "", resultGrid);
                        
                        // Verify if result already exists
                        var isExist = false;
                        foreach (TabItem tabItem in TabCtrlResults.Items)
                        {
                            if (tabItem.Header.ToString() == fileOneHeader + " vs " + fileTwoHeader)
                            {
                                isExist = true;
                                tabItem.Content = resultGrid;
                            }
                        }
                        
                        if (isExist) continue;
                        
                        // If not exist => Create a new tab for the result
                        var resultTab = new TabItem
                        {
                            Header = fileOneHeader + " vs " + fileTwoHeader,
                            Content = new ScrollViewer
                            {
                                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                                Content = resultGrid
                            }
                        };
                        
                        // Add the new tab to the TabCtrlResults
                        TabCtrlResults.Items.Add(resultTab);
                        
                    }
                    catch (JsonException ex)
                    {
                        results.AppendLine($"Error during reading file {tabItemOne.Header} or {tabItemTwo.Header} : {ex.Message}");
                    }
                    
                    // Display the logs results
                    results.AppendLine("--------------------------------------------------------------------------------------");
                    results.AppendLine("Results between " + tabItemOne.Header + " and " + tabItemTwo.Header + ":");
                    results.AppendLine(_logs);
                    results.AppendLine("Count: " + _counter);
                    results.AppendLine();
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