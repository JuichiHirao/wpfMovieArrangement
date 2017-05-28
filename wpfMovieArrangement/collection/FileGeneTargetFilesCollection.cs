using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace wpfMovieArrangement.collection
{
    class FileGeneTargetFilesCollection
    {
        public const string REGEX_TARGETFILE_EXTENTION = @".*\.avi$|.*\.wmv$|.*\.mpg$|.*ts$|.*divx$|.*mp4$|.*asf$|.*png$|.*jpg$|.*jpeg$|.*iso$|.*mkv$|.*\.m4v|.*\.rmvb|.*\.rm|.*\.rar|.*\.mov|.*\.3gp";

        public List<TargetFiles> listTargetFiles = null;
        public ICollectionView ColViewListTargetFiles;

        public string BasePath { get; set; }
        
        private List<string> FilterListProductNumber = null;

        private string _FilterSearchProductNumber;
        public string FilterSearchProductNumber
        {
            get
            {
                return _FilterSearchProductNumber;
            }
            set
            {
                _FilterSearchProductNumber = value;
                if (FilterListProductNumber == null)
                    FilterListProductNumber = new List<string>();
                else
                    FilterListProductNumber.Clear();

                string[] arrStr = value.Split(' ');

                foreach (string data in arrStr)
                {
                    if (data != null && data.Length > 0)
                        FilterListProductNumber.Add(data.ToUpper());
                }

                return;
            }
        }

        public FileGeneTargetFilesCollection(string myBasePath)
        {
            BasePath = myBasePath;

            DataSet();
        }

        private void DataSet()
        {
            SetDestFiles(BasePath, REGEX_TARGETFILE_EXTENTION);

            ColViewListTargetFiles = CollectionViewSource.GetDefaultView(listTargetFiles);
        }

        public void Clear()
        {
            FilterSearchProductNumber = "";
            FilterListProductNumber = null;
        }

        public void Refresh()
        {
            SetDestFiles(BasePath, REGEX_TARGETFILE_EXTENTION);

            Execute();
        }

        public void SetDestFiles(string myPath, string myTargetExtention)
        {
            Regex regex = new Regex(myTargetExtention, RegexOptions.IgnoreCase);
            Regex regexEdited = new Regex("^\\[AV|^\\[裏AV|^\\[IV");
            //IEnumerable<string> files = from file in Directory.GetFiles(@"\\SANDY2500\BitTorrent\JDownloader", "*", SearchOption.AllDirectories) where regex.IsMatch(file) select file;

            string[] files = Directory.GetFiles(@myPath, "*", System.IO.SearchOption.AllDirectories);

            if (listTargetFiles == null)
                listTargetFiles = new List<TargetFiles>();
            else
                listTargetFiles.Clear();

            foreach (var file in files)
            {
                if (!regex.IsMatch(file))
                    continue;

                FileInfo fileinfo = new FileInfo(file.ToString());

                if (regexEdited.IsMatch(fileinfo.Name))
                    continue;

                TargetFiles targetfiles = new TargetFiles();
                targetfiles.FileInfo = fileinfo;
                targetfiles.ListUpdateDate = fileinfo.LastWriteTime;
                targetfiles.FileSize = fileinfo.Length;
                targetfiles.DispRelativePath = fileinfo.Directory.ToString().Replace(@BasePath + "\\", "").Replace(@BasePath, "");
                targetfiles.IsSelected = false;

                listTargetFiles.Add(targetfiles);
            }

            return;
        }

        public List<TargetFiles> GetDestFiles(string myPath, string myTargetExtention)
        {
            Regex regex = new Regex(myTargetExtention, RegexOptions.IgnoreCase);
            Regex regexEdited = new Regex("^\\[AV|^\\[裏AV|^\\[IV");
            //IEnumerable<string> files = from file in Directory.GetFiles(@"\\SANDY2500\BitTorrent\JDownloader", "*", SearchOption.AllDirectories) where regex.IsMatch(file) select file;

            string[] files = Directory.GetFiles(@myPath, "*", System.IO.SearchOption.AllDirectories);

            List<TargetFiles> listDestFIles = new List<TargetFiles>();

            foreach (var file in files)
            {
                if (!regex.IsMatch(file))
                    continue;

                FileInfo fileinfo = new FileInfo(file.ToString());

                if (regexEdited.IsMatch(fileinfo.Name))
                    continue;

                TargetFiles targetfiles = new TargetFiles();
                targetfiles.FileInfo = fileinfo;
                targetfiles.ListUpdateDate = fileinfo.LastWriteTime;
                targetfiles.FileSize = fileinfo.Length;
                targetfiles.DispRelativePath = fileinfo.Directory.ToString().Replace(@BasePath + "\\", "").Replace(@BasePath, "");
                targetfiles.IsSelected = false;

                listDestFIles.Add(targetfiles);
            }

            return listDestFIles;
        }

        /*
        private bool FilterTargetFilesSearchFilter(object item)
        {
            TargetFiles mTarget = item as TargetFiles;

            string[] arrSearchWord = FilterSearchText.Split(' ');
            int Count = arrSearchWord.Length;

            // ファイル名のパスを含めて検索するため、基本フォルダを除いたパス付きファイル名を取得
            string filename = mTarget.FileInfo.FullName.Replace(BasePath, "");

            int MatchCount = 0;
            foreach (string word in arrSearchWord)
            {
                if (word.Length <= 0)
                    continue;

                //if (mTarget.Name.ToUpper().IndexOf(word.ToUpper()) >= 0)

                if (filename.ToUpper().IndexOf(word.ToUpper()) >= 0)
                    MatchCount++;
            }

            // AND検索の場合
            //if (Count <= MatchCount)
            //    return true;

            // OR検索の場合
            if (MatchCount >= 1)
                return true;

            return false;
        }
         */

        /*
        private void ExecuteMatchFiles()
        {
            List<TargetFiles> files = GetDestFiles(txtBasePath.Text, REGEX_TARGETFILE_EXTENTION);
            dgridCheckExistFiles.ItemsSource = files;

            if (txtProductNumber.Text != null && txtProductNumber.Text.Length > 0)
            {
                string searchword = "";
                string HyphenStr = txtProductNumber.Text;
                string HyphenWithoutStr = HyphenStr.Replace("-", "");

                if (HyphenStr.Equals(HyphenWithoutStr))
                    searchword = HyphenStr;
                else
                    searchword = HyphenStr + " " + HyphenWithoutStr;
                txtSearch.Text = searchword;

                dgridCheckExistFiles.Items.Filter = new Predicate<object>(FilterTargetFilesSearchFilter);

            }
        }
         */

        public void Execute()
        {
            ColViewListTargetFiles.Filter = delegate (object o)
            {
                TargetFiles data = o as TargetFiles;

                if (FilterListProductNumber == null || FilterListProductNumber.Count <= 0)
                    return true;

                string filename = data.FileInfo.Name;

                int matchCount = 0;
                foreach(string searchText in FilterListProductNumber)
                {
                    if (filename.ToUpper().IndexOf(searchText) >= 0)
                        matchCount++;
                }

                //Debug.Print(_FilterSearchProductNumber + " " + Convert.ToString(matchCount) + "    " + filename);

                if (matchCount > 0)
                    return true;

                return false;
            };

        }
    }
}
