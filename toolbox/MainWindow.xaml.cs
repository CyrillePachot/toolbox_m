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
                    var tabItem1 = TabCtrlFiles.Items[i] as TabItem;
                    var tabItem2 = TabCtrlFiles.Items[j] as TabItem;

                    if (tabItem1 == null || tabItem2 == null) continue;

                    var textBox1 = tabItem1.Content as TextBox;
                    var textBox2 = tabItem2.Content as TextBox;

                    if (textBox1 == null || textBox2 == null) continue;

                    try
                    {
                        var file1 = JObject.Parse(textBox1.Text);
                        var file2 = JObject.Parse(textBox2.Text);

                        var differences = GetJsonDifferences(tabItem1.Header.ToString(), tabItem2.Header.ToString(),file1, file2);
                        results.AppendLine("--------------------------------------------------------------------------------------");
                        results.AppendLine("Results between " + tabItem1.Header + " and " + tabItem2.Header + ":");
                        results.AppendLine(differences);
                        results.AppendLine();
                    }
                    catch (JsonException ex)
                    {
                        results.AppendLine($"Error during reading file {tabItem1.Header} or {tabItem2.Header} : {ex.Message}");
                    }
                }
            }
            TxtBxResult.Text = results.ToString();
        }
        
        private string GetJsonDifferences(string? tabItem1, string? tabItem2, JObject obj1, JObject obj2, StringBuilder? path = null, StringBuilder? diff = null)
        {
            diff ??= new StringBuilder();
            // Loop through the properties of the first object
            foreach (var property1 in obj1.Properties())
            {
                // Check if we have the same property in obj2
                if (!obj2.ContainsKey(property1.Name))
                {
                    diff.AppendLine($"Propriété manquante dans le deuxième fichier : '{property1.Name}'");
                    continue;
                }
                // Loop through the properties of the second object to compare them with the first
                foreach (var property2 in obj2.Properties())
                {
                    // Check if the property names are the same
                    if (property1.Name != property2.Name)
                    {
                        continue;
                    }
                    // Check if it is an array and loop through it
                    if (property1.Value.Type == JTokenType.Array && property2.Value.Type == JTokenType.Array)
                    {
                        var p1 = (JArray)property1.Value;
                        var p2 = (JArray)property2.Value;
                        var size1 = p1.Count;
                        var size2 = p2.Count;
                        JObject? item1;
                        JObject? item2;
                        
                        if (size1 == 0 || size2 == 0)continue;
                        if (size1 > size2)
                        {
                            diff.AppendLine($"Le tableau dans {tabItem1} est plus grand que le tableau dans {tabItem2}");
                            for (var i = 0; i < size1; i++)
                            {
                                if (p2.Count <= i)
                                {
                                    diff.AppendLine($"Le tableau dans {tabItem1} contient en plus: {p1[i]}");
                                    continue;
                                }
                                item1 = (JObject)p1[i];
                                if (p2.Contains(item1)) continue;
                                item2 = (JObject)p2[i];
                                GetJsonDifferences(
                                    tabItem1,
                                    tabItem2,
                                    item1, 
                                    item2, 
                                    path, diff);
                            }
                        }
                        else if (size1 < size2)
                        {
                            
                            diff.AppendLine($"Le tableau dans {tabItem2} est plus grand que le tableau dans {tabItem1}");
                            for (var i = 0; i < size2; i++)
                            {
                                if (p1.Count <= i)
                                {
                                    diff.AppendLine($"Le tableau dans {tabItem2} contient en plus: {p2[i]}");
                                    continue;
                                }
                                item1 = (JObject)p1[i];
                                if (p1.Contains(item1)) continue;
                                item2 = (JObject)p2[i];
                                GetJsonDifferences(
                                    tabItem1,
                                    tabItem2,
                                    item1, 
                                    item2, 
                                    path, diff);
                            }
                        }
                        else
                        {
                            for (var i = 0; i < size1; i++)
                            {
                                item1 = (JObject)p1[i];
                                if (p1.Contains(item1)) continue;
                                item2 = (JObject)p2[i];
                                GetJsonDifferences(
                                    tabItem1,
                                    tabItem2,
                                    item1, 
                                    item2, 
                                    path, diff);
                            }
                        }
                    
                    }
                    // Compare the values 
                    else if (!JToken.DeepEquals(property1.Value, property2.Value))
                    {
                        diff.AppendLine($"Différence trouvée sur la propriété '{property1.Name}':");
                        diff.AppendLine($" --> Valeur 1 : {property1.Value}");
                        diff.AppendLine($" --> Valeur 2 : {property2.Value}");
                    }
                }
            }
            // Check if we have any properties in obj2 that are not in obj1
            foreach (var property in obj2.Properties())
            {
                if (!obj1.ContainsKey(property.Name))
                {
                    diff.AppendLine($"Propriété supplémentaire dans le deuxième fichier : '{property.Name}'");
                }
            }
            // Check if we have any differences
            if (diff.Length == 0)
            {
                diff.AppendLine("Nothing found.");
            }
            
            return diff.ToString();
        }
        
        private void Replace_Click(object sender, RoutedEventArgs e)
        {
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
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