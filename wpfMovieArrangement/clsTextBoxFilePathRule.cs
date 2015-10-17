using System;
using System.Globalization;
using System.Windows.Controls;
using System.IO;

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
}
