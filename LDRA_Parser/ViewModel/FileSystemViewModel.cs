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
        
        public ParsedHtmlListModelI parsedHLM { get; private set; }

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
            parsedHLM = new ParsedHtmlListModelI(); 

            // 추출된 데이터를 저장할 리스트를 생성합니다.
            beforeViolations = new List<ViolationItem>();
            afterViolations = new List<ViolationItem>();
        }


        public void LoadDrives()
        {
            Items.Clear();
            CommonOpenFileDialog cofd = new CommonOpenFileDialog();
            cofd.IsFolderPicker = true;
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

        private async void LoadChildren(FileSystemItem item)
        {
            try
            {
                item.Children.Clear(); // 기존 자식 항목을 지웁니다.

                // 비동기적으로 폴더 내용을 로드합니다.
                await Task.Run(() =>
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

                        // 하위 폴더와 파일을 로드합니다.
                        LoadChildren(dirItem);

                        // UI 스레드에서 컬렉션에 추가합니다.
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            item.Children.Add(dirItem);
                        });
                    }

                    var files = Directory.GetFiles(item.FullPath);
                    foreach (var file in files)
                    {
                        var fileItem = new FileSystemItem
                        {
                            Name = Path.GetFileName(file),
                            FullPath = file,
                            IsDirectory = false
                        };

                        // UI 스레드에서 컬렉션에 추가합니다.
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            item.Children.Add(fileItem);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                // 예외 로그를 파일에 기록
                string logMessage = $"Error loading children for {item.FullPath}: {ex.Message}\n";
                LogToFile(logMessage);
            }
        }

        private void LogToFile(string message)
        {
            string logFilePath = "error_log.txt";
            bool isLogged = false;
            int retryCount = 3;
            int retryDelay = 1000; // 1 second

            for (int i = 0; i < retryCount && !isLogged; i++)
            {
                try
                {
                    using (FileStream fileStream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            writer.WriteLine($"{DateTime.Now}: {message}");
                        }
                    }
                    isLogged = true;
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Attempt {i + 1} - Failed to write to log file: {ex.Message}");
                    System.Threading.Thread.Sleep(retryDelay);
                }
            }

            if (!isLogged)
            {
                Console.WriteLine("Failed to write to log file after multiple attempts.");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /*
         * 문서 비교 로직
         */
        //Console.WriteLine(beforeItems.Equals(afterItem));
        public void compareBeforeAfter(IEnumerable<BeforeItem>? beforeItems, IEnumerable<AfterItem>? afterItems)
        {
            List<BeforeItem> beforeit = new List<BeforeItem>();
            List<AfterItem> afterit = new List<AfterItem>();


            foreach (var beforeItem in beforeItems)
            {
                int count = 0;
                foreach (var afterItem in afterItems)
                {
                    count++;
                    if (beforeItem.LDRA_Code == afterItem.LDRA_Code) // LDRA_CODE가 같으면 내부까지 확인
                    {
                        //////////
                        //Console.WriteLine("------------");
                        //Console.WriteLine(beforeItem.LDRA_Code);
                        //Console.WriteLine(beforeItem.LDRA_Code);
                        //Console.WriteLine("------------");
                        //////////
                        
                        popupHTMLPasing(beforeItem.HrefValue, afterItem.HrefValue);

                        while (true)
                        {
                            List<ViolationItem> beforeToRemove = new List<ViolationItem>();
                            List<ViolationItem> afterToRemove = new List<ViolationItem>();
                            foreach (var beforeViolationItem in beforeViolations)   // 내부까지 확인하는 이중 for문
                            {                                                       // before에만 있고 after에는 없는거
                                flag = 0;
                                foreach (var afterViolationItem in afterViolations)
                                {
                                    if (beforeViolationItem.IsSame(afterViolationItem))
                                    {
                                        flag = 1;
                                        beforeToRemove.Add(beforeViolationItem);
                                        afterToRemove.Add(afterViolationItem);
                                        break;
                                    }

                                }
                                if (flag != 1)
                                {
                                    beforeItem.violationItems.Add(beforeViolationItem);
                                    beforeToRemove.Add(beforeViolationItem);
                                }
                                else
                                {
                                    break;
                                }
                                //////////
                                //Console.WriteLine("첫번째 for문 내부------------");
                                //Console.WriteLine(beforeToRemove.Count);
                                //Console.WriteLine(afterToRemove.Count);
                                //Console.WriteLine("------------");
                                //////////
                            }
                            if (beforeToRemove.Count > 0)
                            {
                                foreach (var item in beforeToRemove)
                                {
                                    //Console.WriteLine("before 삭제가 두번되야됨------------");
                                    //Console.WriteLine("after VIolations : " + item.Location + " , " + item.ViolationNumber + " : " + item.idNumber);
                                    beforeViolations.Remove(item);
                                }
                                if (afterToRemove.Count > 0)
                                {
                                    foreach (var item2 in afterToRemove)
                                    {
                                        //Console.WriteLine("after 삭제가 두번되야됨------------");
                                        //Console.WriteLine("after VIolations : " + item2.Location + " , " + item2.ViolationNumber + " : " + item2.idNumber);
                                        afterViolations.Remove(item2);
                                        //Console.WriteLine("afterViolation size " + afterViolations.Count);
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        ////Console.WriteLine("after Violations가 있으면 안됨 " + afterViolations.Count);
                        //if(afterViolations.Count>0)
                        //Console.WriteLine("after VIolations : " + afterViolations.First().Location +" , "+ afterViolations.First().ViolationNumber + " : "+ afterViolations.First().idNumber);
                        foreach (var afterViolationItem in afterViolations)  // after에만 있고 before에는 없는거
                        {
                            afterItem.violationItems.Add(afterViolationItem);
                        }


                        ////////
                        Console.WriteLine("------------");
                        Console.WriteLine(beforeItem.violationItems.Count);
                        Console.WriteLine(afterItem.LDRA_Code); 
                        Console.WriteLine(afterItem.violationItems.Count);
                        Console.WriteLine("------------");
                        ////////
                        if (beforeItem.violationItems.Count != 0 || afterItem.violationItems.Count != 0)
                        {
                            
                            beforeit.Add(beforeItem);
                            afterit.Add(afterItem);
                        }
                        break; // 같은놈 있으니깐 for문 더 돌 필요 없다.
                    }
                    else
                    {
                        if (count == afterItems.Count())
                        {
                            Console.WriteLine("else-------------------------------");
                            popupHTMLPasing(beforeItem.HrefValue, afterItem.HrefValue); //  여기서는 beforeItem만 쓴다, 함수인자 After도 필요해서 그냥 같이넣은거
                            foreach (var beforeViolationItem in beforeViolations)   // 내부까지 확인하는 이중 for문
                            {
                                beforeItem.violationItems.Add(beforeViolationItem);
                            }
                            beforeit.Add(beforeItem);
                            afterit.Add(null); // 칸 맞춰줄려고
                            //마지막까지 왔는데 같은 놈 없으면 바로 리스트에 집어넣는다.
                        }
                    }
                }
            }
            //------------------------------------------
            //after기준에서 한번더 check
            foreach (var afterItem in afterItems)
            {
                int count = 0;
                foreach (var beforeItem in beforeItems)
                {
                    count++;
                    if (beforeItem.LDRA_Code == afterItem.LDRA_Code) // LDRA_CODE가 같으면 내부까지 확인
                    {
                        //이미 before 검사할때 넣었으므로 이부분에서 걸리면 바로 continue
                        break; // 같은놈 있으니깐 for문 더 돌 필요 없다.
                    }
                    if (count == beforeItems.Count())
                    {
                        Console.WriteLine("else after-------------------------------");
                        popupHTMLPasing(beforeItem.HrefValue, afterItem.HrefValue);
                        foreach (var afterViolationItem in afterViolations)   // 내부까지 확인하는 이중 for문
                        {
                            afterItem.violationItems.Add(afterViolationItem);
                        }
                        afterit.Add(afterItem);
                        beforeit.Add(null); // 칸 맞춰줄려고
                        //마지막까지 왔는데 같은 놈 없으면 바로 리스트에 집어넣는다.
                    }
                }
            }

            BeforeVM.updateBeforeList(beforeit);
            AfterVM.updateAfterList(afterit);

            parsedHLM.updateParsedHtmlList(beforeViolations);  //여기로 오류띄우기 연결
            parsedHLM.updateParsedHtmlList(afterViolations);  // 여기로 오류띄우기 연결

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

    }
}