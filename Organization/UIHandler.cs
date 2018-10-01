using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Organization.Objects;
using Indexing;

namespace Organization
{
    static class UIHandler
    {
        private static string _buffer = "";
        private static Child<SoftFile> _selectedNode = Program.CentralDirectory;

        public static void HandleKey(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Backspace:
                    if (_buffer.Length > 0) { _buffer = _buffer.Substring(0, _buffer.Length - 1); WriteCharacters(_buffer); }
                    break;
                case ConsoleKey.Enter:
                    HandleCmd(_buffer);
                    _buffer = "";
                    ClearCharacters();
                    WriteCharacters("");
                    break;
                default:
                    if (_buffer.Length < Console.WindowWidth - 3)
                    {
                        WriteCharacters((_buffer += key.KeyChar));
                    }
                    break;
            }
        }

        public static void HandleCmd(string cmd)
        {
            Console.WriteLine(">" + cmd);
            string[] split = cmd.Split(' ');
            
            if (split.Length > 0)
            {
                string arguments = cmd.Remove(0, split[0].Length);
                if (arguments.Length > 0)
                    arguments = arguments.Remove(0, 1);
                //Console.WriteLine($"{cmd}:{arguments}");
                try
                {
                    switch (split[0].ToLower())
                    {
                        case "save":
                            if (arguments.Length < 1)
                                Database.Save(Program.CentralDirectory, "database.ian");
                            else
                                Database.Save(Program.CentralDirectory, arguments);
                            Console.WriteLine($"Success! {Program.CentralDirectory.AllChildCount} children saved!");
                            return;
                        case "load":
                            if (arguments.Length < 1)
                                Program.CentralDirectory.Children = Database.Load("database.ian").Children;
                            else
                                Program.CentralDirectory.Children = Database.Load(arguments).Children;
                            Console.WriteLine($"Success! {Program.CentralDirectory.AllChildCount} children loaded!");
                            return;
                        case "status":
                            if (Program.server != null)
                            {
                                Console.WriteLine($"Server: {(Program.server.Connected ? "connected" : "disconnected")}");
                                Console.WriteLine($"Bind: {(Program.server.Bind)}");
                            }
                            else
                            {
                                Console.WriteLine($"Server is null");
                            }
                            return;
                        case "tags":
                            if (_selectedNode == null)
                            {
                                Console.WriteLine("No node selected");
                            }
                            else
                            {
                                if (_selectedNode.Item == null)
                                {
                                    Console.WriteLine("No item for this node");
                                }
                                else
                                {
                                    PrintTags(_selectedNode);
                                }
                            }
                            return;
                        case "tag":
                            if (_selectedNode == null)
                            {
                                Console.WriteLine("No node selected");
                            }
                            else
                            {
                                Tag(_selectedNode, arguments);
                            }
                            return;
                        case "cd":
                        case "sel":
                        case "select":
                            if (_selectedNode == null)
                            {
                                Console.WriteLine("No node selected");
                            }
                            else
                            {
                                if (int.TryParse(arguments, out int selected))
                                {
                                    int hch = _selectedNode.ChildCount;
                                    if (selected > hch || selected < 1)
                                    {
                                        Console.WriteLine($"{selected} is out of bounds of the children! IDIOT!!!!");
                                    }
                                    else
                                    {
                                        _selectedNode = _selectedNode.Children[selected - 1];
                                        
                                        Console.WriteLine($"Selected [{selected}] ({_selectedNode.Name}) {_selectedNode.Item.Name}");
                                        var dchildren = _selectedNode.Children;
                                        if (dchildren.Count > 0)
                                        {
                                            PrintChildren(_selectedNode);
                                            //Console.WriteLine("Available children");
                                            //for (int i = 0; i < dchildren.Count; i++)
                                            //    Console.WriteLine($"[{i}] ({dchildren[i]}) {dchildren[i].Item.Name}");
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Invalid integer {arguments}");
                                }
                            }
                            return;
                        case ".?":
                            if (_selectedNode == null)
                            {
                                Console.WriteLine("No node selected");
                            }
                            else
                            {
                                if (_selectedNode.Item == null)
                                {
                                    Console.WriteLine(_selectedNode);
                                }
                                else
                                {
                                    Console.WriteLine($"({_selectedNode}) {_selectedNode.Item.Name}");
                                }
                            }
                            return;
                        case "cd..":
                        case "..":
                            if (_selectedNode == null)
                            {
                                Console.WriteLine("No node selected, selected central directory");
                                _selectedNode = Program.CentralDirectory;
                            }
                            else
                            {
                                if (_selectedNode.Parent == null)
                                {
                                    Console.WriteLine("No parent node");
                                }
                                else
                                {
                                    _selectedNode = _selectedNode.Parent;
                                    if (_selectedNode.Item != null)
                                        Console.WriteLine($"Selected ({_selectedNode.Name}) {_selectedNode.Item.Name}");
                                    else
                                        Console.WriteLine($"Selected ({_selectedNode.Name})");
                                }
                            }
                            return;                            
                        case "info":
                            if (_selectedNode == null)
                            {
                                Console.WriteLine("No node selected, selected central directory");
                                _selectedNode = Program.CentralDirectory;
                            }
                            else
                            {
                                PrintInfo(_selectedNode);
                            }
                            return;
                        case "name":
                            if (arguments.Length > 0)
                            {

                            }
                            else
                            {
                                Console.WriteLine("Not enough args for 'name'");
                            }
                            return;
                        case ".":
                        case "dir":
                        case "ls":
                        case "list":
                                if (Program.CentralDirectory == null) { Console.WriteLine("Central Directory is null, be careful"); }
                                _selectedNode = _selectedNode ?? Program.CentralDirectory;
                            if (_selectedNode.Item == null)
                            {
                                Console.WriteLine(_selectedNode);
                            }
                            else
                            {
                                Console.WriteLine($"({_selectedNode}) {_selectedNode.Item.Name}");
                            }
                            PrintChildren(_selectedNode);
                            return;
                        case "tree":
                            if (split.Any(n => n.Equals("-n")))
                                Tree(_selectedNode ?? Program.CentralDirectory, true);
                            else
                                Tree(_selectedNode ?? Program.CentralDirectory);
                            return;
                        case "help":
                            Console.WriteLine("Commands available: help, list, .., info, select, status, load, save");
                            return;
                        default:
                            Console.WriteLine($"Invalid command '{cmd}'");
                            return;
                    }
                } catch(Exception e)
                {
                    Console.WriteLine($"Error '{cmd}' threw exception '{e.Message}'");
                    return;
                }
            }

            Console.WriteLine($"Invalid command '{cmd}'");
        }
        
        private static void Tag(Child<SoftFile> child, string arguments)
        {
            if (child.Item != null)
            {
                arguments = arguments.ToLower();
                string[] tags = arguments.Split(' ');
                child.Item.AddTags(tags);
                Console.Write("Added tags:");
                foreach (var a in tags)
                    Console.Write($" {a},");
                Console.WriteLine();
                PrintTags(child);
            }
            else
            {
                Console.WriteLine("Item is null");
            }
        }

        private static void Tree(Child<SoftFile> child, bool replaceId = false)
        {
            if (!replaceId || child.Item == null || child.Parent == null)
                Console.WriteLine(child);
            else
                Console.WriteLine(child.Parent + child.Item.Name);
            foreach (var a in child.Children)
                Tree(a, replaceId);
        }        

        private static void PrintInfo(Child<SoftFile> child)
        {
            if (child.Item != null)
            {
                Console.WriteLine($"Path: {child}");
                Console.WriteLine($"Name: {child.Item.Name}\r\n" +
                    $"Id: {child.Item.Id}\r\n" +
                    $"Hash: {child.Item.Hash}\r\n" +
                    $"Thumbnail: {child.Item.Thumbnail} (Exists: {child.Item.ThumbnailExists})");
                if (child.Item.Tags.Count > 0)
                {
                    PrintTags(child);
                }
            }
            else
            {
                Console.WriteLine("No item for this node");
            }
            PrintChildren(child);
        }

        private static void PrintTags(Child<SoftFile> child)
        {
            Console.Write("Tags:");
            foreach (var a in child.Item.Tags)
                Console.Write($" {a},");
            Console.WriteLine();
        }

        private static void PrintChildren(Child<SoftFile> child)
        {
            Console.WriteLine($"[{child.ChildCount} children]");
            int chc = child.ChildCount;
            var children = child.Children;
            for (int i = 0; i < chc; i++)
                Console.WriteLine($"[{i + 1}] {children[i].Item.Name} ({children[i]})");
        }

        public static void ClearCharacters()
        {
            int bottom = Console.WindowHeight + Console.WindowTop - 1;
            int last = Console.CursorTop;
            char[] erase = new char[Console.WindowWidth - 1];
            for (int i = 0; i < erase.Length; i++)
                erase[i] = ' ';
            Console.SetCursorPosition(0, bottom);
            Console.Write(erase, 0, erase.Length);
            Console.SetCursorPosition(0, last);
        }

        public static void WriteCharacter(char i)
        {
            int bottom = Console.WindowHeight + Console.WindowTop - 1;
            int last = Console.CursorTop;
            Console.SetCursorPosition(0, bottom);
            Console.Write(i);
            Console.SetCursorPosition(0, last);
        }

        public static void WriteCharacters(string buffer)
        {
            int bottom = Console.WindowHeight + Console.WindowTop - 1;
            int last = Console.CursorTop;
            Console.SetCursorPosition(0, bottom);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(">");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(buffer + " ");
            Console.SetCursorPosition(0, last);
        }
    }
}
