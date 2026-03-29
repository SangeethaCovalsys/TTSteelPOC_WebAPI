
using TTSteelWebAPI.Data;
using TTSteelWebAPI.Model.Login;
using TTSteelWebAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using RTools_NTS.Util;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TTSteelWebAPI.Service;

namespace TTSteelWebAPI.Controllers
{
    [Route("cvs/")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly SapService _sapService;
        private readonly AppDbContext _appDbContext;
       private readonly ILogger<MasterController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        public LoginController(SapService sapService, AppDbContext appDbContext, IConfiguration config, ILogger<MasterController> logger, IMemoryCache cache)
        {
            _sapService = sapService;
            _appDbContext = appDbContext;
            _logger = logger;
            _cache = cache;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] loginModel loginModel)
        {
            try
            {
                var result = await _sapService.LoginUserAsync(loginModel);
                //var tokenJwt = GenerateJwtToken(loginModel.UserName, loginModel.Password, loginModel.CompanyDB);
                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during SAP B1 login.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while logging into SAP B1. Please try again later.",
                    Error = ex.Message
                });
            }
        }

        private string GenerateJwtToken(string username, string password, string dbname)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.Name, username),
        new Claim("PW", password),
        new Claim("Database", dbname),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}

