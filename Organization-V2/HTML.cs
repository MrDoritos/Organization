using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
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
            node.AppendChild(HtmlNode.CreateNode($"<h1 id=\"filetitle\">{Encode.HtmlEncode(i.Name)}</h1>"));
            return node;
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
            dir.AppendChild(Text("Central Directory", "h1", "directorytitle"));
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

        public static HtmlNode Style { get; } = HtmlNode.CreateNode("<style>" +
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
            dir.AppendChild(Text(i.Name, "h1", "directorytitle"));
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
