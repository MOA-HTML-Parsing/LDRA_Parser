
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
using System.Text.RegularExpressions;

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

                // HTML 내용을 문자열로 읽어옵니다.
                string htmlContent = File.ReadAllText(htmlFilePath);

                // 정규 표현식 패턴을 정의합니다.
                string pattern = @"<b>Violation Number</b> : (\d+ - .+?) &nbsp;&nbsp;&nbsp; <b>Location</b>  : <a href = '(.+?)'";

                // 정규 표현식을 사용하여 데이터를 추출합니다.
                MatchCollection matches = Regex.Matches(htmlContent, pattern);

                // 추출된 데이터를 저장할 리스트를 생성합니다.
                var violations = new List<ViolationItem>();
                foreach (Match match in matches)
                {
                    string violationNumber = match.Groups[1].Value;
                    string location = match.Groups[2].Value;
                    violations.Add(new ViolationItem { ViolationNumber = violationNumber, Location = location });
                }

                // ListBox에 데이터를 바인딩합니다.
                ParsedHtmlListBox.ItemsSource = violations;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to parse HTML: {ex.Message}");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // HTML 파일 경로
            string htmlFilePath = @"C:\Users\이승호\Desktop\LDRA Code Review Report\LDRA Code Review Report\Before\MMD_MUX_CWE_link_popup8D.htm";

            // HTML 내용을 문자열로 읽어옵니다.
            string htmlContent = File.ReadAllText(htmlFilePath);

            // 정규 표현식 패턴을 정의합니다.
            string pattern = @"<b>Violation Number</b> : (\d+ - .+?) &nbsp;&nbsp;&nbsp; <b>Location</b>  : <a href = '(.+?)'";

            // 정규 표현식을 사용하여 데이터를 추출합니다.
            MatchCollection matches = Regex.Matches(htmlContent, pattern);

            // 추출된 데이터를 저장할 리스트를 생성합니다.
            List<string> violations = new List<string>();

            // 추출된 각 매치를 처리합니다.
            foreach (Match match in matches)
            {
                string violationNumber = match.Groups[1].Value;
                string location = match.Groups[2].Value;
                string result = $"Violation Number : {violationNumber}     Location : {location}";
                violations.Add(result);
            }

            // 결과를 출력합니다.
            foreach (string violation in violations)
            {
                Console.WriteLine(violation);
            }

            //var BeforeItems = BeforeList.ItemsSource as IEnumerable<BeforeItem>;
            //var AfterItems = AfterList.ItemsSource as IEnumerable<AfterItem>
            //_viewModel.compareBeforeAfter(BeforeItems, AfterItems);
        }


    }

}
 