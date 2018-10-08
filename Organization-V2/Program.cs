using System;
using System.Threading;
using Http;
using Http.HttpMessage;
using Http.HttpMessage.Message;
using Http.HttpMessage.Message.Forms;
using Http.HttpMessage.Message.Forms.FormMultipart;
using Http.HttpMessage.Message.Forms.FormUrlEncode;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HtmlAgilityPack;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Organization_V2
{
    class Program
    {
        static public CentralDirectory CentralDirectory;
        static public Server Server;

        static void Main(string[] args)
        {
            CentralDirectory = new CentralDirectory();
            var dir1 = CentralDirectory.AddDirectory(CentralDirectory.CreateDirectory("Movies"));
            dir1.AddFile(CentralDirectory.CreateFile("Movie1"));
            dir1.AddFile(CentralDirectory.CreateFile("Movie2"));
            var dir2 = CentralDirectory.AddDirectory(CentralDirectory.CreateDirectory("Books"));
            dir2.AddFile(CentralDirectory.CreateFile("Book1"));
            dir2.AddFile(CentralDirectory.CreateFile("Book2")).ThumbnailPath = "C:\\oof2.png";
            CMDHandler.selected = CentralDirectory.Self;
            Server = new Server();
            Server.RequestRecieved += RequestRecieved;
            Console.Write("Bind: ");
            var add = Console.ReadLine().Split('.');
            byte[] ip = new byte[4];
            for (int i = 0; i < add.Length && i < ip.Length; i++)
                ip[i] = Convert.ToByte(add[i]);
            Server.Start(new IPEndPoint(new IPAddress(ip), 80));
            Loop();
        }

        static void Loop()
        {
            int ms = 0;
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    CMDHandler.HandleKey(Console.ReadKey(true));
                } else if (ms > 12000 && !Server.Connected)
                {
                    Console.WriteLine("Server stopped, press any key to exit");
                    Console.ReadKey(true);
                    Environment.Exit(1);
                }
                else
                {
                    Thread.Sleep(50);
                    ms += 50;
                }
            }
        }


        static KeyValuePair<string, DateTime>[] RequestRecieved(HttpRequest request, TcpClient client)
        {
            List<KeyValuePair<string, DateTime>> timings = new List<KeyValuePair<string, DateTime>>();
            OrganizationRequest requestStruct = new OrganizationRequest();
            requestStruct.requestURI = (request.RequestURI = request.RequestURI.TrimStart('/'));
            RequestType a;
            requestStruct.requestType = (a = GetRequestType(request.RequestURI));
            if (a == RequestType.CDir) requestStruct.centralDirectory = true;
            string[] splturi = request.RequestURI.Split('&');
            timings.Add(new KeyValuePair<string, DateTime>("Request Translation", DateTime.Now));
            switch (a)
            {
                case RequestType.Dir:
                    if (splturi.Length > 1)
                    {
                        if ((requestStruct.selectedDir = CentralDirectory.FindDir(splturi[1])) == null) {
                            SendNotFound(client, timings, true);
                            return timings.ToArray();
                        }
                    }
                    else
                    {

                        SendNotFound(client, timings, true);
                        return timings.ToArray();
                    }

                        break;
                case RequestType.Index:
                    break;
                case RequestType.Content:
                    if (splturi.Length > 1)
                    {
                        if ((requestStruct.selectedThumbnail = CentralDirectory.FindThumbnail(splturi[1])) == null) {
                            SendNotFound(client, timings, false);
                            return timings.ToArray();
                        }
                    }
                    else {
                        SendNotFound(client, timings, false);
                        return timings.ToArray();
                    }
                    break;
                case RequestType.SoftFile:
                    if (int.TryParse(splturi[splturi.Length - 1], out int i))
                    {
                        requestStruct.selectedFile = CentralDirectory.FirstOrDefault(i);
                        if (requestStruct.selectedFile == null) {
                            SendNotFound(client, timings, true);
                            return timings.ToArray();
                        }
                    }
                    else
                    {
                        SendNotFound(client, timings, true);
                        return timings.ToArray();
                    }
                    break;                
            }
            HandleOrganizationRequest(client, requestStruct, timings);
            timings.Add(new KeyValuePair<string, DateTime>("Handle Request", DateTime.Now));
            return timings.ToArray();
        }

        static RequestType GetRequestType(string uri)
        {
            uri = uri.TrimStart('/').Split('&')[0].ToLower();
            switch (uri)
            {
                case "cindex":
                    return RequestType.CIndex;
                case "cdir":
                    return RequestType.CDir;
                case "index":
                    return RequestType.Index;
                case "dir":
                    return RequestType.Dir;
                case "file":
                    return RequestType.SoftFile;
                case "content":
                    return RequestType.Content;
                default:
                    return RequestType.CDir;
            }
        }

        static void HandleOrganizationRequest(TcpClient client, OrganizationRequest request, List<KeyValuePair<string, DateTime>> timings)
        {
            switch (request.requestType)
            {
                case RequestType.Dir:
                    SendDir(client, request.selectedDir, timings);
                    timings.Add(new KeyValuePair<string, DateTime>("Sending Directory", DateTime.Now));
                    return;
                case RequestType.SoftFile:
                    SendFile(client, request.selectedFile, timings);
                    timings.Add(new KeyValuePair<string, DateTime>("Sending Softfile", DateTime.Now));
                    return;
                case RequestType.Index:
                    timings.Add(new KeyValuePair<string, DateTime>("Sending Index", DateTime.Now));
                    return;
                case RequestType.Content:
                    SendContent(client, request.selectedThumbnail, timings);
                    timings.Add(new KeyValuePair<string, DateTime>("Sending Content", DateTime.Now));
                    return;
                case RequestType.CDir:
                    SendCDir(client, timings);
                    timings.Add(new KeyValuePair<string, DateTime>("Sending Central Directory", DateTime.Now));
                    return;
                default:
                    SendCDir(client, timings);
                    timings.Add(new KeyValuePair<string, DateTime>("Sending Central Directory", DateTime.Now));
                    return;
            }
            //SendCDir(client);
        }

        static void SendContent(TcpClient client, IThumbable ahhh, List<KeyValuePair<string, DateTime>> timings)
        {
            if (ahhh.ThumbnailExists)
            {
                SendFile(client, ahhh.Thumbnail, timings);
            }
            else
            {
                SendNotFound(client, timings, false);
            }
        }

        static void SendFile(TcpClient client, string filename)
        {
            throw new NotImplementedException();
        }

        static void SendFile(TcpClient client, FileStream file, List<KeyValuePair<string, DateTime>> timings)
        {
            var ahh = new ResponseHeader(GetContentType(file.Name));
            
            client.Client.Send(Encoding.ASCII.GetBytes(HttpResponse.HeaderToString((ulong)file.Length, ahh) + "\r\n"));
            byte[] buffer = new byte[4096];
            int i;
            while ((i = file.Read(buffer, 0, buffer.Length)) != 0)
            {
                client.Client.Send(buffer, 0, i, SocketFlags.None);
            }
            timings.Add(new KeyValuePair<string, DateTime>("Sending", DateTime.Now));
        }

        static HttpResponse.ContentTypes GetContentType(string filename)
        {
            switch (filename.Split('.').Last().ToLower())
            {
                case "jpeg":
                case "jpg":
                    return ResponseHeader.ContentTypes.IMAGEJPEG;
                case "png":
                    return ResponseHeader.ContentTypes.IMAGEPNG;
                default:
                    return ResponseHeader.ContentTypes.PLAIN;
            }
        }

        static void SendFile(TcpClient client, SoftFile file, List<KeyValuePair<string, DateTime>> timings)
        {
            var a = HTML.FilePage(file);
            var b = GetHtml();
            b.DocumentNode.SelectSingleNode("//html/body").AppendChild(a);
            timings.Add(new KeyValuePair<string, DateTime>("HTML Document", DateTime.Now));
            client.Client.Send(new HttpResponse(new ResponseHeader(), new Content(b.Serialize())).Serialize());
            timings.Add(new KeyValuePair<string, DateTime>("Sending", DateTime.Now));
        }

        static void SendDir(TcpClient client, SoftDirectory dir, List<KeyValuePair<string, DateTime>> timings)
        {
            var a = HTML.DirectoryPage(dir);
            var b = GetHtml();
            b.DocumentNode.SelectSingleNode("//html/body").AppendChild(a);
            timings.Add(new KeyValuePair<string, DateTime>("HTML Document", DateTime.Now));
            client.Client.Send(new HttpResponse(new ResponseHeader(), new Content(b.Serialize())).Serialize());
            timings.Add(new KeyValuePair<string, DateTime>("Sending", DateTime.Now));
        }

        static void SendCDir(TcpClient client, List<KeyValuePair<string, DateTime>> timings)
        {
            var a = HTML.CentralDirectoryPage(CentralDirectory);
            var b = GetHtml();
            b.DocumentNode.SelectSingleNode("//html/body").AppendChild(a);
            timings.Add(new KeyValuePair<string, DateTime>("HTML Document", DateTime.Now));
            client.Client.Send(new HttpResponse(new ResponseHeader(), new Content(b.Serialize())).Serialize());
            timings.Add(new KeyValuePair<string, DateTime>("Sending", DateTime.Now));
        }

        static HtmlDocument GetHtml()
        {
            HtmlDocument a = new HtmlDocument();
            a.LoadHtml("<html><head><title>Directory</title></head><body /></html>");
            a.DocumentNode.SelectSingleNode("//html/body").AppendChild(HTML.Style);
            return a;
        }

        static void SendNotFound(TcpClient client, List<KeyValuePair<string, DateTime>> timings, bool includeText = false)
        {
            HttpResponse http;
            if (includeText)
                http = new HttpResponse(new ResponseHeader() { ResponseCode = ResponseHeader.ResponseCodes.NOTFOUND }, new Content(IncText));
            else
                http = new HttpResponse(new ResponseHeader() { ResponseCode = ResponseHeader.ResponseCodes.NOTFOUND });
            timings.Add(new KeyValuePair<string, DateTime>("Create Response", DateTime.Now));
            client.Client.Send(http.Serialize());
            timings.Add(new KeyValuePair<string, DateTime>("Sending", DateTime.Now));
        }

        static byte[] IncText { get; } = Encoding.ASCII.GetBytes("404 Resource unavailable");

        static void SendHTML(HtmlDocument i, TcpClient c)
        {
            HttpResponse d = new HttpResponse();
            byte[] html = i.Serialize();
            string header = HttpResponse.HeaderToString((ulong)html.Length, d) + "\r\n";

            c.Client.Send(System.Text.Encoding.ASCII.GetBytes(header));
            c.Client.Send(html);
        }        
    }

    public enum RequestType
    {
        Unknown = 0,
        CIndex = 1,
        CDir = 2,
        Index = 3,
        Dir = 4,
        SoftFile = 5,
        Content = 6,
    }

    struct OrganizationRequest
    {
        public RequestType requestType;
        public string requestURI;
        public string simplifiedResourceRequest;
        public SoftDirectory selectedDir;
        public SoftFile selectedFile;
        public bool centralDirectory;
        public IThumbable selectedThumbnail;
    }
}
