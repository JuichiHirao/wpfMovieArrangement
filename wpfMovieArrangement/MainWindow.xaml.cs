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
using wpfMovieArrangement.collection;

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

        private MovieFileContentsCollection ColViewMovieFileContents;

        private MovieImportCollection ColViewMovieImport;
        private FileGeneTargetFilesCollection ColViewFileGeneTargetFiles;
        private FileGeneTargetFilesCollection ColViewArrangementTarget; // dgridArrangementTarget
        private FileGeneTargetFilesCollection ColViewDestFiles; // dgridDestFile
        private MakerCollection ColViewMaker;
        private KoreanPornoCollection ColViewKoreanPorno;

        private List<MovieMaker> dispinfoSelectDataGridMakers = null;
        private KoreanPornoData dispinfoSelectDataGridKoreanPorno = null;
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

            private string _koreanPornoExportPath;

            public string KoreanPornoExportPath
            {
                get { return this._koreanPornoExportPath; }
                set
                {
                    this._koreanPornoExportPath = value;
                    var handler = this.PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("KoreanPornoExportPath"));
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

            private string _ChangeFileName;

            public string ChangeFileName
            {
                get { return this._ChangeFileName; }
                set
                {
                    this._ChangeFileName = value;
                    var handler = this.PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ChangeFileName"));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public MainWindow()
        {
            InitializeComponent();

            ViewData = new ViewModel { BasePath = "", FilenameGenDate = "", KoreanPornoExportPath = "", ChangeFileName = "" };
            this.DataContext = ViewData;

            CommandBindings.Add(new CommandBinding(ChangeModeNormalRar, (s, ea) => { ChangeModeNormalRarExecute(s, ea); }, (s, ea) => ea.CanExecute = true));
            CommandBindings.Add(new CommandBinding(ChangeModeNormalMovie, (s, ea) => { ChangeModeNormalMovieExecute(s, ea); }, (s, ea) => ea.CanExecute = true));
            CommandBindings.Add(new CommandBinding(ChangeModeDateCopy, (s, ea) => { ChangeModeDateCopyExecute(s, ea); }, (s, ea) => ea.CanExecute = true));
            CommandBindings.Add(new CommandBinding(CahngeModeFilenameGenerate, (s, ea) => { CahngeModeFilenameGenerateExecute(s, ea); }, (s, ea) => ea.CanExecute = true));
            CommandBindings.Add(new CommandBinding(CahngeModeKoreanPorno, (s, ea) => { CahngeModeKoreanPornoExecute(s, ea); }, (s, ea) => ea.CanExecute = true));
            CommandBindings.Add(new CommandBinding(PasteDateCopy, (s, ea) => { PasteDateCopyExecute(s, ea); }, (s, ea) => ea.CanExecute = true));

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
            txtKoreanPornoExportPath.Text = setting.KoreanPornoExportPath;
            txtFilenameGenDate.Text = "";

            ChangeModeNormalMovieExecute(null, null);

            OnGridTargetDisplay(null, null);

            ColViewFileGeneTargetFiles = new collection.FileGeneTargetFilesCollection(txtBasePath.Text);
            ColViewArrangementTarget = new collection.FileGeneTargetFilesCollection(txtBasePath.Text);
            ColViewDestFiles = new collection.FileGeneTargetFilesCollection(txtBasePath.Text);
            ColViewKoreanPorno = new collection.KoreanPornoCollection(txtKoreanPornoPath.Text, txtKoreanPornoExportPath.Text);
            ColViewMovieImport = new MovieImportCollection();

            ColViewMovieFileContents = new collection.MovieFileContentsCollection();

            ColViewMaker = new collection.MakerCollection();

            dgridCheckExistFiles.ItemsSource = ColViewFileGeneTargetFiles.ColViewListTargetFiles;
            dgridArrangementTarget.ItemsSource = ColViewArrangementTarget.ColViewListTargetFiles;
            dgridDestFile.ItemsSource = ColViewDestFiles.ColViewListTargetFiles;
            dgridKoreanPorno.ItemsSource = ColViewKoreanPorno.ColViewListData;
            dgridSelectTargetFilename.ItemsSource = ColViewMovieImport.collection;

            txtStatusBar.Width = statusbarMain.ActualWidth;
            txtStatusBar.Background = statusbarMain.Background;

            dgridSelectTargetFilename.Width = statusbarMain.ActualWidth;
            ColViewMovieImport.Refresh();

            DbConnection localDbCon = new DbConnection();
            txtbDbNowDate.Text = localDbCon.getDateStringSql("SELECT GETDATE()");
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

            bool IsClickDelete = false;
            if (cell.Column.Header.Equals("削除"))
                IsClickDelete = true;

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
                        {
                            if (IsClickDelete)
                            {
                                file.IsDeleted = true;
                                file.IsSelected = false;
                            }
                            else
                                file.IsSelected = true;
                        }

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
                            if (IsClickDelete)
                            {
                                if (selFile.IsDeleted)
                                    selFile.IsDeleted = false;
                                else
                                {
                                    selFile.IsDeleted = true;
                                    selFile.IsSelected = false;
                                }
                            }
                            else
                            {
                                if (selFile.IsSelected)
                                    selFile.IsSelected = false;
                                else
                                    selFile.IsSelected = true;
                            }
                        }
                        else
                        {
                            if (IsClickDelete)
                            {
                                if (row.IsSelected && selFile.IsDeleted)
                                    row.IsSelected = false;

                                selFile.IsDeleted = false;
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

        private void btnExecuteDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("MOVIE_IMPORT_DATAから削除して、削除選択されたファイルをゴミ箱へ移していいですか？", "削除確認", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.Cancel)
                return;

            FilesRegisterService service = new FilesRegisterService(new DbConnection());

            service.targetImportData = dispinfoSelectMovieImportData;
            service.DeleteExecute(ColViewDestFiles.listTargetFiles);

            ColViewMovieImport.Refresh();

            txtChangeFileName.Text = "";
            txtbTag.Text = "";
            txtSearch.Text = "";
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

            if (!txtChangeFileName.Text.Equals(dispinfoSelectMovieImportData.Filename))
            {
                MessageBoxResult result = MessageBox.Show("ファイル名が変更されていますが宜しいですか？", "変更確認", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.Cancel)
                    return;
            }
            dispinfoSelectMovieImportData.Filename = txtChangeFileName.Text;

            FilesRegisterService service = new FilesRegisterService(new DbConnection());
            FileControl fileControl = new FileControl();

            try
            {
                DateCopyService.CheckDataGridSelectItem(dgridArrangementTarget, "上のファイル", 1);

                service.BasePath = txtBasePath.Text;
                service.DestFilename = txtChangeFileName.Text;
                service.LabelPath = txtLabelPath.Text;

                service.SetSourceFile(dgridArrangementTarget);

                // 選択したファイルのみを対象に内部プロパティへ設定
                service.SetSelectedOnlyFiles(ColViewDestFiles.listTargetFiles);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                MessageBox.Show(ex.Message, "初期設定エラー");
                return;
            }

            if (dispinfoSelectMovieImportData.FileId > 0)
            {
                try
                {
                    service.targetImportData = dispinfoSelectMovieImportData;
                    // HD動画への変更の場合
                    HdUpdateService hdUpdateService = new HdUpdateService(new DbConnection());

                    hdUpdateService.BasePath = txtBasePath.Text;
                    hdUpdateService.SetSelectedOnlyFiles(ColViewDestFiles.listTargetFiles);

                    MovieFileContents contents = ColViewMovieFileContents.MatchId(dispinfoSelectMovieImportData.FileId);

                    if (contents == null)
                    {
                        MessageBox.Show("対象のデータが存在しません " + dispinfoSelectMovieImportData.FileId);
                        return;
                    }
                    hdUpdateService.Execute(dispinfoSelectMovieImportData, contents);

                    txtStatusBar.Text = "";
                }
                catch (Exception ex)
                {
                    Debug.Write(ex);
                    MessageBox.Show(ex.Message, "HD動画変更エラー");
                    return;
                }
            }
            else
            {
                // 動画情報などの登録の場合
                try
                {
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
            }

            try
            {
                service.DeleteFiles(ColViewDestFiles.listTargetFiles);
            }
            catch (Exception ex)
            {
                MessageBox.Show("削除失敗 " + ex.Message);
            }

            ColViewMovieImport.Refresh();

            txtChangeFileName.Text = "";
            txtbTag.Text = "";
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

            ColViewDestFiles.Refresh(collection.FileGeneTargetFilesCollection.REGEX_MOVIE_EXTENTION);

            SetDataGridDefaultSelectSetting();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ColViewArrangementTarget.FilterSearchProductNumber = txtSearch.Text;
            ColViewArrangementTarget.Refresh();
            ColViewDestFiles.FilterSearchProductNumber = txtSearch.Text;
            ColViewDestFiles.Refresh();

            btnExecuteNameChange.Focus();
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

            if (file != null)
                Process.Start(file.FileInfo.FullName);
        }

        private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            txtStatusBar.Width = stsbaritemDispDetail.ActualWidth;
        }

        private void txtChangeFileName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            dgridSelectTargetFilename.ItemsSource = ColViewMovieImport.collection;

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

                List<MovieFileContents> matchFileContentsList = ColViewMovieFileContents.MatchProductNumber(dispinfoSelectMovieImportData.ProductNumber);

                if ((bool)dispinfoSelectMovieImportData.RarFlag)
                {
                    ChangeModeNormalRarExecute(null, null);

                    foreach (TargetFiles file in dgridDestFile.ItemsSource)
                        file.IsSelected = false;
                }
                else
                {
                    string searchStr = (dispinfoSelectMovieImportData != null) ? dispinfoSelectMovieImportData.GetFileSearchString() : "";
                    collection.FileGeneTargetFilesCollection ColViewCheckRarFiles = new collection.FileGeneTargetFilesCollection(txtBasePath.Text, collection.FileGeneTargetFilesCollection.REGEX_RARONLY_EXTENTION, searchStr);
                    ColViewCheckRarFiles.Execute();

                    foreach (var data in ColViewCheckRarFiles.ColViewListTargetFiles)
                    {
                        MessageBoxResult result = MessageBox.Show("RARファイルが存在します、編集に移動しますか？", "確認", MessageBoxButton.OKCancel);

                        if (result == MessageBoxResult.OK)
                        {
                            txtFileGeneSearchText.Text = dispinfoSelectMovieImportData.GetFilterProductNumber();

                            ColViewFileGeneTargetFiles.FilterSearchProductNumber = txtFileGeneSearchText.Text;
                            ColViewFileGeneTargetFiles.Refresh();

                            SetUIElementFromImportData(dispinfoSelectMovieImportData);

                            lgridFilenameGenerate.Visibility = System.Windows.Visibility.Visible;

                            CahngeModeFilenameGenerateExecute(null, null);

                            gridSelectTargetFilename.Visibility = Visibility.Hidden;

                            return;
                        }
                    }

                    ChangeModeNormalMovieExecute(null, null);
                }

                if (matchFileContentsList.Count > 0)
                {
                    MovieFileContents matchFileContents = matchFileContentsList[0];

                    MovieImportData impData = new MovieImportData(matchFileContents.Name);
                    txtbExistFileId.Text = Convert.ToString(matchFileContents.Id);
                    txtbExistTitle.Text = impData.Title;
                }
                else
                {
                    txtbExistFileId.Text = "";
                    txtbExistTitle.Text = "";
                }

                if (dispinfoSelectMovieImportData.FileId > 0)
                    txtbTag.Background = new SolidColorBrush(Colors.PaleGreen);
                else
                    txtbTag.Background = null;

                txtbTag.Text = dispinfoSelectMovieImportData.Tag;

                lgridMain.Visibility = System.Windows.Visibility.Visible;

                SetDataGridDefaultSelectSetting();

                btnExecuteNameChange.Focus();
            }

            txtbTargetIdInfo.Text = Convert.ToString(dispinfoSelectMovieImportData.Id) + "/" + Convert.ToString(dispinfoSelectMovieImportData.FileId);

            gridSelectTargetFilename.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// dgridArramentTargetは動画、dgridDestFileは検索一致のファイルを全て選択チェックの状態にする
        /// </summary>
        private void SetDataGridDefaultSelectSetting()
        {
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
            settingControl.Save(txtBasePath.Text, txtLabelPath.Text, txtKoreanPornoPath.Text, txtKoreanPornoExportPath.Text);
        }

        private void btnPasteTitleText_Click(object sender, RoutedEventArgs e)
        {
            string titletext = ClipBoardCommon.GetText();

            if (titletext.Trim().Length <= 10)
            {
                bool isDate = false;
                try
                {
                    DateTime dt = Convert.ToDateTime(titletext);
                    txtFilenameGenDate.Text = dt.ToString("yyyy/MM/dd");
                    isDate = true;
                }
                catch(Exception)
                {

                }
                if (isDate)
                    return;
            }

            dispinfoSelectMovieImportData = null;
            ClearUIElement();
            txtTitleText.Text = titletext;

            MovieImportData importData;
            if (titletext.IndexOf("RAR") == 0
                || titletext.IndexOf("[AV") == 0
                || titletext.IndexOf("[IV") == 0
                || titletext.IndexOf("[裏") == 0)
            {
                importData = new MovieImportData(titletext);

                txtbFileGenFileId.Text = Convert.ToString(importData.FileId);
                txtKind.Text = Convert.ToString(importData.Kind);
                txtFilenameGenDate.Text = importData.ProductDate.ToString("yyyy/MM/dd");
                txtProductNumber.Text = importData.ProductNumber;
                txtMaker.Text = importData.StrMaker;
                txtTitle.Text = importData.Title;
                tbtnFileGenHdUpdate.IsChecked = importData.HdFlag;

                if (importData.RarFlag == true)
                    tbtnFileGeneTextAddRar.IsChecked = true;

                dispinfoSelectMovieImportData = importData;
                ColViewFileGeneTargetFiles.FilterSearchProductNumber = dispinfoSelectMovieImportData.GetFilterProductNumber();
                ColViewFileGeneTargetFiles.Refresh();

                //return;
            }

            txtStatusBar.Text = "";

            importData = new MovieImportData();
            // メーカー情報と合わせるための製品番号、HDかどうかの情報をParse
            importData.ParseFromPasteText(titletext);

            List<MovieMaker> listMatchMaker = ColViewMaker.GetMatchData(importData);

            if (importData.ProductNumber == null || importData.ProductNumber.Length <= 0)
            {
                if (listMatchMaker.Count == 1)
                {
                    importData.SetMaker(listMatchMaker[0]);
                    importData.SetProductNumber();
                }
            }

            if (importData.ProductNumber != null && importData.ProductNumber.Length > 0)
            {
                // MOVIE_IMPORT_DATAに既存にデータがが存在すれば表示
                dispinfoSelectMovieImportData = ColViewMovieImport.GetDataByProductId(importData.ProductNumber);

                List<MovieFileContents> matchList = null;

                // HDの場合は、MOVIE_FILESからも一致するデータが存在するかを取得
                matchList = ColViewMovieFileContents.MatchProductNumber(importData.ProductNumber);

                if (matchList.Count == 1)
                {
                    MovieFileContents fileContents = matchList[0];
                    dispinfoSelectMovieImportData = new MovieImportData(fileContents.Name);

                    dispinfoSelectMovieImportData.CopyText = titletext;
                    dispinfoSelectMovieImportData.FileId = fileContents.Id;
                    dispinfoSelectMovieImportData.HdKind = importData.HdKind;
                    dispinfoSelectMovieImportData.HdFlag = true;
                    dispinfoSelectMovieImportData.Tag = fileContents.Tag;
                    dispinfoSelectMovieImportData.SetPickupTitle(fileContents);
                }
                else if (matchList.Count > 1)
                {
                    string msg = "対象のMOVIE_FILE_CONTENTSが複数件存在します";

                    foreach(MovieFileContents data in matchList)
                    {
                        msg += "\n" + data.Name;
                    }
                    txtStatusBar.Text = msg;
                }
                else
                {
                    txtStatusBar.Text = "対象のMOVIE_FILE_CONTENTSは存在しません";
                }

                txtTitleText.Text = titletext;

                if (dispinfoSelectMovieImportData != null)
                {
                    SetUIElementFromImportData(dispinfoSelectMovieImportData);

                    ColViewFileGeneTargetFiles.FilterSearchProductNumber = dispinfoSelectMovieImportData.GetFilterProductNumber();
                    txtFileGeneSearchText.Text = ColViewFileGeneTargetFiles.FilterSearchProductNumber;
                    ColViewFileGeneTargetFiles.Refresh();

                    return;
                }
            }

            if (dispinfoSelectMovieImportData == null)
                dispinfoSelectMovieImportData = importData;

            try
            {
                //listMatchMaker = MovieMakers.GetMatchData(dispinfoSelectMovieImportData, listMakers);

                if (listMatchMaker == null || listMatchMaker.Count() <= 0)
                {
                    txtStatusBar.Text = "一致するメーカーが存在しませんでした";
                    dispinfoSelectMovieImportData.SetPickupTitle();
                    SetUIElementFromImportData(dispinfoSelectMovieImportData);
                }
                else if (listMatchMaker.Count() == 1)
                {
                    dispinfoSelectMovieImportData.SetMaker(listMatchMaker[0]);
                    dispinfoSelectMovieImportData.SetPickupTitle();
                    SetUIElementFromImportData(dispinfoSelectMovieImportData);
                }
                else
                {
                    dgridMakers.ItemsSource = null;
                    dgridMakers.ItemsSource = listMatchMaker;

                    lgridMakers.Visibility = System.Windows.Visibility.Visible;

                    // Autoの設定にする
                    ScreenDisableBorder.Width = Double.NaN;
                    ScreenDisableBorder.Height = Double.NaN;

                    isSelectSameMaker = true;

                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                MessageBox.Show(ex.Message);
                return;
            }

            ColViewFileGeneTargetFiles.FilterSearchProductNumber = dispinfoSelectMovieImportData.GetFilterProductNumber();
            txtFileGeneSearchText.Text = ColViewFileGeneTargetFiles.FilterSearchProductNumber;

            ColViewFileGeneTargetFiles.Refresh();
        }

        private void dgridMakers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 文字列一致のメーカー複数の選択の場合
            if (isSelectSameMaker)
            {
                dispinfoSelectMovieImportData.SetMaker((MovieMaker)dgridMakers.SelectedItem);
                dispinfoSelectMovieImportData.SetPickupTitle();
                SetUIElementFromImportData(dispinfoSelectMovieImportData);

                isSelectSameMaker = false;

                ColViewFileGeneTargetFiles.FilterSearchProductNumber = (dispinfoSelectMovieImportData != null) ? dispinfoSelectMovieImportData.GetFilterProductNumber() : txtProductNumber.Text;
                ColViewFileGeneTargetFiles.Refresh();

                ButtonMakerClose(null, null);
            }
            isSelectSameMaker = false;
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

            dgridMakers.ItemsSource = ColViewMaker.ColViewListMakers;
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
            ColViewMaker.Refresh();

            txtRegistId.Text = "";
            txtRegistMakerName.Text = "";
            txtRegistMakerLabel.Text = "";
            txtRegistMakerKind.Text = "";
            txtRegistMakerMatchStr.Text = "";
            txtRegistMatchProductNumber.Text = "";

            dgridMakers.ItemsSource = ColViewMaker.ColViewListMakers;

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

            if (txtbFileGenFileId.Text.Length > 0)
                movieImportData.FileId = Convert.ToInt32(txtbFileGenFileId.Text);
            if (txtKind.Text.Length > 0)
                movieImportData.Kind = Convert.ToInt32(txtKind.Text);
            movieImportData.MatchProduct = txtMatchStr.Text;
            movieImportData.ProductNumber = txtProductNumber.Text;
            if (Validation.GetHasError(txtFilenameGenDate))
                movieImportData.ProductDate = new DateTime(1900, 1, 1);
            else
                movieImportData.StrProductDate = txtFilenameGenDate.Text;
            movieImportData.StrMaker = txtMaker.Text;
            movieImportData.Title = txtTitle.Text;
            movieImportData.Actresses = txtActresses.Text;
            movieImportData.HdFlag = tbtnFileGenHdUpdate.IsChecked;
            movieImportData.RarFlag = tbtnFileGeneTextAddRar.IsChecked;
            movieImportData.SplitFlag = tbtnFileGenSplit.IsChecked;
            movieImportData.Tag = txtTag.Text;
            movieImportData.GenerateFilename();
            movieImportData.Filename = txtFilenameGenerate.Text;

            movieImportData.GenerateFilename();

            return movieImportData;
        }

        private void SetUIElementFromImportData(MovieImportData myData)
        {
            if (myData.FileId > 0)
                txtbFileGenFileId.Text = Convert.ToString(myData.FileId);

            if (myData.Id > 0)
                txtbFileGenImportId.Text = Convert.ToString(myData.Id);

            txtTitleText.Text = myData.CopyText;
            txtKind.Text = myData.Kind.ToString();
            txtMatchStr.Text = myData.GetMatchMaker();
            if (myData.ProductDate.Year > 1900)
                txtFilenameGenDate.Text = myData.ProductDate.ToString("yyyy/MM/dd");
            txtProductNumber.Text = myData.ProductNumber;
            txtMaker.Text = myData.StrMaker;
            txtTitle.Text = myData.Title;
            txtActresses.Text = myData.Actresses;
            txtTag.Text = myData.Tag;
            tbtnFileGeneTextAddRar.IsChecked = myData.RarFlag;
            tbtnFileGenSplit.IsChecked = myData.SplitFlag;
            txtFilenameGenerate.Text = myData.Filename;

            if (myData.FileId > 0)
                tbtnFileGenHdUpdate.IsChecked = true;
        }

        private void ClearUIElement()
        {
            txtbFileGenImportId.Text = "";
            txtbFileGenFileId.Text = "";
            txtTitleText.Text = "";
            if (chkKindFixed.IsChecked == null || !(bool)chkKindFixed.IsChecked) txtKind.Text = "";
            txtMatchStr.Text = "";
            txtProductNumber.Text = "";
            txtMaker.Text = "";
            txtTitle.Text = "";
            txtActresses.Text = "";
            txtTag.Text = "";
            tbtnFileGeneTextAddRar.IsChecked = false;
            tbtnFileGenSplit.IsChecked = false;
            txtFilenameGenerate.Text = "";
            tbtnFileGenHdUpdate.IsChecked = false;

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
            bool b = false;
            if (tbtnFileGenModeFilenameOnly.IsChecked != null)
                b = (bool)tbtnFileGenModeFilenameOnly.IsChecked;

            MovieImportService service = new service.MovieImportService();
            if (GetChecked(tbtnFileGenModeFilenameOnly))
            {
                MovieImportData movieImportData = GetImportDataFromUIElement();

                movieImportData = service.DbExport(movieImportData, new DbConnection());

                ClearUIElement();

                return;
            }

            if (Validation.GetHasError(txtFilenameGenDate))
            {
                txtStatusBar.Text = "日付が正しく入力されていないため登録できません";
                return;
            }

            string importId = txtbFileGenImportId.Text;

            if (importId.Length > 0)
            {
                MovieImportData movieImportData = GetImportDataFromUIElement();
                movieImportData.Id = Convert.ToInt32(importId);

                service.DbUpdate(movieImportData, new DbConnection());
            }
            else
            {
                MovieImportData movieImportData = GetImportDataFromUIElement();

                movieImportData = service.DbExport(movieImportData, new DbConnection());

                ClearUIElement();
            }

            ColViewMovieImport.Refresh();
        }

        private void btnPasteActresses_Click(object sender, RoutedEventArgs e)
        {
            string ClipboardText = ClipBoardCommon.GetText();

            txtActresses.Text = dispinfoSelectMovieImportData.ConvertActress(ClipboardText, "、");
            txtTag.Text = dispinfoSelectMovieImportData.ConvertActress(ClipboardText, ",");

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
            if (dispinfoSelectDataGridKoreanPorno != null)
            {
                if (txtKoreanPornoExportPath.Text.Trim().Length <= 0)
                {
                    MessageBox.Show("出力先のパスが設定されていません");
                    return;
                }
                KoreanPornoData selData = dispinfoSelectDataGridKoreanPorno;
                txtbKoreanPornoName.Text = selData.Name;
                txtbKoreanPornoArchiveFile.Text = selData.ArchiveFile;

                KoreanPornoService service = new KoreanPornoService(txtKoreanPornoPath.Text, txtKoreanPornoExportPath.Text);
                List<KoreanPornoFileInfo> listFiles = service.GetFileInfo(selData.Name, selData.LastWriteTime, dispinfoSelectDataGridKoreanPorno.ArchiveFile);

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
            if (dgridKoreanPorno.SelectedItems != null)
            {
                if (dgridKoreanPorno.SelectedItems.Count == 1)
                    dispinfoSelectDataGridKoreanPorno = (KoreanPornoData)dgridKoreanPorno.SelectedItem;
                else
                {
                    dispinfoSelectDataGridKoreanPorno = (KoreanPornoData)dgridKoreanPorno.SelectedItems[0];
                    txtStatusBar.Text = "複数選択しているので、先頭の選択行を対象とします";
                }
            }
        }

        private void btnKoreanPornoExecute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                KoreanPornoService service = new KoreanPornoService(txtKoreanPornoPath.Text, txtKoreanPornoExportPath.Text);
                service.ExecuteArrangement(dispinfoSelectDataGridKoreanPorno, (List<KoreanPornoFileInfo>)dgridKoreanPornoFolder.ItemsSource);

                ColViewKoreanPorno.Refresh();
                dgridKoreanPorno.Items.Refresh();
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                MessageBox.Show(ex.Message);
            }

            dgridKoreanPorno.Visibility = System.Windows.Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dgridKoreanPorno.Visibility = System.Windows.Visibility.Visible;
        }

        private void OnPasteKoreanPornoPath(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn == null)
                return;

            if (btn.Name.IndexOf("Export") >= 0)
                txtKoreanPornoExportPath.Text = ClipBoardCommon.GetTextPath();
            else
                txtKoreanPornoPath.Text = ClipBoardCommon.GetTextPath();
        }

        private void OnMakersFilter(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                chkKindOne.IsChecked = false;
                chkKindTwo.IsChecked = false;
                chkKindThree.IsChecked = false;
                return;
            }

            List<int> intList = new List<int>();

            if (chkKindOne.IsChecked != null)
                intList.Add(1);
            if (chkKindTwo.IsChecked != null)
                intList.Add(2);
            if (chkKindThree.IsChecked != null)
                intList.Add(3);

            ColViewMaker.SetCondition(intList.ToArray(), txtMakersSearch.Text);
            ColViewMaker.Execute();
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

        private void tbtnFileGenHdUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (tbtnFileGenHdUpdate.IsChecked != null)
            {
                bool updateChecked = (bool)tbtnFileGenHdUpdate.IsChecked;

                if (updateChecked && txtbFileGenFileId.Text.Length <= 0)
                {
                    List<MovieFileContents> fileContentsList = ColViewMovieFileContents.MatchProductNumber(txtProductNumber.Text);

                    if (fileContentsList.Count > 0)
                    {
                        MovieFileContents fileContents = fileContentsList[0];
                        txtbFileGenFileId.Text = Convert.ToString(fileContents.Id);
                    }
                }
            }
        }

        private void btnFileGenClearFileId_Click(object sender, RoutedEventArgs e)
        {
            txtbFileGenFileId.Text = "";
            tbtnFileGenHdUpdate.IsChecked = false;
        }

        private void txtSearchTartgetFilename_TextChanged(object sender, TextChangedEventArgs e)
        {
            ColViewMovieImport.Filter(txtSearchTartgetFilename.Text);
        }

        private void tbtnFileGenModeFilenameOnly_Click(object sender, RoutedEventArgs e)
        {
            if (GetChecked((ToggleButton)sender))
                this.Background = new SolidColorBrush(Colors.LightGreen);
            else
                this.Background = new SolidColorBrush(Colors.White);
        }
        public bool GetChecked(ToggleButton myToggleButton)
        {
            bool b = false;
            if (myToggleButton.IsChecked != null)
                b = (bool)myToggleButton.IsChecked;

            return b;
        }
    }
}
