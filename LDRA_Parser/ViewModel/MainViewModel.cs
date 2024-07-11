using HtmlAgilityPack;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;   
using System.Xml.Linq;

namespace LDRA_Parser.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        private string _htmlContent;
        public string HtmlContent
        {
            get { return _htmlContent; }
            set
            {
                _htmlContent = value;
                OnPropertyChanged();
            }
        }

        public ICommand UploadCommand { get; }

        public MainViewModel()
        {
            UploadCommand = new RelayCommand(UploadHtml);
        }

        private void UploadHtml()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string htmlContent = File.ReadAllText(filePath);
                ParseHtml(htmlContent);
            }
        }

        private void ParseHtml(string htmlContent)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            var paragraphs = htmlDocument.DocumentNode.SelectNodes("//table");
            HtmlContent = "";

            if (paragraphs != null)
            {
                foreach (var paragraph in paragraphs)
                {
                    HtmlContent += paragraph.InnerText + Environment.NewLine;
                }
            }
            else
            {
                HtmlContent = "No <table> tags found.";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
