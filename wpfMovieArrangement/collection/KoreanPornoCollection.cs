using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using wpfMovieArrangement.service;

namespace wpfMovieArrangement.collection
{
    class KoreanPornoCollection
    {
        public List<KoreanPornoData> listData;
        public ICollectionView ColViewListData;

        KoreanPornoService service;

        public string BasePath;

        public KoreanPornoCollection(string myPath, string myExportPath)
        {
            BasePath = myPath;
            service = new KoreanPornoService(BasePath, myExportPath);

            DataSet();
            ColViewListData = CollectionViewSource.GetDefaultView(listData);

            ColViewListData.SortDescriptions.Clear();
            ColViewListData.SortDescriptions.Add(new SortDescription("LastWriteTime", ListSortDirection.Ascending));
        }

        public void DataSet()
        {
            listData = service.GetFolderData(BasePath);
        }

        public void Refresh()
        {
            ColViewListData = null;
            listData = null;
            listData = service.GetFolderData(BasePath);

            ColViewListData = CollectionViewSource.GetDefaultView(listData);
        }
    }
}
