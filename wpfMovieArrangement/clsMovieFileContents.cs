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
        public static string REGEX_MOVIE_EXTENTION = @".*(\.avi)|.*(\.wmv)|.*(\.mpg)|.*(\.ts)|.*(\.divx)|.*(\.mp4)|.*(\.asf)|.*(\.mkv)|.*(\.m4v)|.*(\.rmvb)|.*(\.rm)|.*(\.flv)";
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

        // 以下はParseFromJavSiteTextで使用するプロパティ
        public string EditPasteText { get; set; }
        public int Kind { get; set; } // AVRIP -> 1, IVRIP -> 2, 裏AVRIP -> 3
        public string MatchStrSellDate { get; set; }
        public string MatchStrProductNumber { get; set; }
        public string MatchStrActresses { get; set; }
        public string MatchQuality { get; set; }

        public void ParseFromJavSiteText(string myText)
        {
            // 不要な[FHD]、[HD]の文字列は削除置換する
            Regex regexFHDetc = new Regex("(\\[FHD\\]|\\[HD\\])");
            //Regex regexFHDetc = new Regex("FHD");

            if (regexFHDetc.IsMatch(myText))
            {
                EditPasteText = myText.Replace(regexFHDetc.Match(myText).Value.ToString(), "");
                MatchQuality = "HD";
            }
            else
                EditPasteText = myText;

            MatchStrSellDate = "";
            // 日付を取得
            ParseSetSellDate(EditPasteText);

            // 日付に「-」ハイフンがある場合は品番の区切りと間違えるので日付部分を削除
            if (MatchStrSellDate.IndexOf("-") >= 0)
                EditPasteText = myText.Replace(MatchStrSellDate, "");

            // テキスト内の日付の後ろ文字列は女優名として取得
            ParseSetActress(EditPasteText);

            // 品番を設定
            ParseSetProductNumber(EditPasteText);
        }
        public void ParseSetSellDate(string myText)
        {
            Regex regexDate = new Regex("[12][0-9][0-9][0-9][/-][0-1]{0,1}[0-9][/-][0-9]{0,1}[0-9]");

            if (regexDate.IsMatch(myText))
            {
                MatchStrSellDate = regexDate.Match(myText).Value.ToString();
                try
                {
                    SellDate = Convert.ToDateTime(MatchStrSellDate);
                    DispSellDate = SellDate.ToString("yyyyMMdd");
                }
                catch (Exception)
                {
                    // 何もしない
                }
            }

            return;
        }
        public void ParseSetActress(string myText)
        {
            string edittext = "";
            if (MatchStrSellDate == null || MatchStrSellDate.Length <= 0)
            {
                if (EditPasteText == null)
                    edittext = myText;
                else if (!EditPasteText.Equals(myText))
                    edittext = myText;
            }
            else
            {
                edittext = Regex.Match(myText, MatchStrSellDate + "(.*)").Groups[0].Value;
                edittext = edittext.Replace(MatchStrSellDate, "");
            }

            if (edittext.Trim().Length <= 0)
                return;

            Remark = ConvertActress(edittext);
        }

        private string ConvertActress(string myText)
        {
            string[] arrSplit = { " ", ",", "／" };
            string sepa = "";
            string[] arrActress = null;
            string actresses = "";

            foreach (string split in arrSplit)
            {
                string[] arrsepa = { split };
                arrActress = myText.Split(arrsepa, StringSplitOptions.None);
                if (arrActress.Length > 1)
                {
                    MatchStrActresses = myText;
                    sepa = split;
                    break;
                }
            }
            if (sepa.Length <= 0)
                actresses = myText.Trim();
            else
            {
                foreach (string actress in arrActress)
                {
                    if (actresses.Length > 0)
                        actresses += "、";
                    actresses += actress;
                }
            }

            return actresses;
        }

        private string[] GetArrActress(char mySepaChar, string myText)
        {
            string[] arrActress = myText.Split(mySepaChar);

            if (arrActress.Length >= 2)
                return arrActress;

            return null;
        }

        public void ParseSetProductNumber(string myText)
        {
            Regex regex = new Regex("\\[{0,1}[A-Za-z]{1,}-[0-9]*\\]{0,1}[A-Za-z]{0,1}");

            // 品番っぽい文字列が存在する場合は暫定でAVRIPを設定
            if (regex.IsMatch(myText))
            {
                MatchStrProductNumber = regex.Match(myText).Value.ToString();
                //Debug.Print(matchtext);
                ProductNumber = MatchStrProductNumber.Replace("[", "").Replace("]", "").ToUpper();
                Kind = KIND_AVRIP;
            }
            // 品番っぽいのが無い場合は、数字のみで品番を取得
            else
            {
                Regex regexP1 = new Regex("[0-9]*[_-][0-9]*");
                Regex regexP2 = new Regex(" [0-9]*");
                Regex regexP3 = new Regex(" [A-Za-z]*[0-9]*");

                if (regexP1.IsMatch(myText))
                    ProductNumber = regexP1.Match(myText).Value.ToString().Trim();
                else if (regexP2.IsMatch(myText))
                    ProductNumber = regexP2.Match(myText).Value.ToString().Trim();
                else if (regexP3.IsMatch(myText))
                    ProductNumber = regexP3.Match(myText).Value.ToString().Trim().ToUpper();

                Kind = 0;
                if (ProductNumber != null && ProductNumber.Length > 0)
                    MatchStrProductNumber = ProductNumber;

                if (MatchStrSellDate.Length <= 0)
                {
                    string datestr = "";
                    if (regexP1.IsMatch(myText))
                    {
                        string[] splitStr = { "_", "-" };
                        datestr = ProductNumber.Split(splitStr, StringSplitOptions.None)[0];
                    }
                    else if (regexP2.IsMatch(myText))
                        datestr = ProductNumber;
                    if (datestr.Length > 0)
                    {
                        try
                        {
                            SellDate = DateTime.ParseExact(datestr, "MMddyy", CultureInfo.InvariantCulture);
                        }
                        catch (FormatException)
                        {
                            // 何もしない
                        }
                    }

                    return;
                }

            }

            if (MatchStrSellDate.Length <= 0)
            {
                return;
            }

            string suffixText = Regex.Match(myText, DispSellDate + "(.*)").Groups[0].Value;

            if (suffixText.Trim().Length <= 0)
                return;

            Remark = ConvertActress(suffixText);
        }

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

    }
}
