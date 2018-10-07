using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace Organization_V2
{
    static class CMDHandler
    {
        private static List<string> _commandBuffer = new List<string>();
        private static string _buffer = "";
        private static int _pos = 2;

        public static void HandleKey(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    _commandBuffer.Add(_buffer);
                    ClearCharacters();
                    string[] spl = _buffer.Split(' ');
                    Console.WriteLine(_buffer);
                        string args;
                        if (spl.Length > 1)
                            args = _buffer.Remove(0, spl[0].Length + 1);
                        else
                            args = _buffer.Remove(0, spl[0].Length);
                        HandleCommand(spl[0], args);
                    
                    _buffer = "";
                    _pos = _commandBuffer.Count - 1;
                    return;
                case ConsoleKey.Backspace:
                    if (_buffer.Length > 0)
                    {
                        _buffer = _buffer.Substring(0, _buffer.Length - 1);
                        WriteCharacters(_buffer + ' ');
                    }
                    return;
                case ConsoleKey.UpArrow:
                    return;
                case ConsoleKey.DownArrow:
                    return;
                default:
                    if (_buffer.Length < Console.WindowWidth - 3)
                    { _buffer += key.KeyChar; WriteCharacter(key.KeyChar, _buffer.Length); }
                    return;
            }
        }

        static public void HandleCommand(string command, string args)
        {
            try
            {
                switch (command.ToLower())
                {
                    case "cd":
                        ChangeDirectory(args);
                        return;
                    case "cd..":
                        if (selected.Parent != null)
                        { selected = selected.Parent; }
                        else { selected = Program.CentralDirectory.Self; }
                        DisplaySelected(args);
                        return;
                    case ".":
                    case "ls":
                        DisplaySelected(args);
                        return;
                    case "all":
                        DisplayCentralIndex();
                        return;
                    case "mkdir":
                        MakeDir(args);
                        return;
                    case "hash":
                        HashFile(args);
                        return;
                    case "mkfile":
                        MakeFile(args);
                        return;
                    case "addfile":
                        AddFile(args);
                        return;
                    case "where":
                        Where(args);
                        return;
                    case "save":
                        Save(args);
                        return;
                    case "load":
                        Load(args);
                        return;
                    case "tag":
                        Tag(args);
                        return;
                    case "rmdir":
                        DeleteDirectory(args);
                        return;
                    case "thumb":
                        ChangeThumbnail(args);
                        return;
                    case "ref":
                        Reference();
                        return;
                    case "styles":
                        HTML.Style = HtmlAgilityPack.HtmlNode.CreateNode($"<style>{System.Web.HttpUtility.HtmlEncode(File.ReadAllText((args.Length > 0 ? args : "styles.css")))}</style>");
                        return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"'{command}' threw exception!\r\n{e.Message}");
            }

        }

        public static SoftDirectory selected;

        static void Reference()
        {
            SoftFile fileFirst = QueryFile();
            SoftFile fileRefer = QueryFile();
            if (fileFirst  ==  null || fileRefer == null) { return; }
            fileFirst.AddReference(fileRefer);
            Console.WriteLine($"{fileFirst} ({fileFirst.Id}) => {fileRefer} ({fileRefer.Id})");
            if (fileFirst.Equals(fileRefer))
            {
                Console.WriteLine("This file equals the reference");
            }
            else
            {
                Console.WriteLine("The file does not equal the reference");
            }
        }

        static SoftFile QueryFile()
        {
            int first;
            SoftFile fileFirst;
            while (true)
            {
                first = QueryInt("File Id?");
                fileFirst = Program.CentralDirectory.GetFile(first);
                if (fileFirst == null) { Console.WriteLine("File doesn't exist"); }
                else
                {
                    if (QueryBool("Are you sure?"))
                    {
                        break;
                    }
                    else
                    {
                        if (QueryBool("Quit?"))
                        {
                            return null;
                        }
                    }
                }
            }
            Console.WriteLine($"Selected {fileFirst} ({fileFirst.Id})");
            return fileFirst;
        }

        static void ChangeThumbnail(string args)
        {
            string[] a = args.Split(' ');
            if (int.TryParse(args.Split(' ')[0], out int i))
            {
                var d = Program.CentralDirectory.FindThumbnail(i);

                if (a != null)
                {
                    string p;
                    if (a.Length > 1)
                        p = args.Remove(0, a[0].Length + 1);
                    else
                        p = args.Remove(0, a[0].Length);
                    d.ThumbnailPath = p;
                    Console.WriteLine($"Thumbnail path set to '{p}'");
                }
                else
                {
                    Console.WriteLine("No thumbnail holders contain that id");
                }
            }
            else
            {
                Console.WriteLine("Specify Id");
            }
        }

        static void Tag(string args)
        {

            if (int.TryParse(args.Split(' ')[0], out int i))
            {
                var a = Program.CentralDirectory.GetFile(i);

                if (a != null)
                {
                    string[] newtags = args.Remove(0, args.Split(' ')[0].Length).Split(',').Where(n => n.Length > 1).Select(n => n.Trim()).ToArray();
                    a.AddTags(newtags);
                    Tags(a);
                }
                else
                {
                    Console.WriteLine("File does not exist");
                }                
            }
            else
            {
                string[] newtags = args.Split(',').Where(n => n.Length > 1).Select(n => n.Trim()).ToArray();
                selected.AddTags(newtags);
                Tags(selected);
            }
        }

        static void Tags(ITaggable t)
        {
            var tags = t.Tags;
            Console.Write("Tags:");
            for (int i = 0; i < tags.Count - 1; i++)
                Console.Write($" {tags[i]},");
            Console.WriteLine(" " + tags[tags.Count - 1]);
        }

        static void Save(string args)
        {
            Database.Save(args, Program.CentralDirectory);
            Console.WriteLine($"Saved as {args}!");
        }

        static void Load(string args)
        {
            var a = Database.Load(args);
            Console.WriteLine($"Loaded! {a.Directories.Count} top level directories, {a.Files.Count} central index files");
            Program.CentralDirectory = a;
        }

        static void Where(string args)
        {
            if (int.TryParse(args, out int i))
            {
                var a = Program.CentralDirectory.GetFile(i);
                if (a != null)
                {
                    foreach (var c in Search(a))
                        Console.WriteLine($"[{c.Id}] {c}");
                }
                else
                {
                    Console.WriteLine("File does not exist");
                }
            }
            else
            {
                Console.WriteLine("Specify Id");
            }
        }

        static IEnumerable<SoftDirectory> Search(SoftFile i)
        {
            Console.WriteLine("Search by hash and id");
            List<SoftDirectory> results = new List<SoftDirectory>();
            foreach (var a in Program.CentralDirectory.Directories)
                Directories(results, a, i);
            return results;
        }

        static void Directories(List<SoftDirectory> ok, SoftDirectory cur, SoftFile i)
        {
            foreach (var a in cur.SubDirectories)
                Directories(ok, a, i);
            foreach (var a in cur.SoftFiles)
                if (i == a) { ok.Add(cur); }
        }

        static void DisplayCentralIndex()
        {
            var a = Program.CentralDirectory.Files;
            int namelen = 30;
            int idpos = 32;
            Console.Write($"[Central Index] {a.Count} Files");
            Console.SetCursorPosition(idpos, Console.CursorTop);
            Console.WriteLine("Id");
            foreach (var a1 in a)
                PrintFiles(a1.Name, a1.Id,namelen, idpos);
        }

        static void MakeFile(string name)
        {
            if (name.Length < 1 || name.Contains('/') || selected.SoftFiles.Any(n => n.Name.ToLower() == name.ToLower())) { Console.WriteLine("File error!"); return; }
            var a =/*selected.AddFile(*/Program.CentralDirectory.AddFile(name)/*)*/;
            Console.WriteLine($"Added: [{a.Id}] {a.Name}");
        }

        static void AddFile(string args)
        {
            if (int.TryParse(args, out int i))
            {
                var a = Program.CentralDirectory.GetFile(i);
                if (a != null)
                {
                    if (!selected.Exists(a as IHashable) && !selected.Exists(a.Id))
                        selected.AddFile(a);
                    else
                        Console.WriteLine("File already exists");
                }
                else
                {
                    Console.WriteLine("File does not exist in index. Use mkfile");
                }
            }
            else
            {
                Console.WriteLine("Specify Id");
            }
        }

        static void HashFile(string args)
        {
            var a = args.Split(' ');
            bool err = false;
            if (a.Length > 1)
                if (int.TryParse(a[0], out int i))
                {
                    var b = Program.CentralDirectory.GetFile(i);
                    if (b != null)
                    {
                        string p;
                        if (a.Length > 1)
                            p = args.Remove(0, a[0].Length + 1);
                        else
                            p = args.Remove(0, a[0].Length);
                        if (File.Exists(p))
                        {
                            b.Hash(p);
                            Console.WriteLine(b.Hashes);
                            Console.WriteLine($"Successfully hashed!");
                            
                        }
                        else
                        {
                            Console.WriteLine($"Non existent file: {p}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Non existent Id: {i}");
                    }
                }
                else
                {
                    err = true;
                }
            else
                err = true;
            if (err)

            Console.WriteLine("Specify file Id\r\nusage: hash (id) (FQ-filepath)");
        }
        
        static void MakeDir(string name)
        {
            if (name.Contains('/')) { Console.WriteLine("Invalid character '/'"); return; }
            if (selected.Id == 0)
            {
                Program.CentralDirectory.AddDirectory(Program.CentralDirectory.CreateDirectory(name));
            }
            else
            {
                selected.AddDirectory(Program.CentralDirectory.CreateDirectory(name));
            }
            DisplaySelected("");
        }

        static void PrintFiles(string name, int id, int namelen, int idpos)
        {
            Console.Write('[');
            Console.Write(name.Take((name.Length > namelen) ? namelen : name.Length).ToArray());
            Console.Write(']');
            Console.SetCursorPosition(idpos, Console.CursorTop);
            Console.WriteLine(id);
        }

        static int DisplaySelected(string args)
        {
            Console.WriteLine($"[{selected.Name}] {selected} | {selected.Id}");
            Console.ForegroundColor = ConsoleColor.Blue;
            foreach (var a in selected.SubDirectories)
                Console.WriteLine($"{a.Id} [{a.Name}] {selected}");
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var a in selected.SoftFiles)
                Console.WriteLine($"{a.Id} [{a.Name}] {selected}/{a.Name}");
            Console.ForegroundColor = ConsoleColor.Gray;
            return 0;
        }

        static int ChangeDirectory(string args)
        {
            SoftDirectory dir;
            if (args.Length < 1) return 0;
            if (args[0] == '/') dir = Program.CentralDirectory.FindDir(args);
            else
                dir = selected.FindDir(args);
            if (dir == null) { Console.WriteLine("Unknown directory or invalid path"); }
            else { selected = dir; }
            DisplaySelected(args);
            return 0;
        }

        static void DeleteDirectory(string args)
        {
            SoftDirectory dir;
            if (args.Length < 1) return;
            if (args[0] == '/') dir = Program.CentralDirectory.FindDir(args);
            else dir = selected.FindDir(args);
            if (dir == null) { Console.WriteLine($"Unknown directory or invalid path '{args}'"); }
            else
            {
                if (QueryBool("Are you sure?"))
                {
                    if (selected.Id == 0) { Program.CentralDirectory.RemoveDirectory(dir); }else
                    selected.RemoveSubDirectory(dir);
                    Console.WriteLine("Deleted.");
                }
            }
        }

        static public bool QueryBool(string question)
        {
            ConsoleKeyInfo k;
            while (true)
            {
                Console.Write(question + " [y/n] ");
                k = Console.ReadKey();
                if (k.Key == ConsoleKey.Y || k.Key == ConsoleKey.N) { Console.WriteLine();  return (k.Key == ConsoleKey.Y); }
            }
        }

        static public int QueryInt(string question)
        {
            Console.WriteLine(question);
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int i))
                {
                    return i;
                }
                else
                {
                    Console.WriteLine("Invalid integer");
                }
                
            }
        }

        public static void ClearCharacters()
        {
            int bottom = Console.WindowHeight + Console.WindowTop - 1;
            int last = Console.CursorTop;
            char[] erase = new char[Console.WindowWidth - 1];
            for (int b = 0; b < erase.Length; b++)
                erase[b] = ' ';
            Console.SetCursorPosition(0, bottom);
            Console.Write(erase, 0, erase.Length);
            Console.SetCursorPosition(0, last);
        }

        public static void WriteCharacter(char i, int pos)
        {
            int bottom = Console.WindowHeight + Console.WindowTop - 1;
            int last = Console.CursorTop;
            Console.SetCursorPosition(pos, bottom);
            Console.Write(i);
            Console.SetCursorPosition(0, last);
        }

        public static void WriteCharacters(string i)
        {
            int bottom = Console.WindowHeight + Console.WindowTop - 1;
            int last = Console.CursorTop;
            Console.SetCursorPosition(0, bottom);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(">");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(i + " ");
            Console.SetCursorPosition(0, last);
        }
    }
}
