using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;
using System.Xml.Serialization;

namespace PictureStream.Server
{
    public class VM : INotifyPropertyChanged
    {
        public static ObservableCollection<string> folders = new ObservableCollection<string>();

        public ObservableCollection<string> Folders
        {
            get
            {
                return folders;
            }
        }

        public string SelectedFolder
        {
            get { return _SelectedFolder; }
            set
            {
                if (_SelectedFolder != value)
                {
                    _SelectedFolder = value;
                    OnPropertyChanged(SelectedFolderPropertyName);
                }
            }
        }
        private string _SelectedFolder;
        public const string SelectedFolderPropertyName = "SelectedFolder";

        internal void AddFolder(string folderPath)
        {
            this.Folders.Add(folderPath);
        }

        internal void RemoveFolder()
        {
            if (!string.IsNullOrEmpty(this.SelectedFolder) &&
                this.Folders.Contains(this.SelectedFolder))
                this.Folders.Remove(this.SelectedFolder);
        }

        internal void Load()
        {
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                VM data = default(VM);

                if (myIsolatedStorage.FileExists("Settings.xml"))
                {
                    using (IsolatedStorageFileStream stream = myIsolatedStorage.OpenFile("Settings.xml", FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(VM));
                        data = (VM)serializer.Deserialize(stream);

                    }
                }
            }
        }

        internal void Save()
        {
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                var xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;

                using (IsolatedStorageFileStream stream = myIsolatedStorage.OpenFile("Settings.xml", FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(VM));
                    using (XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                    {
                        serializer.Serialize(xmlWriter, this);
                    }
                }
            }
        }

        #region INotifyPropertyChanged

        internal void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

}
