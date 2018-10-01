using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Indexing;
using Organization.Objects;
using FFile = System.IO.File;
using System.IO;

namespace Organization
{
    static class Database
    {
        static public void Save(Index<SoftFile> index, string filename)
        {
            using (FileStream file = FFile.OpenWrite(filename))
            {
                SaveChild(index, file);
            }
        }

        static private byte[] GetData(Child<SoftFile> child)
        {
            ushort namelength = (ushort)child.Name.Length;
            int softfilelength;
            byte[] softfile = new byte[0];
            if (child.Item == null)
            {
                softfilelength = 0;
            }
            else
            {
                softfile = GetSoftfile(child.Item);
                softfilelength = softfile.Length;
            }
            ushort numberofchildren = (ushort)child.Children.Count;
            byte[][] children = new byte[numberofchildren][];
            for (int i = 0; i < numberofchildren; i++)
                children[i] = GetData(child.Children[i]);
            if (softfilelength > 0)
                return Extension.Combine(Extension.GetBytes(namelength), Encoding.ASCII.GetBytes(child.Name), Extension.GetBytes(softfilelength), softfile, Extension.GetBytes(numberofchildren), Extension.Combine(children));
            else
                return Extension.Combine(Extension.GetBytes(namelength), Encoding.ASCII.GetBytes(child.Name), Extension.GetBytes(softfilelength), Extension.GetBytes(numberofchildren), Extension.Combine(children));
        }

        static private byte[] GetSoftfile(SoftFile softFile)
        {
            return Extension.Combine(Extension.GetBytes(softFile.Description.Length), Encoding.ASCII.GetBytes(softFile.Description), Extension.GetBytes((int)softFile.Id), Extension.GetBytes((ushort)softFile.Name.Length), Encoding.ASCII.GetBytes(softFile.Name), Extension.GetBytes((ushort)softFile.Thumbnail.Length), Encoding.ASCII.GetBytes(softFile.Thumbnail), Extension.GetBytes((ushort)softFile.Hash.Length), Encoding.ASCII.GetBytes(softFile.Hash), GetTags(softFile));
        }

        static private byte[] GetTags(SoftFile file)
        {
            byte[][] tags = new byte[file.Tags.Count][];
            for (int i = 0; i < file.Tags.Count; i++)            
                tags[i] = Extension.Combine(Extension.GetBytes((ushort)file.Tags[i].Length), Encoding.ASCII.GetBytes(file.Tags[i]));
            return Extension.Combine(Extension.GetBytes((ushort)file.Tags.Count), Extension.Combine(tags));
        }

        static private void SaveChild(Child<SoftFile> child, FileStream fs)
        {
            byte[] lol = GetData(child);
            fs.Write(lol, 0, lol.Length);
            fs.Flush(true);
            fs.Close();
        }

        static public Child<SoftFile> Load(string filename)
        {
            using (FileStream file = FFile.OpenRead(filename))
            {
                int pos = 0;
                return LoadChild(file, ref pos);
            }            
        }

        static private Child<SoftFile> LoadChild(FileStream fs, ref int pos)
        {
            Child<SoftFile> toadd = new Child<SoftFile>();
            toadd.Name = LoadText(fs, GetUshort(fs, ref pos), ref pos);
            int softfilelength = GetInt(fs, ref pos);
            if (softfilelength > 0)
                toadd.Item = LoadSoftFile(fs, softfilelength, ref pos);
            ushort numberofchildren = GetUshort(fs, ref pos);
            for (int i = 0; i < numberofchildren; i++)
                toadd.AppendChild(LoadChild(fs, ref pos));
            return toadd;
        }

        static private SoftFile LoadSoftFile(FileStream fs, int length, ref int pos)
        {
            string description = LoadStringIntIden(fs, ref pos);
            uint id = (uint)GetInt(fs, ref pos);
            string name = LoadStringUshortIden(fs, ref pos);
            string thumbnail = LoadStringUshortIden(fs, ref pos);
            string hash = LoadStringUshortIden(fs, ref pos);
            string[] tags = LoadTags(fs, ref pos);
            return new SoftFile(name, thumbnail, id, description, tags, hash);
        }

        static private string[] LoadTags(FileStream fs, ref int pos)
        {
            int numberoftags = GetUshort(fs, ref pos);
            string[] tags = new string[numberoftags];
            for (int i = 0; i < numberoftags; i++)
                tags[i] = LoadStringUshortIden(fs, ref pos);
            return tags;
        }

        static ushort GetUshort(FileStream fs, ref int pos)
        {            
            byte[] vs = new byte[2];
            fs.Read(vs, /*pos*/0, 2);
            pos += 2;
            return Extension.GetUshort(vs);
        }

        static int GetInt(FileStream fs, ref int pos)
        {
            byte[] vs = new byte[4];
            fs.Read(vs, /*pos*/0, 4);
            pos += 4;
            return Extension.GetInt(vs);
        }

        static private string LoadText(FileStream fs, int length, ref int pos)
        {
            if (length > 0)
            {
                byte[] loaded = new byte[length];
                fs.Read(loaded, /*pos*/0, length);
                pos += length;
                return Encoding.ASCII.GetString(loaded);
            }
            else
            {
                return "";
            }
        }

        static private string LoadStringUshortIden(FileStream fs, ref int pos)
        {
            return LoadText(fs, GetUshort(fs, ref pos), ref pos);
        }

        static private string LoadStringIntIden(FileStream fs, ref int pos)
        {
            return LoadText(fs, GetInt(fs, ref pos), ref pos);
        }
    }
}
