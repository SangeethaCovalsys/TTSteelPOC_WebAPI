using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TTSteelWebAPI.Data;
using TTSteelWebAPI.Service;

namespace TTSteelWebAPI.Controllers
{
    [Route("api/")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly SapService _sapService;
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<MasterController> _logger;
        private readonly IMemoryCache _cache;
        public MasterController(SapService sapService, AppDbContext appDbContext, ILogger<MasterController> logger, IMemoryCache cache)
        {
            _sapService = sapService;
            _appDbContext = appDbContext;
            _logger = logger;
            _cache = cache;
        }
        [HttpGet("sapUsers")]
        public async Task<IActionResult> GetSapUsers()
        {
            try
            {
                var data = await _appDbContext.OUSR
                    .OrderBy(x => x.UserId)
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
