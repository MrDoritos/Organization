using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Http;
using Http.HttpMessage.Message;
using Http.HttpMessage;
using Organization.Objects;
using Indexing;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Xml;

namespace Organization
{
    class Program
    {
        public static Server server = new Server();
        public static readonly Index<SoftFile> CentralDirectory = new Index<SoftFile>();
        public static DirectoryInfo ThumbnailDirectory;
        
        static List<SoftFile> Index = new List<SoftFile>();

        static public uint NextId { get => GetNextId(); }

        static private uint GetNextId()
        {
            return (uint)(Index.Count + 1);
        }


        static public XmlNode ToXmlNode(Child<SoftFile> child, XmlNode src, XmlDocument doc)
        {
            foreach (var a in child.Children)
                src.AppendChild(ToXmlNode(a, doc.CreateElement("child"), doc));
            src.AppendChild(doc.CreateElement("id")).InnerText = child.Item.Id.ToString();
            src.AppendChild(doc.CreateElement("name")).InnerText = child.Item.Name;
            src.AppendChild(doc.CreateElement("description")).InnerText = child.Item.Description;
            src.AppendChild(doc.CreateElement("thumb")).InnerText = child.Item.Thumbnail;
            src.AppendChild(doc.CreateElement("hash")).InnerText = child.Item.Hash;
            var tags = src.AppendChild(doc.CreateElement("tags"));
            foreach (var a in child.Item.Tags)
                tags.AppendChild(doc.CreateElement("tag")).InnerText = a;
            return src;
        }
        
        static public void SaveAsXML(string filename)
        {
            XmlDocument xml = new XmlDocument();
            XmlDeclaration declaration = xml.CreateXmlDeclaration("1.0", "utf-8", null);
            xml.PrependChild(declaration);
            XmlNode root = xml.CreateElement("index");
            xml.AppendChild(root);
            foreach (var child in CentralDirectory.Children)
                root.AppendChild(ToXmlNode(child,xml.CreateElement("child"), xml));
            xml.Save(filename);
        }
        static public void SaveAsXML(string filename, Child<SoftFile> child)
        {
            XmlDocument xml = new XmlDocument();
            XmlDeclaration declaration = xml.CreateXmlDeclaration("1.0", "utf-8", null);
            xml.PrependChild(declaration);
            XmlNode root = xml.CreateElement("index");
            xml.AppendChild(root);
            foreach (var childr in child.Children)
                root.AppendChild(ToXmlNode(childr, xml.CreateElement("child"), xml));
            xml.Save(filename);
        }

        static public void LoadXml(string filename)
        {
            if (!System.IO.File.Exists(filename)) return;
            XmlDocument xml = new XmlDocument();
            xml.Load(filename);
            //CentralDirectory = new Index<SoftFile>();
            Index = new List<SoftFile>();
            Child<SoftFile> child = new Child<SoftFile>();
            for (int i = 0; i < xml["index"].ChildNodes.Count; i++)
                Load(CentralDirectory, xml["index"].ChildNodes[i]);
            CentralDirectory.Name = "";
        }

        static public void Load(Child<SoftFile> parent, XmlNode src)
        {
            var file = new Child<SoftFile>(parent);
            //Load softfile data
            if (src.Name != "index")
            {
                List<string> tags = new List<string>();
                var tagcoll = src["tags"].ChildNodes;
                int mcount = tagcoll.Count;
                for (int i = 0; i < mcount; i++)
                    tags.Add(tagcoll[i].InnerText);
                //uint id = Convert.ToUInt32(src["id"].InnerText);
                string name = src["name"].InnerText;
                string description = src["description"].InnerText;
                string thumb = src["thumb"].InnerText;
                string hash = src["hash"].InnerText;
                file.Name = NextId.ToString();
                file.Item = new SoftFile(name, thumb, NextId) { Description = description };
                Index.Add(file.Item);
            }

            //Load children recursively
            var coll = src.SelectNodes("child");
            int count = coll.Count;
            for (int i = 0; i < count; i++)
                //file.AppendChild(Load(file, coll[i]));
                Load(file, coll[i]);

            parent.AppendChild(file);
            //return file;
        }

        static SoftFile AddFile(string name, string thumb)
        {
            SoftFile softFile = new SoftFile(name, thumb, NextId);
            if (softFile.ThumbnailExists)
                AddThumbnail(new FileInfo(softFile.Thumbnail), softFile);
            Index.Add(softFile);
            return softFile;
        }

        static SoftFile AddFile(string name, byte[] thumb, string thumbname)
        {
            SoftFile softFile = new SoftFile(name, NextId);
            AddThumbnail(thumb, softFile, thumbname);
            Index.Add(softFile);
            return softFile;
        }

        static SoftFile AddFile(string name)
        {
            SoftFile softFile = new SoftFile(name, NextId);
            Index.Add(softFile);
            return softFile;
        }
        
        static bool QueryBool(string question)
        {            
            while (true)
            {
                Console.Write($"{question} [y/n] ");
                var key = Console.ReadKey(true);
                Console.WriteLine();
                switch (key.Key)
                {
                    case ConsoleKey.Y:
                        return true;
                    case ConsoleKey.N:
                        return false;                        
                }
            }
        }
        
        static void AddThumbnail(byte[] thumb, SoftFile soft, string thumbname)
        {
            string newpath;
            if (thumbname.Length > 0)
                newpath = AddToDir(thumbname[0].ToString(), thumb, thumbname);
            else
                newpath = AddToDir(new Random().Next(11111111, 99999999).ToString(), thumb, thumbname);
        }

        static void AddThumbnail(FileInfo file, SoftFile soft)
        {
            string newpath;
            if (file.Name.Length > 0)
                newpath = AddToDir(file.Name[0].ToString(), file);
            else
                newpath = AddToDir(new Random().Next(11111111, 99999999).ToString(), file);
            soft.ChangeThumbPath(newpath);
        }
        
        static string AddToDir(string firstletter, byte[] thumb, string thumbname)
        {
            var newdir = ThumbnailDirectory.CreateSubdirectory(firstletter.ToLower());
            string newpath = newdir.FullName.TrimEnd('\\') + '\\' + thumbname;
            if (!System.IO.File.Exists(newpath))
                System.IO.File.WriteAllBytes(newpath, thumb);
            return newpath;
        }

        static string AddToDir(string firstletter, FileInfo file)
        {
            var newdir = ThumbnailDirectory.CreateSubdirectory(firstletter.ToLower());
            string newpath = newdir.FullName.TrimEnd('\\') + '\\' + file.Name;
            if (!System.IO.File.Exists(newpath))
                file.CopyTo(newpath);
            return newpath;
        }

        static byte[] GetThumbnail(SoftFile file)
        {
            return System.IO.File.ReadAllBytes(file.Thumbnail);
        }

        static void SendContent(HttpRequest request, TcpClient client)
        {
            var req = request.RequestURI.Remove(0, 8);

            Child<SoftFile> selected = GetChild(req);

            //COMPLETE /TO-DO: Change from opening all the file bytes to using a filestream to stream the file bytes to the client/
            if (selected.Item.ThumbnailExists)
            {

                //client.Client.Send(new HttpResponse(new ResponseHeader(GetContentType(selected.Item.Thumbnail)) { headerParameters = new HeaderParameter[] { new HeaderParameter(new HeaderVariable[] { new HeaderVariable("cache-control", "max-age=99999999") }) } }, new Content(GetThumbnail(selected.Item))).Serialize());
                //var a = new ResponseHeader(GetContentType(selected.Item.Thumbnail)) { headerParameters = new HeaderParameter[] { new HeaderParameter(new HeaderVariable[] { new HeaderVariable("cache-control", "max-age=99999999") }) } };
                //FileInfo fi = new FileInfo(selected.Item.Thumbnail);
                //string header = HttpResponse.HeaderToString((ulong)fi.Length,a);                
                //FileStream fs = System.IO.File.OpenRead(selected.Item.Thumbnail);
                //client.Client.Send(Encoding.ASCII.GetBytes(header + "\r\n"));
                //byte[] buffer = new byte[4096];
                //int i;
                //while ((i = fs.Read(buffer,0,4096)) != 0)
                //{
                //    client.Client.Send(buffer,0,i,SocketFlags.None);
                //}
                SendFile(client, selected.Item.Thumbnail);
                return;
            }
            else
            {
                SendNotFound(client);
                return;
            }
        }

        static void SendFile(TcpClient client, string filepath)
        {
            var a = new ResponseHeader(GetContentType(filepath)) { headerParameters = new HeaderParameter[] { new HeaderParameter(new HeaderVariable[] { new HeaderVariable("cache-control", "max-age=99999999") }) } };
            FileInfo fi = new FileInfo(filepath);
            string header = HttpResponse.HeaderToString((ulong)fi.Length, a);
            FileStream fs = System.IO.File.OpenRead(filepath);
            client.Client.Send(Encoding.ASCII.GetBytes(header + "\r\n"));
            byte[] buffer = new byte[4096];
            int i;
            while ((i = fs.Read(buffer, 0, 4096)) != 0)
            {
                client.Client.Send(buffer, 0, i, SocketFlags.None);
            }
        }

        static void SendFavicon(TcpClient client)
        {
            if (System.IO.File.Exists("favicon.ico"))
                SendFile(client, "favicon.ico");
            else
                SendNotFound(client);
        }

        static ResponseHeader.ContentTypes GetContentType(string name)
        {
            switch (name.Split('.').Last().ToLower())
            {
                case "png":
                    return ResponseHeader.ContentTypes.IMAGEPNG;
                case "jpeg":
                case "jpg":
                    return ResponseHeader.ContentTypes.IMAGEJPEG;
                case "ico":
                    return ResponseHeader.ContentTypes.ICO;
                default:
                    return ResponseHeader.ContentTypes.PLAIN;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Organization Software Copyright 2018 iansweb.org");

            ThumbnailDirectory = null;
            string dir;
            Console.WriteLine("Thumbnail \"OrganThumbs\\\" Path: ");
            while (true)
            {
                dir = Console.ReadLine();
                if (Directory.Exists(dir))
                {
                    if (QueryBool("Are you sure?"))
                    {
                        ThumbnailDirectory = new DirectoryInfo(dir);
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Directory!");
                }
            }

            if (System.IO.File.Exists("database.xml") && QueryBool("Load database?")) { LoadXml("database.xml"); }
            else
            {
                CentralDirectory.Name = "";
                //CentralDirectory.Item = AddFile("", "");
                var masm = AddFile("MASM", "G:\\Thumbs\\MASMVC_D1.png");
                masm.GetHash("C:\\oof1.png");
                var vs = AddFile("Visual Studio 6.0", "G:\\Thumbs\\VS6PE_D1.png");
                vs.Description = "Online training tools for Visual Studio 6.0";
                vs.AddTag("programming");
                vs.AddTag("guide");
                var java = AddFile("Java I, Java II", "G:\\Thumbs\\DLJJ_D1.png");
                var lotus = AddFile("Lotus Notes", "G:\\Thumbs\\LNDOLA_D1.png");
                var msdnsl = AddFile("MSDN Subscription Library", "G:\\Thumbs\\MSDNSL_D1.png");
                java.AddTag("programming");
                java.AddTag("java");
                CentralDirectory.AppendChild(vs);
                CentralDirectory.AppendChild(java);
                CentralDirectory.AppendChild(lotus);
                CentralDirectory.AppendChild(msdnsl);
            }
            //Database.Save(CentralDirectory, "database.ian");
            //Index<SoftFile> randomindex = new Index<SoftFile>();
            //CreateRandomChildren(randomindex, 0, 4, 3);
            //Database.Save(CentralDirectory, "database.ian");
            //var a = Database.Load("database.ian");
            //SaveAsXML("database.xml", randomindex);
            server.RequestRecieved += RequestRecieved;
            //server.Start(new IPEndPoint(IPAddress.Any, 80));
            server.Start("iansweb.org");
            //while (server.Connected) { Autosaver(); }
            UIThread();
        }

        static Child<SoftFile> CreateRandomChildren(Child<SoftFile> child, int curpos, int maxpos, int children)
        {
            if (curpos >= maxpos) return new Child<SoftFile>() { Name = curpos.ToString(), Item = CreateRandomSoftfile() };
            for (int i = 0; i < children; i++)
                child.AppendChild(CreateRandomChildren(new Child<SoftFile>(), curpos + 1, maxpos, children));
            child.Item = CreateRandomSoftfile();
            child.Name = new Random().Next().ToString();
            
            return child;
        }

        static SoftFile CreateRandomSoftfile()
        {
            var rnd = new Random();
            return new SoftFile(((long)rnd.Next(int.MinValue, int.MaxValue) * rnd.Next(int.MinValue, int.MaxValue)).ToString(), ((long)rnd.Next(int.MinValue, int.MaxValue) * rnd.Next(int.MinValue, int.MaxValue)).ToString(), (uint)rnd.Next(), ((long)rnd.Next(int.MinValue, int.MaxValue) * rnd.Next(int.MinValue, int.MaxValue)).ToString(), RandomTags(rnd), ((long)rnd.Next(int.MinValue, int.MaxValue) * rnd.Next(int.MinValue, int.MaxValue)).ToString());
        }

        static string[] RandomTags(Random rnd)
        {
            int howmany = rnd.Next(0, 10);
            string[] rand = new string[howmany];
            for (int i = 0; i < howmany; i++)
                rand[i] = ((long)rnd.Next(int.MinValue, int.MaxValue) * rnd.Next(int.MinValue, int.MaxValue)).ToString();
            return rand;
        }

        const int TENSECONDSINMS = 10000;

        static void UIThread()
        {
            UIHandler.WriteCharacters("");
            //DateTime lastCheck = DateTime.Now;
            //TimeSpan tenseconds = new TimeSpan(0, 0, 10);
            int MSsincelastcheck = 0;
            while (true)
            {
                //Handle a key, if available
                if (Console.KeyAvailable)
                {
                    UIHandler.HandleKey(Console.ReadKey(true));
                }
                //Check to see if the server is still online every 10 seconds
                else if (TENSECONDSINMS < MSsincelastcheck && !server.Connected)
                {
                    Console.WriteLine($"Server stopped, press any key to continue...");
                    Console.ReadKey(true);
                    Environment.Exit(1);
                }
                //If there are no interrupts, delay the thread by 100 ms
                else
                {
                    Thread.Sleep(100);
                    MSsincelastcheck += 100;
                }
            }
        }



        static void Autosaver()
        {
            while (true)
            {
                //SaveAsXML("database.xml");
                Thread.Sleep(60000);
            }
        }        

        static Child<SoftFile> GetChild(string requestURI)
        {
            Child<SoftFile> selected = CentralDirectory;
            foreach (var a in requestURI.Split('/'))
            {
                if (a != "")
                {
                    var hmm = selected.Children.FirstOrDefault(n => n.Name == a);
                    if (hmm == null)
                    {
                        break;
                    }
                    selected = hmm;
                }
            }
            return selected;
        }

        static int RequestRecieved(HttpRequest httpRequest, TcpClient client)
        {
            Console.WriteLine($"{httpRequest.Method} {httpRequest.RequestURI}");
            httpRequest.RequestURI = httpRequest.RequestURI.Trim('/');
            if (httpRequest.RequestURI.StartsWith("content&"))
            {
                SendContent(httpRequest, client);
            }else if (httpRequest.RequestURI.StartsWith("favicon.ico"))
            {
                SendFavicon(client);
            }
            else if (httpRequest.RequestURI.StartsWith("search&"))
            {
                SearchUTIL(httpRequest, client);
            }
            else if (httpRequest.RequestURI.StartsWith("edit&"))
            {
                EditUTIL(httpRequest, client);
            }
            else if (httpRequest.RequestURI.StartsWith("reload"))
            {
                //LoadXml("database.xml");
                SendDirectoryPage(CentralDirectory, client);
            }
            else if (httpRequest.RequestURI.StartsWith("save"))
            {
                Database.Save(CentralDirectory, "database.ian");
                SaveAsXML("database.xml");

                //SendDirectoryPage(CentralDirectory, client);
                SendRedirect("/", client);
            }
            else if (httpRequest.RequestURI.StartsWith("addchild"))
            {
                HandleAdd(httpRequest, client);
            }
            else
            {
                Child<SoftFile> selected = CentralDirectory;
                foreach (var a in httpRequest.RequestURI.ToLower().Split('/'))
                {
                    if (a == "")
                    {
                        SendDirectoryPage(CentralDirectory, client);
                        return 0;
                    }
                    else
                    {
                        var hmm = selected.Children.FirstOrDefault(n => n.Name == a);
                        if (hmm == null)
                        {
                            SendNotFound(client);
                            break;
                        }
                        else
                        {
                            selected = hmm;
                        }
                    }
                }
                if (selected == null)
                {

                }
                else
                {
                    SendDirectoryPage(selected, client);
                }
            }


            return 0;
        }

        static void SendRedirect(string uri, TcpClient client)
        {
            HttpResponse httpResponse = new HttpResponse();
            httpResponse.ResponseCode = ResponseHeader.ResponseCodes.PERMREDIRECT;
            httpResponse.headerParameters = new HeaderParameter[] { new HeaderParameter(new HeaderVariable[1] { new HeaderVariable("location", uri) }) };
            client.Client.Send(httpResponse.Serialize());
        }

        static void HandleAdd(HttpRequest request, TcpClient client)
        {
            var req = request.RequestURI.Remove(0, 9);

            Child<SoftFile> selected = GetChild(req);

            string name = null;
            string thumb = null;

            foreach (var a in request.form.Multipart.messages)
                switch (a.Name)
                {
                    case "name":
                        name = Encoding.UTF8.GetString(a.Content.ContentBytes).Trim('\r','\n');
                        break;
                    case "thumb":
                        if (a.FileName.Length < 3) { SendDirectoryPage(selected, client); return; }
                        System.IO.File.WriteAllBytes(a.FileName,  a.Content.ContentBytes.Take(a.Content.ContentLength - 2).ToArray());
                        thumb = a.FileName;
                        break;
                }
            selected.AppendChild(AddFile(name ?? "undefined", thumb ?? ""));
            SendDirectoryPage(selected, client);
        }

        static void SearchUTIL(HttpRequest request, TcpClient client)
        {
            var req = request.RequestURI.Remove(0, 7);

            Child<SoftFile> selected = GetChild(req);

            IDictionary<Child<SoftFile>, int> keyValuePairs = new Dictionary<Child<SoftFile>, int>();

            GetResults(request.form.UrlEncode.messages[0].value.Split(' '), selected, keyValuePairs);

            SendSearchResults(client, keyValuePairs.OrderByDescending(n => n.Value).Select(n => n.Key).ToArray());
        }

        static void EditUTIL(HttpRequest request, TcpClient client)
        {
            var req = request.RequestURI.Remove(0, 5);

            Child<SoftFile> selected = GetChild(req);

            
        }

        static void GetResults(string[] queryTags, Child<SoftFile> child, IDictionary<Child<SoftFile>, int> dictionary)
        {
            foreach (var childOfChild in child.Children)
                GetResults(queryTags, childOfChild, dictionary);
            if (child.Item != null)
                dictionary.Add(child, child.Item.Tags.Count(n => queryTags.Any(m => m == n)));
        }

        static void SendNotFound(TcpClient client)
        {
            HttpResponse httpResponse = new HttpResponse(new ResponseHeader() { ResponseCode = ResponseHeader.ResponseCodes.NOTFOUND });
            client.Client.Send(httpResponse.Serialize());
        }

        static HtmlNode styles = HtmlNode.CreateNode("<style>" +
            "html { background-color: darkblue;}" +
            "#card:hover { background:rgba(255,255,255,0.7); }" +
            "#card {background:rgba(255,255,255,0.5); cursor:pointer; }" +
            "#sidebar {position:fixed;top:0;bottom:0;left:0;width:18%;min-height:100%;border:2px solid #787878; padding:10px; background: rgba(255, 255, 255, 0.5); z-index:5; }" +
            "#container {position:relative;padding:0 0 200px 20%; height:auto; min-height:100%; margin: 0% 0% 2% 0%; }" +
            "#cardholder {height:auto;}" +
            "#parent {height: auto; border: 5px solid black;background: rgba(255, 255, 255, 0.5);padding:5%;}" +
            "body { min-height:100%; height:100%; }" +
            "</style>");

        

        static void SendDirectoryPage(Child<SoftFile> child, TcpClient client)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml("<html style=\"font-family: consolas;\"><head><title>Directory</title></head><body></body></html>");
            var body = doc.DocumentNode.SelectSingleNode("//html/body");

            body.AppendChild(styles);
            var div = body.AppendChild(HtmlNode.CreateNode("<div id=\"container\" />"));
            var cardholder = HtmlNode.CreateNode("<div id=\"cardholder\" />");
            div.AppendChild(GetSidebar(child));
            foreach (var a in child.Children)
                cardholder.AppendChild(GetChildNode(a));
            if (child.Parent != null)
                div.AppendChild(GetHeader(child));
            div.AppendChild(cardholder);
            client.Client.Send(new HttpResponse(new ResponseHeader(), new Content(Serialize(doc))).Serialize(doc.Encoding));
        }

        static void SendSearchResults(TcpClient client, params Child<SoftFile>[] results)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml("<html style=\"font-family: consolas;\"><head><title>Directory</title></head><body></body></html>");
            var body = doc.DocumentNode.SelectSingleNode("//html/body");

            body.AppendChild(styles);
            var div = body.AppendChild(HtmlNode.CreateNode("<div id=\"container\" />"));
            var cardholder = HtmlNode.CreateNode("<div id=\"cardholder\" />");
            div.AppendChild(GetSidebar(CentralDirectory));
            foreach (var a in results)
                cardholder.AppendChild(GetChildNode(a));
            div.AppendChild(cardholder);
            client.Client.Send(new HttpResponse(new ResponseHeader(), new Content(Serialize(doc))).Serialize(doc.Encoding));
        }

        static HtmlNode GetHeader(Child<SoftFile> parent)
        {
            HtmlNode parentnode = HtmlNode.CreateNode("<div id=\"parent\" />");
            parentnode.AppendChild(GetTags(parent));
            parentnode.AppendChild(HtmlNode.CreateNode($"<h1 style=\"text-align:center; margin:0; margin-bottom:5%;\">{parent.Item.Name}</h1>"));
            parentnode.AppendChild(HtmlNode.CreateNode($"<img style=\"height: 300px; width: 100%; max-height: 100%; max-width: 100%; margin:0; object-fit: contain\" src=\"/content&{parent.ToString()}\">"));
            parentnode.AppendChild(HtmlNode.CreateNode($"<p>{parent.Item.Description}</p>"));
            return parentnode;
        }

        static HtmlNode GetTags(Child<SoftFile> child)
        {
            HtmlNode tagDiv = HtmlNode.CreateNode("<div id=\"tags\" />");
            if (child.Item != null)
            {
                foreach (var a in child.Item.Tags)
                    tagDiv.AppendChild(HtmlNode.CreateNode($"<p>{a}</p>"));
            }
            return tagDiv;
        }
    
        static HtmlNode GetAddChildForm(Child<SoftFile> parent)
        {
            HtmlNode addDiv = HtmlNode.CreateNode("<div id=\"addDiv\" />");
            HtmlNode addForm = HtmlNode.CreateNode($"<form action=\"/addchild&{parent.ToString()}\" method=\"POST\" enctype=\"multipart/form-data\" />");
            addForm.AppendChild(HtmlNode.CreateNode("<p>Name</p>"));
            addForm.AppendChild(HtmlNode.CreateNode("<input type=\"text\" name=\"name\" />"));
            addForm.AppendChild(HtmlNode.CreateNode("<p>Thumbnail</p>"));
            addForm.AppendChild(HtmlNode.CreateNode("<input type=\"file\" name=\"thumb\" accept=\"image/png|image/jpg|image/jpeg\"/>"));
            addForm.AppendChild(HtmlNode.CreateNode("<input type=\"submit\" value=\"Add Child\" />"));
            addForm.AppendChild(HtmlNode.CreateNode("<output for=\"name thumb\" />"));
            addDiv.AppendChild(addForm);
            return addDiv;
        }

        static HtmlNode GetSearchForm(Child<SoftFile> child)
        {
            HtmlNode searchDiv = HtmlNode.CreateNode("<div id=\"search\" />");
            HtmlNode reloadForm = HtmlNode.CreateNode("<form action=\"/reload\" method=\"GET\" />");
            HtmlNode saveForm = HtmlNode.CreateNode("<form action=\"/save\" method=\"GET\">");
            reloadForm.AppendChild(HtmlNode.CreateNode("<input type=\"submit\" value=\"Reload Database\" />"));
            saveForm.AppendChild(HtmlNode.CreateNode("<input type=\"submit\" value=\"Save Database\" />"));
            HtmlNode searchForm = HtmlNode.CreateNode($"<form action=\"/search&{child.ToString()}\" method=\"POST\" />");
            HtmlNode tagBox = HtmlNode.CreateNode($"<input type=\"text\" name=\"tags\" />");
            HtmlNode submit = HtmlNode.CreateNode($"<input type=\"submit\" value=\"Search\" />");
            HtmlNode output = HtmlNode.CreateNode($"<output for=\"tags\" />");
            searchForm.AppendChild(tagBox);
            searchForm.AppendChild(submit);
            searchForm.AppendChild(output);
            searchDiv.AppendChild(searchForm);
            searchDiv.AppendChild(reloadForm);
            searchDiv.AppendChild(saveForm);
            return searchDiv;
        }

        static HtmlNode GetSidebar(Child<SoftFile> child)
        {
            HtmlNode sidebar = HtmlNode.CreateNode("<div id=\"sidebar\" />");
            HtmlNode sidebarcontent = HtmlNode.CreateNode("<div />");
            sidebarcontent.AppendChild(HtmlNode.CreateNode("<a style=\"font-size: 25px;\" href=\"/\">Central Directory</a>"));
            if (child.Parent != null)
            {
                var p = HtmlNode.CreateNode("<p />");
                p.AppendChild(HtmlNode.CreateNode($"<a href=\"{child.Parent.ToString()}\">Up one level</a>"));
                sidebarcontent.AppendChild(p);
            }
            sidebar.AppendChild(GetSearchForm(child));
            foreach (var a in child.Children)
            {
                
                var p = HtmlNode.CreateNode($"<p />");
                p.AppendChild(HtmlNode.CreateNode($"<a href=\"{a.ToString()}\">{a.Item.Name}</a>"));
                sidebarcontent.AppendChild(p);
            }
            sidebar.AppendChild(sidebarcontent);
            sidebar.AppendChild(GetAddChildForm(child));
            return sidebar;
        }

        static HtmlNode GetChildNode(Child<SoftFile> child)
        {
            var childnode = HtmlNode.CreateNode($"<div id=\"card\" onclick=\"window.location = '{child.ToString()}'\" style=\"border: 5px solid black; margin: 5px; display: inline-grid; width: 300px; height: 300px;\"/>");
            
            childnode.AppendChild(HtmlNode.CreateNode($"<img style=\"height: 100%; width: 100%; max-height: 100%; max-width: 100%; margin:auto; object-fit: contain\" src=\"/content&{child.ToString()}\" />"));
            var span = HtmlNode.CreateNode("<span />");
            //childnode.AppendChild(imagediv);
            span.AppendChild(HtmlNode.CreateNode($"<p style=\"text-align:center;\">{child.Item.Name}</p>"));
            childnode.AppendChild(span);
            return childnode;
        }

        static byte[] Serialize(HtmlDocument doc)
        {
            var s = doc.Encoding.GetString(doc.Encoding.GetBytes(doc.DocumentNode.OuterHtml));
            return doc.Encoding.GetBytes(doc.DocumentNode.OuterHtml);
        }
    }
}
