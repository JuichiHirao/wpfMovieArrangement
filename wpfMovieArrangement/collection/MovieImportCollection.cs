using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using wpfMovieArrangement.service;

namespace wpfMovieArrangement.collection
{
    class MovieImportCollection
    {
        List<MovieImportData> listData;
        MovieImportService service;
        public ICollectionView collection;

        public MovieImportCollection()
        {
            listData = new List<MovieImportData>();
            service = new service.MovieImportService();
            Refresh();
        }

        public void Refresh()
        {
            listData = service.GetList(new DbConnection());
            collection = CollectionViewSource.GetDefaultView(listData);
        }

        public MovieImportData GetDataByProductId(string myProductNumber)
        {
            foreach (MovieImportData imp in listData)
            {
                if (imp.ProductNumber.Equals(myProductNumber))
                    return imp;
            }

            return null;
        }

        public void Filter(string mySearchText)
        {
            if (mySearchText.Length <= 0)
            {
                collection.Filter = null;
                return;
            }

            collection.Filter = delegate (object o)
            {
                MovieImportData data = o as MovieImportData;

                if (data.ProductNumber.IndexOf(mySearchText) >= 0)
                    return true;

                return false;
            };


        }
    }
}
