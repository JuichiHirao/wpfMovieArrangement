﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfMovieArrangement.service
{
    class MovieImportService
    {
        public void DbExport(MovieImportData myData, DbConnection myDbCon)
        {
            DbConnection dbcon;
            string sqlcmd = "";

            // 引数にコネクションが指定されていた場合は指定されたコネクションを使用
            if (myDbCon != null)
                dbcon = myDbCon;
            else
                dbcon = new DbConnection();

            sqlcmd = "INSERT INTO MOVIE_IMPORT ( COPY_TEXT, KIND, MATCH_PRODUCT, PRODUCT_NUMBER, PRODUCT_DATE, MAKER, TITLE, ACTRESSES, RAR_FLAG, TAG, FILENAME ) ";
            sqlcmd = sqlcmd + "VALUES( @CopyText, @Kind, @MatchProduct, @ProductNumber, @ProductDate, @Maker, @Title, @Actresses, @RarFlag, @Tag, @Filename ) ";

            SqlCommand scmd = new SqlCommand(sqlcmd, dbcon.getSqlConnection());
            DataTable dtSaraly = new DataTable();

            List<SqlParameter> listSqlParams = new List<SqlParameter>();

            SqlParameter sqlparam = new SqlParameter("@CopyText", SqlDbType.VarChar);
            sqlparam.Value = myData.CopyText;
            listSqlParams.Add(sqlparam);

            sqlparam = new SqlParameter("@Kind", SqlDbType.Int);
            sqlparam.Value = myData.Kind;
            listSqlParams.Add(sqlparam);

            sqlparam = new SqlParameter("@MatchProduct", SqlDbType.VarChar);
            sqlparam.Value = myData.MatchProduct;
            listSqlParams.Add(sqlparam);

            sqlparam = new SqlParameter("@ProductNumber", SqlDbType.VarChar);
            sqlparam.Value = myData.ProductNumber;
            listSqlParams.Add(sqlparam);

            sqlparam = new SqlParameter("@ProductDate", SqlDbType.DateTime);
            sqlparam.Value = myData.ProductDate;
            listSqlParams.Add(sqlparam);

            sqlparam = new SqlParameter("@Maker", SqlDbType.VarChar);
            sqlparam.Value = myData.Maker;
            listSqlParams.Add(sqlparam);

            sqlparam = new SqlParameter("@Title", SqlDbType.VarChar);
            sqlparam.Value = myData.Title;
            listSqlParams.Add(sqlparam);

            sqlparam = new SqlParameter("@Actresses", SqlDbType.VarChar);
            sqlparam.Value = myData.Actresses;
            listSqlParams.Add(sqlparam);

            sqlparam = new SqlParameter("@RarFlag", SqlDbType.Int);
            sqlparam.Value = myData.RarFlag;
            listSqlParams.Add(sqlparam);

            sqlparam = new SqlParameter("@Tag", SqlDbType.VarChar);
            sqlparam.Value = myData.Tag;
            listSqlParams.Add(sqlparam);

            sqlparam = new SqlParameter("@Filename", SqlDbType.VarChar);
            sqlparam.Value = myData.Filename;
            listSqlParams.Add(sqlparam);

            dbcon.SetParameter(listSqlParams.ToArray());

            dbcon.execSqlCommand(sqlcmd);

            return;
        }

        public List<MovieImportData> GetList(DbConnection myDbCon)
        {
            List<MovieImportData> listData = new List<MovieImportData>();

            DbConnection dbcon;
            string sqlcmd = "";

            // 引数にコネクションが指定されていた場合は指定されたコネクションを使用
            if (myDbCon != null)
                dbcon = myDbCon;
            else
                dbcon = new DbConnection();

            sqlcmd = "SELECT ID, COPY_TEXT, KIND, MATCH_PRODUCT, PRODUCT_NUMBER, PRODUCT_DATE, MAKER, TITLE, ACTRESSES, RAR_FLAG, TAG, FILENAME, CREATE_DATE, UPDATE_DATE ";
            sqlcmd = sqlcmd + "FROM MOVIE_IMPORT ";

            SqlDataReader reader = null;
            try
            {
                reader = myDbCon.GetExecuteReader(sqlcmd);

                do
                {
                    if (reader.IsClosed)
                    {
                        //_logger.Debug("reader.IsClosed");
                        throw new Exception("MOVIE_SITESTOREの取得でreaderがクローズされています");
                    }

                    while (reader.Read())
                    {
                        MovieImportData data = new MovieImportData();

                        data.Id = DbExportCommon.GetDbInt(reader, 0);
                        data.CopyText = DbExportCommon.GetDbString(reader, 1);
                        data.Kind = DbExportCommon.GetDbInt(reader, 2);
                        data.MatchProduct = DbExportCommon.GetDbString(reader, 3);
                        data.ProductNumber = DbExportCommon.GetDbString(reader, 4);
                        data.ProductDate = DbExportCommon.GetDbDateTime(reader, 5);
                        data.Maker = DbExportCommon.GetDbString(reader, 6);
                        data.Title = DbExportCommon.GetDbString(reader, 7);
                        data.Actresses = DbExportCommon.GetDbString(reader, 8);
                        data.RarFlag = Convert.ToBoolean(DbExportCommon.GetDbInt(reader, 9));
                        data.Tag = DbExportCommon.GetDbString(reader, 10);
                        data.Filename = DbExportCommon.GetDbString(reader, 11);
                        data.CreateDate = DbExportCommon.GetDbDateTime(reader, 12);
                        data.UpdateDate = DbExportCommon.GetDbDateTime(reader, 13);

                        listData.Add(data);
                    }
                } while (reader.NextResult());
            }
            finally
            {
                reader.Close();
            }

            reader.Close();

            myDbCon.closeConnection();

            return listData;
        }
    }
}
