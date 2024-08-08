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
using LDRA_Parser.Parser;
//using System.Windows.Forms;



namespace LDRA_Parser.ViewModel
{
    public class FileSystemViewModel : INotifyPropertyChanged
    {
        private static int ViolationItemId = 0;

        private int flag;
        private HtmlParser htmlParser;
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
            htmlParser = new HtmlParser();

            // 추출된 데이터를 저장할 리스트를 생성합니다.
            beforeViolations = new List<ViolationItem>();
            afterViolations = new List<ViolationItem>();
        }


        /**
         * OnLoadFilesClicked 을 클릭했을때 실행되는 첫번째 함수
         * 드라이버를 불러온다.
         */
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

        /**
         * LoadDrives()로부터 호출된다.
         * 특정 드라이버 밑의 폴더구조를 전부 불러오기 위해 사용된다.
         */
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

        /**
        * LoadChildren()으로 부터 사용된다.
        * 디렉터리를 불러오기 위해 사용된다.
        */
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

        /**
        * LoadChildren()으로 부터 사용된다.
        * 파일을 불러오기 위해 사용된다.
        */
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

        /**
         * LoadChildren(FileSystemItem item) 가 호출될 때
         * 에러가 발생할 경우 처리해주기 위해 사용된다.(에러 예시 - 이미 파일이 열려 있을때 등)
         */
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


        public List<ViolationItem> popupHTMLPasing(string path)
        {
            return htmlParser.popupHTMLPasing(path);
        }

        /*
         * 문서 비교 로직
         */
        public void compareBeforeAfter(IEnumerable<BeforeItem>? beforeItems, IEnumerable<AfterItem>? afterItems)
        {
            beforeit = new List<BeforeItem>(); // before와 after가 서로 다르다고 판별나면 최종적으로 이 변수에 담아서 업데이트한다.
            afterit = new List<AfterItem>();
            // BeforeItem과 AfterItem을 비교
            foreach (var beforeItem in beforeItems) //beforeItem을 기준으로 AfterItem을 비교한다.
            {
                bool matchFound = false;
                foreach (var afterItem in afterItems)
                {
                    if (beforeItem.LDRA_Code == afterItem.LDRA_Code) // 1. 에러의 LDRA가 같은 경우에는 내부적으로 같은 에러가 발생헀는지 PopUp을 이용하여 한번 더 확인한다.
                    {
                        beforeViolations = htmlParser.popupHTMLPasing(beforeItem.HrefValue); //before쪽에 해당하는 에러의 popup을 파싱해온다.
                        afterViolations = htmlParser.popupHTMLPasing(afterItem.HrefValue); //after쪽에 해당하는 에러의 popup을 파싱해온다.

                        bool flag = false;

                        foreach (var beforeViolationItem in beforeViolations) // before와 after의 각각의 popup도 before를 기준으로 동일한지 확인한다.
                        {
                            if (!(beforeViolationItem.isDiff)) continue;       // 이미  동일한게 있었다고 판단되면 추가비교하지않고 넘어간다.
                            foreach (var afterViolationItem in afterViolations)
                            {
                                if (!(afterViolationItem.isDiff)) continue;
                                if (beforeViolationItem.IsSame(afterViolationItem))  // 서로 동일한지 비교
                                {
                                    beforeViolationItem.isDiff = false;
                                    afterViolationItem.isDiff = false;
                                    break;
                                }
                            }
                        }

                        foreach (var beforeViolationItem in beforeViolations) // isDiff가 true인 값이 있으면 flag를 true로 바꾼다
                        {                                                     // == before와 after가 popup까지 비교했을 때 차이가 있다는 것을 의미 
                            if (flag == true) break;
                            if (beforeViolationItem.isDiff == true) flag = true;
                        }

                        foreach (var afterViolationItem in afterViolations)
                        {
                            if (flag == true) break;
                            if (afterViolationItem.isDiff == true) flag = true;
                        }


                        if (flag) //flag가 true일 경우에만 beforeItem에 popup내용을 업데이트
                        {
                            foreach (var beforeViolationItem in beforeViolations)
                            {
                                beforeItem.violationItems.Add(beforeViolationItem);
                            }
                            foreach (var afterViolationItem in afterViolations)
                            {
                                afterItem.violationItems.Add(afterViolationItem);
                            }
                        }

                        if (beforeItem.violationItems.Count > 0 || afterItem.violationItems.Count > 0) // 업데이트 되었을 경우에만 변경사항을 반영시킨다.
                        { 
                            beforeit.Add(beforeItem);
                            afterit.Add(afterItem);
                        }
                        matchFound = true; // LDRA가 같으면서 Popup에서 차이가 발견되었다는 의미.
                        break;
                    }
                }

                if (!matchFound) // LDRA 부터 같은게 아예 발견되지 않았다는 의미 => before에만 존재하고 after에는 존재 X
                {
                    ProcessNonMatchingBeforeItem(beforeItem, afterit);
                    beforeit.Add(beforeItem);
                }
            }

            // AfterItem 기준으로도 비교
            foreach (var afterItem in afterItems) // After에만 있고 Before에는 아예 없는 에러를 발견한다.
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

        /**
         * before에만 존재하고 after에는 존재하지 않을때
         */
        private void ProcessNonMatchingBeforeItem(BeforeItem beforeItem, List<AfterItem> afterit)
        {
            beforeViolations = htmlParser.popupHTMLPasing(beforeItem.HrefValue);
            foreach (var beforeViolationItem in beforeViolations)
            {
                beforeItem.violationItems.Add(beforeViolationItem);
            }
            afterit.Add(null); // 칸 맞추기
        }

        /**
         *  after에만 존재하고 before에는 존재하지 않을때
         */
        private void ProcessNonMatchingAfterItem(AfterItem afterItem, List<BeforeItem> beforeit)
        {
            afterViolations = htmlParser.popupHTMLPasing(afterItem.HrefValue);
            foreach (var afterViolationItem in afterViolations)
            {
                afterItem.violationItems.Add(afterViolationItem);
            }
            beforeit.Add(null); // 칸 맞추기
        }

        public List<ViolationItem> beforeHighlightComparedList(BeforeItem beforeCompared)
        {
            Console.WriteLine("highlight function");
            if(beforeit.Count != 0)
            {
                Console.WriteLine("highlight test");
                foreach (BeforeItem beforeItem in beforeit)
                {
                    if(beforeItem == beforeCompared)
                    {
                        return beforeItem.violationItems;
                    }
                }

            }
            var parsingList = htmlParser.popupHTMLPasing(beforeCompared.HrefValue);
            foreach (ViolationItem violationItem in parsingList)
            {
                violationItem.isDiff = false;
            }
            return parsingList;
        }

        public List<ViolationItem> afterHighlightComparedList(AfterItem afterCompared)
        {
            Console.WriteLine("highlight function");
            if (afterit.Count != 0)
            {
                Console.WriteLine("highlight test");
                foreach (AfterItem afterItem in afterit)
                {
                    if (afterItem == afterCompared)
                    {
                        return afterItem.violationItems;
                    }
                }

            }
            var parsingList = htmlParser.popupHTMLPasing(afterCompared.HrefValue);
            foreach (ViolationItem violationItem in parsingList)
            {
                violationItem.isDiff = false;
            }
            return parsingList;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}