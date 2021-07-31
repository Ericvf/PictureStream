using System.ComponentModel;

namespace PictureStream.App.Framework
{
    public class BaseModel : INotifyPropertyChanged
    {
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
