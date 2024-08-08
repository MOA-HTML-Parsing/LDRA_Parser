
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
using System.Text.RegularExpressions;
using System.Diagnostics.Metrics;

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


        private void Import_Document(object sender, MouseButtonEventArgs e)
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
        private void Save_Click(object sender, RoutedEventArgs e)
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

        //private List<ViolationItem> ParseAndDisplayHtml(string htmlFilePath)
        //{
        //    var violations = new List<ViolationItem>();
        //    try
        //    {
        //        HtmlDocument htmlDoc = new HtmlDocument();
        //        htmlDoc.Load(htmlFilePath);

        //        // HTML 내용을 문자열로 읽어옵니다.
        //        string htmlContent = File.ReadAllText(htmlFilePath);

        //        // 정규 표현식 패턴을 정의합니다.
        //        string pattern = @"<b>Violation Number</b> : (\d+ - .+?) &nbsp;&nbsp;&nbsp; <b>Location</b>  : <a href = '(.+?)'";

        //        // 정규 표현식을 사용하여 데이터를 추출합니다.
        //        MatchCollection matches = Regex.Matches(htmlContent, pattern);

        //        foreach (Match match in matches)
        //        {
        //            string violationNumber = match.Groups[1].Value;
        //            string location = match.Groups[2].Value;
        //            violations.Add(new ViolationItem { ViolationNumber = violationNumber, Location = location });
        //        }

        //        // ListBox에 데이터를 바인딩합니다.

        //        //ParsedHtmlListBox.ItemsSource = violations;

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Failed to parse HTML: {ex.Message}");
        //    }
        //    Console.WriteLine(violations);
        //    return violations;
        //}

        private void Button_Click_Compare(object sender, RoutedEventArgs e)
        {
            var BeforeItems = BeforeList.ItemsSource as IEnumerable<BeforeItem>;
            var AfterItems = AfterList.ItemsSource as IEnumerable<AfterItem>;
            _viewModel.compareBeforeAfter(BeforeItems, AfterItems);
        }

        private void BeforeList_MouseDoubleClick(object sender, MouseButtonEventArgs e) // Before ListView의 아이템을 더블클릭할 경우 작동하는 이벤트
        {
            if (BeforeList.SelectedItem != null)
            {
                // 선택된 항목의 ListViewItem을 가져옴
                var selectedItem = BeforeList.ItemContainerGenerator.ContainerFromItem(BeforeList.SelectedItem) as ListViewItem;

                if (selectedItem != null) // 클릭한 아이템을 찾았다면 내부 코드 실행
                {
                    // ListViewItem의 템플릿에서 ParsedHtmlListBox를 찾음
                    var detailsTextBox = FindVisualChild<ListBox>(selectedItem, "ParsedHtmlListBox");
                    var item = selectedItem.Content as BeforeItem;

                    if (detailsTextBox != null) // 세부 내용이 담길 모듈을 찾았다면 내부 코드 실행
                    {
                        string baseDirectory = _viewModel.BaseDirectory;
                        string beforeDirectory = System.IO.Path.Combine(baseDirectory, "Before");
                        string absolutePath = System.IO.Path.Combine(beforeDirectory, item.HrefValue);

                        try
                        {
                            if (File.Exists(absolutePath)) // 경로에 파일이 존재한다면
                            {
                              
                                var highlightList = _viewModel.beforeHighlightComparedList(item); // 하이라이트를 적용할 리스트
                                foreach(var highlights in highlightList)
                                {
                                    Console.WriteLine(highlights.isDiff); // 콘솔에 하이라이트 로그 출력
                                }
                                detailsTextBox.ItemsSource = highlightList; // 아이템 세부 내용을 출력하는 ListBox에 내용 저장

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

                        // 리스트 박스의 가시성 설정
                        detailsTextBox.Visibility = detailsTextBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    }
                }
            }
        }

        private void AfterList_MouseDoubleClick(object sender, MouseButtonEventArgs e) // After ListView의 더블클릭 이벤트, 구조는 BeforeList_MouseDoubleClick 함수와 동일
        {
            if (AfterList.SelectedItem != null)
            {
                // 선택된 항목의 ListViewItem을 가져옴
                var selectedItem = AfterList.ItemContainerGenerator.ContainerFromItem(AfterList.SelectedItem) as ListViewItem;

                if (selectedItem != null)
                {
                    // ListViewItem의 템플릿에서 ParsedHtmlListBox를 찾음
                    var detailsTextBox = FindVisualChild<ListBox>(selectedItem, "ParsedHtmlListBox");
                    var item = selectedItem.Content as AfterItem;

                    if (detailsTextBox != null)
                    {
                        string baseDirectory = _viewModel.BaseDirectory;
                        string afterDirectory = System.IO.Path.Combine(baseDirectory, "After");
                        string absolutePath = System.IO.Path.Combine(afterDirectory, item.HrefValue);

                        try
                        {
                            if (File.Exists(absolutePath))
                            {
                                //detailsTextBox.ItemsSource = _viewModel.popupHTMLPasing(absolutePath);
                                var highlightList = _viewModel.afterHighlightComparedList(item); // 하이라이트를 적용할 리스트
                                foreach (var highlights in highlightList)
                                {
                                    Console.WriteLine(highlights.isDiff);
                                }
                                detailsTextBox.ItemsSource = highlightList;
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

                        // 리스트 박스의 가시성 상태를 토글
                        detailsTextBox.Visibility = detailsTextBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    }
                }
            }
        }

        // 특정 부모의 자식 요소를 찾기 위한 메서드
        private T FindVisualChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // 현재 부모 요소의 모든 자식 요소를 순회
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) // qnahdml wk
            {
                // 현재 인덱스의 자식 요소 가져오기
                var child = VisualTreeHelper.GetChild(parent, i);
                // 자식 요소가 원하는 타입이고 지정된 이름을 가진 경우
                if (child is T tChild && ((FrameworkElement)child).Name == childName)
                {
                    // 조건을 모두 만족하면 이 자식 요소를 반환
                    return tChild;
                }
                // 재귀적으로 이 메서드를 호출하여 하위 트리에서도 검색
                var result = FindVisualChild<T>(child, childName);
                if (result != null)
                {
                    // 원하는 요소를 찾으면 결과 반환
                    return result;
                }
            }
            // 원하는 요소를 찾지 못한 경우 null 반환
            return null;

        }
    }
}


 