using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SrBackup
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>


        private static void CreateFolderIfNotExist(string Folder)
        {
            if (System.IO.Directory.Exists(Folder) == false)
            {
                System.IO.Directory.CreateDirectory(Folder);
            }
        }

        static string GetDataFilename(string Source)
        {
            return  System.IO.File.GetCreationTimeUtc(Source).Ticks.ToString() + System.IO.File.GetLastWriteTimeUtc(Source).Ticks.ToString() + System.IO.Path.GetExtension(Source);
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
            System.Diagnostics.Process.Start(mklink);
        }

     

        static void CopyAndLinkToData(string DataFolder, string Source)
        {
            var Ordner = System.IO.Directory.GetDirectories(Source, "*.*", System.IO.SearchOption.AllDirectories);
            foreach (var d in Ordner)
            {

                var o = System.IO.Path.GetFullPath(d);
                o = o.Remove(0, Source.Length);

                o = DataFolder + @"\" + o;

                CreateFolderIfNotExist(o);
            }

            CopyFilesAndLinkToData(Source, DataFolder);
        }

        static void CopyFilesAndLinkToData(string Source, string Target)
        {
            var Dateien = System.IO.Directory.GetFiles(Source, "*.*", System.IO.SearchOption.AllDirectories);
            foreach (var d in Dateien)
            {
                
                var o = System.IO.Path.GetFullPath(d);
                o = o.Remove(0, Source.Length + 1);

                o = System.IO.Path.Combine(Target, o);

                string dataname = CopyToData(d);
                if (System.IO.File.Exists(o) == false)
                    LinkToData(dataname, o);
            }
        }

        static void Main()
        {
            if (System.IO.Directory.Exists("data") == false)
            {
                System.IO.Directory.CreateDirectory("data");
            }

            string[] Quelle = System.IO.File.ReadAllLines("Include.txt");

            foreach (var q in Quelle)
            {
              
                string Name = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Backup-" + DateTime.Now.ToString("yy-MM-dd HHmm"), System.IO.Path.GetFileName(q));
                CreateFolderIfNotExist(Name);
                CopyAndLinkToData(Name, q);

                List<string> lstDateien = new List<string>();
                foreach(var BackupOrdner in System.IO.Directory.GetDirectories(System.IO.Directory.GetCurrentDirectory(),"Backup*"))
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
}