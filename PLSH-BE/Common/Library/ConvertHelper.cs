using System;
using System.Diagnostics.CodeAnalysis;

namespace Common.Library
{
    [ExcludeFromCodeCoverage]
    public static class ConvertHelper
    {
        private const string DATE_FORMAT = "dd/MM/yyyy";
        private const string DATE_FORMAT2 = "dd-MM-yyyy";
        private const string DATETIME_FORMAT = "dd/MM/yyyy HH:mm";
        private const string DATETIME_FORMAT_MONTH_YEAR = "MMM yyyy";

        public static DateTime? ConvertToStringFormatDate(int? month, int? year)
        {
            if (month == null || year == null)
            {
                return null;
            }
            else
            {
                DateTime dt;
                string date = $"01/{ month.Value}/{ year.Value}";
                if (month < 10)
                {
                    date = $"01/0{ month.Value}/{ year.Value}";
                }
                bool isValid = DateTime.TryParseExact(date, DATE_FORMAT, null, System.Globalization.DateTimeStyles.None, out dt);
                if (isValid)
                {
                    return dt;
                }
                else
                {
                    return null;
                }

            }
        }
        public static bool? ConvertToBool(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            bool bValue = false;
            if (bool.TryParse(value, out bValue))
            {
                return bValue;
            }
            return null;
        }

        public static int? ConvertToInt(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            int intValue = 0;
            if (int.TryParse(value, out intValue))
            {
                return intValue;
            }

            return null;
        }
        public static int ConvertToInt32(object value, int defaultValue)
        {
            try
            {
                if(value != null)
                {
                    return Convert.ToInt32(value.ToString().Trim());
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }


        public static decimal? ConvertToDecimal(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            decimal intValue = 0;
            if (decimal.TryParse(value, out intValue))
            {
                return intValue;
            }

            return null;
        }

        public static float? ConvertToFloat(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            float intValue = 0;
            if (float.TryParse(value, out intValue))
            {
                return intValue;
            }
            return null;
        }
        public static double? ConvertTodouble(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            double intValue = 0;
            if (double.TryParse(value, out intValue))
            {
                return intValue;
            }

            return null;
        }

        public static DateTime? ConvertToDate(string value)
        {
            DateTime dt;
            DateTime dt2;
            bool isValid = DateTime.TryParseExact(value, DATE_FORMAT, null, System.Globalization.DateTimeStyles.None, out dt);
            bool isValid2 = DateTime.TryParseExact(value, DATE_FORMAT2, null, System.Globalization.DateTimeStyles.None, out dt2);
            if (!string.IsNullOrEmpty(value))
            {
                if (isValid)
                {
                    return dt;
                }
                if (isValid2)
                {
                    return dt2;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        public static DateTime? ConvertToDatetime(string value)
        {
            DateTime dt;
            bool isValid = DateTime.TryParseExact(value, DATETIME_FORMAT, null, System.Globalization.DateTimeStyles.None, out dt);
            if (!string.IsNullOrEmpty(value) && isValid)
            {
                return dt;
            }
            else
            {
                return null;
            }

        }
        public static DateTime? ConvertToDate(string value, string dateFormat)
        {
            DateTime dt;
            bool isValid = DateTime.TryParseExact(value, dateFormat, null, System.Globalization.DateTimeStyles.None, out dt);
            if (!string.IsNullOrEmpty(value) && isValid)
            {
                return dt;
            }
            else
            {
                return null;
            }
        }

        public static DateTime? GetStartDateOfMonth(string monthOfYear)
        {
            DateTime dt;

            var value = $"01-{ monthOfYear }";
            bool isValid = DateTime.TryParseExact(value, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out dt);
            if (!string.IsNullOrEmpty(monthOfYear) && isValid)
            {
                return dt.Date;
            }
            else
            {
                return null;
            }
        }
        public static DateTime? GetEndDateOfMonth(string monthOfYear)
        {
            DateTime dt;
            var value = $"01-{ monthOfYear }";
            bool isValid = DateTime.TryParseExact(value, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out dt);
            if (!string.IsNullOrEmpty(monthOfYear) && isValid)
            {
                dt = dt.AddMonths(1).AddDays(-1).Date;
                return dt;
            }
            else
            {
                return null;
            }
        }
    }
}
