//using AnimationExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PictureStream.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerPrompt : Page
    {
        public event EventHandler Cancel;
        public event EventHandler Save;

        public string ServerName { get; set; }
        public string ServerAddress { get; set; }
        public Color ServerColor { get; set; }

        public ServerPrompt()
        {
            this.InitializeComponent();

            var bounds = Window.Current.Bounds;
            this.Width = bounds.Width;
            this.Height = bounds.Height;

            //var colors = typeof(Colors).GetTypeInfo().DeclaredProperties;
            //foreach (var item in colors)
            //    cbBorderColor.Items.Add(item);

            //cbBorderColor.SelectedIndex = 0;

            this.Loaded += BlankPage1_Loaded;
        }

        private void cbBorderColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cbBorderColor.SelectedIndex != -1)
            //{
            //    var pi = cbBorderColor.SelectedItem as PropertyInfo;
            //    ServerColor = (Color)pi.GetValue(null);
            //}
        }

        void BlankPage1_Loaded(object sender, RoutedEventArgs e)
        {
            //this.tbPrompt.Text = this.Text ?? string.Empty;

            //if (!string.IsNullOrEmpty(this.Title))
            //    this.tbTitle.Text = this.Title;

            //if (!string.IsNullOrEmpty(this.Message))
            //    this.tbMessage.Text = this.Message;

            this.tbServerName.Focus(FocusState.Keyboard);
            //this.tbPrompt.SelectAll();

            //this.contentGrid
            //    .Fade()
            //    .Move(0, -150)
            //    .Size(-1, 50)

            //    .Move(0, 0, 500, Eq.OutBack)
            //    .Size(-1, 200, 500, Eq.OutSine)
            //    .Fade(1, 500)
            //    .Play();
        }

        private async void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //await this.contentGrid
            //                  .Size(-1, 50, 500, Eq.OutSine)
            //                  .Fade(0, 500)
            //                  .Move(0, 150, 500, Eq.InBack)
            //                  .PlayAsync();

            this.popup.IsOpen = false;

            if (this.Cancel != null)
                this.Cancel(sender, null);
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.tbServerName.Text))
            {
                this.tbServerName.Focus(Windows.UI.Xaml.FocusState.Keyboard);
                return;
            }
            if (string.IsNullOrEmpty(this.tbServerAddress.Text))
            {
                this.tbServerAddress.Focus(Windows.UI.Xaml.FocusState.Keyboard);
                return;
            }
            
            //await this.contentGrid
            //                 .Size(-1, 50, 500, Eq.OutSine)
            //                 .Fade(0, 500)
            //                 .Move(0, 150, 500, Eq.InBack)
            //                 .PlayAsync();

            this.ServerName = this.tbServerName.Text;
            this.ServerAddress = this.tbServerAddress.Text;
            this.popup.IsOpen = false;

            if (this.Save != null)
                this.Save(sender, null);
        }

        private Popup popup;
        internal void OpenPopup()
        {
            this.popup = new Popup();
            popup.Child = this;
            popup.IsOpen = true;
        }

        private void tbPrompt_KeyDown_1(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                this.btnSave_Click(this, null);
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Escape)
            {
                this.btnCancel_Click(this, null);
                e.Handled = true;
            }
        }

        internal async Task ClosePopup()
        {
              //await this.contentGrid
              //                .Size(-1, 50, 500, Eq.OutSine)
              //                .Fade(0, 500)
              //                .Move(0, 150, 500, Eq.InBack)
              //                .PlayAsync();

            this.popup.IsOpen = false;
        }

        public bool IsOpen
        {
            get
            {
                return this.popup.IsOpen;
            }
        }
    }
}
