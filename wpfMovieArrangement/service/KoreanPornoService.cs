using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace wpfMovieArrangement.service
{
    class KoreanPornoService
    {
        //public static string Path = @"C:\Users\充一\Desktop\Downloads\TEMP";
        public string BasePath;

        public string ExportPath;

        public KoreanPornoService(string myPath, string myExportPath)
        {
            BasePath = myPath;
            ExportPath = myExportPath;
        }

        public List<KoreanPornoData> GetFolderData(string myPath)
        {
            //string path = @"C:\Users\充一\Desktop\Downloads\TEMP";
            List<KoreanPornoData> listData = new List<KoreanPornoData>();

            if (!System.IO.Directory.Exists(myPath))
                return listData;

            string[] files = System.IO.Directory.GetFiles(myPath, "*jpg");

            if (files.Length > 0)
            {
                foreach (string file in files)
                {
                    FileInfo fileinfo = new FileInfo(file);

                    KoreanPornoData data = new KoreanPornoData();
                    data.Name = fileinfo.Name;
                    data.LastWriteTime = fileinfo.LastWriteTime;

                    string[] splitStr = { " " };
                    string[] preFile = data.Name.Split(splitStr, StringSplitOptions.None);

                    if (preFile.Length > 1)
                    {
                        string[] rarFiles = Directory.GetFiles(myPath, preFile[0] + "*rar");

                        if (rarFiles.Length > 0)
                        {
                            FileInfo fileinfoArchive = new FileInfo(rarFiles[0]);
                            data.ArchiveFile = fileinfoArchive.Name;
                        }
                    }

                    // 状態を取得
                    data.Status = GetStatus(myPath, data);

                    listData.Add(data);
                }

            }

            return listData;
        }

        public int GetStatus(string myPath, KoreanPornoData myData)
        {
            string filePattern = myData.Name.Replace(".jpg", "") + "*";

            // 1 : 整理済み
            //  画像のファイル名部分で動画の拡張子のファイルが存在した場合
            int movCnt = 0;
            string[] filesMov = Directory.GetFiles(myPath, filePattern, System.IO.SearchOption.TopDirectoryOnly);

            Regex regex = new Regex(MovieFileContents.REGEX_MOVIE_EXTENTION);
            foreach (string fileMov in filesMov)
            {
                if (regex.IsMatch(fileMov))
                    movCnt++;
            }

            if (movCnt > 0)
                return 1;

            // 2 : 整理実行可
            //  解凍ファイルと同じ名前の解凍されたフォルダが存在する場合
            if (myData.ArchiveFile != null && myData.ArchiveFile.Length > 0)
            {
                filePattern = myData.ArchiveFile.Replace(".rar", "") + "*";

                string[] dir = Directory.GetDirectories(myPath, filePattern, System.IO.SearchOption.TopDirectoryOnly);

                if (dir.Length > 0)
                    return 2;
            }

            return 0;
        }

        public string GetFrozenPathname(string myPath, string myArchiveName)
        {
            if (myArchiveName == null || myArchiveName.Length <= 0)
                return null;

            string pathname = System.IO.Path.Combine(myPath, myArchiveName);

            FileInfo fileinfo = new FileInfo(pathname);

            string Name = fileinfo.Name.Replace(fileinfo.Extension, "");

            pathname = System.IO.Path.Combine(myPath, Name);

            return pathname;
        }

        public List<KoreanPornoFileInfo> GetFileInfo(string myJpegFilename, DateTime myChangeLastWriteTime, string myArchiveName)
        {
            List<KoreanPornoFileInfo> listFiles = new List<KoreanPornoFileInfo>();

            string pathname = GetFrozenPathname(BasePath, myArchiveName);

            if (!Directory.Exists(pathname))
                return null;

            string[] files = Directory.GetFiles(pathname, "*", System.IO.SearchOption.AllDirectories);

            Regex regex = new Regex(MovieFileContents.REGEX_MOVIE_EXTENTION, RegexOptions.IgnoreCase);

            List<KoreanPornoFileInfo> listKFiles = new List<KoreanPornoFileInfo>();
            int fileCnt = 0;
            foreach (string file in files)
            {
                KoreanPornoFileInfo koreanFile = new KoreanPornoFileInfo(file);

                if (regex.IsMatch(koreanFile.FileInfo.Name))
                    fileCnt++;
            }

            int fileNo = 1;
            foreach (string file in files)
            {
                KoreanPornoFileInfo koreanFile = new KoreanPornoFileInfo(file);

                Match match = regex.Match(koreanFile.FileInfo.Name);
                if (match.Success)
                {
                    koreanFile.IsSelected = true;
                    if (fileCnt == 1)
                        koreanFile.ChangeFilename = myJpegFilename.Replace(".jpg", "") + koreanFile.FileInfo.Extension.ToLower();
                    else
                        koreanFile.ChangeFilename = myJpegFilename.Replace(".jpg", "") + "_" + fileNo + koreanFile.FileInfo.Extension.ToLower();

                    koreanFile.ChangeLastWriteTime = myChangeLastWriteTime;

                    fileNo++;
                }
                listFiles.Add(koreanFile);
            }


            return listFiles;
        }

        /// <summary>
        /// アーカイブの中の選択された対象ファイルを取得
        /// </summary>
        private List<KoreanPornoFileInfo> GetTargetFiles(List<KoreanPornoFileInfo> myListFileInfo)
        {
            List<KoreanPornoFileInfo> selFiles = new List<KoreanPornoFileInfo>();

            long selSize = 0;
            foreach (KoreanPornoFileInfo data in myListFileInfo)
            {
                if (data.IsSelected)
                {
                    selFiles.Add(data);
                    selSize += data.FileInfo.Length;
                }
            }

            return selFiles;
        }

        public void ExecuteArrangement(KoreanPornoData myTargetData, List<KoreanPornoFileInfo> myListFileInfo)
        {
            // JPEGファイルのチェック
            string jpegFile = Path.Combine(BasePath, myTargetData.Name);

            if (!File.Exists(jpegFile))
                throw new Exception("JPEGファイルが存在しません");

            // ファイル移動先の生成（D:\Downloads\TEMP\KOREAN_PORNO7のフォルダを無ければ作成）
            string moveDestName = new DirectoryInfo(ExportPath).Name;
            string moveDestPath = Path.Combine(BasePath, moveDestName);

            if (!Directory.Exists(moveDestPath))
                Directory.CreateDirectory(moveDestPath);

            // 対象のファイルを取得
            List<KoreanPornoFileInfo> targetFiles = GetTargetFiles(myListFileInfo);

            // 登録に必要な情報を設定（ファイル数、サイズ）
            long selSize = 0;
            int fileCount = 0;
            string extension = "";
            foreach (KoreanPornoFileInfo data in targetFiles)
            {
                selSize += data.FileInfo.Length;
                fileCount++;

                if (extension.IndexOf(data.FileInfo.Extension.Substring(1).ToUpper()) < 0)
                {
                    if (extension.Length <= 0)
                        extension += data.FileInfo.Extension.Substring(1).ToUpper();
                    else
                        extension += "," + data.FileInfo.Extension.Substring(1).ToUpper();
                }
            }
            myTargetData.Size = selSize;
            myTargetData.FileCount = fileCount;
            myTargetData.Extension = extension;
            myTargetData.Label = ExportPath;

            DbExport(myTargetData, new DbConnection());

            // JPEGファイルの移動
            File.Move(jpegFile, Path.Combine(moveDestPath, myTargetData.Name));

            // 動画ファイルの移動、ファイル更新日
            foreach (KoreanPornoFileInfo fileinfo in targetFiles)
            {
                if (fileinfo.ChangeFilename != null && fileinfo.ChangeFilename.Length > 0)
                {
                    string destFilename = System.IO.Path.Combine(moveDestPath, fileinfo.ChangeFilename);
                    File.SetLastWriteTime(fileinfo.FileInfo.FullName, fileinfo.ChangeLastWriteTime);
                    File.Move(fileinfo.FileInfo.FullName, destFilename);
                }
            }


            string archiveFilePath = System.IO.Path.Combine(BasePath, myTargetData.ArchiveFile);
            string frozenFolderPath = GetFrozenPathname(BasePath, myTargetData.ArchiveFile);

            // 圧縮ファイル(Rar)の削除（ゴミ箱）
            if (File.Exists(archiveFilePath))
            {
                FileSystem.DeleteFile(
                    archiveFilePath,
                    UIOption.OnlyErrorDialogs,
                    RecycleOption.SendToRecycleBin);
            }

            // 解凍フォルダの削除（ゴミ箱）
            if (frozenFolderPath != null && frozenFolderPath.Length > 0)
            {
                FileSystem.DeleteDirectory(frozenFolderPath,
                    UIOption.OnlyErrorDialogs,
                    RecycleOption.SendToRecycleBin);
            }
        }

        private void DbExport(KoreanPornoData myData, DbConnection myDbCon)
        {
            string sqlCommand = "INSERT INTO MOVIE_FILES (NAME, SIZE, FILE_DATE, LABEL, FILE_COUNT, EXTENSION) VALUES( @pName, @pSize, @pFileDate, @pLabel, @pFileCount, @pExtension )";
            SqlCommand command = new SqlCommand();

            command = new SqlCommand(sqlCommand, myDbCon.getSqlConnection());

            List<SqlParameter> sqlparamList = new List<SqlParameter>();
            SqlParameter sqlparam = new SqlParameter();

            // Nameにはjpgの拡張子が付いているのでjpg部分は削除
            FileInfo fileinfo = new FileInfo(Path.Combine(BasePath, myData.Name));
            string name = fileinfo.Name.Replace(fileinfo.Extension, "");

            sqlparam = new SqlParameter("@pName", SqlDbType.VarChar);
            sqlparam.Value = name;
            sqlparamList.Add(sqlparam);

            sqlparam = new SqlParameter("@pSize", SqlDbType.Decimal);
            sqlparam.Value = myData.Size;
            sqlparamList.Add(sqlparam);

            sqlparam = new SqlParameter("@pFileDate", SqlDbType.DateTime);
            sqlparam.Value = myData.LastWriteTime;
            sqlparamList.Add(sqlparam);

            sqlparam = new SqlParameter("@pLabel", SqlDbType.VarChar);
            sqlparam.Value = myData.Label.ToUpper();
            sqlparamList.Add(sqlparam);

            sqlparam = new SqlParameter("@pFileCount", SqlDbType.Int);
            sqlparam.Value = myData.FileCount;
            sqlparamList.Add(sqlparam);

            sqlparam = new SqlParameter("@pExtension", SqlDbType.VarChar);
            sqlparam.Value = myData.Extension.ToUpper();
            sqlparamList.Add(sqlparam);

            myDbCon.SetParameter(sqlparamList.ToArray());
            myDbCon.execSqlCommand(sqlCommand);

            return;
        }

    }
}
