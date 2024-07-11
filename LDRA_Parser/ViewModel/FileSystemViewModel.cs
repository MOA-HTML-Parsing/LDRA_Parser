using System.Collections.ObjectModel;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LDRA_Parser.Model;
using System.ComponentModel;
using System.Printing.IndexedProperties;
using System.Windows.Threading;
using System.Windows;

namespace LDRA_Parser.ViewModel
{
    public class FileSystemViewModel : INotifyPropertyChanged
    {
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

        public FileSystemViewModel()
        {
            Items = new ObservableCollection<FileSystemItem>();
        }

        public void LoadDrives()
        {
            Items.Clear();
            foreach (var drive in DriveInfo.GetDrives())
            {
                var driveItem = new FileSystemItem
                {
                    Name = drive.Name,
                    FullPath = drive.Name,
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
                string logFilePath = "error_log.txt";
                string logMessage = $"Error loading children for {item.FullPath}: {ex.Message}\n";
                File.AppendAllText(logFilePath, logMessage);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}