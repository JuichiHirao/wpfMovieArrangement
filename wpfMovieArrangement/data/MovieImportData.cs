using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                name += "[DMMR-AVRIP]";

            name += "【" + Maker + "】";
            name += Title + " ";
            name += "[" + ProductNumber + " " + strDt + "]";
            if (Actresses.Trim().Length > 0)
                name += "（" + Actresses + "）";

            Filename = name;
        }
    }
}
