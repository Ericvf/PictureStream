using PictureStream.App.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PictureStream.App
{
    public class DirectoryResult
    {
        public List<MyDirectory> Directories { get; set; }
        public List<MyFile> Files { get; set; }
    }

    public class MyDirectory
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Thumbnail
        {
            get
            {
                return App.ManageVM.SelectedServer.ServerAddress + this.Path + "/?thumbnail=true";
            }
        }
    }

    public class MyFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Thumbnail
        {
            get
            {
                return this.ImageUri + "?thumbnail=true";
            }
        }
        public string ImageUri
        {
            get
            {
                return App.ManageVM.SelectedServer.ServerAddress + this.Path;
            }
        }
    }

    public class ViewerParams
    {
        public List<MyFile> Files { get; set; }
        public int Index { get; set; }
    }

    public static class XmlSerializationHelper
    {
        public static T Deserialize<T>(string xmlContents)
        {
            // Create a serializer
            using (StringReader s = new StringReader(xmlContents))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(s);
            }
        }

        public static string Serialize<T>(T value)
        {
            if (value == null)
                return null;

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
            settings.Indent = false;
            settings.OmitXmlDeclaration = false;

            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, value);
                }
                return textWriter.ToString();
            }
        }
    }

    public class MainVM : INotifyPropertyChanged
    {
        #region Properties

        public bool IsEmpty
        {
            get { return _IsEmpty; }
            set
            {
                if (_IsEmpty != value)
                {
                    _IsEmpty = value;
                    OnPropertyChanged(IsEmptyPropertyName);
                }
            }
        }
        private bool _IsEmpty;
        public const string IsEmptyPropertyName = "IsEmpty";

        public string DirectoryPath
        {
            get { return _DirectoryPath; }
            set
            {
                if (_DirectoryPath != value)
                {
                    _DirectoryPath = value;
                    OnPropertyChanged(DirectoryPathPropertyName);
                }
            }
        }
        private string _DirectoryPath;
        public const string DirectoryPathPropertyName = "DirectoryPath";

        public string DirectoryName
        {
            get { return _DirectoryName; }
            set
            {
                if (_DirectoryName != value)
                {
                    _DirectoryName = value;
                    OnPropertyChanged(DirectoryNamePropertyName);
                }
            }
        }
        private string _DirectoryName;
        public const string DirectoryNamePropertyName = "DirectoryName";

        public bool IsLoading
        {
            get { return _IsLoading; }
            set
            {
                if (_IsLoading != value)
                {
                    _IsLoading = value;
                    OnPropertyChanged(IsLoadingPropertyName);
                }
            }
        }
        private bool _IsLoading;
        public const string IsLoadingPropertyName = "IsLoading";

        public ObservableCollection<MyDirectory> Directories
        {
            get { return _Directories; }
            set
            {
                if (_Directories != value)
                {
                    _Directories = value;
                    OnPropertyChanged(DirectoriesPropertyName);
                }
            }
        }
        private ObservableCollection<MyDirectory> _Directories;
        public const string DirectoriesPropertyName = "Directories";

        public ObservableCollection<MyFile> Files
        {
            get { return _Files; }
            set
            {
                if (_Files != value)
                {
                    _Files = value;
                    OnPropertyChanged(FilesPropertyName);
                }
            }
        }
        private ObservableCollection<MyFile> _Files;
        public const string FilesPropertyName = "Files";
        #endregion

        internal async Task Load(string dirName)
        {
            if (string.IsNullOrEmpty(dirName))
                dirName = "/";

            this.DirectoryName = dirName;
            await this.Refresh();
        }

        internal async Task GetDirectories(string directory = @"/")
        {
            if (App.ManageVM.SelectedServer == null)
                return;

            this.DirectoryPath = directory =="/" ? App.ManageVM.SelectedServer.ServerName : directory;

            using (var http = new HttpClient())
            {
                HttpResponseMessage response = await http.GetAsync(App.ManageVM.SelectedServer.ServerAddress + directory);
                var stringResult = await response.Content.ReadAsStringAsync();
                var result = XmlSerializationHelper.Deserialize<DirectoryResult>(stringResult);

                this.Directories = new ObservableCollection<MyDirectory>(result.Directories);
                this.Files = new ObservableCollection<MyFile>(result.Files);
            }

            this.IsEmpty = this.Directories.Count + this.Files.Count == 0;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        internal async Task Refresh()
        {
            this.IsLoading = true;
            await Task.Delay(1000);
            await this.GetDirectories(this.DirectoryName);
            this.IsLoading = false;
        }
    }

    public sealed partial class MainPage : PictureStream.App.Common.LayoutAwarePage
    {
        MainVM vm = new MainVM();

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = vm;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await this.vm.Load((string)e.Parameter);
        }

        private void gv_DirectoryClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is MyDirectory)
            {
                var d = e.ClickedItem as MyDirectory;
                Frame.Navigate(typeof(MainPage), d.Path + "/");
            }
        }

        private void gv_FileClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is MyFile)
            {
                var viewerParams = new ViewerParams();
                viewerParams.Files = this.vm.Files.ToList();
                viewerParams.Index = viewerParams.Files.IndexOf(e.ClickedItem as MyFile);
                Frame.Navigate(typeof(Viewer), viewerParams);
            }
        }

        private async void appbarRefresh_Click(object sender, RoutedEventArgs e)
        {
           await this.vm.Refresh();
        }

        private void appbarManage_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Manager));
        }
    }
}
