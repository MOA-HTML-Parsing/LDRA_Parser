using System.Collections.ObjectModel;

namespace LDRA_Parser.Model
{
    /**
     * 디렉터리 구조를 담기 위한 클래스.
     */
    public class FileSystemItem
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public ObservableCollection<FileSystemItem> Children { get; set; }

        public FileSystemItem()
        {
            Children = new ObservableCollection<FileSystemItem>();
        }

  
    }
}
