using PictureStream.App.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureStream.App.Models
{
    public class Server : BaseModel
    {
        private string serverName;
        public string ServerName
        {
            get
            {
                return this.serverName;
            }
            set
            {
                this.serverName = value;
                this.OnPropertyChanged("ServerName");
            }
        }

        private string serverAddress;
        public string ServerAddress
        {
            get
            {
                return this.serverAddress;
            }
            set
            {
                this.serverAddress = value;
                this.OnPropertyChanged("ServerAddress");
            }
        }

        private string serverAvatar;
        public string Thumbnail
        {
            get
            {
                return this.serverAvatar;
            }
            set
            {
                this.serverAvatar = value;
                this.OnPropertyChanged("ServerAvatar");
            }
        }


    }
}
