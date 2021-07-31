using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PictureStream.Server
{
    public class WebServer
    {
        public Dictionary<string, string> directories = new Dictionary<string, string>();
        private TcpListener listener = new TcpListener(IPAddress.Any, 3000);
        
        public bool IsStarted { get; private set; }

        public void Start()
        {
            this.IsStarted = true;
            listener.Start();
            StartAccept();
        }

        public void Stop()
        {
            this.IsStarted = false;
            listener.Stop();
        }

        private void StartAccept()
        {
            listener.BeginAcceptTcpClient(HandleAsyncConnection, listener);
        }

        private void HandleAsyncConnection(IAsyncResult res)
        {
            if (!this.IsStarted)
                return;

            TcpClient tcpClient = listener.EndAcceptTcpClient(res);
            try
            {

                StartAccept(); //listen for new connections again

                //proceed
                NetworkStream clientStream = tcpClient.GetStream();
                ASCIIEncoding encoder = new ASCIIEncoding();

                byte[] message = new byte[4096];
                int bytesRead;

                while (this.IsStarted)
                {
                    bytesRead = 0;

                    try
                    {
                        //blocks until a client sends a message
                        bytesRead = clientStream.Read(message, 0, 4096);
                    }
                    catch
                    {
                        //a socket error has occured
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        //the client has disconnected from the server
                        break;
                    }

                    //message has successfully been received
                    var request = encoder.GetString(message, 0, bytesRead);
                    this.HandleGetRequest(clientStream, request);
                }

            }
            catch
            {
            }

            tcpClient.Close();
        }

        private void HandleRequest(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                var request = encoder.GetString(message, 0, bytesRead);
                this.HandleGetRequest(clientStream, request);
            }

            tcpClient.Close();
        }

        private void HandleGetRequest(NetworkStream clientstream, string request)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();

            var iGET = request.IndexOf("GET") + 4;
            var iHTTP = request.IndexOf("HTTP");
            var fileName = request.Substring(iGET, iHTTP - iGET - 1);
            var decodedFileName = Uri.UnescapeDataString(fileName);

            NameValueCollection qs = null;
            if (decodedFileName.Contains("?"))
            {
                var queryString = string.Join(string.Empty, decodedFileName.Split('?').Skip(1));
                qs = System.Web.HttpUtility.ParseQueryString(queryString);
                decodedFileName = decodedFileName.Substring(0, decodedFileName.IndexOf('?'));
            }

            bool handled = false;
            byte[] buffer = default(byte[]);
            string smimeheader = "text/html";

            if (qs != null && qs.AllKeys.Contains("avatar"))
            {
                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var dir3 = Path.GetDirectoryName(path);
                var avatar = Path.Combine(dir3, @"avatar.jpg");
                ShellFile shellFile = ShellFile.FromFilePath(avatar);
                var result = shellFile.Thumbnail.LargeBitmap;
                using (MemoryStream stream = new MemoryStream())
                {
                    result.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Close();

                    buffer = stream.ToArray();
                    smimeheader = "image/png";
                    handled = true;
                }
            }

            if (!handled)
            {
                if (decodedFileName.EndsWith("/"))
                {
                    this.DirectoryListing(clientstream, decodedFileName, qs);
                    return;
                }

                var dir = decodedFileName.TrimStart('/').Split('/')[0];
                if (this.directories.ContainsKey(dir))
                {
                    decodedFileName = decodedFileName.Replace("/" + dir, this.directories[dir]);
                    if (File.Exists(decodedFileName))
                    {
                        if (qs != null && qs.AllKeys.Contains("thumbnail"))
                        {
                            ShellFile shellFile = ShellFile.FromFilePath(decodedFileName);
                            var result = shellFile.Thumbnail.LargeBitmap;
                            using (MemoryStream stream = new MemoryStream())
                            {
                                result.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                stream.Close();

                                buffer = stream.ToArray();
                                smimeheader = "image/png";
                                handled = true;
                            }
                        }
                        else
                        {
                            buffer = File.ReadAllBytes(decodedFileName);
                            smimeheader = "image/jpeg";
                            handled = true;
                        }
                    }
                }
            }

            string sbuffer = "";

            if (buffer == null)
            {
                sbuffer += "http/1.1 404 not found" + "\r\n";
                sbuffer += "connection: close" + "\r\n";
            }
            else
            {
                sbuffer += "HTTP/1.1 200 OK" + "\r\n";
                sbuffer += "Last-Modified: Tue, 25 Sep 2007 21:40:05 GMT" + "\r\n";
                sbuffer += "Expires: Tue, 25 Sep 2007 21:40:05 GMT" + "\r\n";
                sbuffer += "Server: PhotoStream 1.0.0.0" + "\r\n";
                sbuffer += "Connection: close" + "\r\n";
                sbuffer += "Content-Type: " + smimeheader + "\r\n";
                sbuffer += "Content-Length: " + buffer.Length + "\r\n\r\n";
            }

            var outputbuffer = encoder.GetBytes(sbuffer);
            clientstream.Write(outputbuffer, 0, outputbuffer.Length);

            if (buffer != null)
                clientstream.Write(buffer, 0, buffer.Length);

            clientstream.Flush();
        }

        private void DirectoryListing(NetworkStream clientstream, string decodedFileName, NameValueCollection qs)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();

            byte[] buffer = default(byte[]);
            string smimeheader = "text/html";

            if (decodedFileName == "/")
            {
                var dirs = new List<MyDirectory>();
                foreach (var item in this.directories)
                {
                    var directory = new MyDirectory()
                    {
                        Name = item.Key,
                        Path = "/" + item.Key
                    };

                    dirs.Add(directory);
                }

                var r = new DirectoryResult();
                r.Directories = dirs;

                var xml = XmlSerializationHelper.Serialize(r);
                buffer = encoder.GetBytes(xml);
                smimeheader = "text/html";
            }
            else
            {
                var dir = decodedFileName.TrimStart('/').Split('/')[0];

                if (this.directories.ContainsKey(dir))
                {
                    var dirname = decodedFileName.Replace("/" + dir, this.directories[dir]);



                    if (Directory.Exists(dirname))
                    {
                        if (qs != null && qs.AllKeys.Contains("thumbnail"))
                        {
                            string patter1 = "*.jpg|*.png|*.bmp|*.nef";
                            string[] filters1 = patter1.Split('|');
                            string path = null;
                            foreach (string filter in filters1)
                            {
                                var filteredFiles = Directory.GetFiles(dirname, filter, SearchOption.AllDirectories);
                                if (filteredFiles.Count() > 0)
                                {
                                    path = filteredFiles.FirstOrDefault();
                                    break;
                                }
                            }

                            if (!string.IsNullOrEmpty(path))
                            {
                                var shellfolder = ShellFile.FromFilePath(path);
                                var result = shellfolder.Thumbnail.LargeBitmap;
                                using (MemoryStream stream = new MemoryStream())
                                {
                                    result.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                    stream.Close();

                                    buffer = stream.ToArray();
                                    smimeheader = "image/png";
                                }
                            }
                        }
                        else
                        {
                            var rdirs = new List<MyDirectory>();
                            var files = new List<MyFile>();

                            var subfirs = Directory.GetDirectories(dirname);
                            foreach (var item in subfirs)
                            {
                                var di = new DirectoryInfo(item);
                                rdirs.Add(new MyDirectory() { Name = di.Name, Path = "/" + item.Replace(this.directories[dir], dir) });
                            }

                            string patter = "*.jpg|*.png|*.bmp|*.nef";
                            string[] filters = patter.Split('|');
                            foreach (string filter in filters)
                            {
                                var filteredFiles = Directory.GetFiles(dirname, filter);
                                foreach (var item in filteredFiles)
                                {
                                    var fi = new FileInfo(item);
                                    files.Add(new MyFile()
                                    {
                                        Name = fi.Name,
                                        Path = "/" + item.Replace(this.directories[dir], dir)
                                    });
                                }
                            }

                            var r = new DirectoryResult()
                            {
                                Directories = rdirs,
                                Files = files.OrderBy(f => f.Name).ToList()
                            };

                            var xml = XmlSerializationHelper.Serialize(r);
                            buffer = encoder.GetBytes(xml);
                            smimeheader = "text/html";
                        }
                    }
                }
            }

            string sbuffer = "";

            if (buffer == null)
            {
                sbuffer += "http/1.1 404 not found" + "\r\n";
                sbuffer += "server: cx1193719-b\r\n";
                sbuffer += "content-type: " + smimeheader + "\r\n";
                sbuffer += "accept-ranges: bytes\r\n";
                sbuffer += "content-length: 0" + "\r\n\r\n";
                sbuffer += "connection: close" + "\r\n";
            }
            else
            {
                sbuffer += "HTTP/1.1 200 OK" + "\r\n";
                sbuffer += "Last-Modified: Tue, 25 Sep 2007 21:40:05 GMT" + "\r\n";
                sbuffer += "Expires: Tue, 25 Sep 2007 21:40:05 GMT" + "\r\n";
                sbuffer += "Server: PhotoStream 1.0.0.0" + "\r\n";
                sbuffer += "Connection: close" + "\r\n";
                sbuffer += "Content-Type: " + smimeheader + "\r\n";
                sbuffer += "Content-Length: " + buffer.Length + "\r\n\r\n";
            }

            var outputbuffer = encoder.GetBytes(sbuffer);
            clientstream.Write(outputbuffer, 0, outputbuffer.Length);

            if (buffer != null)
                clientstream.Write(buffer, 0, buffer.Length);

            clientstream.Flush();
        }
    }

    public class DirectoryResult
    {
        public List<MyDirectory> Directories { get; set; }
        public List<MyFile> Files { get; set; }
    }

    public class MyDirectory
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class MyFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
