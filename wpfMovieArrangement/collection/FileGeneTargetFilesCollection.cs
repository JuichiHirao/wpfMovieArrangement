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
        public static string REGEX_MOVIE_EXTENTION = @".*\.avi$|.*\.wmv$|.*\.mpg$|.*ts$|.*divx$|.*mp4$|.*asf$|.*png$|.*jpg$|.*jpeg$|.*iso$|.*mkv$|.*\.m4v|.*\.rmvb|.*\.rm|.*\.mov|.*\.3gp";
        public static string REGEX_MOVIEONLY_EXTENTION = @".*\.avi$|.*\.wmv$|.*\.mpg$|.*ts$|.*divx$|.*mp4$|.*asf$|.*iso$|.*mkv$|.*\.m4v|.*\.rmvb|.*\.rm|.*\.mov|.*\.3gp";
        public static string REGEX_TARGETFILE_EXTENTION = @".*\.avi$|.*\.wmv$|.*\.mpg$|.*ts$|.*divx$|.*mp4$|.*asf$|.*png$|.*jpg$|.*jpeg$|.*iso$|.*mkv$|.*\.m4v|.*\.rmvb|.*\.rm|.*\.rar|.*\.mov|.*\.3gp";

        public List<TargetFiles> listTargetFiles = null;
        public ICollectionView ColViewListTargetFiles;

        public string TargetExtention { get; set; }

        public bool ExecuteRar = false;

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

        public FileGeneTargetFilesCollection(string myBasePath, string myTargetExtention)
        {
            BasePath = myBasePath;
            TargetExtention = myTargetExtention;

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

            Refresh();
        }

        public TargetFiles GetSelectTargetMovieFile()
        {
            if (ExecuteRar)
            {
                foreach (TargetFiles file in ColViewListTargetFiles)
                    return file;
            }
            else
            {
                Regex regex = new Regex(REGEX_MOVIEONLY_EXTENTION);
                foreach (TargetFiles file in ColViewListTargetFiles)
                {
                    if (regex.IsMatch(file.Name))
                        return file;
                }
            }


            return null;
        }

        public List<TargetFiles> GetSelectTargetFiles()
        {
            List<TargetFiles> targetFilesList = new List<TargetFiles>();
            if (ExecuteRar)
            {
                foreach(TargetFiles data in listTargetFiles)
                {
                    //REGEX_MOVIE_EXTENTION
                    if (data.FileInfo.Extension.ToUpper().Equals(".RAR"))
                        continue;

                    if (FilterListProductNumber == null || FilterListProductNumber.Count <= 0)
                        continue;

                    string filename = data.FileInfo.Name;

                    int matchCount = 0;
                    foreach (string searchText in FilterListProductNumber)
                    {
                        if (filename.ToUpper().IndexOf(searchText) >= 0)
                            matchCount++;
                    }

                    //Debug.Print(_FilterSearchProductNumber + " " + Convert.ToString(matchCount) + "    " + filename);

                    if (matchCount > 0)
                        targetFilesList.Add(data);
                }
            }
            else
            {
                foreach (TargetFiles file in ColViewListTargetFiles)
                    targetFilesList.Add(file);
            }

            return targetFilesList;
        }

        public void SetActionTargetFile()
        {
            Regex regex = new Regex(REGEX_MOVIEONLY_EXTENTION);
            foreach (TargetFiles file in ColViewListTargetFiles)
            {
                file.IsSelected = true;
                Debug.Print("IsSelected true " + file.Name);
            }

            return;
        }

        public void Refresh()
        {
            SetDestFiles(BasePath, REGEX_TARGETFILE_EXTENTION);

            Execute();
        }

        public void Refresh(string myRegEx)
        {
            SetDestFiles(BasePath, myRegEx);

            Execute();
        }

        public void RefreshRarFile()
        {
            SetDestFiles(BasePath, REGEX_TARGETFILE_EXTENTION);

            ExecuteRarFile();
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

        public void ExecuteRarFile()
        {
            ExecuteRar = true;

            ColViewListTargetFiles.Filter = delegate (object o)
            {
                TargetFiles data = o as TargetFiles;

                if (!data.FileInfo.Extension.ToUpper().Equals(".RAR"))
                    return false;

                if (FilterListProductNumber == null || FilterListProductNumber.Count <= 0)
                    return true;

                string filename = data.FileInfo.Name;

                foreach (string searchText in FilterListProductNumber)
                {
                    if (filename.ToUpper().IndexOf(searchText) >= 0)
                    {
                        if (filename.IndexOf("part1.rar") >= 0)
                        {
                            string patternStr = Regex.Replace(filename, "part.[0-9]*", "*");

                            string[] arrFile = Directory.GetFiles(BasePath, patternStr, System.IO.SearchOption.TopDirectoryOnly);
                            data.FileCount = arrFile.Length;

                            return true;
                        }
                    }
                }

                //Debug.Print(_FilterSearchProductNumber + " " + Convert.ToString(matchCount) + "    " + filename);

                return false;
            };
        }

        public void Execute()
        {
            ExecuteRar = false;

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
