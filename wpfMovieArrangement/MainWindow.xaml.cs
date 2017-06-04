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
using System.Windows.Controls.Primitives;
using wpfMovieArrangement.service;

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

        private List<MovieMaker> listMakers = null;
        ICollectionView ColViewListMakers;
        private int MaxListMakers = 0;
        private List<MovieFileContents> listFilesContents = null;
        private List<MovieImportData> listImportTarget = null;

        private collection.FileGeneTargetFilesCollection ColViewFileGeneTargetFiles;
        private collection.FileGeneTargetFilesCollection ColViewArrangementTarget; // dgridArrangementTarget
        private collection.FileGeneTargetFilesCollection ColViewDestFiles; // dgridDestFile

        private List<MovieMaker> dispinfoSelectDataGridMakers = null;
        private List<KoreanPornoData> dispinfoSelectDataGridKoreanPorno = null;
        private string dispinfoKoreanPornoStorePath = null;
        private MovieImportData dispinfoSelectMovieImportData= null;
        // 日付コピー時には各DataGridがColViewではなくなるので、戻すためのフラグ
        private bool dispinfoIsDateCopyPasteExecute = false;

        SettingXmlControl settingControl = null;
        Setting setting = null;
        ViewModel ViewData;
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

            private string _FilenameGenDate;

            public string FilenameGenDate
            {
                get { return this._FilenameGenDate; }
                set
                {
                    this._FilenameGenDate = value;
                    var handler = this.PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("FilenameGenDate"));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public MainWindow()
        {
            InitializeComponent();

            ViewData = new ViewModel { BasePath = "", FilenameGenDate = "" };
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
            txtFilenameGenDate.Text = "";

            ChangeModeNormalMovieExecute(null, null);

            OnGridTargetDisplay(null, null);

            ColViewFileGeneTargetFiles = new collection.FileGeneTargetFilesCollection(txtBasePath.Text);
            ColViewArrangementTarget = new collection.FileGeneTargetFilesCollection(txtBasePath.Text);
            ColViewDestFiles = new collection.FileGeneTargetFilesCollection(txtBasePath.Text);

            dgridCheckExistFiles.ItemsSource = ColViewFileGeneTargetFiles.ColViewListTargetFiles;
            dgridArrangementTarget.ItemsSource = ColViewArrangementTarget.ColViewListTargetFiles;
            dgridDestFile.ItemsSource = ColViewDestFiles.ColViewListTargetFiles;

            txtStatusBar.Width = statusbarMain.ActualWidth;
            txtStatusBar.Background = statusbarMain.Background;

            dgridSelectTargetFilename.Width = statusbarMain.ActualWidth;
            service.MovieImportService service = new service.MovieImportService();
            listImportTarget = service.GetList(new DbConnection());

            MovieFileContentsParent parent = new MovieFileContentsParent();
            listFilesContents = parent.GetDbContents();

            DbConnection localDbCon = new DbConnection();
            txtbDbNowDate.Text = localDbCon.getDateStringSql("SELECT GETDATE()");

            dispinfoKoreanPornoStorePath = txtKoreanPornoPath.Text;
            dgridKoreanPorno.ItemsSource = KoreanPorno.GetFolderData(dispinfoKoreanPornoStorePath);
        }

        /// <summary>
        /// 日付コピーを実行するとDataGridがColViewではなくなるので、日付コピーが終了した後は
        /// DataGridへの紐付けをColViewへ戻す
        /// </summary>
        private void OnModeChangeDataGrid()
        {
            if (dispinfoIsDateCopyPasteExecute)
            {
                dgridArrangementTarget.ItemsSource = ColViewArrangementTarget.ColViewListTargetFiles;
                dgridDestFile.ItemsSource = ColViewDestFiles.ColViewListTargetFiles;

                dispinfoIsDateCopyPasteExecute = false;
            }
        }

        public void ChangeModeNormalRarExecute(object sender, RoutedEventArgs e)
        {
            dispctrlMode = MODE_NORMALRAR;

            OnModeChangeDataGrid();

            ChangeModeNormal(null, null);
        }

        public void ChangeModeNormalMovieExecute(object sender, RoutedEventArgs e)
        {
            dispctrlMode = MODE_NORMALMOVIE;

            OnModeChangeDataGrid();

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

            ColViewArrangementTarget.Clear();
            ColViewDestFiles.Clear();
            //dgridDestFile.ItemsSource = null;
        }

        public void CahngeModeFilenameGenerateExecute(object sender, RoutedEventArgs e)
        {
            dispctrlMode = MODE_FILENAMEGENERATE;

            OnModeChangeDataGrid();

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

            OnModeChangeDataGrid();

            lgridMain.Visibility = System.Windows.Visibility.Collapsed;
            wpanelNormal.Visibility = System.Windows.Visibility.Collapsed;
            lgridNormalChangeFilename.Visibility = System.Windows.Visibility.Collapsed;

            lgridDateCopySource.Visibility = System.Windows.Visibility.Collapsed;
            lgridDateCopyDestination.Visibility = System.Windows.Visibility.Collapsed;

            lgridFilenameGenerate.Visibility = System.Windows.Visibility.Collapsed;

            lgridKoreanPornoArrange.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnDateCopyPasteSource_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.IDataObject data = Clipboard.GetDataObject();

            List<TargetFiles> files = GetClipbardFiles(data);

            if (files.Count > 0)
                dgridArrangementTarget.ItemsSource = files;

            dispinfoIsDateCopyPasteExecute = true;

            return;
        }

        private void btnPasteDestination_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.IDataObject data = Clipboard.GetDataObject();

            List<TargetFiles> files = GetClipbardFiles(data);

            if (files.Count > 0)
                dgridDestFile.ItemsSource = files;

            dispinfoIsDateCopyPasteExecute = true;

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
                DateCopyService serviceDataCopy = new DateCopyService();

                string message = "";
                try
                {
                    serviceDataCopy.SetSourceFile(dgridArrangementTarget);
                    serviceDataCopy.SetDestFile(dgridDestFile);

                    message = serviceDataCopy.Execute();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                txtStatusBar.Text = message;

                return;
            }
            // 入力チェック
            string filename = txtChangeFileName.Text;

            if (filename.Length <= 0)
            {
                MessageBox.Show("ファイル名に入力がありません");
                return;
            }

            FilesRegisterService service = new FilesRegisterService(new DbConnection());
            FileControl fileControl = new FileControl();

            if (!txtChangeFileName.Text.Equals(dispinfoSelectMovieImportData.Filename))
            {
                MessageBoxResult result = MessageBox.Show("ファイル名が変更されていますが宜しいですか？", "変更確認", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.Cancel)
                    return;
            }
            dispinfoSelectMovieImportData.Filename = txtChangeFileName.Text;

            try
            {
                DateCopyService.CheckDataGridSelectItem(dgridArrangementTarget, "上のファイル", 1);

                service.BasePath = txtBasePath.Text;
                service.DestFilename = txtChangeFileName.Text;
                service.LabelPath = txtLabelPath.Text;

                service.SetSourceFile(dgridArrangementTarget);

                // 選択したファイルのみを対象に内部プロパティへ設定
                service.SetSelectedOnlyFiles(ColViewDestFiles.listTargetFiles);

                // JPEGの変換用の情報を生成する（日付コピー等はまだ実行されない）
                service.SetJpegActionInfo();

                // 動画の変換用の情報を生成する（日付コピー等はまだ実行されない）
                service.SetMovieActionInfo();

                // データベースへ登録用の情報を生成する
                service.SetDbMovieFilesInfo(dispinfoSelectMovieImportData);

                service.DbExport();

                // 動画、画像ファイルの移動、日付コピー等の実行
                service.Execute();
            }
            catch (Exception exp)
            {
                Debug.Write(exp);
                MessageBox.Show(exp.Message);
                return;
            }
            finally
            {
                // 選択中のファイル一覧はクリアする（次の対象動画になってしまうので）
                foreach (TargetFiles file in dgridDestFile.ItemsSource)
                    file.IsSelected = false;

                // フィルターをクリアしないと再取得した直後に動作して不要なチェックが付いてしまう
                dgridDestFile.Items.Filter = null;
            }

            try
            {
                service.DeleteFiles();
            }
            catch(Exception ex)
            {
                MessageBox.Show("削除失敗 " + ex.Message);
            }

            txtChangeFileName.Text = "";
            txtChangeTag.Text = "";
            txtSearch.Text = "";
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

            string searchStr = (dispinfoSelectMovieImportData != null) ? dispinfoSelectMovieImportData.GetFileSearchString() : "";
            if (dispctrlMode == MODE_NORMALRAR)
            {
                menuitemRarFiles.IsChecked = true;
                menuitemMovieFiles.IsChecked = false;

                if (ColViewArrangementTarget != null)
                {
                    ColViewArrangementTarget.FilterSearchProductNumber = searchStr;
                    ColViewArrangementTarget.RefreshRarFile();
                }
            }
            else
            {
                menuitemRarFiles.IsChecked = false;
                menuitemMovieFiles.IsChecked = true;

                if (!CanGetDirectoryInfo())
                    return;

                if (ColViewArrangementTarget != null)
                {
                    ColViewArrangementTarget.FilterSearchProductNumber = searchStr;
                    ColViewArrangementTarget.Refresh(collection.FileGeneTargetFilesCollection.REGEX_MOVIE_EXTENTION);
                }
            }
            if (ColViewArrangementTarget != null)
            {
                ColViewDestFiles.FilterSearchProductNumber = searchStr;
                ColViewDestFiles.Refresh(collection.FileGeneTargetFilesCollection.REGEX_MOVIE_EXTENTION);
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

            ColViewDestFiles.Refresh();
            //dgridDestFile.ItemsSource = ColViewFileGeneTargetFiles.GetDestFiles(txtBasePath.Text, REGEX_MOVIE_EXTENTION);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ColViewArrangementTarget.FilterSearchProductNumber = txtSearch.Text;
            ColViewArrangementTarget.Refresh();
            ColViewDestFiles.FilterSearchProductNumber = dispinfoSelectMovieImportData.GetFileSearchString();
            ColViewDestFiles.Refresh();

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
                        if (word.Length <= 0)
                            continue;

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
            dgridSelectTargetFilename.ItemsSource = listImportTarget;
            if (dispctrlMode == MODE_FILENAMEGENERATE)
                lgridFilenameGenerate.Visibility = System.Windows.Visibility.Collapsed;
            else
                lgridMain.Visibility = System.Windows.Visibility.Collapsed;

            gridSelectTargetFilename.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            if (dispctrlMode == MODE_FILENAMEGENERATE)
                lgridFilenameGenerate.Visibility = System.Windows.Visibility.Visible;
            else
                lgridMain.Visibility = System.Windows.Visibility.Visible;

            dispinfoSelectMovieImportData = null;

            gridSelectTargetFilename.Visibility = System.Windows.Visibility.Hidden;
        }

        private void dgridSelectTargetFilename_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            dispinfoSelectMovieImportData = (MovieImportData)dgridSelectTargetFilename.SelectedItem;

            if (dispctrlMode == MODE_FILENAMEGENERATE)
            {
                txtFileGeneSearchText.Text = dispinfoSelectMovieImportData.GetFilterProductNumber();

                ColViewFileGeneTargetFiles.FilterSearchProductNumber = txtFileGeneSearchText.Text;
                ColViewFileGeneTargetFiles.Refresh();

                SetUIElementFromImportData(dispinfoSelectMovieImportData);

                lgridFilenameGenerate.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                txtSearch.Text = dispinfoSelectMovieImportData.GetFileSearchString();

                txtbSourceFilename.Text = dispinfoSelectMovieImportData.Filename;
                txtChangeFileName.Text = dispinfoSelectMovieImportData.Filename;

                if ((bool)dispinfoSelectMovieImportData.RarFlag)
                {
                    ChangeModeNormalRarExecute(null, null);

                    foreach (TargetFiles file in dgridDestFile.ItemsSource)
                        file.IsSelected = false;
                }
                else
                {
                    ChangeModeNormalMovieExecute(null, null);
                }

                lgridMain.Visibility = System.Windows.Visibility.Visible;

                dgridArrangementTarget.SelectedItem = ColViewArrangementTarget.GetSelectTargetMovieFile();

                List<TargetFiles> filesList = ColViewArrangementTarget.GetSelectTargetFiles();
                foreach (TargetFiles file in filesList)
                {
                    foreach (TargetFiles item in dgridDestFile.ItemsSource)
                    {
                        if (file.Name.Equals(item.Name))
                            item.IsSelected = true;
                    }
                }

                btnExecuteNameChange.Focus();
            }
            txtbTargetImportId.Text = Convert.ToString(dispinfoSelectMovieImportData.Id);

            gridSelectTargetFilename.Visibility = Visibility.Hidden;
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

        private void btnPasteTitleText_Click(object sender, RoutedEventArgs e)
        {
            string titletext = ClipBoardCommon.GetText();

            if (titletext.Trim().Length <= 10)
            {
                try
                {
                    DateTime dt = Convert.ToDateTime(titletext);
                    txtFilenameGenDate.Text = dt.ToString("yyyy/MM/dd");
                }
                catch(Exception)
                {

                }
                return;
            }
            ClearUIElement();
            txtTitleText.Text = titletext;

            MovieImportData importData;
            if (titletext.IndexOf("RAR") == 0
                || titletext.IndexOf("[AV") == 0
                || titletext.IndexOf("[IV") == 0
                || titletext.IndexOf("[裏") == 0)
            {
                importData = new MovieImportData(titletext);

                txtKind.Text = Convert.ToString(importData.Kind);
                txtFilenameGenDate.Text = importData.ProductDate.ToString("yyyy/MM/dd");
                txtProductNumber.Text = importData.ProductNumber;
                txtMaker.Text = importData.Maker;
                txtTitle.Text = importData.Title;

                if (importData.RarFlag == true)
                    tbtnFileGeneTextAddRar.IsChecked = true;

                ColViewFileGeneTargetFiles.FilterSearchProductNumber = dispinfoSelectMovieImportData.GetFilterProductNumber();
                ColViewFileGeneTargetFiles.Refresh();

                return;
            }

            txtStatusBar.Text = "";

            if (MaxListMakers > listMakers.Count())
            {
                listMakers = MovieMakers.GetAllData();
                MaxListMakers = listMakers.Count();
            }

            MovieFileContents moviefile = new MovieFileContents();
            moviefile.ParseFromJavSiteText(titletext);

            List<MovieMaker> listMatchMaker;

            try
            {
                listMatchMaker = MovieMakers.GetMatchData(moviefile.EditPasteText, listMakers, moviefile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

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

            MovieImportData data = GetImportDataFromUIElement();
            ColViewFileGeneTargetFiles.FilterSearchProductNumber = data.GetFilterProductNumber();
            txtFileGeneSearchText.Text = ColViewFileGeneTargetFiles.FilterSearchProductNumber;

            ColViewFileGeneTargetFiles.Refresh();
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

                ColViewFileGeneTargetFiles.FilterSearchProductNumber = (dispinfoSelectMovieImportData != null) ? dispinfoSelectMovieImportData.GetFilterProductNumber() : txtProductNumber.Text;
                ColViewFileGeneTargetFiles.Refresh();

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

            if (chkActressFixed.IsChecked != null)
            {
                bool b = (bool)chkActressFixed.IsChecked;

                if (!b)
                    txtActresses.Text = myMovieFile.Remark;
            }
            else
                txtActresses.Text = myMovieFile.Remark;

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

                //edittext = edittext.Replace(cuttext, "");
                //Debug.Print("edittext 【" + Regex.Escape(edittext) + "】 cuttext【" + Regex.Escape(cuttext) + "】");
                edittext = Regex.Replace(Regex.Escape(edittext), Regex.Escape(cuttext), "", RegexOptions.IgnoreCase);
                if (edittext.Substring(0,1).Equals("\\"))
                    edittext = edittext.Replace("\\", "");
                //Debug.Print("【" + edittext + "】");
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

        private MovieImportData GetImportDataFromUIElement()
        {
            MovieImportData movieImportData = new MovieImportData();

            movieImportData.CopyText = txtTitleText.Text;
            if (txtKind.Text.Length > 0)
                movieImportData.Kind = Convert.ToInt32(txtKind.Text);
            movieImportData.MatchProduct = txtMatchStr.Text;
            movieImportData.ProductNumber = txtProductNumber.Text;
            movieImportData.StrProductDate = txtFilenameGenDate.Text;
            movieImportData.Maker = txtMaker.Text;
            movieImportData.Title = txtTitle.Text;
            movieImportData.Actresses = txtActresses.Text;
            movieImportData.RarFlag = tbtnFileGeneTextAddRar.IsChecked;
            movieImportData.Tag = txtTag.Text;
            movieImportData.GenerateFilename();
            movieImportData.Filename = txtFilenameGenerate.Text;

            movieImportData.GenerateFilename();

            return movieImportData;
        }

        private void SetUIElementFromImportData(MovieImportData myData)
        {
            txtbImportId.Text = Convert.ToString(myData.Id);
            txtTitleText.Text = myData.CopyText;
            txtKind.Text = myData.Kind.ToString();
            txtMatchStr.Text = myData.MatchProduct;
            txtFilenameGenDate.Text = myData.ProductDate.ToString("yyyy/MM/dd");
            txtProductNumber.Text = myData.ProductNumber;
            txtMaker.Text = myData.Maker;
            txtTitle.Text = myData.Title;
            txtActresses.Text = myData.Actresses;
            txtTag.Text = myData.Tag;
            tbtnFileGeneTextAddRar.IsChecked = myData.RarFlag;
            txtFilenameGenerate.Text = myData.Filename;
        }

        private void ClearUIElement()
        {
            txtbImportId.Text = "";
            txtTitleText.Text = "";
            if (chkKindFixed.IsChecked == null || !(bool)chkKindFixed.IsChecked) txtKind.Text = "";
            txtMatchStr.Text = "";
            //txtFilenameGenDate.Text = "";
            txtProductNumber.Text = "";
            txtMaker.Text = "";
            txtTitle.Text = "";
            txtActresses.Text = "";
            txtTag.Text = "";
            tbtnFileGeneTextAddRar.IsChecked = false;
            txtFilenameGenerate.Text = "";
        }

        private void GenerateFilename(object sender, RoutedEventArgs e)
        {
            if (Validation.GetHasError(txtFilenameGenDate))
                return;

            MovieImportData movieImportData = GetImportDataFromUIElement();

            txtFilenameGenerate.Text = movieImportData.Filename;

            return;

        }

        private void btnOpenRegistMaker_Click(object sender, MouseButtonEventArgs e)
        {
            Debug.Print("TEST btnOpenRegistMaker_Click");
        }

        private void btnGenerateFilenameCopy_Click(object sender, RoutedEventArgs e)
        {
            if (Validation.GetHasError(txtFilenameGenDate))
                return;

            string importId = txtbImportId.Text;

            service.MovieImportService service = new service.MovieImportService();
            if (importId.Length > 0)
            {
                MovieImportData movieImportData = GetImportDataFromUIElement();
                movieImportData.Id = Convert.ToInt32(importId);

                service.DbUpdate(movieImportData, new DbConnection());

                //ClearUIElement();

                //listImportTarget.Add(movieImportData);
            }
            else
            {
                MovieImportData movieImportData = GetImportDataFromUIElement();

                movieImportData = service.DbExport(movieImportData, new DbConnection());

                ClearUIElement();

                listImportTarget.Add(movieImportData);
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
                OnSelectionRowDelete(null, null);
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
                Regex regex = new Regex(search, RegexOptions.IgnoreCase);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                return;
            }

            string[] arrSearchAnd, arrSearchOr = null;
            bool IsSearchAnd = false, IsSearchOr = false;
            arrSearchAnd = search.Split(' ');

            if (arrSearchAnd.Length >= 2)
            {
                int cnt = 0;
                foreach (string str in arrSearchAnd)
                {
                    if (str != null && str.Length > 0)
                        cnt++;
                }
                if (cnt > 1)
                    IsSearchAnd = true;
            }
            else
            {
                arrSearchOr = search.Split(',');

                if (arrSearchOr.Length >= 2)
                {
                    int cnt = 0;
                    foreach (string str in arrSearchOr)
                    {
                        if (str != null && str.Length > 0)
                            cnt++;
                    }
                    if (cnt > 1)
                        IsSearchOr = true;
                }

            }

            int matchCnt = 0;
            ColViewListMakers.Filter = delegate (object o)
            {
                MovieMaker data = o as MovieMaker;

                if (search.Length > 0)
                {
                    if (IsSearchAnd)
                    {
                        matchCnt = 0;
                        foreach (string str in arrSearchAnd)
                        {
                            if (data.Name.ToUpper().IndexOf(str) >= 0
                                || data.Label.ToUpper().IndexOf(str) >= 0)
                                matchCnt++;
                        }

                        if (arrSearchAnd.Length == matchCnt)
                            return true;

                        return false;
                    }
                    else if (IsSearchOr)
                    {
                        foreach (string str in arrSearchOr)
                        {
                            if (data.Name.ToUpper().IndexOf(str) >= 0
                            || data.Label.ToUpper().IndexOf(str) >= 0)
                                return true;
                        }

                        return false;
                    }
                    else
                    {
                        if (data.Name.ToUpper().IndexOf(search.ToUpper()) >= 0
                            || data.Label.ToUpper().IndexOf(search.ToUpper()) >= 0)
                            return true;

                        return false;
                    }

                    return false;
                }

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

            btnFileGenSearch_Click(null, null);
        }

        private void btnFileGenSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = txtFileGeneSearchText.Text;

            ColViewFileGeneTargetFiles.FilterSearchProductNumber = txtSearch.Text;
            ColViewFileGeneTargetFiles.Refresh();
        }
    }
}
