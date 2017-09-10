using System;
using System.Globalization;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;

namespace wpfMovieArrangement
{
    class TextBoxFilePathRule : ValidationRule
    {

        public override ValidationResult Validate( object value, CultureInfo cultureInfo )
        {
            string parameter = "";

            try
            {
                if( ((string) value).Length > 0 )
                {
                    if (!Directory.Exists(((string)value)))
                        return new ValidationResult(false, "存在しないパスが指定されています");
                    parameter = (string) value;
                }
            }
            catch( Exception e )
            {
                return new ValidationResult( false, "Illegal characters or " + e.Message );
            }

            return new ValidationResult( true, null );
        }
    }

    class TextBoxFilenameRule : ValidationRule
    {
        public override ValidationResult Validate( object value, CultureInfo cultureInfo )
        {
            string parameter = "";

            try
            {
                if( ((string) value).Length > 0 )
                {
                    string tempPath = Path.GetTempPath();
                    string filename = Path.Combine(tempPath, (string)value);

                    try
                    {
                        StreamWriter toucher = new StreamWriter(filename);
                        toucher.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.Write(e);
                        return new ValidationResult(false, "有効ではないファイル名です");
                    }
                    parameter = (string) value;
                }
            }
            catch( Exception e )
            {
                return new ValidationResult( false, "Illegal characters or " + e.Message );
            }

            return new ValidationResult( true, null );
        }
    }

    class TextBoxDateRule : ValidationRule
    {

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || ((string)value).Length <= 0)
                return new ValidationResult(false, "datetime required ");

            DateTime dt;
            try
            {
                dt = Convert.ToDateTime(value);
            }
            catch (Exception)
            {
                return new ValidationResult(false, "Illegal datetime format ");
            }

            return new ValidationResult(true, null);
        }
    }
}
