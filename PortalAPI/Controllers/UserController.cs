using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PortalAPI.Data;
using PortalAPI.Model;
using System;
using System.Data;
using System.Data.Odbc;
using System.Net.Mail;
using System.Net;
using System.Net.Http;

namespace PortalAPI.Controllers
{
    [Route("api/Users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DbConectionContext _databaseContext;
        private readonly ILogger<LoginController> _logger;
        string _dbType = string.Empty;
        string _query = string.Empty;
        string _kanbanDbName = string.Empty;
        IDbConnection dbConnection = null;
        string errMsg = string.Empty;
        int statusCode = 0;
        int newUserId;
        public UserController(DbConectionContext dbConectionContext, IConfiguration configuration, ILogger<LoginController> logger)
        {
            _databaseContext = dbConectionContext;
            _dbType = configuration.GetValue<string>("dbType");
            _kanbanDbName = configuration.GetValue<string>("KanbanPortalName");
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<UserModal> User()
        {
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    dbConnection.Open();
                    if (_dbType == "SQL")
                    {
                        _query = "exec SP_UserCURD 'GetAllUser','','','','','',''";
                    }
                    else
                    {
                        _query = "CALL COV_Kanban_SP_UserMasterCRUD('GetAllUserMaster', 0,'','','','','','','')";
                    }

                    return dbConnection.Query<UserModal>(_query);
                }
            }
            
            catch (Exception ex)
            {
                _logger.LogError("User GetUser Ex:", ex.Message);
                return Enumerable.Empty<UserModal>();//context.Response.WriteAsync("Internal Server Error");
            }
        }


 
        [HttpGet("Edit")]
        public UserModal User(int userID)
        {
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    dbConnection.Open();
                    if (_dbType == "SQL")
                    {
                        _query = "exec SP_UserCURD 'GetAllUser','','','','','',''";
                    }
                    else
                    {
                        _query = "CALL COV_Kanban_SP_UserMasterCRUD('EditSingleUser',  " + userID + ",'','','','','','','')";

                    }

                    return dbConnection.QueryFirstOrDefault<UserModal>(_query);
                }
            }catch(Exception ex)
            {
                _logger.LogError("User UserEdit Ex:", ex.Message);
                return new UserModal();
            }
        }

        [HttpPost]
        [Route("POST")]
        public IActionResult CreateUser([FromBody] UserModal userModal)
        {
           
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    
                    dbConnection.Open();
                   _query = "call COV_Kanban_SP_UserMasterCRUD('InsertUser',0,'" + userModal.UserName + "','" + userModal.CompanyName + "','" + userModal.EmailID + "','" + userModal.Password + "','Y','" + userModal.RoleName + "','"+userModal.DepartmentName+"')";
                    var _result = dbConnection.Execute(_query);
                    if (_result == 1)
                    {
                        // Fetch newly created UserID
                        string getUserIdQuery = "SELECT \"USERID\" FROM \"COV_Kanban_OUSR\" WHERE \"USERNAME\" ='" + userModal.UserName + "' ORDER BY \"USERID\" DESC LIMIT 1";

                        int? fetchedUserId = dbConnection.QueryFirstOrDefault<int?>(getUserIdQuery, new { UserName = userModal.UserName });

                        // Assign fetched ID if not null
                        if (fetchedUserId.HasValue)
                        {
                            newUserId = fetchedUserId.Value;
                        }

                        errMsg = "User created successfully";
                        statusCode = 200;
                    }
                    else
                    {
                        errMsg = "User creation failed.";
                        statusCode = 400;
                    }
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                statusCode = 400;
                _logger.LogError("User CreateUser Ex:", ex.Message);
                return BadRequest(errMsg);
            }
            return Ok(new { StatusCode = statusCode, Message = errMsg, UserID = newUserId, errorName = "CreateUser()\n " + _query });
        }

        [HttpPost]
        [Route("PUT")]
        public IActionResult UpdateUser([FromBody] UserModal userModal)
        {
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    dbConnection.Open();
                    _query = "call COV_Kanban_SP_UserMasterCRUD ('UpdateUser','" + userModal.UserID + "','" + userModal.UserName + "','" + userModal.CompanyName + "','" + userModal.EmailID + "','" + userModal.Password + "','"+userModal.Status+"','"+userModal.RoleName+ "','" + userModal.DepartmentName + "')";
                    var _result = dbConnection.Execute(_query);
                    if (_result == 1)
                    {
                        errMsg = "User updated successfully";
                        statusCode = 204;
                    }
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                statusCode = 400;
                _logger.LogError("User UpdateUser Ex:", ex.Message);
                return BadRequest(errMsg);
            }
            return Ok(new { StatusCode = statusCode, Message = errMsg,UserID=userModal.UserID, errorName = "UpdateUser() \n" + _query });
        }

        [HttpPost]
        [Route("Delete")]
        public IActionResult DeleteUser([FromQuery] int id)
        {
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    dbConnection.Open();
                    _query = "Update \"COV_Kanban_OUSR\" set STATUS='N' where USERID=" + id + " ";
                    var _result = dbConnection.Execute(_query);
                    if (_result == 1)
                    {
                        errMsg = "User deleted successfully";
                        statusCode = 203;
                    }
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                statusCode = 400;
                _logger.LogError("User DeleteUser Ex:", ex.Message);
                return BadRequest(errMsg);
            }
            return Ok(new { StatusCode = statusCode, Message = errMsg,UserID=id, errorName = "DeleteUser() \n" + _query });
        }

        #region Role Master
        [HttpGet]
        [Route("Role")]
        public IEnumerable<Role> role()
        {
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    dbConnection.Open();
                    if (_dbType == "SQL")
                    {
                        _query = "exec SP_UserCURD 'GetAllUser','','','','','',''";
                    }
                    else
                    {
                        _query = "select ROLEID,ROLENAME FROM \"COV_Kanban_OROL\"";

                    }

                    return dbConnection.Query<Role>(_query);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("User Role Ex:", ex.Message);
                return Enumerable.Empty<Role>();//context.Response.WriteAsync("Internal Server Error");
            }


        }
        #endregion


        #region Depoartmnet Master
        [HttpGet]
        [Route("Department")]
        public IEnumerable<Department> Department()
        {
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    dbConnection.Open();
                    if (_dbType == "SQL")
                    {
                        _query = "exec SP_UserCURD 'GetAllUser','','','','','',''";
                    }
                    else
                    {
                        _query = "select \"DepartmentID\",\"DepartmentName\" FROM \"COV_Kanban_ODEP\"";

                    }

                    return dbConnection.Query<Department>(_query);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("User Department Ex:", ex.Message);
                return Enumerable.Empty<Department>();//context.Response.WriteAsync("Internal Server Error");
            }
        }
        #endregion

        #region Sending a mail while new user creation
        [HttpPost("send-user-credentials")]
        public IActionResult SendCredentialsToNewUser([FromBody] EmailPayload emailPayload)
        {
            if (emailPayload == null || string.IsNullOrEmpty(emailPayload.Email) || string.IsNullOrEmpty(emailPayload.Password))
            {
                Console.WriteLine("Email or password is missing in the payload.");
                return BadRequest(new { statusCode = 400, message = "Email or password is missing." });
            }

            try
            {
                using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.Credentials = new NetworkCredential("kristydurga@gmail.com", "lncx wzvc vshz qnfv");
                    client.EnableSsl = true;

                    string subject = "Welcome to Covalsys - Your Account Details";
                    string body = $@"
<html>
<body>
<p>Dear {(string.IsNullOrEmpty(emailPayload.UserName) ? "User" : emailPayload.UserName)},</p>
<p>Your Covalsys account has been created successfully.</p>
<p><strong>Email:</strong> {emailPayload.Email}<br/>
<strong>Password:</strong> {emailPayload.Password}</p>
<p>Please log with the EmailId and Password.</p>
<p>Regards,<br/>Covalsys Team</p>
</body>
</html>";

                    MailMessage mail = new MailMessage
                    {
                        From = new MailAddress("kristydurga@gmail.com", "Covalsys"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mail.To.Add(new MailAddress(emailPayload.Email));
                    client.Send(mail);

                    Console.WriteLine($"Account credentials sent to {emailPayload.Email}");

                    return Ok(new { statusCode = 200, message = "Email sent successfully." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email to {emailPayload.Email}: {ex.Message}");
                _logger.LogError("User SendCredentialsToNewUser Ex:", ex.Message);
                return StatusCode(500, new { statusCode = 500, message = "Failed to send email." });
            }
        }
        #endregion
    }
}
