using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PortalAPI.Data;
using PortalAPI.Model;
using System.Data;
using System.Globalization;
using System.Numerics;

namespace PortalAPI.Controllers
{
    [Route("api/")]
    [ApiController]
    public class WorkFlowController : ControllerBase
    {
        private readonly DbConectionContext _databaseContext;
        private readonly ILogger<LoginController> _logger;
        string _dbType = string.Empty;
        string _query = string.Empty;
        IDbConnection dbConnection = null;
        string errMsg = string.Empty;
        int statusCode = 0;
        public WorkFlowController(DbConectionContext dbConectionContext, IConfiguration configuration, ILogger<LoginController> logger)
        {
            _databaseContext = dbConectionContext;
            _dbType = configuration.GetValue<string>("dbType");
            _logger = logger;
        }

        [HttpGet]
        [Route("Userworkflow")]
        public IActionResult GetUserDetails()
        {
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    dbConnection.Open();

                    // Define your SQL query
                    _query = @"call COV_Kanban_SP_WorkFlowMapping ('User','')";
                    // Execute the query using Dapper
                    var result =  dbConnection.Query<UserModal>(_query);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("User GetUserDetails Ex:", ex.Message);
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("Menu")]
        public IActionResult GetMenuDetails()
        {
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    dbConnection.Open();

                    // Define your SQL query
                    _query = @"call COV_Kanban_SP_WorkFlowMapping ('Menu','')";

                    // Execute the query using Dapper
                    var result = dbConnection.Query<WorkFlow>(_query);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("User GetMenuDetails Ex:", ex.Message);
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("LoadMenu")]
        public IEnumerable<headerMenu> GetMenu([FromQuery] string userID)
        {
            List<headerMenu> _headerMenu=new List<headerMenu>();
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    dbConnection.Open();

                    // Define your SQL query
                    _query = @"call COV_Kanban_SP_WorkFlowMapping ('UserMenu','"+ userID + "')";
                    // Execute the query using Dapper
                     UserMenus _userMeuns =(UserMenus) dbConnection.Query<UserMenus>(_query).FirstOrDefault();
                    if (!string.IsNullOrEmpty(_userMeuns.userID))
                    {
                        string[] _uMenu = _userMeuns.menuID.Split(',');
                        if(_uMenu.Length > 0 )
                        {
                            List<childName> _allChild = new List<childName>();
                            for(int i=0;i< _uMenu.Length;i++)
                            {
                                _query = @"call COV_Kanban_SP_WorkFlowMapping ('ChildMenu','" + _uMenu[i] + "')";
                                childName _childMenu= (childName)dbConnection.Query<childName>(_query).FirstOrDefault();
                                _allChild.Add(_childMenu);
                            }
                            _query = @"call COV_Kanban_SP_WorkFlowMapping ('HeaderMenu','" + userID + "')";
                              _headerMenu = (List<headerMenu>)dbConnection.Query<headerMenu>(_query);
                            if(_headerMenu.Count > 0 )
                            {
                                for(int j=0;j< _headerMenu.Count; j++)
                                {
                                   //var _curMenuChild= _allChild.Where(x => x.headerID == _headerMenu[j].MenuID ).OrderBy(l=>l.headerLineID).ToList();
                                    var _curMenuChild = _allChild
                                    .Where(x => x.headerID == _headerMenu[j].MenuID)  // Filter by headerID
                                    .OrderBy(l => l.headerLineID)                     // Sort by headerLineID
                                    .ToList();                                        // Convert to List

                                    _headerMenu[j].childName= _curMenuChild;
                                }
                            }
                        }
                    }
                    return _headerMenu;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("User GetMenu Ex:", ex.Message);
                return  new List<headerMenu>();
            }
        }
        
        [HttpPost]
        [Route("WorkFlowPost")]
        public IActionResult WorkFlowPostingPlan([FromBody] FlowAdd flowAdd)
        {
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    dbConnection.Open();
                    
                     _query = "Call COV_Kanban_SP_WorkFlowCRUD('InsertWorkFlow',0,'" + flowAdd.RoleID + "','" + flowAdd.MenuID + "'," + flowAdd.UserID + ") ";
                    
                     var _result = dbConnection.Execute(_query);
                    if (_result == 1)
                    {
                        errMsg = "Flow created successfully";
                        statusCode = 200;
                    }
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                statusCode = 400;
                _logger.LogError("User WorkFlowPostingPlan Ex:", ex.Message);
                return BadRequest(errMsg);
            }
            return Ok(new { StatusCode = statusCode, Message = errMsg, errorName = "WorkFlowPostingPlan()\n " + _query });
        }

        [HttpGet("WorkFlowGet")]
        public IEnumerable<GetWorkFlow> WorkFlowGet()
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
                        _query = "call COV_Kanban_SP_WorkFlowCRUD('GetAllWorkFlow',0,'','',0) ";

                    }

                    return dbConnection.Query<GetWorkFlow>(_query);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("User WorkFlowGet Ex:", ex.Message);
                return Enumerable.Empty<GetWorkFlow>();//context.Response.WriteAsync("Internal Server Error");
            }
        }

        [HttpGet("WorkFlowEdit")]
        public FlowAdd WorkFlowEdit(int roleMenuID)
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
                        _query = "Call COV_Kanban_SP_WorkFlowCRUD('EditWorkFlow'," + roleMenuID + ",'','',0)";

                    }

                    return dbConnection.QueryFirstOrDefault<FlowAdd>(_query);
                }
            }catch(Exception ex)
            {
                _logger.LogError("User WorkFlowEdit Ex:", ex.Message);
                return new FlowAdd();
            }
        }

        [HttpPost("WorkFlowUpdate")]
        public IActionResult UpdateWorkFlow([FromBody] FlowAdd roleModal)
        {
            try
            {
                using (IDbConnection dbConnection = _databaseContext.CreateConnection())
                {
                    dbConnection.Open();
                    _query = "call COV_Kanban_SP_WorkFlowCRUD ('UpdateWorkFlow'," + roleModal.RoleMenuID+",'" + roleModal.RoleID + "','" + roleModal.MenuID + "',"+roleModal.UserID+")";
                    var _result = dbConnection.Execute(_query);
                    if (_result == 1)
                    {
                        errMsg = "Flow updated successfully";
                        statusCode = 204;
                    }
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                statusCode = 400;
                _logger.LogError("User UpdateWorkFlow Ex:", ex.Message);
                return BadRequest(errMsg);
            }
            return Ok(new { StatusCode = statusCode, Message = errMsg, errorName = "UpdateWorkFlow() \n" + _query });
        }
    }
}
