using HtmlAgilityPack;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

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
                Filter = "HTML files (*.htm)|*.htm|All files (*.*)|*.*"
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

            var tables = htmlDocument.DocumentNode.SelectNodes("//table");

            if (tables != null)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var table in tables)
                {
                    var thNodes = table.SelectNodes(".//tr/th");
                    if (thNodes != null && thNodes.Count == 4)
                    {
                        var rows = table.SelectNodes(".//tr");
                        if (rows != null)
                        {
                            foreach (var row in rows)
                            {
                                var cells = row.SelectNodes(".//td");
                                if (cells != null)
                                {
                                    foreach (var cell in cells)
                                    {
                                        sb.AppendLine($"Text: {cell.InnerText.Trim()}");
                                    }
                                    sb.AppendLine();
                                }
                            }
                        }
                    }
                }

                if (sb.Length > 0)
                {
                    HtmlContent = sb.ToString();
                }
                else
                {
                    HtmlContent = "No tables found with exactly 4 <th> elements.";
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