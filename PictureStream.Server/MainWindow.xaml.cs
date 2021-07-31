using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace PictureStream.Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VM vm = new VM();
        WebServer server = new WebServer();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = vm;
            this.vm.Load();
            VM.folders.CollectionChanged += folders_CollectionChanged;
            this.UpdateServerFolders();
            server.Start();
        }

        void folders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateServerFolders();
            this.vm.Save();
        }

        private void UpdateServerFolders()
        {
            var directories = new Dictionary<string, string>();
            foreach (var item in VM.folders)
            {
                var di = new DirectoryInfo(item);
                directories.Add(di.Name, item);
            }

            this.server.directories = directories;
        }

        private void btnAddFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            var result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.vm.AddFolder(folderBrowserDialog1.SelectedPath);
            }
        }

        private void btnRemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            this.vm.RemoveFolder();
        }

        private void btnChangeAvatar_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Image Files (*.jpg)|*.jpg";
            var result = openFileDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    var dir = Path.GetDirectoryName(path);
                    var extension = new FileInfo(openFileDialog1.FileName).Extension.ToLower();
                    File.Copy(openFileDialog1.FileName, Path.Combine(dir, @"avatar" + extension), true);
                    System.Windows.MessageBox.Show("Thanks, your avatar was changed!");
                }
                catch
                {
                    System.Windows.MessageBox.Show("An error has occured while copying the avatar, please try again");
                }
            }
        }
    }

  

}
