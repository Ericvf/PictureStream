using PictureStream.App.Models;
using PictureStream.App.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PictureStream.App.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Manager : PictureStream.App.Common.LayoutAwarePage
    {
        ManageVM vm = App.ManageVM;
        ServerPrompt prompt;

        public Manager()
        {
            this.InitializeComponent();
            this.DataContext = vm;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await this.vm.Load();
        }
        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (this.prompt != null && this.prompt.IsOpen)
                await this.prompt.ClosePopup();

            await this.vm.Save();
        }

        private void AddServer()
        {
            var p = new ServerPrompt();
            p.Save += async (s, ex) => 
                {
                    this.vm.IsLoading = true;
                    await vm.AddServer(p.ServerName, p.ServerAddress);
                    this.vm.IsLoading = false;
                };
            p.OpenPopup();
            this.prompt = p;
        }

        protected override void GoBack(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.Navigate(typeof(MainPage));
        }

        private void tbAddServer_Click(object sender, TappedRoutedEventArgs e)
        {
            this.AddServer();
        }

        private void abAddServer_Click(object sender, RoutedEventArgs e)
        {
            this.AddServer();
        }

        private void abDeleteServer_Click(object sender, RoutedEventArgs e)
        {
            var gv = this.gridView;
            if (gv != null && gv.SelectedItem != null && gv.SelectedItem is Server)
            {
                this.vm.DeleteServer(gv.SelectedItem as Server);
                return;
            }
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var gv = this.gridView;
            if (gv != null && gv.SelectedItem != null)
            {
                this.vm.CanDelete = true;

                if (!this.bottomAppBar.IsOpen)
                    this.bottomAppBar.IsOpen = true;

                return;
            }

            this.vm.CanDelete = false;
        }

        private void gv_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.vm.SelectedServer = e.ClickedItem as Server;
            Frame.Navigate(typeof(MainPage));
        }
    }
}
