using PictureStream.App.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace PictureStream.App.Views
{
    public class ViewerVM : BaseModel
    {
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

        public MyFile SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (_SelectedItem != value)
                {
                    _SelectedItem = value;
                    if (this.Files.Contains(value))
                        this.CurrentPage = this.Files.IndexOf(value) + 1;
                    OnPropertyChanged(SelectedItemPropertyName);
                }
            }
        }
        private MyFile _SelectedItem;
        public const string SelectedItemPropertyName = "SelectedItem";

        private int currentPage;
        public int CurrentPage
        {
            get
            {
                return this.currentPage;
            }
            private set
            {
                this.currentPage = value;
                this.OnPropertyChanged("CurrentPage");
            }
        }


        internal void Load(ViewerParams vparams)
        {
            this.Files = new ObservableCollection<MyFile>(vparams.Files);
            this.SelectedItem = vparams.Files[vparams.Index];
        }
    }

    public sealed partial class Viewer : PictureStream.App.Common.LayoutAwarePage
    {
        ViewerVM vm = new ViewerVM();

        public Viewer()
        {
            this.InitializeComponent();
            this.DataContext = vm;
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            
            this.vm.Load(e.Parameter as ViewerParams);
            this.blackBg.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //this.blackBg.Fade(1, 500, Eq.OutSine).Play();
            this.blackBg.Opacity = 1;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            //await this.blackBg.Fade(0, 300, Eq.OutSine).PlayAsync();  
            base.OnNavigatedFrom(e);
        }

        private async Task First()
        {
            var curIndex = this.flipView.SelectedIndex;
            if (curIndex > 0)
            {
                var curContainer = this.flipView.ItemContainerGenerator.ContainerFromIndex(curIndex) as FrameworkElement;
                if (curContainer != null)
                {
                    //await curContainer
                    //      .Fade(0, 300, Eq.OutSine)
                    //      .Move(100, 0, 300, Eq.OutSine)
                    //  .PlayAsync();
                    //fadeInRight = true;
                }

                this.flipView.SelectedIndex = 0;
            }
        }

        private async Task Prev()
        {
            var curIndex = this.flipView.SelectedIndex;
            var newIndex = Math.Max(0, curIndex - 1);

            //if (this.VM.IsTwoPage && this.VM.SelectedItem.IsRegular &&
            //    this.VM.SelectedItem.PrevPage != null &&
            //    this.VM.SelectedItem.PrevPage.IsRegular &&
            //    this.VM.SelectedItem.PrevPage.PrevPage != null &&
            //    this.VM.SelectedItem.PrevPage.PrevPage.IsRegular)
            //{
            //    newIndex = Math.Max(0, curIndex - 2);
            //}

            if (curIndex > newIndex)
            {
                var curContainer = this.flipView.ItemContainerGenerator.ContainerFromIndex(curIndex) as FrameworkElement;
                if (curContainer != null)
                {
                    //await curContainer
                    //      .Fade(0, 300, Eq.OutSine)
                    //      .Move(100, 0, 300, Eq.OutSine)
                    //  .PlayAsync();
                    //fadeInRight = true;
                }

                this.flipView.SelectedIndex = newIndex;
            }
        }

        private async Task Next()
        {

            var curIndex = this.flipView.SelectedIndex;
            var newIndex = Math.Min(curIndex + 1, this.vm.Files.Count - 1);

            //if (this.VM.IsTwoPage && this.VM.SelectedItem.IsRegular &&
            //    this.VM.SelectedItem.NextPage != null &&
            //    this.VM.SelectedItem.NextPage.IsRegular &&
            //    this.VM.SelectedItem.NextPage.NextPage != null &&
            //    this.VM.SelectedItem.NextPage.NextPage.IsRegular)
            //{
            //    newIndex = Math.Min(curIndex + 2, this.VM.Pages.Count - 1);
            //}

            if (curIndex < newIndex)
            {
                var curContainer = this.flipView.ItemContainerGenerator.ContainerFromIndex(curIndex) as FrameworkElement;
                if (curContainer != null)
                {
                    //await curContainer
                    //    .Fade(0, 300, Eq.OutSine)
                    //    .Move(-100, 0, 300, Eq.OutSine)
                    //.PlayAsync();
                    //fadeInLeft = true;
                }

                this.flipView.SelectedIndex = newIndex;
            }
        }

        private async Task Last()
        {
            var curIndex = this.flipView.SelectedIndex;
            var newIndex = this.vm.Files.Count - 1;

            if (curIndex < newIndex)
            {
                var curContainer = this.flipView.ItemContainerGenerator.ContainerFromIndex(curIndex) as FrameworkElement;
                if (curContainer != null)
                {
                    //await curContainer
                    //    .Fade(0, 300, Eq.OutSine)
                    //    .Move(-100, 0, 300, Eq.OutSine)
                    //.PlayAsync();
                    //fadeInLeft = true;
                }

                this.flipView.SelectedIndex = newIndex;
            }
        }

        private async void rectPrev_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Prev();
        }

        private async void rectNext_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Next();
        }

        private async void btnAppBarFirst_Click(object sender, RoutedEventArgs e)
        {
            await this.First();
        }

        private async void btnAppBarPrev_Click(object sender, RoutedEventArgs e)
        {
            await this.Prev();
        }

        private async void btnAppBarNext_Click(object sender, RoutedEventArgs e)
        {
            await this.Next();
        }

        private async void btnAppBarLast_Click(object sender, RoutedEventArgs e)
        {
            await this.Last();
        }
    }
}
