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
                    case "remtag":
                        RemTag(args);
                        return;
                    case "tags":
                        Tags(selected as ITaggable ?? Program.CentralDirectory as ITaggable);
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
                    case "bench":
                        Bench(args);
                        return;
                    case "dir":
                        QueryDir(args);
                        return;
                    case "file":
                        FileInfo(args);
                        return;
                    case "move":
                        MoveDirectory(args);
                        return;
                    case "copy":
                        MoveDirectory(args, true);
                        return;
                    case "search":
                        Search(args);
                        return;
                    case "rename":
                        Rename(args);
                        return;
                    case "styles":
                        HTML.Style = HtmlAgilityPack.HtmlNode.CreateNode($"<style>{System.Web.HttpUtility.HtmlEncode(File.ReadAllText((args.Length > 0 ? args : "styles.css")))}</style>");
                        return;
                    case "import":
                        Import(args);
                        return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"'{command}' threw exception!\r\n{e.Message}");
            }

        }

        static void Import(string args)
        {
            if (Directory.Exists(args))
            {
                SoftDirectory s = null;
                if (QueryBool("Are you sure you want to import?"))
                    if ((s = Import(new DirectoryInfo(args))) != null) { Program.CentralDirectory.AddDirectory(s); }
                
            }
        }

        static SoftDirectory Import(DirectoryInfo i)
        {
            var d = i.GetDirectories();
            var e = i.GetFiles();
            SoftDirectory[] ss = new SoftDirectory[d.Length];
            SoftFile[] ee = new SoftFile[e.Length];
            for (int m = 0; m < d.Length; m++)
                ss[m] = Import(d[m]);
            for (int g = 0; g < e.Length; g++)
                ee[g] = Import(e[g], Program.CentralDirectory);
            return new SoftDirectory(Program.CentralDirectory.NextId, i.Name, ss, ee, new string[0]);
        }

        static SoftFile Import(FileInfo i, IIndex d)
        {
            var hash = new Hash(i.FullName);
            return d.AddFile(Program.CentralDirectory.Find(hash) ?? new SoftFile(Program.CentralDirectory.NextId, i.Name, "", new string[0], hash));                
        }

        static void Rename(string args)
        {
            IThumbable a;
            int i;
            if (int.TryParse(args.Split(' ')[0], out i))
            {
                a = Program.CentralDirectory.FindThumbnail(i);
            }
            else
            {
                a = Program.CentralDirectory.FindThumbnail(i = QueryInt("Id? (dir or file)"));
            }
            if (a == null) { Console.WriteLine("Not found"); return; }
            string name = GetToNextWhiteSpace(args);
            while (name.Length < 1)
            {
                Console.Write("New name: ");
                name = Console.ReadLine();
            }
            a.Name = name;
        }

        public static string GetToNextWhiteSpace(string i)
        {
            string[] s = i.Split(' ');
            if (s.Length < 1) return i;
            return (i.Remove(0, s[0].Length + 1));
        }

        public static SoftDirectory selected;

        static void PrintDir(SoftDirectory dir)
        {
            Console.WriteLine($"{dir.Name} ({dir.Id})");
            Console.WriteLine(dir);
            Console.ForegroundColor = ConsoleColor.Blue;
            foreach (var a in dir.SubDirectories)
                Console.WriteLine($"{a.Name} ({a.Id})");
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var a in dir.SoftFiles)
                Console.WriteLine($"{a.Name} ({a.Id})");
            Console.ForegroundColor = ConsoleColor.Gray;
            Tags(dir);
        }

        static void FileInfo(string args)
        {
            if (int.TryParse(args, out int i))
            {
                var a = Program.CentralDirectory.FirstOrDefault(i);
                if (a == null) { Console.WriteLine("Could not find file"); return; }
                else
                {
                    FileInfo(a);
                }
            }
            else
            {
                Console.WriteLine("Specify an Id");
            }
        }

        static void FileInfo(SoftFile f)
        {
            Console.WriteLine($"{f.Name} ({f.Id})");
            Console.WriteLine(f.Hashes);
            Console.WriteLine($"Thumbnail Path: {f.ThumbnailPath})");
            Tags(f);
        }

        static void QueryDir(string args)
        {
            SoftDirectory dir;
            if (int.TryParse(args, out int i))
            {
                dir = Program.CentralDirectory.FindDir(i);
                if (dir == null)
                    Console.WriteLine("Directory not found");
                else
                {
                    PrintDir(dir);
                }
            }
            else
            {
                PrintDir(QueryDirectory(true));
            }
        }

        static void Bench(string args)
        {
            int iterations = 10000;
            if (int.TryParse(args, out int s)) iterations = s;
            int i = 0;
            int num = 95;
            DateTime start;
            DateTime starttwo;
            start = DateTime.Now;
            for (; i < iterations; i++) { }
            //Program.CentralDirectory.OpenCLFirstOrDefault(num, false);
            TimeSpan openclstarttofinish = DateTime.Now.Subtract(start);
            i = 0;
            starttwo = DateTime.Now;
            for (; i < iterations; i++)
                Program.CentralDirectory.FirstOrDefault(num);
            TimeSpan ccc = DateTime.Now.Subtract(starttwo);
            Console.WriteLine($"Benchmark over {iterations} iterations");
            PrintTime(DateTime.Now.Subtract(start));
            Console.WriteLine("OpenCL Query");
            PrintTime(openclstarttofinish, iterations);
            Console.WriteLine("Linq");
            PrintTime(ccc, iterations);
        }

        static void PrintTime(TimeSpan i, int divisor = 1)
        {
            divisor = (divisor > 0 ? divisor : 1);
            Console.WriteLine($"{(i.TotalSeconds > 0 ? i.TotalSeconds / divisor : 0)} Seconds");
            Console.WriteLine($"{(i.TotalMilliseconds > 0 ? i.TotalMilliseconds / divisor : 0)} Milliseconds");
            Console.WriteLine($"{(i.TotalMilliseconds > 0 ? i.Ticks / divisor : 0)} Ticks");
        }

        static void Reference()
        {
            SoftFile fileFirst = QueryFile();
            SoftFile fileRefer = QueryFile();
            if (fileFirst == null || fileRefer == null) { return; }
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

        static void Search(string args)
        {
            int index = 0;
            bool dirs = false;
            bool files = true;
            bool curdir = false;
            SearchModes searchMode = SearchModes.nameonly;
            if (args.Length < 1) { Console.WriteLine(SearchHelp); return; }
            searchMode = (SearchModes)LoadInt(args, ref index);
            //curdir = LoadBool(args, true, ref index);
            //files = LoadBool(args, true, ref index);
            //dirs = LoadBool(args, true, ref index);
            string searchtext = args.Remove(0, index).Trim();
            List<KeyValuePair<int, IID>> values = new List<KeyValuePair<int, IID>>();
            if (curdir) { selected.Search(searchtext, dirs, files, values); }
            else { Program.CentralDirectory.Search(searchtext, dirs, files, values); }
            if (files && !dirs)
            {
                foreach (var a in values.OrderByDescending(n => n.Key))
                {
                    var b = a.Value as SoftFile;
                    Console.Write(a.Key + "|"); PrintFiles(b.Name, b.Id, 10, 10);
                }
            }
            else
                if (!files && dirs)
            {
                foreach (var a in values.OrderByDescending(n => n.Key))
                {
                    var b = a.Value as SoftDirectory;
                    Console.WriteLine($"{a.Key}|{b.Name} ({b.Id})");
                }
            } else
                if (files && dirs)
            {
                foreach (var a in values)
                {
                    var b = a.Value as IID;
                    Console.WriteLine($"{a.Key}|{b.Id}");
                }
            }
        }

        static bool LoadBool(string a, bool skip, ref int startindex)
        {
            a = a.Trim();
            if (startindex + 4 < a.Length && a.Substring(startindex, 4).ToLower() == "true") { startindex += 4; return true;  }
            if (startindex + 5 < a.Length && a.Substring(startindex, 5).ToLower() == "false") { startindex += 5; return false; }
            startindex += a.Split(' ')[0].Length;
            return false;
        }

        static int LoadInt(string a, ref int startindex)
        {
            a = a.Trim();
            var s = a.Split(' ')[0];
            if (int.TryParse(s, out int i))
            {
                startindex += s.Length;
                return i;
            }
            return 0;
        }

        static string SearchHelp = "" +
            "Search\r\n" +
            "search (searchmode) [curdir] [files] [dirs] (search text)\r\n" +
            "--searchmodes--\r\n" +
            "Name Only = 0\r\n" +
            "Tags Only = 1\r\n" +
            "Name and Tags = 2\r\n" +
            "Tags thorough = 3\r\n" +
            "Names thorough = 4\r\n" +
            "Tags and Names thorough = 5";

        private enum SearchModes
        {
            nameonly = 0,
            tags = 1,
            nameandtags = 2,
            tagsthorough = 3,
            namethorough = 4,
            nameandtagsthorough = 5,
        }

        static void MoveDirectory(string args, bool copy = false)
        {
            int src = 0;
            int dest = 0;
            SoftDirectory srcdir;
            SoftDirectory destdir;
            string[] splt = args.Split(' ');
            if (splt.Length < 2) return;
            if (int.TryParse(splt[0], out src))
            {
                srcdir = Program.CentralDirectory.FindDir(src);
                if (srcdir == null)
                {
                    Console.WriteLine($"Could not find directory {src}");
                    srcdir = QueryDirectory(true);
                }
                if (int.TryParse(splt[1], out dest))
                {
                    destdir = Program.CentralDirectory.FindDir(dest);
                    if (destdir == null)
                    {
                        Console.WriteLine($"Could not find directory {dest}");
                        destdir = QueryDirectory(true);
                    }
                }
                else
                {
                    Console.WriteLine($"Could not find directory '{splt[1]}'");
                    destdir = QueryDirectory(true);
                }
            }
            else
            {
                Console.WriteLine($"Could not find directory '{splt[1]}'");
                srcdir = QueryDirectory(true);
                if (int.TryParse(splt[1], out dest))
                {
                    destdir = Program.CentralDirectory.FindDir(dest);
                    if (destdir == null)
                    {
                        Console.WriteLine($"Could not find directory {dest}");
                        destdir = QueryDirectory(true);
                    }
                }
                else
                {
                    Console.WriteLine($"Could not find directory '{splt[1]}'");
                    destdir = QueryDirectory(true);
                }
            }
            if (srcdir.Id == 0)
            {
                Console.WriteLine("Cannot move the central directory");
            }
            if (destdir.Parent != null && destdir.Parent.Id == srcdir.Id)
            {
                Console.WriteLine("Cannot move parent to self");
            }            
            else
            {
                if (copy)
                    destdir.AddDirectory(srcdir);
                else
                {
                    if (srcdir.Parent != null)
                    {
                        srcdir.Parent.RemoveDirectory(srcdir);
                    }
                    else
                    {
                        Program.CentralDirectory.RemoveDirectory(srcdir);
                    }
                    destdir.AddDirectory(srcdir);
                }
                Console.WriteLine($"{destdir.Name} ({destdir.Id})/{srcdir.Name} ({srcdir.Id})");

            }
        }

        static SoftDirectory QueryDirectory(bool continueIfNull = false)
        {
            SoftDirectory dir = null;
            while (true)
            {
                dir = Program.CentralDirectory.FindDir(QueryInt("Directory Id?"));
                if (dir != null)
                Console.WriteLine($"Selected {dir.Name} ({dir.Id})");
                if (!continueIfNull || dir != null) break; else Console.WriteLine("Invalid Directory");
            }
            return dir;
        }

        static void Waterfall(DateTime start, DateTime end, params KeyValuePair<string, TimeSpan>[] timespans)
        {
            TimeSpan startToFinish = new TimeSpan(timespans.Sum(n => n.Value.Ticks));

            TimeSpan total = end.Subtract(start);

            Console.WriteLine($"{total.Ticks} total ticks\r\nRange of {startToFinish.Ticks} ticks");

            foreach (var a in timespans)
                PrintFall(a.Value.Ticks, 30, a.Key);
        }

        static void PrintFall(float p, int n, string name)
        {
            int ch = (int)Math.Ceiling(p / n);
            Console.Write(name.Take(10).ToArray());
            Console.SetCursorPosition(10, 0);
            Console.Write('|');
            for (int i = 0; i < ch; i++)
                Console.Write('-');
            Console.Write('>');
            for (int i = ch; i < n; i++)
                Console.Write(' ');
            Console.Write(' ');
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

        static void RemTag(string args)
        {
            if (int.TryParse(args.Split(' ')[0], out int i))
            {
                var a = Program.CentralDirectory.FindThumbnail(i) as ITaggable;

                if (a != null)
                {
                    string[] remtags = args.Remove(0, args.Split(' ')[0].Length).Split(',').Where(n => n.Length > 1).Select(n => n.Trim().ToLower()).ToArray();
                    Console.Write("Before: ");
                    Tags(a);
                    a.RemoveTags(remtags);
                    Console.Write("After: ");
                    Tags(a);
                }
                else
                {
                    Console.WriteLine("No tag holders contain that id");
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
            if (t.Tags.Count > 0)
            {
                Console.Write("Tags: ");
                Console.WriteLine(t.Tags.Aggregate((a, b) => $"{a}, {b}"));
            }
            //for (int i = 0; i < tags.Count - 1; i++)
            //    Console.Write($" {tags[i]},");
            //Console.WriteLine(" " + tags[tags.Count - 1]);
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
            bool altcolr = true;
            foreach (var a1 in a)
                PrintFiles(a1.Name, a1.Id,namelen, idpos, (altcolr = !altcolr));
        }

        static string QueryString(string question)
        {
            Console.WriteLine(question);
            return Console.ReadLine();
        }

        static void MakeFile(string name)
        {
            SoftFile a;
            if (QueryBool("Import file?"))
            {
                while (!File.Exists(name)) { name = Console.ReadLine(); }
                FileInfo fileInfo = new FileInfo(name);
                var b = new Hash(name);
                if (Program.CentralDirectory.Exists(b)) { Console.WriteLine("A file with the same hash already exists"); return; }
                string fname = fileInfo.Name.Split('.')[0];
                if (QueryBool($"Keep file name '{fname}'"))
                a = Program.CentralDirectory.AddFile(fname);
                else
                {
                    Console.Write("New name: ");
                    a = Program.CentralDirectory.AddFile(Console.ReadLine());
                }
                try
                {
                    a.Hash(b);
                    Console.WriteLine(a.Hashes);
                }
                catch (Exception)
                {
                    Console.WriteLine("Could not hash");
                }                
            }
            else
            {
                if (name.Length < 1 || name.Contains('/') || selected.SoftFiles.Any(n => n.Name.ToLower() == name.ToLower())) { Console.WriteLine("File error!"); return; }
                a =/*selected.AddFile(*/Program.CentralDirectory.AddFile(name)/*)*/;
            }
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

        static void PrintFiles(string name, int id, int namelen, int idpos, bool altclr = false)
        {
            ConsoleColor lastclr = ConsoleColor.White;
            if (altclr)
            {
                lastclr = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.Write('[');
            Console.Write(name.Take((name.Length > namelen) ? namelen : name.Length).ToArray());
            Console.Write(']');
            Console.SetCursorPosition(idpos, Console.CursorTop);
            Console.WriteLine(id);
            if (altclr) Console.ForegroundColor = lastclr;
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
            for (int b = 0; b < 5; b++)
                if (int.TryParse(Console.ReadLine(), out int i))
                {
                    return i;
                }
                else
                {
                    Console.WriteLine("Invalid integer");
                }
            throw new Exception("Needed to exit command, somehow");
            
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
