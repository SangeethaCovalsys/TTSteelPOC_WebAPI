using Dapper;
using PortalAPI.Model;
using System.Data;
using System.Data.Odbc;

namespace PortalAPI.Data
{
    public class AuditLogClass
    {
        private readonly DbConectionContext _databaseContext;
        IDbConnection dbConnection = null;
        string _CompanyDbName = string.Empty;
        string _strQry=string.Empty;
        public AuditLogClass(DbConectionContext dbConectionContext, IConfiguration configuration) 
        {
            _databaseContext = dbConectionContext;            
            _CompanyDbName = configuration.GetValue<string>("CompanyName");
        }
        public void AuditLogPOST(string _docNum, string _userId,string _userName,string _pageName)
        {
            try
            {
                using(dbConnection=_databaseContext.CreateConnection())
                {
                    dbConnection.Open();
                    _strQry = "CALL COV_Kanban_SP_Auditlog('"+_docNum+"','"+_userId+"','"+_userName+"','"+_pageName+"')";
                    dbConnection.Query<string>(_strQry);
                }

            }
            catch(Exception ex)
            {

            }

        }
    }
}
