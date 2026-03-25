using System.Data;
using System.Data.SqlClient;

using System.Data.Odbc;
using Microsoft.AspNetCore.SignalR;


namespace PortalAPI.Data

{
    public class DbConectionContext
    {
        private readonly string _connectionString;
        private readonly string _connectionHanaString;
        private readonly IConfiguration _configuration;
        string _dbType = string.Empty;
        public DbConectionContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ConncetionStr");
            _configuration = configuration;
            _connectionHanaString = configuration.GetConnectionString("HanaCon");
            _dbType = _configuration.GetValue<string>("dbType");
        }

        public IDbConnection CreateConnection()
        {
            IDbConnection dbConnection=null;
            
            if (!string.IsNullOrWhiteSpace(_dbType))
            {
                if (_dbType == "SQL")
                {
                    dbConnection= new SqlConnection(_connectionString);
                }
                else                     
                { 
                    dbConnection= new OdbcConnection(_connectionHanaString);                    
                }
            }
            
            return dbConnection;
        }
    }
}
