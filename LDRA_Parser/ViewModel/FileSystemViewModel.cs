using System.Collections.ObjectModel;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LDRA_Parser.Model;
using System.ComponentModel;

using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections;
using System.Text.RegularExpressions;



namespace LDRA_Parser.ViewModel
{
    public class FileSystemViewModel : INotifyPropertyChanged
    {
        private int flag;
        private FileSystemItem _selectedItem;
        private List<ViolationItem> beforeViolations;
        private List<ViolationItem> afterViolations;
        private List<BeforeItem> beforeit = new List<BeforeItem>();
        private List<AfterItem> afterit = new List<AfterItem>();

        public FileSystemItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        private string _htmlContent;
        public string HtmlContent
        {
            get { return _htmlContent; }
            set
            {
                _htmlContent = value;
                OnPropertyChanged(nameof(HtmlContent));
            }
        }
        private string _htmlContent2;
        public string HtmlContent2
        {
            get { return _htmlContent2; }
            set
            {
                _htmlContent2 = value;
                OnPropertyChanged(nameof(HtmlContent2));
            }
        }

        private ObservableCollection<FileSystemItem> _items;

        public ObservableCollection<FileSystemItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public BeforeViewModel BeforeVM { get; private set; }
        public AfterViewModel AfterVM { get; private set; }

        private string _baseDirectory;
        public string BaseDirectory
        {
            get { return _baseDirectory; }
            set
            {
                _baseDirectory = value;
                OnPropertyChanged(nameof(BaseDirectory));
            }
        }
        public FileSystemViewModel()
        {
            Items = new ObservableCollection<FileSystemItem>();
            SelectedItem = new FileSystemItem();
            BeforeVM = new BeforeViewModel();
            AfterVM = new AfterViewModel();

            // 추출된 데이터를 저장할 리스트를 생성합니다.
            beforeViolations = new List<ViolationItem>();
            afterViolations = new List<ViolationItem>();
        }


        public void LoadDrives()
        {
            Items.Clear();
            using (CommonOpenFileDialog cofd = new CommonOpenFileDialog { IsFolderPicker = true })
            {
                if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var driveItem = new FileSystemItem
                    {
                        Name = cofd.FileName,
                        FullPath = cofd.FileName,
                        IsDirectory = true
                    };

                    BaseDirectory = cofd.FileName;
                    LoadChildren(driveItem);
                    Items.Add(driveItem);
                }
            }

        }

        private async void LoadChildren(FileSystemItem item)
        {
            try
            {
                item.Children.Clear();
                await Task.Run(() =>
                {
                    LoadDirectories(item);
                    LoadFiles(item);
                });
            }
            catch (Exception ex)
            {
                LogToFile($"Error loading children for {item.FullPath}: {ex.Message}\n");
            }
        }

        private void LoadDirectories(FileSystemItem item)
        {
            var directories = Directory.GetDirectories(item.FullPath);
            foreach (var directory in directories)
            {
                var dirItem = new FileSystemItem
                {
                    Name = Path.GetFileName(directory),
                    FullPath = directory,
                    IsDirectory = true
                };
                LoadChildren(dirItem);
                Application.Current.Dispatcher.Invoke(() => item.Children.Add(dirItem));
            }
        }

        private void LoadFiles(FileSystemItem item)
        {
            var files = Directory.GetFiles(item.FullPath);
            foreach (var file in files)
            {
                var fileItem = new FileSystemItem
                {
                    Name = Path.GetFileName(file),
                    FullPath = file,
                    IsDirectory = false
                };
                Application.Current.Dispatcher.Invoke(() => item.Children.Add(fileItem));
            }
        }

        private void LogToFile(string message)
        {
            string logFilePath = "error_log.txt";
            const int retryCount = 3;
            const int retryDelay = 1000;

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    using (var writer = new StreamWriter(new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
                    {
                        writer.WriteLine($"{DateTime.Now}: {message}");
                    }
                    return;
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Attempt {i + 1} - Failed to write to log file: {ex.Message}");
                    System.Threading.Thread.Sleep(retryDelay);
                }
            }
            Console.WriteLine("Failed to write to log file after multiple attempts.");
        }

       

        public void htmlView(FileSystemItem item)
        {
            if (item != null && File.Exists(item.FullPath))
            {
                OpenHtmlFile(item.FullPath);
            }
        }

        private void OpenHtmlFile(string filePath)
        {
            // Logic to open the HTML file
            // For example, you could read the file content and bind it to a TextBox
            string fileContent = File.ReadAllText(filePath);

            if (filePath.Contains(@"\Before\"))
            {
                HtmlContent = fileContent;
                BeforeVM.LoadHtmlContent(filePath, _baseDirectory,"Before");
            }
            else if (filePath.Contains(@"\After\"))
            {
                HtmlContent2 = fileContent;
                AfterVM.LoadHtmlContent(filePath, _baseDirectory, "After");
            }
        }

     

        /*
         * 문서 비교 로직
         */
        //Console.WriteLine(beforeItems.Equals(afterItem));
        public void compareBeforeAfter(IEnumerable<BeforeItem>? beforeItems, IEnumerable<AfterItem>? afterItems)
        {


            beforeit = new List<BeforeItem>();
            afterit = new List<AfterItem>();
            // BeforeItem과 AfterItem을 비교
            foreach (var beforeItem in beforeItems)
            {
                bool matchFound = false;
                foreach (var afterItem in afterItems)
                {
                    if (beforeItem.LDRA_Code == afterItem.LDRA_Code)
                    {
                        ProcessMatchingItems(beforeItem, afterItem);
                        if (beforeItem.violationItems.Count > 0 || afterItem.violationItems.Count > 0)
                        {
                            beforeit.Add(beforeItem);
                            afterit.Add(afterItem);
                        }
                        matchFound = true;
                        break;
                    }
                }

                if (!matchFound)
                {
                    ProcessNonMatchingBeforeItem(beforeItem, afterit);
                    beforeit.Add(beforeItem);
                }
            }

            // AfterItem 기준으로도 비교
            foreach (var afterItem in afterItems)
            {
                if (beforeItems.All(beforeItem => beforeItem.LDRA_Code != afterItem.LDRA_Code))
                {
                    ProcessNonMatchingAfterItem(afterItem, beforeit);
                    afterit.Add(afterItem);
                }
            }

            BeforeVM.updateBeforeList(beforeit);
            AfterVM.updateAfterList(afterit);
        }



        private void ProcessMatchingItems(BeforeItem beforeItem, AfterItem afterItem)
        {
            popupHTMLPasing(beforeItem.HrefValue, afterItem.HrefValue);

            while (true)
            {
                var beforeToRemove = new List<ViolationItem>();
                var afterToRemove = new List<ViolationItem>();

                foreach (var beforeViolationItem in beforeViolations)
                {
                    bool isMatched = false;
                    foreach (var afterViolationItem in afterViolations)
                    {
                        if (beforeViolationItem.IsSame(afterViolationItem))
                        {
                            isMatched = true;
                            beforeToRemove.Add(beforeViolationItem);
                            afterToRemove.Add(afterViolationItem);
                            break;
                        }
                    }

                    if (!isMatched)
                    {
                        beforeItem.violationItems.Add(beforeViolationItem);
                        beforeToRemove.Add(beforeViolationItem);
                    }
                    else
                    {
                        break;
                    }
                }

                if (beforeToRemove.Count > 0)
                {
                    RemoveViolations(beforeToRemove, afterToRemove);
                }
                else
                {
                    break;
                }
            }

            foreach (var afterViolationItem in afterViolations)
            {
                afterItem.violationItems.Add(afterViolationItem);
            }
        }

        private void ProcessNonMatchingBeforeItem(BeforeItem beforeItem, List<AfterItem> afterit)
        {
            popupHTMLPasing(beforeItem.HrefValue, beforeItem.HrefValue);
            foreach (var beforeViolationItem in beforeViolations)
            {
                beforeItem.violationItems.Add(beforeViolationItem);
            }
            afterit.Add(null); // 칸 맞추기
        }

        private void ProcessNonMatchingAfterItem(AfterItem afterItem, List<BeforeItem> beforeit)
        {
            popupHTMLPasing(afterItem.HrefValue, afterItem.HrefValue);
            foreach (var afterViolationItem in afterViolations)
            {
                afterItem.violationItems.Add(afterViolationItem);
            }
            beforeit.Add(null); // 칸 맞추기
        }

        private void RemoveViolations(List<ViolationItem> beforeToRemove, List<ViolationItem> afterToRemove)
        {
            foreach (var item in beforeToRemove)
            {
                beforeViolations.Remove(item);
            }
            if (afterToRemove.Count > 0)
            {
                foreach (var item in afterToRemove)
                {
                    afterViolations.Remove(item);
                }
            }
        }

        public void popupHTMLPasing(string beforehtmlPath, string afterhtmlPath)
        {
            // HTML 내용을 문자열로 읽어옵니다.
            string beforeHtmlContent = File.ReadAllText(beforehtmlPath);
            string afterHtmlContent = File.ReadAllText(afterhtmlPath);

            // 정규 표현식 패턴을 정의합니다.
            string pattern = @"<b>Violation Number</b> : (\d+ - .+?) &nbsp;&nbsp;&nbsp; <b>Location</b>  : <a href = '(.+?)'.*?>(.+?)</a> - <a href=.*?>(\d+)</a>";
            // 정규 표현식을 사용하여 데이터를 추출합니다.
            MatchCollection beforeMatches = Regex.Matches(beforeHtmlContent, pattern);
            MatchCollection afterMatches = Regex.Matches(afterHtmlContent, pattern);


            beforeViolations.Clear();
            afterViolations.Clear();
            // 추출된 각 매치를 처리합니다.
            int id = 0; // 고유의 id번호 줄려고
            foreach (Match match in beforeMatches)
            {
                id++;
                string violationNumber = match.Groups[1].Value;
                string location = match.Groups[2].Value;
                string mainLocation = match.Groups[3].Value; // main
                string lineNumber = match.Groups[4].Value; // 6

                string result = $"Violation Number : {violationNumber}     Location : {location}";
                //Console.WriteLine("before-------------------------------");
                //Console.WriteLine(violationNumber);
                //Console.WriteLine(location);
                //Console.WriteLine(mainLocation);
                //Console.WriteLine(lineNumber);
                beforeViolations.Add(new ViolationItem { ViolationNumber = violationNumber, Location = location, MainLocation = mainLocation, LineNumber = lineNumber, idNumber = id });
            }
            id = 0;
            foreach (Match match in afterMatches)
            {
                id++;
                string violationNumber = match.Groups[1].Value;
                string location = match.Groups[2].Value;
                string mainLocation = match.Groups[3].Value; // main
                string lineNumber = match.Groups[4].Value; // 6

                string result = $"Violation Number : {violationNumber}     Location : {location}";
                //Console.WriteLine("after------------------------------");
                //Console.WriteLine(violationNumber);
                //Console.WriteLine(location);
                //Console.WriteLine(mainLocation);
                //Console.WriteLine(lineNumber);
                afterViolations.Add(new ViolationItem { ViolationNumber = violationNumber, Location = location, MainLocation = mainLocation, LineNumber = lineNumber, idNumber = id });
            }
           
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}