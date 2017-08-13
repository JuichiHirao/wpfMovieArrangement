using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace wpfMovieArrangement.service
{
    /// <summary>
    /// 既存でMOVIE_FILESへ登録されたデータをHD動画用に更新する、ファイルは直下にフォルダを作成して移動する
    /// </summary>
    class HdUpdateService
    {
        DbConnection dbcon;

        public string BasePath = "";
        List<TargetFiles> listSelectedFiles = null;

        public HdUpdateService(DbConnection myDbCon)
        {
            if (myDbCon == null)
                dbcon = new DbConnection();
            else
                dbcon = myDbCon;

            //listActionInfo = new List<ActionInfo>();
        }

        public void SetSelectedOnlyFiles(List<TargetFiles> myListTargetFiles)
        {
            listSelectedFiles = new List<TargetFiles>();

            foreach (TargetFiles files in myListTargetFiles)
            {
                if (files.IsSelected)
                    listSelectedFiles.Add(files);
            }

            if (listSelectedFiles.Count <= 0)
                throw new Exception("対象のファイルが存在しません");
        }

        public void Execute(MovieImportData myImportData, MovieFileContents myFileContents)
        {
            string destDir = "HD-" + myFileContents.GetLastSentenceFromLabel();

            string fullPath = Path.Combine(BasePath, destDir);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            long size = 0;
            string ext = "";
            DateTime dt = new DateTime(1900,1,1);
            foreach (TargetFiles data in listSelectedFiles)
            {
                string destFilename = Path.Combine(fullPath, myImportData.Filename + data.FileInfo.Extension);
                size += data.FileInfo.Length;
                ext = data.FileInfo.Extension.Substring(1);
                dt = data.FileInfo.LastWriteTime;
                File.Move(data.FileInfo.FullName, destFilename);
            }

            // JPEGなどのファイルのファイル名を変更（ファイル名を「XXX FHD」に更新する前にファイル名変更を実行）
            string[] mvFiles = Directory.GetFiles(myFileContents.Label, myFileContents.Name + "*");
            Debug.Print(" " + myFileContents.Label + "   " + myFileContents.Name);

            foreach (string pathname in mvFiles)
            {
                FileInfo fileinfo = new FileInfo(pathname);
                string sourcePathname = pathname;
                string sufix = fileinfo.Name.Replace(myFileContents.Name, "");
                Debug.Print("sufix" + sufix + " pathname " + pathname);
                string destPathname = Path.Combine(myFileContents.Label, myImportData.Filename + sufix.ToLower());

                File.Move(sourcePathname, destPathname);
            }

            MovieFileContentsService service = new MovieFileContentsService();

            myFileContents.Name = myImportData.Filename;
            myFileContents.Size = size;
            if (!myFileContents.Extension.ToUpper().Equals(ext.ToUpper()))
            {
                MessageBoxResult result = MessageBox.Show("既存のファイルは削除しますか？", "削除確認", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    string[] files = Directory.GetFiles(myFileContents.Label, myFileContents.Name + "*" + myFileContents.Extension);

                    List<TargetFiles> delTargetList = new List<TargetFiles>();

                    foreach (string pathname in files)
                    {
                        TargetFiles targetFiles = new TargetFiles();
                        targetFiles.FileInfo = new FileInfo(pathname);
                        delTargetList.Add(targetFiles);
                    }

                    FilesRegisterService serviceFileRegister = new FilesRegisterService(dbcon);
                    serviceFileRegister.BasePath = BasePath;
                    serviceFileRegister.SetDbMovieFilesInfo(myImportData);
                    serviceFileRegister.DeleteFiles(delTargetList);
                }
                myFileContents.Extension = ext;
            }

            try
            {
                dbcon.BeginTransaction("MOVIE_REGISTER");

                service.DbUpdateFileInfo(myFileContents, dbcon);

                // MOVIE_IMPORTから削除
                MovieImportService serviceImportService = new MovieImportService();
                serviceImportService.DbDelete(myImportData, dbcon);

                dbcon.CommitTransaction();
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                dbcon.RollbackTransaction();
                throw new Exception(ex.Message);
            }

            return;
        }
    }
}
