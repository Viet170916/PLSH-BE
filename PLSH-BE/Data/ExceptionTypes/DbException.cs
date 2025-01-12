using System;
using System.Diagnostics.CodeAnalysis;

namespace Data.ExceptionTypes
{
    [ExcludeFromCodeCoverage]
    public class DbException : Exception
    {
        public DbException()
        {
        }

        public DbException(string message)
            : base("In Db: " + message)
        {
        }

        public DbException(string message, Exception innerException)
            : base("In Db: " + message, innerException)
        {
        }

        public DbException(string sql, object[] parms, Exception innerException)
            : base("In Db: " + string.Format("Sql: {0}  Parms: {1}", (sql ?? "--"),
                    (parms != null ? string.Join(",", Array.ConvertAll<object, string>(parms, o => (o ?? "null").ToString())) : "--")),
                innerException)
        {
        }
    }
}
