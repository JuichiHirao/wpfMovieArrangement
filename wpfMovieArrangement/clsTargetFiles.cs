using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace wpfMovieArrangement
{
    public class TargetFiles : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        /// <summary>
        /// FileControlの中で処理済みかのフラグ（FileControlクラスでのみ使用）
        /// </summary>
        public bool IsFinished { get; set; }

        protected bool _IsSelected;
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                _IsSelected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }

        protected FileInfo _FileInfo;
        public FileInfo FileInfo
        {
            get
            {
                return _FileInfo;
            }
            set
            {
                _FileInfo = value;
                Name = _FileInfo.Name;
            }
        }
        public string Name { get; set; }

        public string DispRelativePath { get; set; }

        public int FileCount { get; set; }

        public long FileSize { get; set; }

        public string PatternMatch { get; set; }

        public string[] MatchFiles { get; set; }

        protected string _strListUpdateDate;
        public string strListUpdateDate
        {
            get
            {
                return _strListUpdateDate;
            }
            set
            {
                _strListUpdateDate = value;
                NotifyPropertyChanged("strListUpdateDate");
            }
        }
        protected DateTime _ListUpdateDate;
        public DateTime ListUpdateDate
        {
            get
            {
                return _ListUpdateDate;
            }
            set
            {
                _ListUpdateDate = value;
                if (value == null)
                    strListUpdateDate = "";
                else
                    strListUpdateDate = ListUpdateDate.ToString("yyyy/MM/dd HH:mm:ss");

                NotifyPropertyChanged("ListUpdateDate");
            }
        }
        protected string _strMovieNewDate;
        public string strMovieNewDate
        {
            get
            {
                return _strMovieNewDate;
            }
            set
            {
                _strMovieNewDate = value;
                NotifyPropertyChanged("strMovieNewDate");
            }
        }
        protected DateTime _MovieNewDate;
        public DateTime MovieNewDate
        {
            get
            {
                return _MovieNewDate;
            }
            set
            {
                _MovieNewDate = value;
                if (value == null)
                    strMovieNewDate = "";
                else
                {
                    if (_MovieNewDate.Year == 1900)
                        strMovieNewDate = "";
                    else
                        strMovieNewDate = MovieNewDate.ToString("yyyy/MM/dd HH:mm:ss");
                }

                NotifyPropertyChanged("MovieNewDate");
            }
        }
        protected string _Message;
        public string Message
        {
            get
            {
                return _Message;
            }
            set
            {
                if (_Message == null)
                    _Message = value;
                else if (_Message.Length > 0)
                    _Message = _Message + "、" + value;
                else
                    _Message = value;
                NotifyPropertyChanged("Message");
            }
        }
    }
}
