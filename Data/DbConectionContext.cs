using System.Data;

using System.Data.Odbc;

using System.Data.SqlClient;

namespace TTSteelWebAPI.Data

{

    public class DbConectionContext

    {

        private readonly string _connectionString;

        private readonly string _connectionHanaString;

        private readonly string _dbType;

        public DbConectionContext(IConfiguration configuration)

        {

            _connectionString = configuration.GetConnectionString("ConncetionStr");

            _connectionHanaString = configuration.GetConnectionString("HanaCon");

            _dbType = configuration.GetValue<string>("DbType"); // ✅ FIXED

        }

        public IDbConnection CreateConnection()

        {

            IDbConnection dbConnection = null;

            if (!string.IsNullOrWhiteSpace(_dbType))

            {

                if (_dbType == "SQL")

                {

                    dbConnection = new SqlConnection(_connectionString);

                }

                else

                {

                    dbConnection = new OdbcConnection(_connectionHanaString);

                }

            }

            return dbConnection;

        }

        //public IDbConnection CreateConnection()

        //{

        //    if (string.Equals(_dbType, "SQL", StringComparison.OrdinalIgnoreCase))

        //    {

        //        return new SqlConnection(_connectionString);

        //    }

        //    else if (string.Equals(_dbType, "Hana", StringComparison.OrdinalIgnoreCase))

        //    {

        //        return new OdbcConnection(_connectionHanaString);

        //    }

        //    else

        //    {

        //        throw new Exception("Invalid DbType in appsettings");

        //    }

        //}

    }

}
