//using HtmlAgilityPack;
//using LDRA_Parser.Model;
//using Microsoft.Win32;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.IO;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Windows.Input;

//namespace LDRA_Parser.ViewModel
//{
//    class MainViewModel : INotifyPropertyChanged
//    {
//        private string _htmlContent;
//        public string HtmlContent
//        {
//            get { return _htmlContent; }
//            set
//            {
//                _htmlContent = value;
//                OnPropertyChanged();
//            }
//        }

//        public ICommand UploadBeforeCommand { get; }
//        public ICommand UploadAfterCommand { get; }
//        public BeforeViewModel BeforeVM { get; }

//        public AfterViewModel AfterVM { get; }

//        public MainViewModel()
//        {
//            UploadBeforeCommand = new RelayCommand(UploadBeforeHtml);
//            UploadAfterCommand = new RelayCommand(UploadAfterHtml);
//            BeforeVM = new BeforeViewModel();
//            AfterVM = new AfterViewModel();
//        }

//        private void UploadBeforeHtml()
//        {
//            OpenFileDialog openFileDialog = new OpenFileDialog
//            {
//                Filter = "HTML files (*.htm)|*.htm|All files (*.*)|*.*"
//            };
//            if (openFileDialog.ShowDialog() == true)
//            {
//                string filePath = openFileDialog.FileName;
//                BeforeVM.LoadHtmlContent(filePath);
//            }
//        }

//        private void UploadAfterHtml()
//        {
//            OpenFileDialog openFileDialog = new OpenFileDialog
//            {
//                Filter = "HTML files (*.htm)|*.htm|All files (*.*)|*.*"
//            };
//            if (openFileDialog.ShowDialog() == true)
//            {
//                string filePath = openFileDialog.FileName;
//                AfterVM.LoadHtmlContent(filePath);
//            }
//        }


//        public event PropertyChangedEventHandler PropertyChanged;

//        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }
//    }
//}