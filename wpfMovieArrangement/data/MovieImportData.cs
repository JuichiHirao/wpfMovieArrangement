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
        public int Id { get; set; }

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
    }
}
