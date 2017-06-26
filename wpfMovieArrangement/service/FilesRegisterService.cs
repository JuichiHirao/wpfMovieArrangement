using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic.FileIO;

namespace wpfMovieArrangement.service
{
    class FilesRegisterService
    {
        DbConnection dbcon;

        public string BasePath = "";        // txtBasePath.Text
        public string DestFilename = "";    // txtChangeFileName.Text
        public string LabelPath = "";       // txtLabelPath.Text

        List<string> listExtension;

        List<TargetFiles> listSelectedFiles;
        List<ActionInfo> listActionInfo = null;
        public MovieImportData targetImportData;
        MovieFileContents targetFileContentsData;

        TargetFiles SourceFile = new TargetFiles();

        public FilesRegisterService(DbConnection myDbCon)
        {
            if (myDbCon == null)
                dbcon = new DbConnection();
            else
                dbcon = myDbCon;

            listActionInfo = new List<ActionInfo>();
        }

        public void SetSourceFile(DataGrid myDataGrid)
        {
            DateCopyService.CheckDataGridSelectItem(myDataGrid, "コピー元", 1);
            SourceFile = (TargetFiles)myDataGrid.SelectedItem;
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

            SetExtension();
        }

        /// <summary>
        /// パラメータの選択ファイルから、画像JPEGファイルの処理情報を設定する
        /// </summary>
        public void SetJpegActionInfo()
        {
            // 画像のファイル名の変更、元ファイルの更新日をコピー
            Regex regexImage = new Regex(@".*jpg$|.*jpeg$|.*png$");
            string ExtensionJpg = ".jpg";

            foreach (TargetFiles files in listSelectedFiles)
            {
                if (regexImage.IsMatch(files.Name))
                {
                    string SrcPathname = files.FileInfo.FullName;
                    string DestPathname = "";

                    // 「Big」が入っている場合は_thを付加する
                    Regex regexImageTh = new Regex(@".*[Bb]ig\.jpg$|.*[Bb]ig\.jpeg$|.*[Bb]ig\.png$");
                    if (regexImageTh.IsMatch(files.Name))
                        DestPathname = System.IO.Path.Combine(BasePath, DestFilename + "_th" + ExtensionJpg);
                    else
                        DestPathname = System.IO.Path.Combine(BasePath, DestFilename + ExtensionJpg);

                    AddActionInfo(SrcPathname, DestPathname, ActionInfo.EXEC_KIND_MOVE);
                    AddActionInfo(SourceFile.FileInfo.FullName, DestPathname, ActionInfo.EXEC_KIND_COPY_LASTWRITE);

                    files.IsFinished = true;
                }
            }
        }

        public void SetMovieActionInfo()
        {
            foreach (string ext in listExtension)
            {
                // 動画ファイルが複数合った場合に後ろに付加する「_1」などのために名前順に並べ替える
                var dataMatch = from seldata in listSelectedFiles
                                where seldata.FileInfo.Extension == ext
                                orderby seldata.Name ascending
                                select seldata;

                // 動画のファイル名の変更、元ファイルの更新日をコピー
                int Count = 1;
                foreach (TargetFiles sel in dataMatch)
                {
                    if (sel.IsFinished)
                        continue;

                    string SrcPathname = sel.FileInfo.FullName;
                    string DestPathname = "";

                    if (dataMatch.Count() == 1)
                        DestPathname = System.IO.Path.Combine(BasePath, DestFilename + sel.FileInfo.Extension.ToLower());
                    else
                        DestPathname = System.IO.Path.Combine(BasePath, DestFilename + "_" + Count + sel.FileInfo.Extension.ToLower());

                    AddActionInfo(SrcPathname, DestPathname, ActionInfo.EXEC_KIND_MOVE);
                    AddActionInfo(SourceFile.FileInfo.FullName, DestPathname, ActionInfo.EXEC_KIND_COPY_LASTWRITE);

                    sel.IsFinished = true;
                    Count++;
                }
            }
        }

        public void SetDbMovieFilesInfo(MovieImportData myImportData)
        {
            targetImportData = myImportData;
            targetFileContentsData = new MovieFileContents();

            targetFileContentsData.Name = DestFilename;
            targetFileContentsData.Label = LabelPath;

            if (listExtension == null)
                return;

            foreach (string ext in listExtension)
            {
                if (ext.ToUpper().Equals(".JPG")
                    || ext.ToUpper().Equals(".JPEG")
                    || ext.ToUpper().Equals(".PNG")
                    || ext.ToUpper().Equals(".ISO"))
                    continue;

                // 動画ファイルが複数合った場合に後ろに付加する「_1」などのために名前順に並べ替える
                var dataMatch = from seldata in listSelectedFiles
                                where seldata.FileInfo.Extension == ext
                                orderby seldata.Name ascending
                                select seldata;

                targetFileContentsData.Extension = ext.Replace(".", "").ToUpper();
                targetFileContentsData.FileCount = dataMatch.Count();

                if (dataMatch.Count() == 1)
                {
                    foreach (TargetFiles sel in dataMatch)
                    {
                        targetFileContentsData.Size = sel.FileInfo.Length;
                        targetFileContentsData.FileDate = sel.FileInfo.LastWriteTime;
                    }
                }
                else
                {
                    long size = 0;
                    foreach (TargetFiles sel in dataMatch)
                    {
                        size += sel.FileInfo.Length;
                        targetFileContentsData.FileDate = sel.FileInfo.LastWriteTime;
                    }

                    targetFileContentsData.Size = size;
                }
            }
            // 品番、販売日を設定
            //DatabaseMovieFile.Parse();
            targetFileContentsData.SellDate = targetImportData.ProductDate;
            targetFileContentsData.ProductNumber = targetImportData.ProductNumber;

            targetFileContentsData.Tag = targetImportData.Tag;
        }


        public void DbExport()
        {
            try
            {
                dbcon.BeginTransaction("MOVIE_REGISTER");

                // データベースへ登録
                DbExportMovieFiles();

                // MOVIE_IMPORTから削除
                MovieImportService service = new MovieImportService();
                service.DbDelete(targetImportData, dbcon);

                dbcon.CommitTransaction();
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                dbcon.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 全削除ボタンを押下された場合に実行、dgridDestFileファイルの削除、MOVIE_IMPORT_DATAのDb削除を行う
        /// </summary>
        public void DeleteExecute(List<TargetFiles> myListTargetFiles)
        {
            if (targetImportData == null)
                throw new Exception("targetImportDataが設定されていません");

            try
            {
                dbcon.BeginTransaction("MOVIE_REGISTER");

                // MOVIE_IMPORTから削除
                MovieImportService service = new MovieImportService();
                service.DbDelete(targetImportData, dbcon);

                DeleteFiles(myListTargetFiles);

                dbcon.CommitTransaction();
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                dbcon.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public void Execute()
        {
            foreach (ActionInfo act in listActionInfo)
            {
                if (act.Kind == ActionInfo.EXEC_KIND_COPY_LASTWRITE)
                {
                    bool isFinished = false;
                    while (isFinished == false)
                    {
                        try
                        {
                            File.SetLastWriteTime(act.fileDestination.FullName, act.fileSource.LastWriteTime);
                            isFinished = true;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            MessageBoxResult res = MessageBox.Show("ファイルの読取専用を外して下さい、再実行しますか？", "", MessageBoxButton.YesNo);

                            if (res == MessageBoxResult.No)
                                break;
                        }
                    }
                }
                else if (act.Kind == ActionInfo.EXEC_KIND_MOVE)
                {
                    if (File.Exists(act.fileDestination.FullName))
                    {
                        string msg = act.fileSource.Name + " " + act.fileSource.Length + " --> \n" + act.fileDestination.Name + " " + act.fileDestination.Length;
                        MessageBoxResult result = MessageBox.Show("ファイルが存在します、上書きしても良いですか？\n" + msg, "上書き確認", MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.Yes)
                        {
                            File.Delete(act.fileDestination.FullName);
                            File.Move(act.fileSource.FullName, act.fileDestination.FullName);
                        }
                    }
                    else
                        File.Move(act.fileSource.FullName, act.fileDestination.FullName);
                }
            }
        }

        public void DeleteFiles(List<TargetFiles> myListTargetFiles)
        {
            if (targetImportData.RarFlag == true)
            {
                if (SourceFile != null && SourceFile.FileInfo != null)
                {
                    // ソースファイルのパターンマッチする全ファイルをゴミ箱に移動
                    string patternStr = SourceFile.FileInfo.Name.Replace("part1", "part*");
                    string[] MatchFiles = Directory.GetFiles(BasePath, patternStr, System.IO.SearchOption.TopDirectoryOnly);

                    // ネットワーク経由のパスの場合はゴミ箱でなく「DELETE」フォルダに移動
                    if (BasePath.IndexOf("\\") == 0)
                    {
                        if (MatchFiles != null)
                        {
                            string DeleteDir = System.IO.Path.Combine(BasePath, "DELETE");

                            if (!System.IO.File.Exists(DeleteDir))
                                System.IO.Directory.CreateDirectory(DeleteDir);

                            Debug.Print("Source Name [" + SourceFile.Name + "] --> [" + DeleteDir + "]");

                            foreach (string file in MatchFiles)
                            {
                                Debug.Print("DELETEフォルダ移動 [" + file + "]");
                                string DestPathname = System.IO.Path.Combine(DeleteDir, new FileInfo(file).Name);

                                System.IO.File.Move(file, DestPathname);
                                //FileSystem.DeleteFile(
                                //    file,
                                //    UIOption.OnlyErrorDialogs,
                                //    RecycleOption.SendToRecycleBin);
                            }
                        }
                    }
                    else
                    {
                        // ソースファイルのパターンマッチする全ファイルをゴミ箱に移動
                        Debug.Print("Source Name [" + SourceFile.Name + "]");

                        if (MatchFiles != null)
                        {
                            foreach (string file in MatchFiles)
                            {
                                Debug.Print("ゴミ箱移動 [" + file + "]");

                                FileSystem.DeleteFile(
                                    file,
                                    UIOption.OnlyErrorDialogs,
                                    RecycleOption.SendToRecycleBin);
                            }
                        }
                    }
                }
            }

            if (myListTargetFiles == null || myListTargetFiles.Count <= 0)
                return;

            foreach (TargetFiles file in myListTargetFiles)
            {
                if (file.IsDeleted)
                {
                    Debug.Print("削除フラグ ゴミ箱移動 [" + file.FileInfo.FullName + "]");
                    FileSystem.DeleteFile(
                        file.FileInfo.FullName,
                        UIOption.OnlyErrorDialogs,
                        RecycleOption.SendToRecycleBin);
                }
            }
        }

        public void DbExportMovieFiles()
        {
            DbConnection dbcon = new DbConnection();

            // データベースへ登録
            string sqlCommand = "INSERT INTO MOVIE_FILES (NAME, SIZE, FILE_DATE, LABEL, SELL_DATE, PRODUCT_NUMBER, FILE_COUNT, EXTENSION, TAG) VALUES( @pName, @pSize, @pFileDate, @pLabel, @pSellDate, @pProductNumber, @pFileCount, @pExtension, @Tag )";

            SqlCommand command = new SqlCommand(sqlCommand, dbcon.getSqlConnection());
            SqlParameter[] sqlparams = new SqlParameter[9];
            // Create and append the parameters for the Update command.
            sqlparams[0] = new SqlParameter("@pName", SqlDbType.VarChar);
            sqlparams[0].Value = targetFileContentsData.Name;

            sqlparams[1] = new SqlParameter("@pSize", SqlDbType.Decimal);
            sqlparams[1].Value = targetFileContentsData.Size;

            sqlparams[2] = new SqlParameter("@pFileDate", SqlDbType.DateTime);
            sqlparams[2].Value = targetFileContentsData.FileDate;

            sqlparams[3] = new SqlParameter("@pLabel", SqlDbType.VarChar);
            sqlparams[3].Value = LabelPath;

            sqlparams[4] = new SqlParameter("@pSellDate", SqlDbType.DateTime);
            if (targetFileContentsData.SellDate.Year >= 2000)
                sqlparams[4].Value = targetFileContentsData.SellDate;
            else
                sqlparams[4].Value = Convert.DBNull;

            sqlparams[5] = new SqlParameter("@pProductNumber", SqlDbType.VarChar);
            sqlparams[5].Value = targetFileContentsData.ProductNumber;

            sqlparams[6] = new SqlParameter("@pFileCount", SqlDbType.Int);
            sqlparams[6].Value = targetFileContentsData.FileCount;

            sqlparams[7] = new SqlParameter("@pExtension", SqlDbType.VarChar);
            sqlparams[7].Value = targetFileContentsData.Extension;

            sqlparams[8] = new SqlParameter("@Tag", SqlDbType.VarChar);
            sqlparams[8].Value = targetFileContentsData.Tag;

            dbcon.SetParameter(sqlparams);
            dbcon.execSqlCommand(sqlCommand);
        }

        private void SetExtension()
        {
            // 対象ファイルの種類が複数有るかをチェック（iso, avi...など）
            //List<string> listExt = new List<string>();

            listExtension = new List<string>();

            foreach (TargetFiles file in listSelectedFiles)
            {
                string ext = file.FileInfo.Extension;
                Boolean isSame = false;

                foreach (string strExt in listExtension)
                {
                    if (strExt.Equals(ext))
                    {
                        isSame = true;
                        break;
                    }
                }
                if (!isSame)
                    listExtension.Add(ext);
            }
        }

        private void AddActionInfo(string mySourcePathname, string myDestPathname, int myKind)
        {
            ActionInfo actinfo = new ActionInfo();
            actinfo.fileSource = new FileInfo(mySourcePathname);
            actinfo.fileDestination = new FileInfo(myDestPathname);
            actinfo.Kind = myKind;

            listActionInfo.Add(actinfo);
        }
    }
}
