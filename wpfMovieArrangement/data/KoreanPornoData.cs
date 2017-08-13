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
        public long Size { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string ArchiveFile { get; set; }
        public int FileCount { get; set; }
        public string Extension { get; set; }
        public string Label { get; set; }
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
}
