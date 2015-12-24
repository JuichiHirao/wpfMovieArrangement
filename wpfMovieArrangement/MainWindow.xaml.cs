using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;
using System.ComponentModel;

namespace wpfMovieArrangement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly static RoutedCommand PasteDateCopy = new RoutedCommand("PasteDateCopy", typeof(MainWindow));
        public readonly static RoutedCommand ChangeModeNormalRar = new RoutedCommand("ChangeModeNormalRar", typeof(MainWindow));
        public readonly static RoutedCommand ChangeModeNormalMovie = new RoutedCommand("ChangeModeNormalMovie", typeof(MainWindow));
        public readonly static RoutedCommand ChangeModeDateCopy = new RoutedCommand("ChangeModeDateCopy", typeof(MainWindow));
        public readonly static RoutedCommand CahngeModeFilenameGenerate = new RoutedCommand("CahngeModeFilenameGenerate", typeof(MainWindow));
        public readonly static RoutedCommand CahngeModeKoreanPorno = new RoutedCommand("CahngeModeKoreanPorno", typeof(MainWindow));

        private const string REGEX_MOVIE_EXTENTION = @".*\.avi$|.*\.wmv$|.*\.mpg$|.*ts$|.*divx$|.*mp4$|.*asf$|.*jpg$|.*jpeg$|.*iso$|.*mkv$|.*\.m4v|.*\.rmvb|.*\.rm";
        private const string REGEX_MOVIEONLY_EXTENTION = @".*\.avi$|.*\.wmv$|.*\.mpg$|.*ts$|.*divx$|.*mp4$|.*asf$|.*iso$|.*mkv$|.*\.m4v|.*\.rmvb|.*\.rm";

        private List<MovieMaker> listMakers = null;
        ICollectionView ColViewListMakers;
        private int MaxListMakers = 0;
        private List<MovieFileContents> listFilesContents = null;
        private List<string> listTextTargetFileName = null;

        private List<MovieMaker> dispinfoSelectDataGridMakers = null;
        private List<KoreanPornoData> dispinfoSelectDataGridKoreanPorno = null;
        private string dispinfoKoreanPornoStorePath = null;

        SettingXmlControl settingControl = null;
        Setting setting = null;
        ViewModel ViewData;
        DateTime dispctrlAvripHistoryAccessDateTime;
        bool isSelectSameMaker = false;

        public int dispctrlMode = 0;
        public const int MODE_NORMALRAR = 1;
        public const int MODE_NORMALMOVIE = 2;
        public const int MODE_DATECOPY = 3;
        public const int MODE_FILENAMEGENERATE = 4;
        public const int MODE_KOREANPORNO = 5;

        public class ViewModel : INotifyPropertyChanged
        {
            private string _basePath;

            public string BasePath
            {
                get { return this._basePath; }
                set
                {
                    this._basePath = value;
                    var handler = this.PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("BasePath"));
                    }
                }
            }

            private string _labelPath;

            public string LabelPath
            {
                get { return this._labelPath; }
                set
                {
                    this._labelPath = value;
                    var handler = this.PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("LabelPath"));
                    }
                }
            }

            private string _koreanPornoPath;

            public string KoreanPornoPath
            {
                get { return this._koreanPornoPath; }
                set
                {
                    this._koreanPornoPath = value;
                    var handler = this.PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("KoreanPornoPath"));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public MainWindow()
        {
            InitializeComponent();

            ViewData = new ViewModel { BasePath = "" };
            this.DataContext = ViewData;

            CommandBindings.Add(new CommandBinding(ChangeModeNormalRar, (s, ea) => { ChangeModeNormalRarExecute(s, ea); }, (s, ea) => ea.CanExecute = true));
            CommandBindings.Add(new CommandBinding(ChangeModeNormalMovie, (s, ea) => { ChangeModeNormalMovieExecute(s, ea); }, (s, ea) => ea.CanExecute = true));
            CommandBindings.Add(new CommandBinding(ChangeModeDateCopy, (s, ea) => { ChangeModeDateCopyExecute(s, ea); }, (s, ea) => ea.CanExecute = true));
            CommandBindings.Add(new CommandBinding(CahngeModeFilenameGenerate, (s, ea) => { CahngeModeFilenameGenerateExecute(s, ea); }, (s, ea) => ea.CanExecute = true));
            CommandBindings.Add(new CommandBinding(CahngeModeKoreanPorno, (s, ea) => { CahngeModeKoreanPornoExecute(s, ea); }, (s, ea) => ea.CanExecute = true));
            CommandBindings.Add(new CommandBinding(PasteDateCopy, (s, ea) => { PasteDateCopyExecute(s, ea); }, (s, ea) => ea.CanExecute = true));

            listMakers = MovieMakers.GetAllData();
            MaxListMakers = listMakers.Count();

            dispctrlMode = MODE_NORMALMOVIE;

        }

        public void ChangeModeNormalRarExecute(object sender, RoutedEventArgs e)
        {
            dispctrlMode = MODE_NORMALRAR;

            ChangeModeNormal(null, null);
        }

        public void ChangeModeNormalMovieExecute(object sender, RoutedEventArgs e)
        {
            dispctrlMode = MODE_NORMALMOVIE;

            ChangeModeNormal(null, null);
        }

        public void ChangeModeNormal(object sender, RoutedEventArgs e)
        {
            lgridMain.Visibility = System.Windows.Visibility.Visible;

            wpanelNormal.Visibility = System.Windows.Visibility.Visible;
            lgridNormalChangeFilename.Visibility = System.Windows.Visibility.Visible;

            lgridDateCopySource.Visibility = System.Windows.Visibility.Collapsed;
            lgridDateCopyDestination.Visibility = System.Windows.Visibility.Collapsed;

            lgridFilenameGenerate.Visibility = System.Windows.Visibility.Collapsed;

            lgridKoreanPornoArrange.Visibility = System.Windows.Visibility.Collapsed;

            OnGridTargetDisplay(null, null);
        }
        public void ChangeModeDateCopyExecute(object sender, RoutedEventArgs e)
        {
            dispctrlMode = MODE_DATECOPY;

            lgridMain.Visibility = System.Windows.Visibility.Visible;

            wpanelNormal.Visibility = System.Windows.Visibility.Collapsed;
            lgridNormalChangeFilename.Visibility = System.Windows.Visibility.Collapsed;

            lgridDateCopySource.Visibility = System.Windows.Visibility.Visible;
            lgridDateCopyDestination.Visibility = System.Windows.Visibility.Visible;

            lgridFilenameGenerate.Visibility = System.Windows.Visibility.Collapsed;

            dgridArrangementTarget.ItemsSource = null;
            dgridDestFile.ItemsSource = null;
        }

        public void CahngeModeFilenameGenerateExecute(object sender, RoutedEventArgs e)
        {
            dispctrlMode = MODE_FILENAMEGENERATE;

            lgridMain.Visibility = System.Windows.Visibility.Collapsed;
            wpanelNormal.Visibility = System.Windows.Visibility.Collapsed;
            lgridNormalChangeFilename.Visibility = System.Windows.Visibility.Collapsed;

            lgridDateCopySource.Visibility = System.Windows.Visibility.Collapsed;
            lgridDateCopyDestination.Visibility = System.Windows.Visibility.Collapsed;

            lgridFilenameGenerate.Width = this.ActualWidth - 30;
            lgridFilenameGenerate.Visibility = System.Windows.Visibility.Visible;

            lgridKoreanPornoArrange.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void CahngeModeKoreanPornoExecute(object sender, RoutedEventArgs e)
        {
            dispctrlMode = MODE_KOREANPORNO;

            lgridMain.Visibility = System.Windows.Visibility.Collapsed;
            wpanelNormal.Visibility = System.Windows.Visibility.Collapsed;
            lgridNormalChangeFilename.Visibility = System.Windows.Visibility.Collapsed;

            lgridDateCopySource.Visibility = System.Windows.Visibility.Collapsed;
            lgridDateCopyDestination.Visibility = System.Windows.Visibility.Collapsed;

            lgridFilenameGenerate.Visibility = System.Windows.Visibility.Collapsed;

            lgridKoreanPornoArrange.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnPasteSource_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.IDataObject data = Clipboard.GetDataObject();

            List<TargetFiles> files = GetClipbardFiles(data);

            if (files.Count > 0 && dgridArrangementTarget.ItemsSource == null)
                dgridArrangementTarget.ItemsSource = files;

            return;
        }

        private void btnPasteDestination_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.IDataObject data = Clipboard.GetDataObject();

            List<TargetFiles> files = GetClipbardFiles(data);

            if (files.Count > 0 && dgridArrangementTarget.ItemsSource != null)
                dgridDestFile.ItemsSource = files;

            return;
        }

        public void PasteDateCopyExecute(object sender, RoutedEventArgs e)
        {
            System.Windows.IDataObject data = Clipboard.GetDataObject();

            if (dispctrlMode == MODE_DATECOPY)
            {
                List<TargetFiles> files = GetClipbardFiles(data);

                if (files.Count > 0 && dgridArrangementTarget.ItemsSource == null)
                    dgridArrangementTarget.ItemsSource = files;
                else if (files.Count > 0)
                    dgridDestFile.ItemsSource = files;
            }
            else if (dispctrlMode == MODE_FILENAMEGENERATE)
            {
                btnPasteTitleText_Click(null, null);
            }

            return;
        }

        public List<TargetFiles> GetClipbardFiles(System.Windows.IDataObject myData)
        {
            List<TargetFiles> listPasteFile = new List<TargetFiles>();

            try
            {
                if (myData.GetDataPresent(DataFormats.Text))
                {
                    string ClipboardText = (string)myData.GetData(DataFormats.Text);
                    // クリップボードのテキストを改行毎に配列に設定
                    string[] ClipBoardList = ClipboardText.Split('\n');

                    foreach (string file in ClipBoardList)
                    {
                        FileInfo fileinfo = new FileInfo(file);

                        TargetFiles targetfiles = new TargetFiles();
                        targetfiles.FileInfo = fileinfo;
                        targetfiles.ListUpdateDate = fileinfo.LastWriteTime;
                        targetfiles.FileSize = fileinfo.Length;
                        targetfiles.DispRelativePath = fileinfo.Directory.ToString().Replace(@txtBasePath.Text + "\\", "").Replace(@txtBasePath.Text, "");

                        listPasteFile.Add(targetfiles);
                    }
                }

                if (myData.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] arrfiles = (string[])myData.GetData(DataFormats.FileDrop);
                    foreach (string file in arrfiles)
                    {
                        FileInfo fileinfo = new FileInfo(file);

                        TargetFiles targetfiles = new TargetFiles();
                        targetfiles.FileInfo = fileinfo;
                        targetfiles.ListUpdateDate = fileinfo.LastWriteTime;
                        targetfiles.FileSize = fileinfo.Length;
                        targetfiles.DispRelativePath = fileinfo.Directory.ToString().Replace(@txtBasePath.Text + "\\", "").Replace(@txtBasePath.Text, "");

                        listPasteFile.Add(targetfiles);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return listPasteFile;
        }

        private bool CanGetDirectoryInfo()
        {
            if (Validation.GetHasError(txtBasePath))
                return false;

            if (Validation.GetHasError(txtLabelPath))
                return false;

            if (Validation.GetHasError(txtKoreanPornoPath))
                return false;

            return true;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            settingControl = new SettingXmlControl();
            setting = settingControl.GetData();

            if (setting.BasePath == null)
            {
                MessageBox.Show("SETTING.xmlが存在しないか、BasePathが設定されていません");
                return;
            }
            txtBasePath.Text = setting.BasePath;
            txtLabelPath.Text = setting.LabelPath;
            txtKoreanPornoPath.Text = setting.KoreanPornoPath;

            //if (!CanGetDirectoryInfo())
            //    return;

            ChangeModeNormalMovieExecute(null, null);

            List<string> listTextFileName = new List<string>();
            listTextTargetFileName = new List<string>();

            OnGridTargetDisplay(null, null);

            dgridDestFile.ItemsSource = GetDestFiles(txtBasePath.Text);

            //txtStatusBar.IsReadOnly = true;
            txtStatusBar.Width = statusbarMain.ActualWidth;
            txtStatusBar.Background = statusbarMain.Background;

            dgridSelectTargetFilename.Width = statusbarMain.ActualWidth;

            System.IO.StreamReader strmReader = null;
            try
            {
                // 行数の取得用のテキストファイルを読み込み
                strmReader = new System.IO.StreamReader(@"Z:\TEXT\AVRIP 履歴.txt", System.Text.Encoding.GetEncoding("UTF-16"));

                FileInfo fileinfo = new FileInfo(@"Z:\TEXT\AVRIP 履歴.txt");
                dispctrlAvripHistoryAccessDateTime = fileinfo.LastWriteTime;

                string line = "";
                bool isFinished = false;
                bool isStart = false;
                while ((line = strmReader.ReadLine()) != null)
                {
                    if (line.Trim().Length <= 0)
                        continue;

                    if (isFinished == false)
                    {
                        if (line.Equals("--------------------------------------START-------------------------------------------"))
                        {
                            isStart = true;
                            continue;
                        }
                        else
                        {
                            if (isStart)
                                listTextTargetFileName.Add(line);
                        }
                    }

                    if (isFinished == false && line.Equals("【【【完了】】】"))
                    {
                        isFinished = true;
                        continue;
                    }

                    if (!isFinished)
                        continue;

                    listTextFileName.Add(line);

                }
                MovieFileContentsParent parent = new MovieFileContentsParent();

                listFilesContents = parent.GetDbContents();
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
            finally
            {
                if (strmReader != null)
                    strmReader.Close();
            }

            DbConnection localDbCon = new DbConnection();
            txtbDbNowDate.Text = localDbCon.getDateStringSql("SELECT GETDATE()");

            //lgridKoreanPornoArrange.Visibility = System.Windows.Visibility.Visible;
            //lgridMain.Visibility = System.Windows.Visibility.Collapsed;
            dispinfoKoreanPornoStorePath = txtKoreanPornoPath.Text;

            dgridKoreanPorno.ItemsSource = KoreanPorno.GetFolderData(dispinfoKoreanPornoStorePath);

            //            listTextFileName.Add();
        }
        public List<TargetFiles> GetDestFiles(string myPath)
        {
            Regex regex = new Regex(REGEX_MOVIE_EXTENTION, RegexOptions.IgnoreCase);
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
                targetfiles.DispRelativePath = fileinfo.Directory.ToString().Replace(@txtBasePath.Text + "\\", "").Replace(@txtBasePath.Text, "");

                listDestFIles.Add(targetfiles);
            }

            return listDestFIles;
        }

        public List<TargetFiles> GetRarFileInfo(string myPath)
        {
            if (myPath == null)
                return null;

            FileInfo fileinfoMain = new FileInfo(myPath);

            List<TargetFiles> listTargetFiles = new List<TargetFiles>();
            string[] arrTargetRarFiles = null;

            try
            {
                arrTargetRarFiles = Directory.GetFiles(myPath, "*rar", System.IO.SearchOption.TopDirectoryOnly);
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                //Directory.CreateDirectory(myPath);
                return null;
            }

            List<FileInfo> SameFiles = new List<FileInfo>();

            var SortedRarFiles = from rarfile in arrTargetRarFiles
                                 orderby rarfile ascending
                                 select rarfile;

            foreach (string rarfile in SortedRarFiles)
            {
                FileInfo fileinfo = new FileInfo(rarfile);

                bool IsSame = false;
                foreach (FileInfo SameFile in SameFiles)
                {
                    if (SameFile.Name.Equals(fileinfo.Name))
                    {
                        IsSame = true;
                        break;
                    }
                }

                if (IsSame)
                    continue;

                Regex regex = new Regex("part.[0-9]*");
                Match match = regex.Match(fileinfo.Name);

                int FileCount = 0;
                string PatternMatchStr = "";
                string[] MatchFiles = null;
                if (match.Success)
                {
                    PatternMatchStr = Regex.Replace(fileinfo.Name, "part.[0-9]*", "*");

                    MatchFiles = Directory.GetFiles(myPath, PatternMatchStr, System.IO.SearchOption.TopDirectoryOnly);
                    FileCount = MatchFiles.Length;

                    for (int IdxArr = 0; IdxArr < MatchFiles.Length; IdxArr++)
                        SameFiles.Add(new FileInfo(MatchFiles[IdxArr]));
                }

                TargetFiles file = new TargetFiles();
                file.FileInfo = fileinfo;
                file.ListUpdateDate = fileinfo.LastWriteTime;
                file.FileCount = FileCount;
                file.PatternMatch = PatternMatchStr;
                file.MatchFiles = MatchFiles;

                listTargetFiles.Add(file);
            }

            return listTargetFiles;
        }

        private void OnDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 「wpf datagrid checkbox single click」で検索
            // 参考：http://social.msdn.microsoft.com/Forums/ja-JP/wpfja/thread/8a9a0654-1aff-4144-9167-232b2a91fafe/
            //       http://wpf.codeplex.com/wikipage?title=Single-Click Editing&ProjectName=wpf
            DataGridCell cell = sender as DataGridCell;

            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                TargetFiles selStartFile = (TargetFiles)dgridArrangementTarget.SelectedItem;

                if (selStartFile != null)
                {
                    DataGridRow row = FindVisualParent<DataGridRow>(cell);
                    TargetFiles selEndFile = row.Item as TargetFiles;
                    //Debug.Print("Shiftキーが押されたよ name [" + selStartFile.Name + "] ～ [" + selEndFile.Name + "]");

                    bool selStart = false;
                    foreach (TargetFiles file in dgridArrangementTarget.ItemsSource)
                    {
                        if (file.Name.Equals(selStartFile.Name))
                            selStart = true;

                        if (selStart)
                            file.IsSelected = true;

                        if (file.Name.Equals(selEndFile.Name))
                            break;
                    }

                    return;
                }
            }

            // 編集可能なセルの場合のみ実行
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                // フォーカスが無い場合はフォーカスを取得
                if (!cell.IsFocused)
                    cell.Focus();

                DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    }
                    else
                    {
                        DataGridRow row = FindVisualParent<DataGridRow>(cell);

                        TargetFiles selFile = row.Item as TargetFiles;
                        if (row != null && !row.IsSelected)
                        {
                            if (selFile.IsSelected)
                                selFile.IsSelected = false;
                            else
                                //row.IsSelected = true;
                                selFile.IsSelected = true;
                        }
                        else
                        {
                            if (row.IsSelected && selFile.IsSelected)
                                row.IsSelected = false;

                            selFile.IsSelected = false;
                        }
                    }
                }
            }


            /* Original Code
                // 編集可能なセルの場合のみ実行
                if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
                {
                    // フォーカスが無い場合はフォーカスを取得
                    if (!cell.IsFocused)
                        cell.Focus();

                    DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                    if (dataGrid != null)
                    {
                        if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                        {
                            if (!cell.IsSelected)
                                cell.IsSelected = true;
                        }
                        else
                        {
                            DataGridRow row = FindVisualParent<DataGridRow>(cell);

                            if (row != null && !row.IsSelected)
                                row.IsSelected = true;
                        }
                    }
                }
             */
        }
        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        private void btnExecuteNameChange_Click(object sender, RoutedEventArgs e)
        {
            if (dispctrlMode == MODE_DATECOPY)
            {
                List<TargetFiles> listTarget = (List<TargetFiles>)dgridArrangementTarget.ItemsSource;
                TargetFiles srcfile = null;

                if (listTarget == null)
                {
                    MessageBox.Show("コピー元のファイルが存在しません");
                    return;
                }
                if (listTarget.Count > 1)
                {
                    List<TargetFiles> listSelTarget = (List<TargetFiles>)dgridArrangementTarget.SelectedItems;

                    if (listSelTarget != null && listSelTarget.Count > 1)
                    {
                        MessageBox.Show("複数存在するので、上のファイル名からコピー元を選択して下さい");
                        return;
                    }
                    srcfile = listSelTarget[0];
                }
                else if (listTarget.Count <= 0)
                {
                    if (listTarget.Count != 1)
                    {
                        MessageBox.Show("コピー元のファイルが存在しません");
                        return;
                    }

                    return;
                }
                srcfile = listTarget[0];

                List<TargetFiles> listDestFiles = (List<TargetFiles>)dgridDestFile.ItemsSource;

                if (listDestFiles == null || listDestFiles.Count <= 0)
                {
                    MessageBox.Show("コピー先のファイルが存在しません");
                    return;
                }

                string message = "";
                foreach (TargetFiles dest in listDestFiles)
                {
                    bool isFinished = false;
                    while (isFinished == false)
                    {
                        try
                        {
                            dest.FileInfo.LastWriteTime = srcfile.ListUpdateDate;
                            isFinished = true;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            MessageBoxResult res = MessageBox.Show("ファイルの読取専用を外して下さい、再実行しますか？", "", MessageBoxButton.YesNo);

                            if (res == MessageBoxResult.No)
                                break;
                        }
                    }

                    if (isFinished)
                        message += dest.Name + "を" + srcfile.ListUpdateDate.ToString("yyyy/MM/dd HH:mm:ss") + "に変更しました\n";
                }

                txtStatusBar.Text = message;
                dgridArrangementTarget.ItemsSource = null;
                dgridDestFile.ItemsSource = null;

                return;
            }
            // 入力チェック
            string filename = txtChangeFileName.Text;

            if (filename.Length <= 0)
            {
                MessageBox.Show("ファイル名に入力がありません");
                return;
            }

            // 元のファイル情報を取得
            TargetFiles srcfiles = (TargetFiles)dgridArrangementTarget.SelectedItem;

            if (srcfiles == null)
            {
                MessageBox.Show("上のファイル名からコピー元を選択して下さい");
                return;
            }

            List<TargetFiles> listSelected = new List<TargetFiles>();

            foreach (TargetFiles files in dgridDestFile.ItemsSource)
            {
                if (files.IsSelected)
                    listSelected.Add(files);
            }

            FileControl fileControl = new FileControl();

            fileControl.BasePath = txtBasePath.Text;
            fileControl.DestFilename = txtChangeFileName.Text;
            fileControl.LabelPath = txtLabelPath.Text;
            fileControl.SourceFile = srcfiles.FileInfo;

            List<TargetFiles> listTargetFiles = (List<TargetFiles>)dgridDestFile.ItemsSource;

            try
            {
                // 選択したファイルのみを対象に内部プロパティへ設定
                fileControl.SetSelectedOnlyFiles(listTargetFiles);

                // JPEGの変換用の情報を生成する（日付コピー等はまだ実行されない）
                fileControl.SetJpegActionInfo();

                // 動画の変換用の情報を生成する（日付コピー等はまだ実行されない）
                fileControl.SetMovieActionInfo();

                // データベースへ登録用の情報を生成する
                fileControl.SetDbMovieFilesInfo();

                // 動画、画像ファイルの移動、日付コピー等の実行
                fileControl.Execute();

                // データベースへ登録
                fileControl.DatabaseExport();

                // テキストファイルから削除
                fileControl.RemoveTextFilenameLine(txtbSourceFilename.Text);
            }
            catch (Exception exp)
            {
                Debug.Write(exp);
                MessageBox.Show(exp.Message);
                return;
            }
            finally
            {
                foreach (TargetFiles file in dgridDestFile.ItemsSource)
                    file.IsSelected = false;
            }

            // RARファイルの場合は一式を削除
            if (menuitemRarFiles.IsChecked)
            {
                // ネットワーク経由のパスの場合はゴミ箱でなく「DELETE」フォルダに移動
                if (txtBasePath.Text.IndexOf("\\") == 0)
                {
                    string DeleteDir = System.IO.Path.Combine(txtBasePath.Text, "DELETE");

                    if (!System.IO.File.Exists(DeleteDir))
                        System.IO.Directory.CreateDirectory(DeleteDir);

                    // ソースファイルのパターンマッチする全ファイルをゴミ箱に移動
                    Debug.Print("Source Name [" + srcfiles.Name + "] --> [" + DeleteDir + "]");
                    string[] MatchFiles = srcfiles.MatchFiles;

                    if (MatchFiles != null)
                    {
                        foreach (string file in MatchFiles)
                        {
                            Debug.Print("DELETEフォルダ移動 [" + file + "]");
                            string DestPathname = System.IO.Path.Combine(DeleteDir, new FileInfo(file).Name);

                            System.IO.File.Move(file, DestPathname);
                            //FileSystem.DeleteFile(
                            //    file,
                            //    UIOption.OnlyErrorDialogs,
                            //    RecycleOption.SendToRecycleBin);
                        }
                    }
                }
                else
                {
                    // ソースファイルのパターンマッチする全ファイルをゴミ箱に移動
                    Debug.Print("Source Name [" + srcfiles.Name + "]");
                    string[] MatchFiles = srcfiles.MatchFiles;

                    if (MatchFiles != null)
                    {
                        foreach (string file in MatchFiles)
                        {
                            Debug.Print("ゴミ箱移動 [" + file + "]");

                            FileSystem.DeleteFile(
                                file,
                                UIOption.OnlyErrorDialogs,
                                RecycleOption.SendToRecycleBin);
                        }
                    }
                }
            }

            Window_Loaded(null, null);

            txtChangeFileName.Text = "";
            txtSearch.Text = "";
            dgridArrangementTarget.Items.Filter = null;
            dgridDestFile.Items.Filter = null;
        }

        private void menuitemNameCopy(object sender, RoutedEventArgs e)
        {
            //コピーするファイルのパスをStringCollectionに追加する
            TargetFiles file = (TargetFiles)dgridDestFile.SelectedItem;
            string NameWithoutExt = file.FileInfo.Name.Replace(file.FileInfo.Extension, "");
            Clipboard.SetText(NameWithoutExt);
        }

        private void OnGridTargetDisplay(object sender, RoutedEventArgs e)
        {
            string SelectMenuHeader = "";
            MenuItem menu = (MenuItem)sender;

            if (menu != null)
            {
                SelectMenuHeader = menu.Header.ToString();

                if (SelectMenuHeader.IndexOf("RARファイル一致") >= 0)
                    dispctrlMode = MODE_NORMALRAR;
                else
                    dispctrlMode = MODE_NORMALMOVIE;
            }

            if (dispctrlMode == MODE_NORMALRAR)
            {
                menuitemRarFiles.IsChecked = true;
                menuitemMovieFiles.IsChecked = false;
                dgridArrangementTarget.ItemsSource = GetRarFileInfo(@txtBasePath.Text);
            }
            else
            {
                menuitemRarFiles.IsChecked = false;
                menuitemMovieFiles.IsChecked = true;

                if (!CanGetDirectoryInfo())
                    return;

                dgridArrangementTarget.ItemsSource = GetDestFiles(txtBasePath.Text);
            }
        }

        private void dgridArrangementTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TargetFiles file = (TargetFiles)dgridArrangementTarget.CurrentItem;

            int i = 0;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            OnGridTargetDisplay(null, null);

            if (!CanGetDirectoryInfo())
                return;

            dgridDestFile.ItemsSource = GetDestFiles(txtBasePath.Text);
        }

        private bool FilterContentsItemAndSearchFilter(object item)
        {
            TargetFiles mTarget = item as TargetFiles;

            string[] arrSearchWord = txtSearch.Text.Split(' ');
            int Count = arrSearchWord.Length;

            // ファイル名のパスを含めて検索するため、基本フォルダを除いたパス付きファイル名を取得
            string filename = mTarget.FileInfo.FullName.Replace(txtBasePath.Text, "");

            int MatchCount = 0;
            foreach (string word in arrSearchWord)
            {
                //if (mTarget.Name.ToUpper().IndexOf(word.ToUpper()) >= 0)

                if (filename.ToUpper().IndexOf(word.ToUpper()) >= 0)
                    MatchCount++;
            }

            // AND検索の場合
            //if (Count <= MatchCount)
            //    return true;

            // OR検索の場合
            if (MatchCount >= 1)
            {
                mTarget.IsSelected = true;
                return true;
            }

            return false;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            dgridArrangementTarget.Items.Filter = new Predicate<object>(FilterContentsItemAndSearchFilter);

            List<TargetFiles> files = (List<TargetFiles>)dgridArrangementTarget.ItemsSource;

            Regex regex = new Regex(REGEX_MOVIEONLY_EXTENTION, RegexOptions.IgnoreCase);

            dgridArrangementTarget.SelectedItem = null;
            TargetFiles selFile = null;
            foreach(TargetFiles file in files)
            {
                if (file.IsSelected)
                {
                    if (regex.IsMatch(file.Name))
                    {
                        selFile = file;
                        break;
                    }
                }
            }
            if (selFile != null)
                dgridArrangementTarget.SelectedItem = selFile;

            dgridDestFile.Items.Filter = new Predicate<object>(FilterContentsItemAndSearchFilter);
            btnExecuteNameChange.Focus();

            // MovieFileContentsからの二重登録防止のための検索は以下の順番で行う
            // １）品番で完全一致（ハイフンを削除して大文字変換での一致）
            // ２）１）で無い場合は品番で文字列を含むかどうかで検索
            // ３）２）で無い場合はファイル名全体で検索
            string bartext = "";
            string searchword = txtSearch.Text.Replace("-", "");
            // 品番で完全一致（ハイフンを削除して大文字変換での一致）
            foreach (MovieFileContents data in listFilesContents)
            {
                if (data.ProductNumber.Length <= 0)
                    continue;

                string pnum = data.ProductNumber.Replace("-", "");

                if (searchword.ToUpper().Equals(pnum.ToUpper()))
                    bartext = bartext + data.Name;
            }

            // 品番で文字列を含むかどうかで検索
            if (bartext.Length <= 0)
            {
                foreach (MovieFileContents data in listFilesContents)
                {
                    //string pnum = data.ProductNumber.Replace("-", "");
                    if (data.ProductNumber.Length <= 0)
                        continue;

                    if (data.ProductNumber.Equals("SD") || data.ProductNumber.Equals("HD") || data.ProductNumber.Equals("DMM"))
                        continue;

                    if (txtSearch.Text.ToUpper().IndexOf(data.ProductNumber.ToUpper()) >= 0)
                        bartext = bartext + data.Name;
                }
            }

            // ファイル名全体で検索
            if (bartext.Length <= 0)
            {
                string[] arrSearchWord = txtSearch.Text.Split(' ');
                int Count = arrSearchWord.Length;
                foreach (MovieFileContents data in listFilesContents)
                {
                    int MatchCount = 0;
                    foreach (string word in arrSearchWord)
                    {
                        if (data.Name.ToUpper().IndexOf(word.ToUpper()) >= 0)
                            MatchCount++;
                    }

                    // OR検索の場合
                    if (MatchCount >= 1)
                        bartext = bartext + data.Name;
                }
            }

            txtStatusBar.Text = bartext;
        }

        private void btnSearchCancel_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            txtStatusBar.Text = "";
            dgridArrangementTarget.Items.Filter = null;
            dgridDestFile.Items.Filter = null;
        }

        private void menuitemRenameFile_Click(object sender, RoutedEventArgs e)
        {
            gridRenameFile.Visibility = System.Windows.Visibility.Visible;

            TargetFiles selStartFile = (TargetFiles)dgridArrangementTarget.SelectedItem;
            txtRenameSourceFile.Text = selStartFile.Name;

        }

        private void btnRenameFileCancel_Click(object sender, RoutedEventArgs e)
        {
            gridRenameFile.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnRenameFileExecute_Click(object sender, RoutedEventArgs e)
        {
            string SourcePathFile = System.IO.Path.Combine(txtBasePath.Text, txtRenameSourceFile.Text);
            string DestPathFile = System.IO.Path.Combine(txtBasePath.Text, txtRenameDestFilename.Text);
            File.Move(SourcePathFile, DestPathFile);

            gridRenameFile.Visibility = System.Windows.Visibility.Hidden;
        }

        private void menuitemDeleteFile_Click(object sender, RoutedEventArgs e)
        {
            TargetFiles file = (TargetFiles)dgridArrangementTarget.SelectedItem;
            FileSystem.DeleteFile(
                file.FileInfo.FullName,
                UIOption.OnlyErrorDialogs,
                RecycleOption.SendToRecycleBin);
        }

        private void dgridDestFile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dgrid = sender as DataGrid;
            TargetFiles file = (TargetFiles)dgrid.SelectedItem;

            Process.Start(file.FileInfo.FullName);
        }

        private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            txtStatusBar.Width = stsbaritemDispDetail.ActualWidth;
        }

        private void txtChangeFileName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            dgridSelectTargetFilename.ItemsSource = listTextTargetFileName;
            gridSelectTargetFilename.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            gridSelectTargetFilename.Visibility = System.Windows.Visibility.Hidden;

        }

        private void dgridSelectTargetFilename_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Regex regex = new Regex("^RAR");

            string selectText = dgridSelectTargetFilename.SelectedItem.ToString();
            if (regex.IsMatch(dgridSelectTargetFilename.SelectedItem.ToString()))
            {
                ChangeModeNormalRarExecute(null, null);
                selectText = regex.Replace(selectText, "^RAR.[ ]");
            }

            foreach (TargetFiles file in dgridDestFile.ItemsSource)
                file.IsSelected = false;

            regex = new Regex(".* \\[.* ");
            txtbSourceFilename.Text = dgridSelectTargetFilename.SelectedItem.ToString();
            txtChangeFileName.Text = selectText;

            string WorkStr = "";
            if (regex.IsMatch(txtChangeFileName.Text))
            {
                WorkStr = Regex.Replace(txtChangeFileName.Text, ".* \\[", "");
                string HyphenStr = Regex.Replace(WorkStr, " [0-9]*.*", "");
                string HyphenWithoutStr = HyphenStr.Replace("-", "");

                if (HyphenStr.Equals(HyphenWithoutStr))
                    txtSearch.Text = HyphenStr;
                else
                    txtSearch.Text = HyphenStr + " " + HyphenWithoutStr;

                btnSearch_Click(null, null);
            }

            gridSelectTargetFilename.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnBasePathPaste_Click(object sender, RoutedEventArgs e)
        {
            txtBasePath.Text = ClipBoardCommon.GetTextPath();
        }

        private void btnLabelPathPaste_Click(object sender, RoutedEventArgs e)
        {
            txtLabelPath.Text = ClipBoardCommon.GetTextPath();
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            settingControl.Save(txtBasePath.Text, txtLabelPath.Text, txtKoreanPornoPath.Text);
        }

        private void btnAllSelect_Click(object sender, RoutedEventArgs e)
        {
            List<TargetFiles> listTargetFiles = (List<TargetFiles>)dgridDestFile.ItemsSource;

            foreach (TargetFiles file in listTargetFiles)
                file.IsSelected = true;
        }

        private void btnPasteTitleText_Click(object sender, RoutedEventArgs e)
        {
            string titletext = ClipBoardCommon.GetText();
            txtTitleText.Text = titletext;

            txtStatusBar.Text = "";
            if (chkKindFixed.IsChecked == null || !(bool)chkKindFixed.IsChecked) txtKind.Text = "";
            txtMatchStr.Text = "";
            txtMaker.Text = "";
            txtTitle.Text = "";

            if (MaxListMakers > listMakers.Count())
            {
                listMakers = MovieMakers.GetAllData();
                MaxListMakers = listMakers.Count();
            }

            MovieFileContents moviefile = new MovieFileContents();
            moviefile.ParseFromJavSiteText(titletext);

            List<MovieMaker> listMatchMaker = MovieMakers.GetMatchData(moviefile.EditPasteText, listMakers, moviefile);

            if (listMatchMaker == null || listMatchMaker.Count() <= 0)
            {
                txtStatusBar.Text = "一致するメーカーが存在しませんでした";
                RefrectMakerInfo(moviefile, null, MovieFileContents.KIND_URAAVRIP);
                return;
            }

            if (listMatchMaker.Count() == 1)
            {
                RefrectMakerInfo(moviefile, listMatchMaker[0], MovieFileContents.KIND_URAAVRIP);
            }
            else
            {
                dgridMakers.ItemsSource = null;
                listMakers = listMatchMaker;

                dgridMakers.ItemsSource = listMakers;

                lgridMakers.Visibility = System.Windows.Visibility.Visible;

                // Autoの設定にする
                ScreenDisableBorder.Width = Double.NaN;
                ScreenDisableBorder.Height = Double.NaN;

                isSelectSameMaker = true;

                return;
            }

            ExecuteMatchFiles();
            txtFileGeneSearchText.Text = txtSearch.Text;
        }

        private void ExecuteMatchFiles()
        {
            List<TargetFiles> files = GetDestFiles(txtBasePath.Text);
            dgridCheckExistFiles.ItemsSource = files;

            if (txtProductNumber.Text != null && txtProductNumber.Text.Length > 0)
            {
                string bartext = "";
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

        private bool FilterTargetFilesSearchFilter(object item)
        {
            TargetFiles mTarget = item as TargetFiles;

            string[] arrSearchWord = txtSearch.Text.Split(' ');
            int Count = arrSearchWord.Length;

            // ファイル名のパスを含めて検索するため、基本フォルダを除いたパス付きファイル名を取得
            string filename = mTarget.FileInfo.FullName.Replace(txtBasePath.Text, "");

            int MatchCount = 0;
            foreach (string word in arrSearchWord)
            {
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

        private void dgridMakers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 文字列一致のメーカー複数の選択の場合
            if (isSelectSameMaker)
            {
                MovieFileContents moviefile = new MovieFileContents();
                moviefile.ParseFromJavSiteText(txtTitleText.Text);

                MovieMaker maker = (MovieMaker)dgridMakers.SelectedItem;
                RefrectMakerInfo(moviefile, maker, MovieFileContents.KIND_URAAVRIP);

                isSelectSameMaker = false;

                ExecuteMatchFiles();

                ButtonMakerClose(null, null);
            }
            isSelectSameMaker = false;
        }

        private void RefrectMakerInfo(MovieFileContents myMovieFile, MovieMaker myMaker, int myKind)
        {
            // クリップボードテキストから不要な削除文字列を設定する
            List<string> listCutText = new List<string>();

            if (myMaker != null && (myMaker.Kind == 1 || myMaker.Kind == 2))
                listCutText.Add(myMovieFile.MatchStrProductNumber);
            else
            {
                if (myMaker != null)
                    listCutText.Add(myMaker.MatchStr);
            }
            listCutText.Add(myMovieFile.Remark);
            listCutText.Add(myMovieFile.MatchStrSellDate);
            listCutText.Add(myMovieFile.MatchStrActresses);
            listCutText.Add(myMovieFile.Remark);

            if (myMaker != null && myMaker.MatchProductNumberValue != null && myMaker.MatchProductNumberValue.Length > 0)
            {
                listCutText.Add(myMaker.MatchProductNumberValue);
                txtProductNumber.Text = myMaker.MatchProductNumberValue;
            }
            else
            {
                txtProductNumber.Text = myMovieFile.ProductNumber;
            }

            if (myMaker != null)
            {
                if (myMovieFile.Kind == myKind)
                    listCutText.Add(myMaker.Name);
                else
                    listCutText.Add(myMaker.MatchStr);
            }

            if (myMovieFile.SellDate.Year >= 1900)
                txtFilenameGenDate.Text = myMovieFile.SellDate.ToString("yyyy/MM/dd");

            if (myMovieFile.Remark != null && myMovieFile.Remark.Length > 0)
            {
                if (chkActressFixed.IsChecked != null)
                {
                    bool b = (bool)chkActressFixed.IsChecked;

                    if (!b)
                        txtActresses.Text = myMovieFile.Remark;
                }
            }

            if (myMaker == null)
            {
                myMaker = MovieMakers.GetSearchByProductNumber(myMovieFile.ProductNumber);
                if (myMaker != null)
                    txtStatusBar.Text = "MOVIE_FILE_CONTENTSから検索して取得しました";
            }

            if (myMaker != null)
            {
                txtMaker.Text = myMaker.GetNameLabel();
                txtMatchStr.Text = myMaker.MatchStr;
                if (chkKindFixed.IsChecked == null || !(bool)chkKindFixed.IsChecked)
                    txtKind.Text = myMaker.Kind.ToString();
            }

            string edittext = myMovieFile.EditPasteText;
            foreach (string cuttext in listCutText)
            {
                if (cuttext == null || cuttext.Length <= 0)
                    continue;

                edittext = Regex.Replace(Regex.Escape(edittext), Regex.Escape(cuttext), "", RegexOptions.IgnoreCase);
                edittext = Regex.Unescape(edittext);
            }
            myMovieFile.MatchStrProductNumber = "";
            if (myMovieFile.MatchQuality != null && myMovieFile.MatchQuality.Length > 0)
                txtTitle.Text = edittext.Trim() + " " + myMovieFile.MatchQuality;
            else
                txtTitle.Text = edittext.Trim();

            GenerateFilename(null, null);
        }

        private void btnPasteDate_Click(object sender, RoutedEventArgs e)
        {
            string ClipboardText = ClipBoardCommon.GetText();

            txtFilenameGenDate.Text = ClipboardText;
            GenerateFilename(null, null);
        }

        private void txtMaker_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            lgridMakers.Visibility = System.Windows.Visibility.Visible;

            // Autoの設定にする
            ScreenDisableBorder.Width = Double.NaN;
            ScreenDisableBorder.Height = Double.NaN;

            dgridMakers.ItemsSource = listMakers;
        }

        private void ButtonMakerClose(object sender, RoutedEventArgs e)
        {
            isSelectSameMaker = false;

            if (lgridRegistMaker.Visibility == System.Windows.Visibility.Visible)
            {
                lgridRegistMaker.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }
            if (lgridMakers.Visibility == System.Windows.Visibility.Visible)
            {
                lgridMakers.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }
        }

        private void btnRegistMaker_Click(object sender, RoutedEventArgs e)
        {
            MovieMaker regdata = new MovieMaker();

            regdata.Name = txtRegistMakerName.Text;
            regdata.Label = txtRegistMakerLabel.Text;
            regdata.Kind = Convert.ToInt32(txtRegistMakerKind.Text);
            regdata.MatchStr = txtRegistMakerMatchStr.Text;
            regdata.MatchProductNumber = txtRegistMatchProductNumber.Text;

            if (txtRegistId.Text.Length > 0)
            {
                regdata.Id = Convert.ToInt32(txtRegistId.Text);
                regdata.DbUpdate(null);
            }
            else
                regdata.DbExport(null);

            lgridRegistMaker.Visibility = System.Windows.Visibility.Collapsed;

            dgridMakers.ItemsSource = null;
            listMakers = null;
            listMakers = MovieMakers.GetAllData();

            txtRegistId.Text = "";
            txtRegistMakerName.Text = "";
            txtRegistMakerLabel.Text = "";
            txtRegistMakerKind.Text = "";
            txtRegistMakerMatchStr.Text = "";
            txtRegistMatchProductNumber.Text = "";

            dgridMakers.ItemsSource = listMakers;

            return;
        }

        /// <summary>
        /// 追加・編集ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenRegistMaker_Click(object sender, RoutedEventArgs e)
        {
            if (lgridMakers.Visibility == System.Windows.Visibility.Visible)
            {
                var sel = dgridMakers.SelectedItems;

                if (sel.Count == 1)
                {
                    MovieMaker data = sel[0] as MovieMaker;
                    txtRegistId.Text = data.Id.ToString();
                    txtRegistMakerName.Text = data.Name;
                    txtRegistMakerKind.Text = data.Kind.ToString();
                    txtRegistMakerLabel.Text = data.Label;
                    txtRegistMakerMatchStr.Text = data.MatchStr;
                    txtRegistMatchProductNumber.Text = data.MatchProductNumber;
                }
            }
            else
            {
                txtRegistId.Text = "";
                if (txtMaker.Text.IndexOf("：") > 0)
                {
                    string[] splitStr = { "：" };
                    string[] makerandlabel = txtMaker.Text.Split(splitStr, StringSplitOptions.RemoveEmptyEntries);
                    txtRegistMakerName.Text = makerandlabel[0];
                    txtRegistMakerLabel.Text = makerandlabel[1];
                }
                else
                {
                    //txtRegistMakerLabel = "";
                    txtRegistMakerName.Text = txtMaker.Text;
                }
                txtRegistMakerMatchStr.Text = txtMatchStr.Text;
                txtRegistMakerKind.Text = txtKind.Text;
            }
            lgridRegistMaker.Visibility = System.Windows.Visibility.Visible;
        }

        private void GenerateFilename(object sender, RoutedEventArgs e)
        {
            DateTime dt;
            string strDt = "";
            try
            {
                dt = Convert.ToDateTime(txtFilenameGenDate.Text);
                strDt = dt.ToString("yyyyMMdd");
            }
            catch (Exception)
            {
                dt = new DateTime(1900, 1, 1);
            }

            string name = "";
            if (txtKind.Text.Equals("1"))
                name += "[AVRIP]";
            else if (txtKind.Text.Equals("2"))
                name += "[IVRIP]";
            else if (txtKind.Text.Equals("3"))
                name += "[裏AVRIP]";
            else if (txtKind.Text.Equals("4"))
                name += "[DMMR-AVRIP]";
            else if (txtKind.Text.Equals("5"))
                name += "[DMMR-AVRIP]";

            name += "【" + txtMaker.Text + "】";
            name += txtTitle.Text + " ";
            name += "[" + txtProductNumber.Text + " " + strDt + "]";
            if (txtActresses.Text.Trim().Length > 0)
                name += "（" + txtActresses.Text + "）";

            txtFilenameGenerate.Text = name;

        }

        private void btnOpenRegistMaker_Click(object sender, MouseButtonEventArgs e)
        {
            Debug.Print("TEST btnOpenRegistMaker_Click");
        }

        private void btnGenerateFilenameCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetData(DataFormats.Text, txtFilenameGenerate.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("クリップボードの取得に失敗しました");
            }
        }

        private void btnPasteActresses_Click(object sender, RoutedEventArgs e)
        {
            string ClipboardText = ClipBoardCommon.GetText();

            MovieFileContents moviefile = new MovieFileContents();
            moviefile.ParseSetActress(ClipboardText);

            txtActresses.Text = moviefile.Remark;

            GenerateFilename(null, null);
        }

        private void dgridMakers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var colSel = dgridMakers.SelectedItems;

            if (dispinfoSelectDataGridMakers == null)
                dispinfoSelectDataGridMakers = new List<MovieMaker>();
            else
                dispinfoSelectDataGridMakers.Clear();

            foreach (MovieMaker data in colSel)
                dispinfoSelectDataGridMakers.Add(data);
        }

        private void dgridMakers_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                OnSelectionRowDelete(null, null);
            }
        }

        private void OnSelectionRowDelete(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("選択行を削除して宜しいですか？", "削除確認", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.Cancel)
                return;

            DbConnection dbcon = new DbConnection();

            try
            {
                dbcon.BeginTransaction("DELETE_ARTIST");
                foreach (MovieMaker data in dispinfoSelectDataGridMakers)
                {
                    data.DbDelete(dbcon);
                    Debug.Print("ID [" + data.Id + "]  Name [" + data.Name + "]");
                }
                dbcon.CommitTransaction();
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                MessageBox.Show(ex.Message, "エラー発生");
            }
        }

        private void dgridKoreanPorno_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dispinfoSelectDataGridKoreanPorno.Count() == 1)
            {
                KoreanPornoData selData = dispinfoSelectDataGridKoreanPorno[0];
                txtbKoreanPornoName.Text = selData.Name;
                txtbKoreanPornoArchiveFile.Text = selData.ArchiveFile;

                List<KoreanPornoFileInfo> listFiles = KoreanPorno.GetFileInfo(dispinfoKoreanPornoStorePath, selData.Name, selData.LastWriteTime, dispinfoSelectDataGridKoreanPorno[0].ArchiveFile);

                if (listFiles == null)
                {
                    MessageBox.Show("まだ解凍されていません");
                    return;
                }
                else
                    dgridKoreanPornoFolder.ItemsSource = listFiles;
            }

            dgridKoreanPorno.Visibility = System.Windows.Visibility.Hidden;
            btnKoreanPornoExecute.Focus();
        }

        private void dgridKoreanPorno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dispinfoSelectDataGridKoreanPorno = new List<KoreanPornoData>();
            foreach (KoreanPornoData data in dgridKoreanPorno.SelectedItems)
            {
                dispinfoSelectDataGridKoreanPorno.Add(data);
            }
        }

        private void btnKoreanPornoExecute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                KoreanPorno.ExecuteArrangement(dispinfoKoreanPornoStorePath, dispinfoSelectDataGridKoreanPorno[0], (List<KoreanPornoFileInfo>)dgridKoreanPornoFolder.ItemsSource);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            dgridKoreanPorno.ItemsSource = KoreanPorno.GetFolderData(dispinfoKoreanPornoStorePath);

            dgridKoreanPorno.Visibility = System.Windows.Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dgridKoreanPorno.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnKoreanPornoArrangePasteTitleText_Click(object sender, RoutedEventArgs e)
        {
            txtKoreanPornoPath.Text = ClipBoardCommon.GetTextPath();
        }

        private void OnMakersFilter(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                ColViewListMakers.Filter = null;
                chkKindOne.IsChecked = false;
                chkKindTwo.IsChecked = false;
                chkKindThree.IsChecked = false;
                return;
            }

            bool one = false, two = false, three = false;
            ColViewListMakers = CollectionViewSource.GetDefaultView(listMakers);
            if (chkKindOne.IsChecked != null)
                one = (bool)chkKindOne.IsChecked;
            if (chkKindTwo.IsChecked != null)
                two = (bool)chkKindTwo.IsChecked;
            if (chkKindThree.IsChecked != null)
                three = (bool)chkKindThree.IsChecked;

            string search = txtMakersSearch.Text;
            try
            {
                Regex regex = new Regex(search);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            ColViewListMakers.Filter = delegate (object o)
            {
                MovieMaker data = o as MovieMaker;

                //if (search.Length > 0)

                int kind = data.Kind;
                if (one && kind == 1)
                    return true;
                if (two && kind == 2)
                    return true;
                if (three && kind == 3)
                    return true;

                if (!one && !two && !three)
                    return true;
                return false;
            };
        }

        private void btnClearActress_Click(object sender, RoutedEventArgs e)
        {
            txtActresses.Text = "";
        }

        private void btnFileGenDeleteSelectFile_Click(object sender, RoutedEventArgs e)
        {
            if (dgridCheckExistFiles.SelectedItems == null || dgridCheckExistFiles.SelectedItems.Count <= 0)
                return;

            MessageBoxResult result = MessageBox.Show("削除しますか？", "削除確認", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.Cancel)
                return;

            foreach(TargetFiles file in dgridCheckExistFiles.SelectedItems)
            {
                FileSystem.DeleteFile(
                    file.FileInfo.FullName,
                    UIOption.OnlyErrorDialogs,
                    RecycleOption.SendToRecycleBin);
            }

            ExecuteMatchFiles();

            btnFileGenSearch_Click(null, null);
        }

        private void btnFileGenSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = txtFileGeneSearchText.Text;

            dgridCheckExistFiles.Items.Filter = new Predicate<object>(FilterTargetFilesSearchFilter);
        }
    }
}
