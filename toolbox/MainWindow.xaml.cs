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
        private ObservableCollection<Difference> Differences {  get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Differences = new ObservableCollection<Difference>
            {
                new Difference("Path", "Value One", "Value Two") // Test
            };


            this.DataContext = this; // Set the DataContext for data binding
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
            // Compare each element in both list & change the path ----- NEW VERSION
            foreach (var elementOne in jArrayOne)
            {
                var endPath = "";
                //var isComplexObject = false;
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
                                endPath = keyNameOne; // Use the key name as the end path
                                break;
                            case "Value":
                                keyValueOne = property.Value.ToString();
                                break;
                        }
                    }
                    
                    // Maybe if it's not found, we can call CompareNestedDictionary with the index
                    // Because it's not a key-value pair but a complex object
                    // => before call CompareNestedDictionary, we need to check if in list dataTwo it exists a similar object
                    /*if (keyNameOne == "" || keyValueOne == "")
                    {
                        // !!! TO DETERMINE
                        endPath = ""; // Use the index as the end path
                        isComplexObject = true; // Mark it as a complex object
                    }*/
                    
                    var existInBoth = false;
                    var keyValueTwo = "";
                    foreach (var elementTwo in jArrayTwo)
                    {
                        if (elementTwo is not JObject subObjectTwo) continue;
                        /*if (isComplexObject)
                        {
                            // TO DETERMINE WHAT TO DO
                            Console.WriteLine("Votre message ici");
                        }*/
                        
                        foreach (var property in subObjectTwo.Properties())
                        {
                            switch (property.Name)
                            {
                                case "OptionKey" or "Key" or "Name":
                                    var keyNameTwo = property.Value.ToString();
                                    if (keyNameTwo == keyNameOne) existInBoth = true; // If the key names are different, we can stop checking this element
                                    break;
                                case "Value":
                                    keyValueTwo = property.Value.ToString();
                                    break;
                            }
                        }
                        if (!existInBoth) continue; // We found a match, no need to continue to iterating through dataTwo
                        if (keyValueOne == keyValueTwo) break; // If the values are equal, we can stop checking this element
                        // Else we need to add a row in the result grid
                        var newPath = string.IsNullOrEmpty(path) ? endPath : $"{path}/{endPath}";
                        Difference difference = new Difference(newPath, keyValueTwo, keyValueOne);
                        currentComparison.AddRowResultGrid(difference);
                        Differences.Add(difference);
                        break;
                    }
                }
            }
            
            // If jArrayTwo is not empty, we need to check if there are elements in jArrayTwo that are not in jArrayOne
            foreach (var elementTwo in jArrayTwo)
            {
                var endPath = "";
                //var isComplexObject = false;
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
                                endPath = keyNameTwo; // Use the key name as the end path
                                break;
                            case "Value":
                                keyValueTwo = property.Value.ToString();
                                break;
                        }
                    }
                    
                    var existInBoth = false;
                    var keyValueOne = "";
                    foreach (var elementOne in jArrayOne)
                    {
                        if (elementOne is not JObject subObjectOne) continue;
                        
                        foreach (var property in subObjectOne.Properties())
                        {
                            switch (property.Name)
                            {
                                case "OptionKey" or "Key" or "Name":
                                    var keyNameOne = property.Value.ToString();
                                    if (keyNameTwo == keyNameOne) existInBoth = true; // If the key names are different, we can stop checking this element
                                    break;
                                case "Value":
                                    keyValueOne = property.Value.ToString();
                                    break;
                            }
                        }
                        if (existInBoth) break; // We found a match, no need to continue to iterating through dataTwo
                    }
                    if (existInBoth) break; // We found a match, no need to continue to iterating through dataTwo
                    var newPath = string.IsNullOrEmpty(path) ? endPath : $"{path}/{endPath}";
                    Difference difference = new Difference(newPath, keyValueTwo, keyValueOne);
                    currentComparison.AddRowResultGrid(difference);
                    Differences.Add(difference);
                    break;
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
                    Differences.Add(difference);
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