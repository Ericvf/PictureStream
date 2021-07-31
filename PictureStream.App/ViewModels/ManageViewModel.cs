using PictureStream.App.Framework;
using PictureStream.App.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace PictureStream.App.ViewModels
{
    public class ManageVM : BaseModel
    {
        private ObservableCollection<Server> servers = new ObservableCollection<Server>();
        public ObservableCollection<Server> Servers
        {
            get
            {
                return this.servers;
            }
            set
            {
                this.servers = value;
                this.OnPropertyChanged("Servers");
            }
        }

        public Server SelectedServer = null;

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

        public ManageVM()
        {
            this.servers.CollectionChanged += servers_CollectionChanged;
        }

        void servers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged("Servers");
        }

        private bool canDelete;
        public bool CanDelete
        {
            get
            {
                return this.canDelete;
            }
            set
            {
                this.canDelete = value;
                this.OnPropertyChanged("CanDelete");
            }
        }

        internal void DeleteServer(Server server)
        {
            if (this.Servers.Contains(server))
                this.Servers.Remove(server);
        }

        internal async Task AddServer(string serverName, string serverAddress)
        {
            var fixedAddress = serverAddress.TrimEnd('/') + ":3000";
            var ex = default(Exception);
            try
            {
                using (var http = new HttpClient())
                {
                    HttpResponseMessage response = await http.GetAsync(fixedAddress);
                    var result = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(result))
                        throw new Exception("Server not responding");
                }
            }
            catch (Exception e)
            {
                ex = e;
            }


            if (ex != null)
            {
                var dialog = new MessageDialog(ex.Message, "Error");
                await dialog.ShowAsync();
                return;
            }

            var server = new Server
            {
                ServerName = serverName,
                ServerAddress = fixedAddress,
                Thumbnail = fixedAddress + "?avatar=1"
            };

            this.Servers.Add(server);
        }

        internal async Task Save()
        {
            var file = await App.LocalStorageFolder.CreateFileAsync("Servers.xml", CreationCollisionOption.ReplaceExisting);

            IRandomAccessStream sessionRandomAccess = await file.OpenAsync(FileAccessMode.ReadWrite);
            IOutputStream sessionOutputStream = sessionRandomAccess.GetOutputStreamAt(0);
            var serializer = new XmlSerializer(typeof(ObservableCollection<Server>));

            serializer.Serialize(sessionOutputStream.AsStreamForWrite(), this.Servers);
            sessionRandomAccess.Dispose();
            await sessionOutputStream.FlushAsync();
            sessionOutputStream.Dispose();
        }

        internal async Task Load()
        {
            try
            {
                var file = await App.LocalStorageFolder.GetFileAsync("Servers.xml");

                IInputStream sessionInputStream = await file.OpenReadAsync();
                var serializer = new XmlSerializer(typeof(ObservableCollection<Server>));
                var data = (ObservableCollection<Server>)serializer.Deserialize(sessionInputStream.AsStreamForRead());
                sessionInputStream.Dispose();
                this.Servers = data;
            }
            catch
            {
            }
        }
    }
}
