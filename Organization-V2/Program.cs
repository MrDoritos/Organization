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
            dir2.AddFile(CentralDirectory.CreateFile("Book2"));
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


        static int RequestRecieved(HttpRequest request, TcpClient client)
        {
            OrganizationRequest requestStruct = new OrganizationRequest();
            requestStruct.requestURI = (request.RequestURI = request.RequestURI.TrimStart('/'));
            RequestType a;
            requestStruct.requestType = (a = GetRequestType(request.RequestURI));
            if (a == RequestType.CDir) requestStruct.centralDirectory = true;
            string[] splturi = request.RequestURI.Split('&');
            switch (a)
            {
                case RequestType.Dir:
                    if (splturi.Length > 1)
                        requestStruct.selectedDir = CentralDirectory.FindDir(splturi[1]);
                    else
                    {

                        SendNotFound(client, true);
                        return 0;
                    }

                        break;
                case RequestType.Index:
                    break;
                case RequestType.SoftFile:
                    if (int.TryParse(splturi[splturi.Length - 1], out int i))
                    {
                        requestStruct.selectedFile = CentralDirectory.FirstOrDefault(i);
                        if (requestStruct.selectedFile == null) { SendNotFound(client, true); return 0; }
                    }
                    else
                    {
                        SendNotFound(client, true);
                        return 0;
                    }
                    break;                
            }
            HandleOrganizationRequest(client, requestStruct);
            return 0;
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
                default:
                    return RequestType.CDir;
            }
        }

        static void HandleOrganizationRequest(TcpClient client, OrganizationRequest request)
        {
            switch (request.requestType)
            {
                case RequestType.Dir:
                    SendDir(client, request.selectedDir);
                    return;
                case RequestType.SoftFile:
                    SendFile(client, request.selectedFile);
                    return;
                case RequestType.Index:
                    return;
                case RequestType.CDir:
                    SendCDir(client);
                    return;
                default:
                    SendCDir(client);
                    return;
            }
            //SendCDir(client);
        }

        static void SendFile(TcpClient client, SoftFile file)
        {
            var a = HTML.FilePage(file);
            var b = GetHtml();
            b.DocumentNode.SelectSingleNode("//html/body").AppendChild(a);
            client.Client.Send(new HttpResponse(new ResponseHeader(), new Content(b.Serialize())).Serialize());
        }

        static void SendDir(TcpClient client, SoftDirectory dir)
        {
            var a = HTML.DirectoryPage(dir);
            var b = GetHtml();
            b.DocumentNode.SelectSingleNode("//html/body").AppendChild(a);
            client.Client.Send(new HttpResponse(new ResponseHeader(), new Content(b.Serialize())).Serialize());
        }

        static void SendCDir(TcpClient client)
        {
            var a = HTML.CentralDirectoryPage(CentralDirectory);
            var b = GetHtml();
            b.DocumentNode.SelectSingleNode("//html/body").AppendChild(a);
            client.Client.Send(new HttpResponse(new ResponseHeader(), new Content(b.Serialize())).Serialize());            
        }

        static HtmlDocument GetHtml()
        {
            HtmlDocument a = new HtmlDocument();
            a.LoadHtml("<html><head><title>Directory</title></head><body /></html>");
            a.DocumentNode.SelectSingleNode("//html/body").AppendChild(HTML.Style);
            return a;
        }

        static void SendNotFound(TcpClient client, bool includeText = false)
        {
            HttpResponse http;
            if (includeText)
                http = new HttpResponse(new ResponseHeader() { ResponseCode = ResponseHeader.ResponseCodes.NOTFOUND }, new Content(IncText));
            else
                http = new HttpResponse(new ResponseHeader() { ResponseCode = ResponseHeader.ResponseCodes.NOTFOUND });
            client.Client.Send(http.Serialize());
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
    }

    struct OrganizationRequest
    {
        public RequestType requestType;
        public string requestURI;
        public string simplifiedResourceRequest;
        public SoftDirectory selectedDir;
        public SoftFile selectedFile;
        public bool centralDirectory;
    }
}
