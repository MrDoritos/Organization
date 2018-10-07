using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
using System.Linq;
using Encode = System.Web.HttpUtility;

namespace Organization_V2
{
    public static class HTML
    {
        public static HtmlNode IndexPage(CentralIndex i)
        {
            HtmlNode node = HtmlNode.CreateNode("<div id=\"indexpage\" />");
            node.AppendChild(HtmlNode.CreateNode($"<h1 id=\"indextitle\">{Encode.HtmlEncode("Index of /")}</h1>"));
            foreach (var a in i.Files)
                node.AppendChild(FileStrip(a));
            return node;
        }        

        public static HtmlNode FilePage(SoftFile i)
        {
            HtmlNode node = HtmlNode.CreateNode("<div id=\"filepage\" />");
            var head = node.AppendChild(HtmlNode.CreateNode("<div id=\"fileheader\" />"));
            head.AppendChild(HtmlNode.CreateNode($"<h1 id=\"filetitle\">{Encode.HtmlEncode(i.Name)}</h1>"));
            if (i.ThumbnailExists) head.AppendChild(Thumbnail(i, i.Id.ToString()));
            //node.AppendChild(Thumbnail(i, i.Id.ToString()));
            var tagsandref = node.AppendChild(HtmlNode.CreateNode("<div id=\"tagsandref\" />"));
            var refrr = i.References;
            
            if (refrr.Count > 0)
                tagsandref.AppendChild(List("div", "References", "filereferencelist", refrr.Select(n => $"<a id=\"filereferences\" href=\"/file&{n.Id}\">{Encode.HtmlEncode(n.Name)}</a>").ToArray()));
            node.AppendChild(HtmlNode.CreateNode($"<p id=\"hashes\">{i.Hashes}</p>"));
            var tags = i.Tags;
            if (tags.Count > 0)
                tagsandref.AppendChild(List("div", "Tags", "taglist", tags.Select(n => $"<p id=\"tag\">{n}</p>").ToArray()));
            return node;
        }

        public static HtmlNode List(string tag, string title, string id, params string[] vs)
        {
            HtmlNode node = HtmlNode.CreateNode($"<{tag} id=\"{id}\" />");
            node.AppendChild(HtmlNode.CreateNode("<div id=\"listtitle\" />")).AppendChild(HtmlNode.CreateNode($"<h1>{Encode.HtmlEncode(title)}</h1>"));
            var div = node.AppendChild(HtmlNode.CreateNode("<div id=\"list\" />"));
            foreach (var a in vs)
                div.AppendChild(HtmlNode.CreateNode(a));
            return node;
        }

        public static HtmlNode Thumbnail(IThumbable i, string thumbpath)
        {
            //if (i == null) return HtmlNode.CreateNode("<a />");
            return HtmlNode.CreateNode($"<img id=\"thumbnail\" src=\"/content&{thumbpath}\" />");
        }

        public static HtmlNode IndexPage(SoftDirectory i)
        {
            HtmlNode node = HtmlNode.CreateNode("<div id=\"indexpage\" />");
            node.AppendChild(HtmlNode.CreateNode($"<h1 id=\"indextitle\">{Encode.HtmlEncode($"Index of {i}")}</h1>"));
            foreach (var a in i.SoftFiles)
                node.AppendChild(FileStrip(a));
            return node;
        }

        public static HtmlNode FileStrip(SoftFile i)
        {
            HtmlNode node = HtmlNode.CreateNode("<div id=\"file\" />");
            HtmlNode name = Text(i.Name, "div", "filename");
            node.AppendChild(name);
            return node;
        }

        private static HtmlNode Text(string text, string tag)
        {
            return HtmlNode.CreateNode($"<{tag}>{Encode.HtmlEncode(text)}</{tag}>");
        }

        private static HtmlNode Text(string text, string tag, string id)
        {
            return HtmlNode.CreateNode($"<{tag} id=\"{id}\">{Encode.HtmlEncode(text)}</{tag}>");
        }

        public static HtmlNode CentralDirectoryPage(CentralDirectory i)
        {
            HtmlNode dir = HtmlNode.CreateNode("<div id=\"directory\" />");
            var div = dir.AppendChild(HtmlNode.CreateNode("<div id=\"fileheader\" />"));
            div.AppendChild(Text("Central Directory", "h1", "directorytitle"));
            HtmlNode cards = dir.AppendChild(HtmlNode.CreateNode("<div id=\"filecards\" />"));
            foreach (var a in i.Directories)
                cards.AppendChild(DirCard(a));
            return dir;
        }

        public static HtmlNode FileCard(SoftFile i)
        {
            HtmlNode card = HtmlNode.CreateNode("<div id=\"filecard\" />");
            (card.AppendChild(Text("", "div", "filename"))).AppendChild(HtmlNode.CreateNode($"<a href=\"/file&{i.Id}\">{i.Name}</a>"));
            //HtmlNode name = Text(i.Name, "div", "filename");
            //card.AppendChild(name);
            return card;
        }

        public static HtmlNode DirCard(SoftDirectory i)
        {
            HtmlNode card = HtmlNode.CreateNode("<div id=\"dircard\" />");
            (card.AppendChild(Text("", "div", "dirname"))).AppendChild(HtmlNode.CreateNode($"<a href=\"/dir&{i.URI()}\">{i.Name}</a>")/*Text($"/dir&{i}", "a", "dirname")*/);            
            return card;
        }

        public static HtmlNode Style { get; set; } = HtmlNode.CreateNode("<style>" +
            "#directorytitle { text-align:center; }" +
            "#directory {background-color:rgba(155,155,255,0.5);}" +
            "html {background-color:black; font-family:consolas;}" +
            "#filecard {background-color:rgba(100,100,100,0.2); border:5px solid black; }" +
            "#filecard:hover {background-color:rgba(100,100,100,0.7); }" +
            "#dircard {background-color:rgba(100,100,100,0.2); border:5px solid black; }" +
            "#dircard:hover {background-color:rgba(100,100,100,0.7); }" +
            "</style>");

        public static HtmlNode DirectoryPage(SoftDirectory i)
        {
            HtmlNode dir = HtmlNode.CreateNode("<div id=\"directory\" />");
            var div = dir.AppendChild(HtmlNode.CreateNode("<div id=\"fileheader\">"));
            var ass = div.AppendChild(Text(i.Name, "h1", "directorytitle"));
            //dir.AppendChild(Text(i.Name, "h1", "directorytitle")).AppendChild(Thumbnail(i, i.URI()));
            if (i.ThumbnailExists) div.AppendChild(Thumbnail(i, i.URI()));
            HtmlNode cards = dir.AppendChild(HtmlNode.CreateNode("<div id=\"filecards\" />"));
            foreach (var a in i.SubDirectories)
                cards.AppendChild(DirCard(a));
            foreach (var a in i.SoftFiles)
                cards.AppendChild(FileCard(a));
            return dir;
        }

        public static byte[] Serialize(this HtmlNode i)
        {
            return Encoding.Default.GetBytes(i.OuterHtml);
        }

        public static byte[] Serialize(this HtmlDocument i)
        {
            return i.Encoding.GetBytes(i.DocumentNode.OuterHtml);
        }
    }
}
