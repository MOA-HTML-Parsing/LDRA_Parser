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



namespace LDRA_Parser.ViewModel
{
    public class FileSystemViewModel : INotifyPropertyChanged
    {
        private FileSystemItem _selectedItem;

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

        public FileSystemViewModel()
        {
            Items = new ObservableCollection<FileSystemItem>();
            SelectedItem = new FileSystemItem();
            BeforeVM = new BeforeViewModel();
            AfterVM = new AfterViewModel();

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
                BeforeVM.LoadHtmlContent(filePath);
            }
            else if (filePath.Contains(@"\After\"))
            {
                HtmlContent2 = fileContent;
                AfterVM.LoadHtmlContent(filePath);
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
                Console.WriteLine(beforeItems.Count());
                int count = 0;
                foreach (var afterItem in afterItems)
                {
                    count++;
                    if (beforeItem.LDRA_Code == afterItem.LDRA_Code) // LDRA_CODE가 같으면 내부까지 확인
                    {
                        
                        beforeit.Add(beforeItem);
                        afterit.Add(afterItem); 
                        //같으면 내부 더 확인
                        //이 부분은 나중에



                        break; // 같은놈 있으니깐 for문 더 돌 필요 없다.
                    }
                    else
                    {
                        if (count == afterItems.Count())
                        {
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
                        afterit.Add(afterItem);
                        beforeit.Add(null); // 칸 맞춰줄려고
                        //마지막까지 왔는데 같은 놈 없으면 바로 리스트에 집어넣는다.
                    }
                }
            }

            BeforeVM.updateBeforeList(beforeit);
            AfterVM.updateAfterList(afterit);


        }

    }
}