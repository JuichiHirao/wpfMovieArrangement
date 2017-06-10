using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace wpfMovieArrangement
{
    class MovieImportData
    {
        public List<HdInfo> HdKindList = new List<HdInfo>();

        private void SetHdInfo()
        {
            HdKindList.Add(new HdInfo(1, "60fps HD", "\\[FHD60fps\\]"));
            HdKindList.Add(new HdInfo(2, "FHD", "\\[FHD\\]"));
            HdKindList.Add(new HdInfo(3, "FHD", "FHD"));
            HdKindList.Add(new HdInfo(4, "HD", "\\[HD\\]"));
        }
        public class HdInfo
        {
            public HdInfo(int myKind, string myName, string myStrRegex)
            {
                Kind = myKind;
                Name = myName;
                StrtRegex = myStrRegex;
            }
            public int Kind { get; set; }

            public string StrtRegex { get; set; }

            public string Name { get; set; }
        }

        public int Id { get; set; }

        public int FileId { get; set; }

        public string CopyText { get; set; }

        public int Kind { get; set; }

        public string MatchProduct { get; set; }

        public string ProductNumber { get; set; }

        public string GetFileSearchString()
        {
            return ProductNumber + " " + ProductNumber.Replace("-", "");
        }

        private string _StrProductDate;
        public string StrProductDate
        {
            get
            {
                return _StrProductDate;
            }
            set
            {
                try
                {
                    DateTime dt = Convert.ToDateTime(value);
                    ProductDate = dt;
                }
                catch (Exception ex)
                {
                }

            }
        }
        public DateTime ProductDate{ get; set; }

        public string Maker { get; set; }

        public string Title { get; set; }

        public string Actresses { get; set; }

        public HdInfo HdKind { get; set; }

        private bool _HdFlag;
        public bool? HdFlag
        {
            get
            {
                return _HdFlag;
            }
            set
            {
                if (value == null)
                    _HdFlag = false;
                else
                {
                    _HdFlag = (bool)value;
                }
            }
        }

        public void SetHdKind(int myKind)
        {
            if (myKind == 0)
            {
                _HdFlag = false;
                HdKind = null;
            }

            foreach (HdInfo hdInfo in HdKindList)
            {
                if (hdInfo.Kind == myKind)
                {
                    HdKind = hdInfo;
                    _HdFlag = true;
                    break;
                }
            }

            return;
        }

        private bool _RarFlag;
        public bool? RarFlag
        {
            get
            {
                return _RarFlag;
            }
            set
            {
                if (value == null)
                    _RarFlag = false;
                else
                {
                    _RarFlag = (bool)value;
                }
            }
        }

        public string Tag { get; set; }

        public string Filename { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public void GenerateFilename()
        {
            string strDt = ProductDate.ToString("yyyMMdd");

            string name = "";
            if (Kind == 1)
                name += "[AVRIP]";
            else if (Kind == 2)
                name += "[IVRIP]";
            else if (Kind == 3)
                name += "[裏AVRIP]";
            else if (Kind == 4)
                name += "[DMMR-AVRIP]";
            else if (Kind == 5)
                name += "[DMMR-IVRIP]";

            name += "【" + Maker + "】";
            name += Title + " ";
            name += "[" + ProductNumber + " " + strDt + "]";
            if (Actresses.Trim().Length > 0)
                name += "（" + Actresses + "）";

            Filename = name;
        }

        public MovieImportData()
        {
            HdKind = null;
            SetHdInfo();
        }

        public string GetFilterProductNumber()
        {
            string searchText = "";

            if (ProductNumber != null && ProductNumber.Length > 0)
            {
                string HyphenStr = ProductNumber;
                string HyphenWithoutStr = HyphenStr.Replace("-", "");

                if (HyphenStr.Equals(HyphenWithoutStr))
                    searchText = HyphenStr;
                else
                    searchText = HyphenStr + " " + HyphenWithoutStr;
            }

            return searchText;
        }

        public MovieImportData(string myPasteText)
        {
            SetHdInfo();
            string pasteText = "";

            if (myPasteText.IndexOf("RAR") == 0)
            {
                pasteText = myPasteText.Replace("RAR ", "");
                _RarFlag = true;
            }
            else
                pasteText = myPasteText;

            if (pasteText.IndexOf("[AVRIP]") == 0
                || pasteText.IndexOf("[IVRIP]") == 0
                || pasteText.IndexOf("[裏AVRIP]") == 0
                || pasteText.IndexOf("[DMMR-AVRIP]") == 0
                || pasteText.IndexOf("[DMMR-IVRIP]") == 0)
            {
                Regex regexDate = new Regex("[12][0-9][0-9][0-9][01][0-9][0-3][0-9]");
                if (regexDate.IsMatch(pasteText))
                {
                    MatchCollection mc = regexDate.Matches(pasteText);
                    string strDate = mc[0].Value.ToString();

                    string[] expectedFormat = { "yyyyMMdd" };
                    ProductDate = System.DateTime.ParseExact(strDate,
                                expectedFormat,
                                System.Globalization.DateTimeFormatInfo.InvariantInfo,
                                System.Globalization.DateTimeStyles.None);

                    StrProductDate = ProductDate.ToString("yyyy/MM/dd");
                }

                Regex regexProductNumber = new Regex("^.* ");
                int lastPos = pasteText.LastIndexOf("[");
                string str = pasteText.Substring(lastPos+1);
                if (regexProductNumber.IsMatch(str))
                {
                    MatchCollection mc = regexProductNumber.Matches(str);
                    ProductNumber = mc[0].Value.ToString().Trim();
                }

                if (pasteText.IndexOf("[AVRIP]") == 0)
                    Kind = 1;
                else if (pasteText.IndexOf("[IVRIP]") == 0)
                    Kind = 2;
                else if (pasteText.IndexOf("[裏AVRIP]") == 0)
                    Kind = 3;

                int posFrom = pasteText.IndexOf("【");
                int posTo = pasteText.IndexOf("】");
                if (posFrom >= 0)
                    Maker = pasteText.Substring(posFrom+1, (posTo - posFrom)-1);

                Title = pasteText.Substring(posTo+1, (lastPos - posTo)-1).Trim();

                int acPos = pasteText.Substring(lastPos).IndexOf("（");
                if (acPos >= 0)
                    Actresses = pasteText.Substring(lastPos).Replace("（", "").Replace("）", "");
            }

            return;
        }

        public void ParseFromPasteText(string myPasteText)
        {
            string editText = "";
            Regex regexHd = null;
            HdInfo matchHdInfo = null;
            foreach (HdInfo hd in HdKindList)
            {
                regexHd = new Regex(hd.Name);

                if (regexHd.IsMatch(myPasteText))
                {
                    editText = myPasteText.Replace(regexHd.Match(myPasteText).Value.ToString(), "");
                    editText.Trim();
                    HdKind = hd;
                    break;
                }
            }

            if (matchHdInfo == null)
            {
                editText = myPasteText.Trim();
            }

            // 日付を取得
            string matchProductDate = ParseSetSellDate(editText);

            // 日付に「-」ハイフンがある場合は品番の区切りと間違えるので日付部分を削除
            //if (MatchStrSellDate.IndexOf("-") >= 0)
            //    EditPasteText = myText.Replace(MatchStrSellDate, "");

            // テキスト内の日付の後ろ文字列は女優名として取得
            ParseSetActress(editText, matchProductDate);

            // 品番を設定
            ParseSetProductNumber(editText);
        }

        public string ParseSetSellDate(string myText)
        {
            if (myText == null || myText.Length > 0)
                return "";

            Regex regexDate = new Regex("[12][0-9][0-9][0-9][/-][0-1]{0,1}[0-9][/-][0-9]{0,1}[0-9]");

            string matchStr = "";
            if (regexDate.IsMatch(myText))
            {
                matchStr = regexDate.Match(myText).Value.ToString();
                try
                {
                    ProductDate = Convert.ToDateTime(matchStr);
                    //DispSellDate = ProductDate.ToString("yyyyMMdd");
                }
                catch (Exception)
                {
                    // 何もしない
                }
            }

            return matchStr;
        }
        public void ParseSetActress(string myText, string myMatchProductDate)
        {
            string matchStr = "";
            if (myMatchProductDate != null && myMatchProductDate.Length > 0)
            {
                matchStr = Regex.Match(myText, myMatchProductDate + "(.*)").Groups[0].Value;
                matchStr = matchStr.Replace(myMatchProductDate, "").Trim();
            }

            //Remark = ConvertActress(matchStr);
        }

        public void ParseSetProductNumber(string myText)
        {
            Regex regex = new Regex("\\[{0,1}[0-9A-Za-z]{1,}-[0-9]*\\]{0,1}[A-Za-z]{0,1}");

            string matchStr = "";
            // 品番っぽい文字列が存在する場合は暫定でAVRIPを設定
            if (regex.IsMatch(myText))
            {
                matchStr = regex.Match(myText).Value.ToString();
                ProductNumber = matchStr.Replace("[", "").Replace("]", "").ToUpper();
            }
            // 品番っぽいのが無い場合は、数字のみで品番を取得
            else
            {
                Regex regexP1 = new Regex("[0-9]*[_-][0-9]*");
                Regex regexP2 = new Regex(" [0-9]*");
                Regex regexP3 = new Regex(" [A-Za-z]*[0-9]*");

                if (regexP1.IsMatch(myText))
                    matchStr = regexP1.Match(myText).Value.ToString().Trim();
                else if (regexP2.IsMatch(myText))
                    matchStr = regexP2.Match(myText).Value.ToString().Trim();
                else if (regexP3.IsMatch(myText))
                    matchStr = regexP3.Match(myText).Value.ToString().Trim().ToUpper();

                Kind = 0;
                if (matchStr != null && matchStr.Length > 0)
                    ProductNumber = matchStr;
            }
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
                    //MatchStrActresses = myText;
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
    }
}
