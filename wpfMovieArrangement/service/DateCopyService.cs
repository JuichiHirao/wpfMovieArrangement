using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace wpfMovieArrangement.service
{
    class DateCopyService
    {
        private TargetFiles SourceFile = null;
        private List<TargetFiles> listDestFiles = null;
        public static List<TargetFiles> CheckDataGridSelectItem(DataGrid myDataGrid, string myBaseMessage, int myMaxFiles)
        {
            var itemSource = myDataGrid.ItemsSource;

            if (itemSource == null)
                throw new Exception(myBaseMessage + "のファイルが存在しません");

            var selecteItems = myDataGrid.SelectedItems;

            if (selecteItems == null)
                throw new Exception(myBaseMessage + "のファイルが選択されていません");

            List<TargetFiles> listTarget = new List<TargetFiles>();

            foreach(TargetFiles data in selecteItems)
            {
                listTarget.Add(data);
            }

            if (listTarget.Count > myMaxFiles)
                throw new Exception(myBaseMessage + "は複数ファイルの選択は出来ません");

            return listTarget;
        }

        public void SetSourceFile(DataGrid myDataGrid)
        {
            DateCopyService.CheckDataGridSelectItem(myDataGrid, "コピー元", 1);
            SourceFile = (TargetFiles)myDataGrid.SelectedItem;
        }
        public void SetDestFile(DataGrid myDataGrid)
        {
            if (listDestFiles == null)
                listDestFiles = new List<TargetFiles>();
            else
                listDestFiles.Clear();

            DateCopyService.CheckDataGridSelectItem(myDataGrid, "コピー先", 99);

            foreach (TargetFiles data in myDataGrid.SelectedItems)
                listDestFiles.Add(data);

            return;
        }

        public string Execute()
        {
            bool isFinished = false;
            string message = "";
            foreach (TargetFiles dest in listDestFiles)
            {
                while (isFinished == false)
                {
                    try
                    {
                        dest.FileInfo.LastWriteTime = SourceFile.ListUpdateDate;
                        isFinished = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBoxResult res = MessageBox.Show("ファイルの読取専用を外して下さい、再実行しますか？", "", MessageBoxButton.YesNo);

                        if (res == MessageBoxResult.No)
                            break;
                    }
                }
            }

            if (isFinished)
                message += listDestFiles[0].Name + "を" + SourceFile.ListUpdateDate.ToString("yyyy/MM/dd HH:mm:ss") + "に変更しました\n";

            return message;
        }

    }
}
