using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace toolbox.Model
{
    internal class Comparison
    {
        public string FileNameOne { get; set; }
        public string FileNameTwo { get; set; }

        public List<Difference> Differences { get; set; }

        public string Logs { get; set; }

        public int Count { get; set; }

        public Grid GridResult { get; set; }

        public Comparison(string fileNameOne, string fileNameTwo)
        {
            this.FileNameOne = fileNameOne;
            this.FileNameTwo = fileNameTwo;
            Differences = new List<Difference>();
            Logs = string.Empty;
            Count = 0;
            GridResult = new Grid();
        }

        public void FillLogs(Difference difference)
        {

            // Log in terminal the path and the differences

            var path = difference.Path;
            var dataOneStg = difference.ValueOne;
            var dataTwoStg = difference.ValueTwo;

            // Part path
            Logs += "Différence found in " + path + ": \n";

            var sizeNameOne = FileNameOne.Length;
            var sizeNameTwo = FileNameTwo.Length;
            var sizeDataOne = dataOneStg.Length;
            var sizeDataTwo = dataTwoStg.Length;

            // Part names
            Logs += "| " + FileNameOne;
            if (sizeDataOne > sizeNameOne)
            {
                for (var i = 0; i < (sizeDataOne - sizeNameOne); i++)
                {
                    Logs += " ";
                }
            }
            Logs += " | " + FileNameTwo;
            if (sizeDataTwo > sizeNameTwo)
            {
                for (var i = 0; i < (sizeDataTwo - sizeNameTwo); i++)
                {
                    Logs += " ";
                }
            }
            Logs += " |\n";

            // Part data
            Logs += "| " + dataOneStg;
            if (sizeNameOne > sizeDataOne)
            {
                for (var i = 0; i < (sizeNameOne - sizeDataOne); i++)
                {
                    Logs += " ";
                }
            }
            Logs += " | " + dataTwoStg;
            if (sizeNameTwo > sizeDataTwo)
            {
                for (var i = 0; i < (sizeNameTwo - sizeDataTwo); i++)
                {
                    Logs += " ";
                }
            }
            Logs += " |\n";
        }

        public void GenerateGridResult(string fileOneHeader, string fileTwoHeader)
        {
            // Create a new grid for the result
            GridResult = new Grid
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
                        Text = "Number of differences: " + Count,
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
            Grid.SetColumn(GridResult.Children[0], 0);
            Grid.SetRow(GridResult.Children[0], 0);

            // -- Set position of the column titles
            Grid.SetColumn(GridResult.Children[1], 0);
            Grid.SetRow(GridResult.Children[1], 1);
            Grid.SetColumn(GridResult.Children[2], 1);
            Grid.SetRow(GridResult.Children[2], 1);
            Grid.SetColumn(GridResult.Children[3], 2);
            Grid.SetRow(GridResult.Children[3], 1);

        }

        public void AddRowResultGrid(Difference difference)
        {
            // Increment the counter of differences
            Count++;
            Differences.Add(difference);

            // Log the differences
            FillLogs(difference);

            // Create a new row in the result grid
            GridResult.RowDefinitions.Add(new RowDefinition());

            // Get the row index
            var rowIndex = GridResult.RowDefinitions.Count;

            // Add a background color to the row
            var backgroundColor = rowIndex % 2 == 0 ? System.Windows.Media.Brushes.LightGray : System.Windows.Media.Brushes.LightSlateGray;
            var backgroundRectangle = new System.Windows.Shapes.Rectangle { Fill = backgroundColor };
            Grid.SetRow(backgroundRectangle, rowIndex);
            Grid.SetColumnSpan(backgroundRectangle, 3);
            GridResult.Children.Add(backgroundRectangle);

            // Create a new text block for each column
            var textBlockPath = new TextBlock
            {
                Text = difference.Path,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var textBlockDataOne = new TextBlock
            {
                Text = difference.ValueOne,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var textBlockDataTwo = new TextBlock
            {
                Text = difference.ValueTwo,
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
            GridResult.Children.Add(textBlockPath);
            GridResult.Children.Add(textBlockDataOne);
            GridResult.Children.Add(textBlockDataTwo);
        }

        public void ReplaceOrCreateTabItem(TabControl TabCtrlResults)
        {
            // Verify if result already exists
            var isExist = false;
            foreach (TabItem tabItem in TabCtrlResults.Items)
            {
                if (tabItem.Header.ToString() == "Logs") continue;
                if (tabItem.Header is not StackPanel headerPanel) continue;
                if (headerPanel.Children.Count < 2) continue;
                if (headerPanel.Children[0] is not TextBlock textBlock) continue;
                if (textBlock.Text != FileNameOne + " vs " + FileNameTwo) continue;
                isExist = true;
                var sv = tabItem.Content as ScrollViewer;
                sv!.Content = GridResult;
            }

            // If not exist => Create a new tab for the result
            if (!isExist)
            {
                var resultTab = new TabItem
                {
                    Content = new ScrollViewer
                    {
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Content = GridResult
                    }
                };

                // Create title for the tab
                var title = new TextBlock
                {
                    Text = FileNameOne + " vs " + FileNameTwo
                };

                // Create a close button for the tab
                var closeButton = new Button
                {
                    Content = "X",
                    Width = 20,
                    Height = 20,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                closeButton.Click += (_, _) =>
                {
                    TabCtrlResults.Items.Remove(resultTab);
                };

                // Create a stack panel to hold the close button and the tab header
                var headerPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                // Add the close button and the tab header to the stack panel
                headerPanel.Children.Add(title);
                headerPanel.Children.Add(closeButton);

                // Add headerPanel to the tab item
                resultTab.Header = headerPanel;

                // Add the new tab to the TabCtrlResults
                TabCtrlResults.Items.Add(resultTab);
            }
        }
    }
}
