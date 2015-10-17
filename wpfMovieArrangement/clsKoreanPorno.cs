using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace wpfMovieArrangement
{
    public class KoreanPornoData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PrefixName { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string ArchiveFile { get; set; }
        public int Status { get; set; }
    }
    public class KoreanPornoFileInfo
    {
        public bool IsSelected { get; set; }
        public FileInfo FileInfo { get; set; }
        public string ChangeFilename { get; set; }
        public DateTime ChangeLastWriteTime { get; set; }

        public KoreanPornoFileInfo(string myFileInfo)
        {
            FileInfo = new FileInfo(myFileInfo);
        }
    }

    class KoreanPorno
    {
        //public static string Path = @"C:\Users\充一\Desktop\Downloads\TEMP";

        public static List<KoreanPornoData> GetFolderData(string myPath)
        {
            //string path = @"C:\Users\充一\Desktop\Downloads\TEMP";
            List<KoreanPornoData> listData = new List<KoreanPornoData>();

            if (!System.IO.Directory.Exists(myPath))
                return listData;

            string[] files = System.IO.Directory.GetFiles(myPath, "*jpg");

            if (files.Length > 0)
            {
                foreach(string file in files)
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

        public static int GetStatus(string myPath, KoreanPornoData myData)
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

        public static string GetFrozenPathname(string myPath, string myArchiveName)
        {
            string pathname = System.IO.Path.Combine(myPath, myArchiveName);

            FileInfo fileinfo = new FileInfo(pathname);

            string Name = fileinfo.Name.Replace(fileinfo.Extension, "");

            pathname = System.IO.Path.Combine(myPath, Name);

            return pathname;
        }

        public static List<KoreanPornoFileInfo> GetFileInfo(string myPath, string myJpegFilename, DateTime myChangeLastWriteTime, string myArchiveName)
        {
            List<KoreanPornoFileInfo> listFiles = new List<KoreanPornoFileInfo>();

            string pathname = GetFrozenPathname(myPath, myArchiveName);

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
            foreach(string file in files)
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
        public static void ExecuteArrangement(string myPath, KoreanPornoData myTargetData, List<KoreanPornoFileInfo> myListFileInfo)
        {
            // ファイルの移動、ファイル更新日
            foreach (KoreanPornoFileInfo fileinfo in myListFileInfo)
            {
                if (fileinfo.ChangeFilename != null && fileinfo.ChangeFilename.Length > 0)
                {
                    string destFilename = System.IO.Path.Combine(myPath, fileinfo.ChangeFilename);
                    File.SetLastWriteTime(fileinfo.FileInfo.FullName, fileinfo.ChangeLastWriteTime);
                    File.Move(fileinfo.FileInfo.FullName, destFilename);
                }
            }

            string archiveFilePath = System.IO.Path.Combine(myPath, myTargetData.ArchiveFile);
            string frozenFolderPath = GetFrozenPathname(myPath, myTargetData.ArchiveFile);

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
    }
}
