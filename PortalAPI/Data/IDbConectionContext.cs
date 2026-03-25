using System.Data.Odbc;
using System.Data.SqlClient;
using System.Data;

namespace PortalAPI.Data
{
    public class IDbConectionContext
    {
        private readonly string _connectionString;
        private readonly string _connectionHanaString;
        private readonly IConfiguration _configuration;
        private readonly string _dbType;

        public IDbConectionContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("ConncetionStr");
            _connectionHanaString = configuration.GetConnectionString("HanaCon");
            _dbType = _configuration.GetValue<string>("dbType");
        }

        public IDbConnection CreateConnection()
        {
            return _dbType switch
            {
                "SQL" => new SqlConnection(_connectionString),
                _ => new OdbcConnection(_connectionHanaString)
            };
        }

        // ✅ Async-friendly method to open the connection safely
        public async Task<IDbConnection> CreateAndOpenConnectionAsync()
        {
            var connection = CreateConnection();

            if (connection is SqlConnection sqlConn)
            {
                await sqlConn.OpenAsync();
            }
            else
            {
                connection.Open(); // OdbcConnection does not support OpenAsync
            }

            return connection;
        }
    }
}
