using System.Data.SqlClient;

namespace WindowsFormsApp4
{
    internal static class Db
    {
        internal const string ConnectionString = "Server=.;Database=MarketDB;Trusted_Connection=True;";

        internal static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}

