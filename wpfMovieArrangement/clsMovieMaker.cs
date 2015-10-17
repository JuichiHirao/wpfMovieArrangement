using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace wpfMovieArrangement
{
    class MovieMaker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public int Kind { get; set; }
        public string MatchStr { get; set; }
        public string MatchProductNumber { get; set; }
        public string MatchProductNumberValue { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public string GetNameLabel()
        {
            string namelabel = "";
            if (Label == null || Label.Length <= 0)
                namelabel = Name;
            else
                namelabel = Name + "：" + Label;

            return namelabel;
        }

        public void ParseFromFileName(MovieFileContents myFile)
        {
            if (myFile.Name.IndexOf("[AVRIP]") == 0)
                Kind = MovieFileContents.KIND_AVRIP;
            else if (myFile.Name.IndexOf("[IVRIP]") == 0)
                Kind = MovieFileContents.KIND_IVRIP;
            else if (myFile.Name.IndexOf("[裏AVRIP]") == 0)
                Kind = MovieFileContents.KIND_URAAVRIP;

            Regex regex = new Regex("【(.*)】");

            if (regex.IsMatch(myFile.Name))
            {
                string work = regex.Match(myFile.Name).Groups[1].Value.ToString();

                string[] arrSplitStr = {"："};
                string[] arrMakerNames = work.Split(arrSplitStr, StringSplitOptions.None);
                if (arrMakerNames.Length >= 2)
                {
                    Name = arrMakerNames[0];
                    Label = arrMakerNames[1];
                }
                else
                    Name = work;
            }
        }

        public void DbUpdate(DbConnection myDbCon)
        {
            DbConnection dbcon;
            string sqlcmd = "";

            // 引数にコネクションが指定されていた場合は指定されたコネクションを使用
            if (myDbCon != null)
                dbcon = myDbCon;
            else
                dbcon = new DbConnection();

            sqlcmd = "UPDATE MOVIE_MAKERS ";
            sqlcmd = sqlcmd + "SET NAME = @NAME, LABEL = @LABEL, KIND = @KIND, MATCH_STR = @MATCH_STR, MATCH_PRODUCT_NUMBER = @MATCH_PRODUCT_NUMBER ";
            sqlcmd = sqlcmd + "WHERE ID = @ID ";

            SqlCommand scmd = new SqlCommand(sqlcmd, dbcon.getSqlConnection());
            DataTable dtSaraly = new DataTable();

            SqlParameter[] sqlparams = new SqlParameter[6];

            sqlparams[0] = new SqlParameter("@NAME", SqlDbType.VarChar);
            sqlparams[0].Value = Name;
            sqlparams[1] = new SqlParameter("@LABEL", SqlDbType.VarChar);
            sqlparams[1].Value = Label;
            sqlparams[2] = new SqlParameter("@KIND", SqlDbType.Int);
            sqlparams[2].Value = Kind;
            sqlparams[3] = new SqlParameter("@MATCH_STR", SqlDbType.VarChar);
            sqlparams[3].Value = MatchStr;
            sqlparams[4] = new SqlParameter("@MATCH_PRODUCT_NUMBER", SqlDbType.VarChar);
            sqlparams[4].Value = MatchProductNumber;
            sqlparams[5] = new SqlParameter("@ID", SqlDbType.Int);
            sqlparams[5].Value = Id;

            dbcon.SetParameter(sqlparams);

            dbcon.execSqlCommand(sqlcmd);

            return;
        }

        public void DbExport(DbConnection myDbCon)
        {
            DbConnection dbcon;
            string sqlcmd = "";

            // 引数にコネクションが指定されていた場合は指定されたコネクションを使用
            if (myDbCon != null)
                dbcon = myDbCon;
            else
                dbcon = new DbConnection();

            sqlcmd = "INSERT INTO MOVIE_MAKERS ( NAME, LABEL, KIND, MATCH_STR, MATCH_PRODUCT_NUMBER ) ";
            sqlcmd = sqlcmd + "VALUES( @NAME, @LABEL, @KIND, @MATCH_STR, @MATCH_PRODUCT_NUMBER ) ";

            SqlCommand scmd = new SqlCommand(sqlcmd, dbcon.getSqlConnection());
            DataTable dtSaraly = new DataTable();

            SqlParameter[] sqlparams = new SqlParameter[5];

            sqlparams[0] = new SqlParameter("@NAME", SqlDbType.VarChar);
            sqlparams[0].Value = Name;
            sqlparams[1] = new SqlParameter("@LABEL", SqlDbType.VarChar);
            sqlparams[1].Value = Label;
            sqlparams[2] = new SqlParameter("@KIND", SqlDbType.Int);
            sqlparams[2].Value = Kind;
            sqlparams[3] = new SqlParameter("@MATCH_STR", SqlDbType.VarChar);
            sqlparams[3].Value = MatchStr;
            sqlparams[4] = new SqlParameter("@MATCH_PRODUCT_NUMBER", SqlDbType.VarChar);
            sqlparams[4].Value = MatchProductNumber;

            dbcon.SetParameter(sqlparams);

            dbcon.execSqlCommand(sqlcmd);

            return;
        }

        public void DbDelete(DbConnection myDbCon)
        {
            string DeleteCommand = "";

            DeleteCommand = "DELETE MOVIE_MAKERS WHERE ID = @pId ";

            //Debug.Print(InsertCommand);

            SqlParameter[] sqlparams = new SqlParameter[1];

            // ID
            sqlparams[0] = new SqlParameter("@pId", SqlDbType.Int);
            sqlparams[0].Value = Id;

            myDbCon.SetParameter(sqlparams);

            // DELETE文の実行
            int cnt = myDbCon.execSqlCommand(DeleteCommand);

            if (cnt == 0)
                throw new Exception("ID[" + Id + "]の削除対象のデータが存在しません");
        }
    }
}
