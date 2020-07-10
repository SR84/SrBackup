using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace robobackup
{
    class Program
    {


        private static void CreateFolderIfNotExist(string Folder)
        {
            if (System.IO.Directory.Exists(Folder) == false)
            {
                System.IO.Directory.CreateDirectory(Folder);
            }
        }

        static string GetDataFilename(string Source)
        {
            return System.IO.File.GetCreationTimeUtc(Source).Ticks.ToString() + System.IO.File.GetLastWriteTimeUtc(Source).Ticks.ToString() + System.IO.Path.GetExtension(Source);
        }

        static string CopyToData(string Source)
        {
            string dataname = @"data\" + GetDataFilename(Source);
            dataname = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), dataname);
            if (System.IO.File.Exists(dataname) == false)
            {
                Console.WriteLine("copy " + Source);
                System.IO.File.Copy(Source, dataname);
                System.IO.File.SetCreationTimeUtc(dataname, System.IO.File.GetCreationTimeUtc(Source));
                System.IO.File.SetLastAccessTimeUtc(dataname, System.IO.File.GetLastAccessTimeUtc(Source));
                System.IO.File.SetLastWriteTimeUtc(dataname, System.IO.File.GetLastWriteTimeUtc(Source));
            }

            return dataname;
        }

        static void LinkToData(string Data, string Target)
        {
            string cmd = "mklink / h " + ((char)34) + Target + ((char)34) + " " + ((char)34) + Data + ((char)34);
            var mklink = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/c " + cmd);
            mklink.UseShellExecute = false;
            System.Diagnostics.Process.Start(mklink);
        }

        static void CopyAndLinkToData(string DataFolder, string Source)
        {
            var Ordner = System.IO.Directory.EnumerateDirectories(Source, "*.*", System.IO.SearchOption.AllDirectories);

            foreach (var d in Ordner)
            {

                var o = System.IO.Path.GetFullPath(d);
                o = o.Remove(0, Source.Length);

                o = DataFolder + @"\" + o;

                CreateFolderIfNotExist(o);

                System.IO.Directory.SetCreationTimeUtc(o, System.IO.Directory.GetCreationTimeUtc(d));
                System.IO.Directory.SetLastAccessTimeUtc(o, System.IO.Directory.GetLastAccessTimeUtc(d));
                System.IO.Directory.SetLastWriteTimeUtc(o, System.IO.Directory.GetLastWriteTimeUtc(d));

                CopyAndLinkToData(System.IO.Path.Combine( DataFolder, System.IO.Path.GetFileName(d)) , d);
            }

            CopyFilesAndLinkToData(Source, DataFolder);
        }

        static void CopyFilesAndLinkToData(string Source, string Target)
        {
            var Dateien = System.IO.Directory.EnumerateFiles(Source, "*.*", System.IO.SearchOption.AllDirectories);
            Parallel.ForEach(Dateien, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, d =>
            {
                var o = System.IO.Path.GetFullPath(d);
                o = o.Remove(0, Source.Length + 1);

                o = System.IO.Path.Combine(Target, o);

                string dataname = CopyToData(d);
                if (System.IO.File.Exists(o) == false)
                    LinkToData(dataname, o);
            });
            
        }

        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("robobackup [SOURCE] [TARGET]");
                return;
            }

            string Quelle = args[0];
            string Ziel = args[1];


            if (System.IO.Directory.Exists(Ziel) == false)
            {
                System.IO.Directory.CreateDirectory(Ziel);
            }

            System.IO.Directory.SetCurrentDirectory(Ziel);

            if (System.IO.Directory.Exists("data") == false)
            {
                System.IO.Directory.CreateDirectory("data");
            }


            string Name = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Backup-" + DateTime.Now.ToString("yy-MM-dd HH_mm"), System.IO.Path.GetFileName(Quelle));
            CreateFolderIfNotExist(Name);
            CopyAndLinkToData(Name, Quelle);

            List<string> lstDateien = new List<string>();
            foreach (var BackupOrdner in System.IO.Directory.GetDirectories(System.IO.Directory.GetCurrentDirectory(), "Backup*"))
            {
                foreach (var d in System.IO.Directory.GetFiles(BackupOrdner, "*.*", System.IO.SearchOption.AllDirectories))
                {
                    var data = GetDataFilename(d);
                    if (lstDateien.Contains(data) == false)
                        lstDateien.Add(data);
                }
            }

            foreach (var d in System.IO.Directory.GetFiles("data", "*.*"))
            {
                var n = System.IO.Path.GetFileName(d);
                if (lstDateien.Contains(n) == false)
                    System.IO.File.Delete(d);
            }

        }
    }
}