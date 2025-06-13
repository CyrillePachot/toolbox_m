using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System.IO;
using toolbox.Model;
using System.Collections.ObjectModel;

namespace toolbox
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Comparison> comparisons = new List<Comparison>();
        private Comparison currentComparison;

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
            var counter = 0;
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
                
                // Print in logs the file opened
                var logFile = $"File '{fileName}' opened successfully.";
                TxtBxLogs.Text += logFile + "\n";
                counter++;
            }
            var logCounter = "" + counter + " files opened successfully.";
            TxtBxLogs.Text += logCounter + "\n";
        }
        
        private void FillLogs(string fileOneHeader, string fileTwoHeader, StringBuilder results)
        {
            // Display the logs results
            results.AppendLine("--------------------------------------------------------------------------------------");
            results.AppendLine("Results between " + fileOneHeader + " and " + fileTwoHeader + ":");
            results.AppendLine(currentComparison.Logs);
            results.AppendLine("Count: " + currentComparison.Count);
            results.AppendLine();
        }

        private void CompareJObject(JObject jObjectOne, JObject jObjectTwo, string path, Grid resultGrid)
        {
            // Get the properties of both objects
            var keys = new HashSet<string>(jObjectOne.Properties().Select(p => p.Name));
            // Link the properties of the second object
            keys.UnionWith(jObjectTwo.Properties().Select(p => p.Name));

            // Compare each property & change the path
            foreach (var key in keys)
            {
                //if (key is "OptionKey" or "Key" or "Name") // Not sure useful but maybe necesary to don't show /key
                //continue; // Skip the properties because normally used to identify a key "Value" in the JSON object and compare it.
                var newPath = string.IsNullOrEmpty(path) ? key : $"{path}/{key}";
                CompareNestedDictionary(jObjectOne[key], jObjectTwo[key], newPath, resultGrid);
            }
        }

        private void CompareJArray(JArray jArrayOne, JArray jArrayTwo, string path, Grid resultGrid)
        {
            foreach (var elementOne in jArrayOne)
            {
                var endPath = "";
                if (elementOne is JObject subObjectOne)
                {
                    var keyNameOne = "";
                    var keyValueOne = "";
                    foreach (var property in subObjectOne.Properties())
                    {
                        switch (property.Name)
                        {
                            case "OptionKey" or "Key" or "Name":
                                keyNameOne = property.Value.ToString();
                                endPath = keyNameOne;
                                break;
                            case "Value":
                                keyValueOne = property.Value.ToString();
                                break;
                        }
                    }

                    var found = false;
                    var keyValueTwo = "";
                    foreach (var elementTwo in jArrayTwo)
                    {
                        if (elementTwo is not JObject subObjectTwo) continue;
                        var keyNameTwo = "";
                        foreach (var property in subObjectTwo.Properties())
                        {
                            switch (property.Name)
                            {
                                case "OptionKey" or "Key" or "Name":
                                    keyNameTwo = property.Value.ToString();
                                    break;
                                case "Value":
                                    keyValueTwo = property.Value.ToString();
                                    break;
                            }
                        }
                        if (keyNameTwo == keyNameOne)
                        {
                            found = true;
                            if (keyValueOne != keyValueTwo)
                            {
                                var newPath = string.IsNullOrEmpty(path) ? endPath : $"{path}/{endPath}";
                                Difference difference = new Difference(newPath, keyValueOne, keyValueTwo);
                                currentComparison.AddRowResultGrid(difference);
                            }
                            break; // On a trouvé le matching, inutile de continuer
                        }
                    }
                    if (!found)
                    {
                        var newPath = string.IsNullOrEmpty(path) ? endPath : $"{path}/{endPath}";
                        Difference difference = new Difference(newPath, keyValueOne, "");
                        currentComparison.AddRowResultGrid(difference);
                    }
                }
            }

            // Même logique pour jArrayTwo pour les éléments non présents dans jArrayOne
            foreach (var elementTwo in jArrayTwo)
            {
                var endPath = "";
                if (elementTwo is JObject subObjectTwo)
                {
                    var keyNameTwo = "";
                    var keyValueTwo = "";
                    foreach (var property in subObjectTwo.Properties())
                    {
                        switch (property.Name)
                        {
                            case "OptionKey" or "Key" or "Name":
                                keyNameTwo = property.Value.ToString();
                                endPath = keyNameTwo;
                                break;
                            case "Value":
                                keyValueTwo = property.Value.ToString();
                                break;
                        }
                    }

                    var found = false;
                    foreach (var elementOne in jArrayOne)
                    {
                        if (elementOne is not JObject subObjectOne) continue;
                        var keyNameOne = "";
                        foreach (var property in subObjectOne.Properties())
                        {
                            if (property.Name is "OptionKey" or "Key" or "Name")
                                keyNameOne = property.Value.ToString();
                        }
                        if (keyNameTwo == keyNameOne)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        var newPath = string.IsNullOrEmpty(path) ? endPath : $"{path}/{endPath}";
                        Difference difference = new Difference(newPath, "", keyValueTwo);
                        currentComparison.AddRowResultGrid(difference);
                    }
                }
            }
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
                    // Compare the two JSON objects
                    CompareJObject(jObjectOne, jObjectTwo, path, resultGrid);
                    break;
                }
                
                // If both is JSON arrays
                case JArray jArrayOne when dataTwo is JArray jArrayTwo:
                {
                    // Compare the two JSON arrays
                    CompareJArray(jArrayOne, jArrayTwo, path, resultGrid);
                    break;
                }
                
                // Else they are different 
                default:
                {
                    var dateOneStg = dataOne?.ToString() ?? "null";
                    var dateTwoStg = dataTwo?.ToString() ?? "null";
                    Difference difference = new Difference(path, dateOneStg, dateTwoStg);
                    currentComparison.AddRowResultGrid(difference);
                    break;
                }
            }
        }

        private static string ChoosePathToExport()
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
                                    
                    // Get the file names
                    var fileNameOne = tabItemOne.Header.ToString() ?? "File 1";
                    var fileNameTwo = tabItemTwo.Header.ToString() ?? "File 2";

                    // Initialize the current comparison
                    currentComparison = new Comparison(fileNameOne, fileNameTwo);

                    try
                    {
                        // Parse the JSON files
                        var fileOne = JObject.Parse(textBoxOne.Text);
                        var fileTwo = JObject.Parse(textBoxTwo.Text);

                        currentComparison.GenerateGridResult(fileNameOne, fileNameTwo);
                        
                        // Compare the files
                        CompareNestedDictionary(fileOne, fileTwo, "", currentComparison.GridResult);
                        
                        // Modify the counter of differences
                        var textBlock = currentComparison.GridResult.Children.OfType<TextBlock>().FirstOrDefault(tb => tb.Text.Contains("Number of differences:"));
                        if (textBlock != null) textBlock.Text = "Number of differences: " + currentComparison.Count;

                        // Replace or create the tab item for the result
                        currentComparison.ReplaceOrCreateTabItem(TabCtrlResults);
                    }
                    catch (JsonException ex)
                    {
                        results.AppendLine($"Error during reading file {currentComparison.FileNameOne} or {currentComparison.FileNameTwo} : {ex.Message}");
                    }
                    
                    // Fill the logs
                    FillLogs(currentComparison.FileNameOne, currentComparison.FileNameTwo, results);
                }
            }
            TxtBxLogs.Text += results.ToString();
            if (TabCtrlResults.Items.Count < 2) return;
            ((TabItem)TabCtrlResults.Items[1]!).IsSelected = true;
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
                var name = "";
                if (tab.Header is StackPanel sp)
                {
                    if (sp.Children[0] is TextBlock tb)
                    {
                        name = tb.Text; 
                    }
                }
                else
                {
                    name = tab.Header.ToString();
                }

                var worksheet = package.Workbook.Worksheets.Add(name);

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

            // Display a message box to inform the user
            MessageBox.Show($"Results exported successfully to {filePath}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);

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
            TabCtrlFiles.Visibility = Visibility.Visible;
        }
        
        private void Expand_Click(object sender, RoutedEventArgs e)
        {
            BtnReduce.Visibility = Visibility.Visible;
            BtnExpand.Visibility = Visibility.Collapsed;
            TabCtrlResults.Visibility = Visibility.Visible;
            TabCtrlFiles.Visibility = Visibility.Collapsed;
        }
        
        private void Split_Click(object sender, RoutedEventArgs e)
        {
            BtnReduce.Visibility = Visibility.Visible;
            BtnExpand.Visibility = Visibility.Visible;
            TabCtrlResults.Visibility = Visibility.Visible;
            TabCtrlFiles.Visibility = Visibility.Visible;
        }
    }
}