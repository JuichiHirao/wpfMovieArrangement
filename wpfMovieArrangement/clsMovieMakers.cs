using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace wpfMovieArrangement
{
    class MovieMakers
    {
        public static List<MovieMaker> GetMatchData(MovieImportData myImportData, List<MovieMaker> myListMakers)
        {
            List<MovieMaker> listMatchMaker = null;

            // 品番っぽい文字列が存在する場合
            //if (myFileContents.Kind == MovieFileContents.KIND_AVRIP)
            if (myImportData.ProductNumber != null && myImportData.ProductNumber.Length > 0)
            {
                string[] label = myImportData.ProductNumber.Split('-');
                var matchdata = from makerdata in myListMakers
                                where makerdata.MatchStr.ToUpper() == label[0].ToUpper() // + '-'
                                    && makerdata.MatchProductNumber.Length <= 0
                                select makerdata;

                if (matchdata.Count() > 0)
                {
                    listMatchMaker = new List<MovieMaker>();

                    foreach (MovieMaker m in matchdata)
                        listMatchMaker.Add(m);
                }
            }
            // 品番っぽいのが無い場合は、品番一致文字列があるタイトルと一致
            if (listMatchMaker == null || listMatchMaker.Count() <= 0)
            {
                foreach(MovieMaker makerdata in myListMakers)
                {
                    if (makerdata.MatchProductNumber.Length <= 0)
                        continue;

                    Regex regex = new Regex(makerdata.MatchStr, RegexOptions.IgnoreCase);
                    if (regex.IsMatch(myImportData.CopyText))
                    {
                        try
                        {
                            Regex regexPN = new Regex(makerdata.MatchProductNumber);
                            if (regexPN.IsMatch(myImportData.CopyText))
                            {
                                makerdata.MatchProductNumberValue = regexPN.Match(myImportData.CopyText).Value;
                                if (listMatchMaker == null)
                                    listMatchMaker = new List<MovieMaker>();

                                listMatchMaker.Add(makerdata);
                            }
                        }
                        catch (ArgumentException)
                        {
                            throw new Exception("正規表現が不正です\n" + makerdata.Name + "\n" + makerdata.MatchProductNumber);
                        }
                    }
                }
            }

            return listMatchMaker;
        }

        public static MovieMaker GetSearchByProductNumber(string myProductNumber)
        {
            DbConnection dbcon = new DbConnection();

            if (myProductNumber == null)
                myProductNumber = "";

            string[] product = myProductNumber.Split('-');
            List<MovieFileContents> listMContents = new List<MovieFileContents>();

            string queryString = "SELECT ID, NAME, SIZE, FILE_DATE, LABEL, SELL_DATE, PRODUCT_NUMBER FROM MOVIE_FILES WHERE PRODUCT_NUMBER LIKE @pProduct ORDER BY FILE_DATE DESC ";

            dbcon.openConnection();

            SqlCommand command = new SqlCommand(queryString, dbcon.getSqlConnection());

            SqlParameter param = new SqlParameter("@pProduct", SqlDbType.VarChar);
            param.Value = product[0] + "-%";
            command.Parameters.Add(param);

            SqlDataReader reader = command.ExecuteReader();

            do
            {
                while (reader.Read())
                {
                    MovieFileContents data = new MovieFileContents();

                    data.Id = DbExportCommon.GetDbInt(reader, 0);
                    data.Name = DbExportCommon.GetDbString(reader, 1);
                    data.Size = DbExportCommon.GetLong(reader, 2);
                    data.FileDate = DbExportCommon.GetDbDateTime(reader, 3);
                    data.Label = DbExportCommon.GetDbString(reader, 4);
                    data.SellDate = DbExportCommon.GetDbDateTime(reader, 5);
                    data.ProductNumber = DbExportCommon.GetDbString(reader, 6);

                    listMContents.Add(data);
                }
            } while (reader.NextResult());

            reader.Close();

            dbcon.closeConnection();

            MovieMaker maker = null;
            foreach(MovieFileContents data in listMContents)
            {
                maker = new MovieMaker();
                maker.ParseFromFileName(data);
                break;
            }

            return maker;
        }

        public static List<MovieMaker> GetAllData()
        {
            DbConnection dbcon = new DbConnection();

            List<MovieMaker> listMMakers = new List<MovieMaker>();

            string queryString = "SELECT ID, NAME, LABEL, KIND, MATCH_STR, MATCH_PRODUCT_NUMBER, CREATE_DATE, UPDATE_DATE FROM MOVIE_MAKERS ORDER BY NAME DESC ";

            dbcon.openConnection();

            SqlCommand command = new SqlCommand(queryString, dbcon.getSqlConnection());

            //SqlParameter param = new SqlParameter("@pLabel", SqlDbType.VarChar);
            //param.Value = myLabel;
            //command.Parameters.Add(param);

            SqlDataReader reader = command.ExecuteReader();

            do
            {
                while (reader.Read())
                {
                    MovieMaker data = new MovieMaker();

                    data.Id = DbExportCommon.GetDbInt(reader, 0);
                    data.Name = DbExportCommon.GetDbString(reader, 1);
                    data.Label = DbExportCommon.GetDbString(reader, 2);
                    data.Kind = DbExportCommon.GetDbInt(reader, 3);
                    data.MatchStr = DbExportCommon.GetDbString(reader, 4);
                    data.MatchProductNumber = DbExportCommon.GetDbString(reader, 5);
                    data.CreateDate = DbExportCommon.GetDbDateTime(reader, 6);
                    data.UpdateDate = DbExportCommon.GetDbDateTime(reader, 7);

                    listMMakers.Add(data);
                }
            } while (reader.NextResult());

            reader.Close();

            dbcon.closeConnection();

            return listMMakers;
        }
    }
}
