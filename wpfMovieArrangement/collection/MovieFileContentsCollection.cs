using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfMovieArrangement.collection
{
    class MovieFileContentsCollection
    {
        List<MovieFileContents> listFileContents;
        MovieFileContentsService service = new MovieFileContentsService();

        public MovieFileContentsCollection()
        {
             listFileContents = new List<MovieFileContents>();
            DataSet();
        }

        private void DataSet()
        {
            listFileContents = service.GetDbContents();

            return;
        }

        public List<MovieFileContents> MatchProductNumber(string myProductNumber)
        {
            List<MovieFileContents> fileList = new List<MovieFileContents>();
            foreach (MovieFileContents file in listFileContents)
            {
                if (file.ProductNumber.Length <= 0)
                    continue;

                if (file.ProductNumber.Equals("SD") || file.ProductNumber.Equals("HD") || file.ProductNumber.Equals("DMM"))
                    continue;

                if (file.ProductNumber.Equals(myProductNumber))
                    fileList.Add(file);
            }

            return fileList;
            //throw new Exception("マッチデータが複数件存在します");
        }

        public MovieFileContents MatchId(int myId)
        {
            foreach (MovieFileContents data in listFileContents)
            {
                if (data.Id == myId)
                    return data;
            }

            return null;
        }

    }
}
