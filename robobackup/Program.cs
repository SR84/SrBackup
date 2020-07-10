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
            //Create Uniqe File-Filename Based on Creation and LastWriteTime
            return System.IO.File.GetCreationTimeUtc(Source).Ticks.ToString() + System.IO.File.GetLastWriteTimeUtc(Source).Ticks.ToString() + System.IO.Path.GetExtension(Source);
        }

        static string CopyToData(string Source)
        {
            string dataname = @"data\" + GetDataFilename(Source);
            dataname = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), dataname);
            if (System.IO.File.Exists(dataname) == false)
            {
                Console.WriteLine("Copy " + Source);

                //Copys file to Data-Pool
                System.IO.File.Copy(Source, dataname);

                //Set File-System Values FROM SOURCE TO TARGET
                System.IO.File.SetCreationTimeUtc(dataname, System.IO.File.GetCreationTimeUtc(Source));
                System.IO.File.SetLastAccessTimeUtc(dataname, System.IO.File.GetLastAccessTimeUtc(Source));
                System.IO.File.SetLastWriteTimeUtc(dataname, System.IO.File.GetLastWriteTimeUtc(Source));
            }

            return dataname;
        }

        static void LinkToData(string Data, string Target)
        {
            //Call Microsoft MKLINK function, whichs create a link MTF-Filedata Table
            string cmd = "mklink / h " + ((char)34) + Target + ((char)34) + " " + ((char)34) + Data + ((char)34);
            var mklink = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/c " + cmd);
            mklink.UseShellExecute = false;
            System.Diagnostics.Process.Start(mklink);
        }

        static void CopyAndLinkToData(string TargetFolder, string SourceFolder)
        {
            //Enumearte Folders and Create it in Target
            var Ordner = System.IO.Directory.EnumerateDirectories(SourceFolder, "*.*", System.IO.SearchOption.AllDirectories);
            foreach (var d in Ordner)
            {

                var o = System.IO.Path.GetFullPath(d);
                o = o.Remove(0, SourceFolder.Length);

                o = TargetFolder + @"\" + o;

                CreateFolderIfNotExist(o);

                //Set File-System Values FROM SOURCE TO TARGET
                System.IO.Directory.SetCreationTimeUtc(o, System.IO.Directory.GetCreationTimeUtc(d));
                System.IO.Directory.SetLastAccessTimeUtc(o, System.IO.Directory.GetLastAccessTimeUtc(d));
                System.IO.Directory.SetLastWriteTimeUtc(o, System.IO.Directory.GetLastWriteTimeUtc(d));

                //Recursic Call of this Function
                CopyAndLinkToData(System.IO.Path.Combine(TargetFolder, System.IO.Path.GetFileName(d)), d);
            }

            //Enumare Files
            var Dateien = System.IO.Directory.EnumerateFiles(SourceFolder, "*.*", System.IO.SearchOption.TopDirectoryOnly);
            Parallel.ForEach(Dateien, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, d =>
            {
                var o = System.IO.Path.GetFullPath(d);
                o = o.Remove(0, SourceFolder.Length + 1);

                o = System.IO.Path.Combine(TargetFolder, o);

                //Copy to DATA Target
                string dataname = CopyToData(d);
                //Create Link to DATA TARGET
                if (System.IO.File.Exists(o) == false)
                    LinkToData(dataname, o);
            });

        }



        static void Main(string[] args)
        {
            if (args.Length == 0)
            {

                //Print Usage
                Console.WriteLine("robobackup [SOURCE] [TARGET]");
                return;
            }

            string SourceFolder = args[0];
            string TargetFolder = args[1];

            //Create TargetFolder if not exist
            if (System.IO.Directory.Exists(TargetFolder) == false)
            {
                System.IO.Directory.CreateDirectory(TargetFolder);
            }

            //SET TargetFolder as CurrentDirectory
            System.IO.Directory.SetCurrentDirectory(TargetFolder);

            //Create our Data folder which is something like a pool
            if (System.IO.Directory.Exists("data") == false)
            {
                System.IO.Directory.CreateDirectory("data");
            }

            //Create Backup Folder with Name of Today
            string TargetBackupFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Backup-" + DateTime.Now.ToString("yy-MM-dd HH_mm"), System.IO.Path.GetFileName(SourceFolder));
            CreateFolderIfNotExist(TargetBackupFolder);

            //Start Backup Source Folder to TargetFolder
            CopyAndLinkToData(TargetBackupFolder, SourceFolder);


            //Clean Upda Data Folder
            //First generate Uniqe Filenames Of all Files in Backup Folders
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


            //Read all Files in Data and Check against lstDateien.
            //If FileName Exist in Array, than file will be used otherwise delete. File will not be used
            foreach (var d in System.IO.Directory.GetFiles("data", "*.*"))
            {
                var n = System.IO.Path.GetFileName(d);
                if (lstDateien.Contains(n) == false)
                    System.IO.File.Delete(d);
            }

        }
    }
}