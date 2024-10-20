// Data/Database.cs
using MySql.Data.MySqlClient;

namespace Volunteers.Data
{
    public static class Database
    {
        // Замените параметры на ваши
        private static string connectionString = "Server=localhost;Database=voli;Uid=root;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
