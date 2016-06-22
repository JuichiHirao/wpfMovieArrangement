using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Windows;

namespace wpfMovieArrangement
{
    class ActionInfo
    {
        public static int EXEC_KIND_COPY_LASTWRITE = 1;
        public static int EXEC_KIND_MOVE = 2;

        public int Kind { get; set; }
        public FileInfo fileSource { get; set; }
        public FileInfo fileDestination { get; set; }
    }
    public class ClipBoardCommon
    {
        public static string GetText()
        {
            System.Windows.IDataObject data = Clipboard.GetDataObject();

            string ClipboardText = "";
            if (data.GetDataPresent(DataFormats.Text))
            {
                ClipboardText = (string)data.GetData(DataFormats.Text);
                // クリップボードのテキストを改行毎に配列に設定
                string[] ClipBoardList = ClipboardText.Split('\n');

                foreach (string text in ClipBoardList)
                {
                    if (text.Trim().Length > 0)
                        ClipboardText = text;
                }
            }

            return ClipboardText;
        }
        public static string GetTextPath()
        {
            System.Windows.IDataObject data = Clipboard.GetDataObject();

            string ClipboardText = "";
            if (data.GetDataPresent(DataFormats.Text))
            {
                ClipboardText = (string)data.GetData(DataFormats.Text);
                // クリップボードのテキストを改行毎に配列に設定
                string[] ClipBoardList = ClipboardText.Split('\n');

                foreach (string file in ClipBoardList)
                {
                    FileInfo fileinfo = new FileInfo(file);
                    DirectoryInfo dirinfo = new DirectoryInfo(file);

                    if (fileinfo.Exists || dirinfo.Exists)
                    {
                        ClipboardText = fileinfo.FullName;
                        break;
                    }
                }
            }

            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (string file in (string[])data.GetData(DataFormats.FileDrop))
                {
                    FileInfo fileinfo = new FileInfo(file);
                    DirectoryInfo dirinfo = new DirectoryInfo(file);

                    if (fileinfo.Exists || dirinfo.Exists)
                    {
                        ClipboardText = fileinfo.FullName;
                        break;
                    }
                }
            }

            return ClipboardText;
        }
    }
    class FileControl
    {
        public static string AVRIP_HISTROY_PATHNAME = @"C:\Users\JuuichiHirao\Dropbox\Interest\TEXT\AVRIP_HISTORY.txt";

        public string BasePath = "";        // txtBasePath.Text
        public string DestFilename = "";    // txtChangeFileName.Text
        public string LabelPath = "";       // txtLabelPath.Text

        public FileInfo SourceFile = null;

        private List<TargetFiles> listSelectedFiles;

        List<ActionInfo> listActionInfo;
        List<string> listExtension;

        MovieFileContents DatabaseMovieFile;

        public MovieFileContents GetMovieFilesData()
        {
            return DatabaseMovieFile;
        }

        public FileControl()
        {
            listActionInfo = new List<ActionInfo>();
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

        public void SetExtension()
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
                    AddActionInfo(SourceFile.FullName, DestPathname, ActionInfo.EXEC_KIND_COPY_LASTWRITE);

                    files.IsFinished = true;
                }
            }
        }
        public void AddActionInfo(string mySourcePathname, string myDestPathname, int myKind)
        {
            ActionInfo actinfo = new ActionInfo();
            actinfo.fileSource = new FileInfo(mySourcePathname);
            actinfo.fileDestination = new FileInfo(myDestPathname);
            actinfo.Kind = myKind;

            listActionInfo.Add(actinfo);
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
                    AddActionInfo(SourceFile.FullName, DestPathname, ActionInfo.EXEC_KIND_COPY_LASTWRITE);

                    sel.IsFinished = true;
                    Count++;
                }
            }
        }
        public void SetDbMovieFilesInfo()
        {
            DatabaseMovieFile = new MovieFileContents();

            DatabaseMovieFile.Name = DestFilename;
            DatabaseMovieFile.Label = LabelPath;

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

                DatabaseMovieFile.Extension = ext.Replace(".", "").ToUpper();
                DatabaseMovieFile.FileCount = dataMatch.Count();

                if (dataMatch.Count() == 1)
                {
                    foreach (TargetFiles sel in dataMatch)
                    {
                        DatabaseMovieFile.Size = sel.FileInfo.Length;
                        DatabaseMovieFile.FileDate = sel.FileInfo.LastWriteTime;
                    }
                }
                else
                {
                    long size = 0;
                    foreach (TargetFiles sel in dataMatch)
                    {
                        size += sel.FileInfo.Length;
                        DatabaseMovieFile.FileDate = sel.FileInfo.LastWriteTime;
                    }

                    DatabaseMovieFile.Size = size;
                }
            }
            // 品番、販売日を設定
            DatabaseMovieFile.Parse();
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

        public void DatabaseExport()
        {
            DbConnection dbcon = new DbConnection();

            // データベースへ登録
            string sqlCommand = "INSERT INTO MOVIE_FILES (NAME, SIZE, FILE_DATE, LABEL, SELL_DATE, PRODUCT_NUMBER, FILE_COUNT, EXTENSION) VALUES( @pName, @pSize, @pFileDate, @pLabel, @pSellDate, @pProductNumber, @pFileCount, @pExtension )";

            SqlCommand command = new SqlCommand(sqlCommand, dbcon.getSqlConnection());
            SqlParameter[] sqlparams = new SqlParameter[8];
            // Create and append the parameters for the Update command.
            sqlparams[0] = new SqlParameter("@pName", SqlDbType.VarChar);
            sqlparams[0].Value = DatabaseMovieFile.Name;

            sqlparams[1] = new SqlParameter("@pSize", SqlDbType.Decimal);
            sqlparams[1].Value = DatabaseMovieFile.Size;

            sqlparams[2] = new SqlParameter("@pFileDate", SqlDbType.DateTime);
            sqlparams[2].Value = DatabaseMovieFile.FileDate;

            sqlparams[3] = new SqlParameter("@pLabel", SqlDbType.VarChar);
            sqlparams[3].Value = LabelPath;

            sqlparams[4] = new SqlParameter("@pSellDate", SqlDbType.DateTime);
            if (DatabaseMovieFile.SellDate.Year >= 2000)
                sqlparams[4].Value = DatabaseMovieFile.SellDate;
            else
                sqlparams[4].Value = Convert.DBNull;

            sqlparams[5] = new SqlParameter("@pProductNumber", SqlDbType.VarChar);
            sqlparams[5].Value = DatabaseMovieFile.ProductNumber;

            sqlparams[6] = new SqlParameter("@pFileCount", SqlDbType.Int);
            sqlparams[6].Value = DatabaseMovieFile.FileCount;

            sqlparams[7] = new SqlParameter("@pExtension", SqlDbType.VarChar);
            sqlparams[7].Value = DatabaseMovieFile.Extension;

            dbcon.SetParameter(sqlparams);
            dbcon.execSqlCommand(sqlCommand);
        }

        public void RemoveTextFilenameLine()
        {
            System.IO.StreamReader strmReader = null;
            System.IO.StreamWriter strmWriter = null;
            string FilePathname = FileControl.AVRIP_HISTROY_PATHNAME;

            // 行数の取得用のテキストファイルを読み込み
            strmReader = new System.IO.StreamReader(FilePathname, System.Text.Encoding.GetEncoding("UTF-16"));

            string line = "";

            List<string> listLine = new List<string>();

            while ((line = strmReader.ReadLine()) != null)
            {
                if (DatabaseMovieFile.Name.IndexOf(line) >= 0)
                {
                    listLine.Add("");
                    continue;
                }

                listLine.Add(line);
            }
            strmReader.Close();

            strmWriter = new System.IO.StreamWriter(FilePathname, false, System.Text.Encoding.GetEncoding("UTF-16"));

            foreach (string lineData in listLine)
            {
                strmWriter.WriteLine(lineData);
            }

            strmWriter.Flush();

            strmWriter.Close();
        }
        public void RemoveTextFilenameLine(string myFilename)
        {
            System.IO.StreamReader strmReader = null;
            System.IO.StreamWriter strmWriter = null;
            string FilePathname = FileControl.AVRIP_HISTROY_PATHNAME;

            // 行数の取得用のテキストファイルを読み込み
            strmReader = new System.IO.StreamReader(FilePathname, System.Text.Encoding.GetEncoding("UTF-16"));

            string line = "";

            List<string> listLine = new List<string>();

            while ((line = strmReader.ReadLine()) != null)
            {
                if (myFilename.IndexOf(line) >= 0)
                {
                    listLine.Add("");
                    continue;
                }

                listLine.Add(line);
            }
            strmReader.Close();

            strmWriter = new System.IO.StreamWriter(FilePathname, false, System.Text.Encoding.GetEncoding("UTF-16"));

            foreach (string lineData in listLine)
            {
                strmWriter.WriteLine(lineData);
            }

            strmWriter.Flush();

            strmWriter.Close();
        }
    }
}
