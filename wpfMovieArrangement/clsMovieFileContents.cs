using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;

namespace wpfMovieArrangement
{
    class MovieFileContents
    {
        public static string REGEX_MOVIE_EXTENTION = @".*(\.avi)|.*(\.wmv)|.*(\.mpg)|.*(\.ts)|.*(\.divx)|.*(\.mp4)|.*(\.asf)|.*(\.mkv)|.*(\.m4v)|.*(\.rmvb)|.*(\.rm)|.*(\.flv)|.*(\.mov)|.*(\.3gp)";
        //  @".*\.avi$|.*\.wmv$|.*\.mpg$|.*ts$|.*divx$|.*mp4$|.*asf$|.*jpg$|.*jpeg$|.*iso$|.*mkv$";

        public const int KIND_AVRIP = 1;
        public const int KIND_IVRIP = 2;
        public const int KIND_URAAVRIP = 3;
        public int Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public long Size { get; set; }
        public string DispFileDate { get; set; }
        public DateTime FileDate { get; set; }
        public string DispSellDate { get; set; }
        public string ProductNumber { get; set; }
        public DateTime SellDate { get; set; }
        public string Extension { get; set; }
        public int FileCount { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string Remark { get; set; }
        public string Tag { get; set; }

        // 以下はParseFromJavSiteTextで使用するプロパティ
        public string EditPasteText { get; set; }
        public int Kind { get; set; } // AVRIP -> 1, IVRIP -> 2, 裏AVRIP -> 3
        public string MatchStrSellDate { get; set; }
        public string MatchStrProductNumber { get; set; }
        public string MatchStrActresses { get; set; }
        public string MatchQuality { get; set; }

        public void Parse()
        {
            string WorkStr = Regex.Replace(Name.Substring(1), ".* \\[", "");
            string WorkStr2 = Regex.Replace(WorkStr, "\\].*", "");
            WorkStr = Regex.Replace(WorkStr2, " [0-9]*.*", "");
            ProductNumber = WorkStr.ToUpper();

            string DateStr = Regex.Replace(WorkStr2, ".* ", "");

            if (DateStr.Length != 8)
                return;

            string format = "yyyyMMdd"; // "yyyyMMddHHmmss";
            try
            {
                SellDate = DateTime.ParseExact(DateStr, format, null);
            }
            catch (Exception)
            {
                return;
            }
        }

        private bool _IsExistsThumbnail;

        public bool IsExistsThumbnail
        {
            get
            {
                return _IsExistsThumbnail;
            }
            set
            {
                _IsExistsThumbnail = value;
                ImageUri = GetImageUri(_IsExistsThumbnail);
            }
        }

        private string _ImageUri;

        public string ImageUri
        {
            get
            {
                return _ImageUri;
            }
            set
            {
                _ImageUri = value;
            }
        }

        private string GetImageUri(bool myExistsThumbnail)
        {
            string WorkImageUri = "";

            DirectoryInfo dirinfo = new DirectoryInfo(Environment.CurrentDirectory);

            if (IsExistsThumbnail)        // サムネイル画像あり
                WorkImageUri = System.IO.Path.Combine(dirinfo.FullName, "32.png");
            else
                WorkImageUri = System.IO.Path.Combine(dirinfo.FullName, "00.png");

            return WorkImageUri;
        }

        public string GetLastSentenceFromLabel()
        {
            if (Label.LastIndexOf("\\") >= 0)
            {
                return Label.Substring(Label.LastIndexOf("\\") + 1);
            }

            return Label;
        }
    }
}
