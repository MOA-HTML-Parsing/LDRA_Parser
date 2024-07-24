
﻿using LDRA_Parser.ViewModel;
﻿using Microsoft.Win32;
using System;
using System.IO;
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
using HtmlAgilityPack;
using LDRA_Parser.Model;
using System.Xml;

namespace LDRA_Parser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileSystemViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new FileSystemViewModel();

            _viewModel = DataContext as FileSystemViewModel;
        }

        private void OnLoadFilesClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.LoadDrives();
            }
            else
            {
                MessageBox.Show("ViewModel is not set.");
            }

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void TextBlock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.DataContext is FileSystemItem item)
            {
                Console.WriteLine("bbbbbbb");
                _viewModel.htmlView(item);
            }
            else
            {
                MessageBox.Show("ViewModel is not set.");
            }
        }

        /*
         * 저장하기
         */
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var items = BeforeList.ItemsSource as IEnumerable<BeforeItem>;
            if (items != null)
            {
                var htmlContent = GenerateHtml(items);
                SaveHtmlToFile(htmlContent);
            }

        }

        /*
        public string Number_of_Violations { get; set; }    
        public string LDRA_Code { get; set; }
        public string Rule_Standards { get; set; }
        public string MISRA_Code { get; set; }

         */

        private string GenerateHtml(IEnumerable<BeforeItem> items)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<head><title>ListView Data</title></head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<table border='1'>");
            sb.AppendLine("<tr><th>Number_of_Violations</th><th>LDRA_Code</th><th>Rule_Standards</th><th>MISRA_Code</th></tr>");

            foreach (var item in items)
            {
                sb.AppendLine($"<tr><td>{item.Number_of_Violations}</td><td>{item.LDRA_Code}</td><td>{item.Rule_Standards}</td><td>{item.MISRA_Code}</td></tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private void SaveHtmlToFile(string htmlContent)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "ListViewData",
                DefaultExt = ".html",
                Filter = "HTML files (.html)|*.html"
            };

            var result = dialog.ShowDialog();

            if (result == true)
            {
                var filename = dialog.FileName;
                File.WriteAllText(filename, htmlContent);
                MessageBox.Show("Data saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listViewItem = sender as ListViewItem;
            if (listViewItem != null)
            {
                var item = listViewItem.Content as BeforeItem;
                if (item != null && !string.IsNullOrEmpty(item.HrefValue))
                {
                    string baseDirectory = _viewModel.BaseDirectory;
                    string beforeDirectory = System.IO.Path.Combine(baseDirectory, "Before");
                    Console.WriteLine(baseDirectory);
                    string absolutePath = System.IO.Path.Combine(beforeDirectory, item.HrefValue);

                    try
                    {
                        if (File.Exists(absolutePath))
                        {
                            ParseAndDisplayHtml(absolutePath);
                        }
                        else if (Uri.IsWellFormedUriString(item.HrefValue, UriKind.Absolute))
                        {
                            System.Diagnostics.Process.Start(item.HrefValue);
                        }
                        else
                        {
                            MessageBox.Show("File does not exist or invalid URL.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to open link: {ex.Message}");
                    }
                }
            }
        }

        private void ParseAndDisplayHtml(string htmlFilePath)
        {
            try
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.Load(htmlFilePath);

                StringBuilder sb = new StringBuilder();

                // Extract all <b> tags and their related text and links
                var violationNodes = htmlDoc.DocumentNode.SelectNodes("//b[normalize-space(text())='Violation Number']");
                var locationNodes = htmlDoc.DocumentNode.SelectNodes("//b[normalize-space(text())='Location']");

                if (violationNodes != null)
                {
                    foreach (var violationNode in violationNodes)
                    {
                        // Extract the violation number
                        var violationNumber = violationNode.NextSibling.InnerText.Trim();
                        sb.AppendLine($"Violation Number: {violationNumber}");
                    }
                }

                if (locationNodes != null)
                {
                    foreach (var locationNode in locationNodes)
                    {
                        // Extract the location text and links
                        var locationText = locationNode.ParentNode.InnerHtml;
                        sb.AppendLine($"Location: {locationText}");
                    }
                }

                // Set the extracted text to the TextBox
                ParsedHtmlTextBox.Text = sb.ToString();
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Failed to parse HTML: {ex.Message}");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
          
            var BeforeItems = BeforeList.ItemsSource as IEnumerable<BeforeItem>;
            var AfterItems = AfterList.ItemsSource as IEnumerable<AfterItem>;

            _viewModel.compareBeforeAfter(BeforeItems,AfterItems);
        }

        private void BeforeList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 확인: 선택된 항목이 있는지 여부
            if (BeforeList.SelectedItem != null)
            {
                // 선택된 항목의 ListViewItem을 가져옴
                var selectedItem = BeforeList.ItemContainerGenerator.ContainerFromItem(BeforeList.SelectedItem) as ListViewItem;

                if (selectedItem != null)
                {
                    // ListViewItem의 템플릿에서 DetailsTextBox를 찾음
                    var detailsTextBox = FindVisualChild<TextBox>(selectedItem, "DetailsTextBox");

                    if (detailsTextBox != null)
                    {
                        // 텍스트 박스의 현재 가시성 상태를 토글
                        if (detailsTextBox.Visibility == Visibility.Visible)
                        {
                            detailsTextBox.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            detailsTextBox.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        private void AfterList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 확인: 선택된 항목이 있는지 여부
            if (AfterList.SelectedItem != null)
            {
                // 선택된 항목의 ListViewItem을 가져옴
                var selectedItem = AfterList.ItemContainerGenerator.ContainerFromItem(AfterList.SelectedItem) as ListViewItem;

                if (selectedItem != null)
                {
                    // ListViewItem의 템플릿에서 DetailsTextBox를 찾음
                    var detailsTextBox = FindVisualChild<TextBox>(selectedItem, "DetailsTextBox");

                    if (detailsTextBox != null)
                    {
                        // 텍스트 박스의 현재 가시성 상태를 토글
                        if (detailsTextBox.Visibility == Visibility.Visible)
                        {
                            detailsTextBox.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            detailsTextBox.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        // 특정 자식 요소를 찾기 위한 헬퍼 메서드
        private T FindVisualChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild && ((FrameworkElement)child).Name == childName)
                {
                    return tChild;
                }
                var result = FindVisualChild<T>(child, childName);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

    }
}