using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System.IO;

namespace toolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private int _counter;
        // private string _logs = "";
        
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
                var fileName = Path.GetFileName(file);
                var fileContent = File.ReadAllText(file);
                
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
        
        /*
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
        */

        private void FillLogs(string fileOneHeader, string fileTwoHeader, StringBuilder results)
        {
            // Display the logs results
            results.AppendLine("--------------------------------------------------------------------------------------");
            results.AppendLine("Results between " + fileOneHeader + " and " + fileTwoHeader + ":");
            // results.AppendLine(_logs);
            results.AppendLine("Count: " + _counter);
            results.AppendLine();
        }
        
        private void CompareNestedDictionary(dynamic? dataOne, dynamic? dataTwo,
            string path, Grid resultGrid)
        {
            // If equals don't need to continue
            if (JToken.DeepEquals(dataOne, dataTwo)) return;

            switch (dataOne)
            {
                // If both is JSON objects
                case JObject jObjectOne when dataTwo is JObject jObjectTwo:
                {
                    // Get the properties of both objects
                    var keys = new HashSet<string>(jObjectOne.Properties().Select(p => p.Name));
                    // Link the properties of the second object
                    keys.UnionWith(jObjectTwo.Properties().Select(p => p.Name));

                    // Compare each property & change the path
                    foreach (var key in keys)
                    {
                        var newPath = string.IsNullOrEmpty(path) ? key : $"{path}/{key}";
                        CompareNestedDictionary(jObjectOne[key], jObjectTwo[key], newPath, resultGrid);
                    }

                    break;
                }
                
                // If both is JSON arrays
                case JArray when dataTwo is JArray:
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
                        CompareNestedDictionary(dataOne[i], dataTwo[i], newPath, resultGrid);
                    }

                    break;
                }
                
                // Else they are different 
                default:
                {
                    _counter++;
                    var dateOneStg = dataOne?.ToString() ?? "null";
                    var dateTwoStg = dataTwo?.ToString() ?? "null";
                    // LogDifferences(fileNameOne, fileNameTwo, dateOneStg, dateTwoStg, path); 
                    AddRowResultGrid(path, dateOneStg, dateTwoStg, resultGrid);
                    break;
                }
            }
        }

        private void ReplaceOrCreateTabItem(string fileOneHeader, string fileTwoHeader, Grid resultGrid)
        {
            // Verify if result already exists
            var isExist = false;
            foreach (TabItem tabItem in TabCtrlResults.Items)
            {
                if (tabItem.Header.ToString() != fileOneHeader + " vs " + fileTwoHeader) continue;
                isExist = true;
                var sv = tabItem.Content as ScrollViewer;
                sv!.Content = resultGrid;
            }

            // If not exist => Create a new tab for the result
            if (!isExist)
            {
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
        }
        
        private void AddRowResultGrid(string path, string dataOne, string dataTwo, Grid resultGrid)
        {
            // Create a new row in the result grid
            resultGrid.RowDefinitions.Add(new RowDefinition());
            
            // Get the row index
            var rowIndex = resultGrid.RowDefinitions.Count;
            
            // Add a background color to the row
            var backgroundColor = rowIndex % 2 == 0 ? System.Windows.Media.Brushes.LightGray : System.Windows.Media.Brushes.LightSlateGray;
            var backgroundRectangle = new System.Windows.Shapes.Rectangle { Fill = backgroundColor };
            Grid.SetRow(backgroundRectangle, rowIndex);
            Grid.SetColumnSpan(backgroundRectangle, 3);
            resultGrid.Children.Add(backgroundRectangle);
            
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
            resultGrid.Children.Add(textBlockPath);
            resultGrid.Children.Add(textBlockDataOne);
            resultGrid.Children.Add(textBlockDataTwo);
        }
        
        private Grid GenerateResultGrid(string fileOneHeader, string fileTwoHeader)
        {
            // Create a new grid for the result
            var resultGrid = new Grid
            {
                Margin = new Thickness(5),
                // MaxWidth = 1400,
                // Grid layout
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                }, 
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                },
                // Column titles
                Children =
                {
                    new TextBlock
                    {
                        Text = "Number of differences: " + _counter,
                    },
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
            
            // -- Set the position of the numbers of differences
            Grid.SetColumn(resultGrid.Children[0], 0);
            Grid.SetRow(resultGrid.Children[0], 0);
            
            // -- Set position of the column titles
            Grid.SetColumn(resultGrid.Children[1], 0);
            Grid.SetRow(resultGrid.Children[1], 1);
            Grid.SetColumn(resultGrid.Children[2], 1);
            Grid.SetRow(resultGrid.Children[2], 1);
            Grid.SetColumn(resultGrid.Children[3], 2);
            Grid.SetRow(resultGrid.Children[3], 1);
            
            return resultGrid;
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
                    
                    var scrollViewerOne = tabItemOne.Content as ScrollViewer;
                    var scrollViewerTwo = tabItemTwo.Content as ScrollViewer;

                    if (scrollViewerOne?.Content is not TextBox textBoxOne || scrollViewerTwo?.Content is not TextBox textBoxTwo) continue;
                    
                    // Reset the counter and logs
                    // _logs = "";
                    _counter = 0;
                
                    // Get the file names
                    var fileOneHeader = tabItemOne.Header.ToString() ?? "File 1";
                    var fileTwoHeader = tabItemTwo.Header.ToString() ?? "File 2";
                    
                    try
                    {
                        // Parse the JSON files
                        var fileOne = JObject.Parse(textBoxOne.Text);
                        var fileTwo = JObject.Parse(textBoxTwo.Text);
                        
                        var resultGrid = GenerateResultGrid(fileOneHeader, fileTwoHeader);
                        
                        // Compare the files
                        CompareNestedDictionary(fileOne, fileTwo, "", resultGrid);
                        
                        // Modify the counter of differences
                        var textBlock = resultGrid.Children.OfType<TextBlock>().FirstOrDefault(tb => tb.Text.Contains("Number of differences:"));
                        if (textBlock != null) textBlock.Text = "Number of differences: " + _counter;
                        
                        // Replace or create the tab item for the result
                        ReplaceOrCreateTabItem(fileOneHeader, fileTwoHeader, resultGrid);
                    }
                    catch (JsonException ex)
                    {
                        results.AppendLine($"Error during reading file {fileOneHeader} or {fileTwoHeader} : {ex.Message}");
                    }
                    
                    // Fill the logs
                    FillLogs(fileOneHeader, fileTwoHeader, results);
                }
            }
            TxtBxLogs.Text += results.ToString();
            if (TabCtrlResults.Items.Count < 2) return;
            ((TabItem)TabCtrlResults.Items[1]!).IsSelected = true;
        }

        private string ChoosePathToExport()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                DefaultExt = ".xlsx"
            };
            if (saveFileDialog.ShowDialog() != true) return "";
            var filePath = saveFileDialog.FileName;
            return filePath;
        }
        
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            
            // Choose the path to save the file
            var filePath = ChoosePathToExport();
            if (string.IsNullOrEmpty(filePath)) return;
            
            // Determine licence for EPPlus
            ExcelPackage.License.SetNonCommercialOrganization("SIP");
            using var package = new ExcelPackage();
            
            // Create a new worksheet for each tab in TabCtrlResults
            foreach (var tabItem in TabCtrlResults.Items)
            {
                if (tabItem is not TabItem tab || tab.Header.ToString() == "Logs") continue;

                var scrollViewer = tab.Content as ScrollViewer;
                var grid = scrollViewer?.Content as Grid;

                var worksheet = package.Workbook.Worksheets.Add(tab.Header.ToString());

                for (var i = 0; i < grid?.RowDefinitions.Count; i++)
                {
                    for (var j = 0; j < grid.ColumnDefinitions.Count; j++)
                    {
                        var textBlock = grid.Children.OfType<TextBlock>().FirstOrDefault(tb => Grid.GetRow(tb) == i && Grid.GetColumn(tb) == j);
                        if (textBlock != null)
                        {
                            worksheet.Cells[i + 1, j + 1].Value = textBlock.Text;
                        }
                    }
                }
            }

            // Save the package to the specified file path
            package.SaveAs(new FileInfo(filePath));
            
        }
        
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // Close the selected tab
            if (TabCtrlFiles.SelectedItem is TabItem selectedTab)
            {
                TabCtrlFiles.Items.Remove(selectedTab);
            }
        }

        private void Reduce_Click(object sender, RoutedEventArgs e)
        {
            BtnReduce.Visibility = Visibility.Collapsed;
            BtnExpand.Visibility = Visibility.Visible;
            TabCtrlResults.Visibility = Visibility.Collapsed;
        }
        
        private void Expand_Click(object sender, RoutedEventArgs e)
        {
            BtnReduce.Visibility = Visibility.Visible;
            BtnExpand.Visibility = Visibility.Collapsed;
            TabCtrlResults.Visibility = Visibility.Visible;
        }
        
    }
}