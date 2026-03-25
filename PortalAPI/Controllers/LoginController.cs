using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using PortalAPI.Data;
using static Dapper.SqlMapper;
using System.Data;
using System.IO;
using System.Reflection;
using System;
using PortalAPI.Model;

namespace PortalAPI.Controllers
{
    [Route("api/login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly DbConectionContext _databaseContext;
        private readonly ILogger<LoginController> _logger;
        string _dbType = string.Empty;
        string _query = string.Empty;
        
        public LoginController(DbConectionContext databaseContext, ILogger<LoginController> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<UserModal> Login()
        {
            var t = new List<UserModal>();
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    dbConnection.Open();
                    string query = "SELECT * FROM OEMP";
                    t = (List<UserModal>)dbConnection.Query<UserModal>(query);
                }
            }catch (Exception ex)
            {
                _logger.LogError("Login user list Ex:", ex.Message);
            }
            return t;
        }

        [HttpPost]
        public IActionResult LoginUser([FromBody] UserModal userModal)
        {
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    dbConnection.Open();
                    if (_dbType == "SQL")
                    {
                        _query = "exec SP_SupplierDetails 'SupplierDetails',''";
                    }
                    else
                    {
                        _query = "select \"Code\" as \"UserID\",\"U_User_Name\" as \"UserName\",\"U_COM_Name\" as \"CompanyName\", \"U_E_Mail_ID\" as \"EmailID\",\"U_Status\" as \"IsActive\",\"U_Password\" as\"Password\" from \"@COV_USER\" where \"U_User_Name\" ='"+ userModal.UserName+ "' and \"U_Password\"='"+ userModal .Password+ "' ";
                    }

                    dbConnection.Query<UserModal>(_query);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Login validation Ex:", ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
            return Ok(new { StatusCode = StatusCode(200), Message = "Login Successfully" });
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] UserModal userModal)
        {
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    dbConnection.Open();
                    string insertQuery = "INSERT INTO OEMP (Name, Details) VALUES (@Name, @Details)";
                    dbConnection.Execute(insertQuery, userModal);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Login user creation Ex:", ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
            return Ok(new{ StatusCode = StatusCode(200),Message="Created Successfully" });
        }

    }
}
