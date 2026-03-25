using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAPI.Data;
using System.Data;
using static TTSteelWebAPI.Model.ProductionExecutionClass;

namespace TTSteelWebAPI.Controllers
{
    [Route("api/")]
    [ApiController]
    public class ProductionExecutionController : ControllerBase
    {
        private readonly DbConectionContext _databaseContext;
        // private readonly ILogger<LoginController> _logger;
        string _dbType = string.Empty;
        string _query = string.Empty;
        IDbConnection dbConnection = null;

        string _CompanyDbName = string.Empty;
        string _location = string.Empty;

        public ProductionExecutionController(DbConectionContext databaseContext, IConfiguration configuration)
        {
            _databaseContext = databaseContext;

            _CompanyDbName = configuration.GetValue<string>("CompanyName");
            _location = configuration.GetValue<string>("FileLocation");
        }
        [HttpGet("GetSlitRewindCutCounts")]
        public async Task<IActionResult> GetUnitCounts()
        {
            try
            {
                using var conn = _databaseContext.CreateConnection();
                conn.Open();

                var query = @"
            SELECT 
                SUM(CASE WHEN ""U_UnitCode"" = 'STL' THEN 1 ELSE 0 END) AS SlittingCount,
                SUM(CASE WHEN ""U_UnitCode"" = 'CTL1' THEN 1 ELSE 0 END) AS CuttingLengthCount,
                SUM(CASE WHEN ""U_UnitCode"" = 'Rewindor' THEN 1 ELSE 0 END) AS RewindingCount
                FROM ""@CCO_TRNS_PRCSCH_HD"" 
             where ""U_Source"" NOT IN ('Toll Work') AND ""U_SchSts"" not in ('Cancelled','Completed','Planned','Completed-old')";

                var result = await conn.QueryFirstOrDefaultAsync(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error fetching unit counts",
                    error = ex.Message
                });
            }
        }
        [HttpGet("GetEachJobOwnCounts")]
        public async Task<IActionResult> GetUnitSourceCounts([FromQuery] string unitCode)
        {
            try
            {
                using var conn = _databaseContext.CreateConnection();
                conn.Open();

                var query = @"
            SELECT 
                SUM(CASE WHEN ""U_Source"" = 'Own' THEN 1 ELSE 0 END) AS OwnCount,
                SUM(CASE WHEN ""U_Source"" = 'Jobwork' THEN 1 ELSE 0 END) AS JobworkCount
            FROM ""@CCO_TRNS_PRCSCH_HD""
            WHERE  ""U_SchSts"" not in ('Cancelled','Completed','Planned','Completed-old') and ""U_UnitCode"" ='" + unitCode + "' ";

                var result = await conn.QueryFirstOrDefaultAsync(query, new
                {
                    UnitCode = unitCode
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error fetching counts",
                    error = ex.Message
                });
            }
        }

        
        [HttpGet("GetSchduleOfJobOwn")]
        public async Task<IActionResult> GetScheduleDetails([FromQuery] string unitCode, [FromQuery] string source)
        {
            try
            {
                using var conn = _databaseContext.CreateConnection();
                conn.Open();

                var query = $@"
            SELECT DISTINCT  
                a.""U_SchNo"",
                b.""U_CustName"",
                b.""U_CustCode"",
                a.""U_TIPSQty"",
                d.""U_CoilNo"" 
            FROM ""@CCO_TRNS_PRCSCH_HD"" a  
            INNER JOIN ""@CCO_TRNS_PRCSCH_C2"" b ON a.""DocEntry"" = b.""DocEntry""
            INNER JOIN ""@CCO_TRNS_PRCSCH_C1"" c ON c.""DocEntry"" = b.""DocEntry""
            INNER JOIN OBTN d ON d.""DistNumber"" = c.""U_IPBatch"" 
                            AND d.""ItemCode"" = c.""U_ItemCode""
            WHERE a.""U_SchSts"" NOT IN ('Cancelled','Completed','Planned','Completed-old') 
              AND a.""U_Source"" = '{source}' AND a.""U_UnitCode"" = '{unitCode}' ";// Source code pass Jobwork, Own and Unit Pass STL,CTL1,Rewindor 

                var result = await conn.QueryAsync<ScheduleDto>(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error fetching schedule details",
                    error = ex.Message
                });
            }
        }
        [HttpGet("GetScheduleSizeCount")]
        public async Task<IActionResult> GetScheduleSizeCount([FromQuery]string schNo)
        {
            try
            {
                using var conn = _databaseContext.CreateConnection();
                conn.Open();

                var query = $@" SELECT COUNT(*) AS ""TotalCount""
FROM (
    SELECT DISTINCT 
        wh.""U_IPLn"",
        COALESCE(wh.""U_Roll"", 'N') AS ""U_Roll"",
        sc1.""U_ItemCode"" AS ""U_IPItem"",
        wc1.""U_IPBatch"",
        wc1.""U_MTS"",
        wc1.""U_WIP"",
        wc1.""U_OPType"",
        wc1.""U_OPItem"",
        OITM.""U_SpecNo"" AS ""EqSpec"",
        wc1.""U_Type"",
        wc1.""U_Grade"",
        wc1.""U_Form"",
        wc1.""U_Thick"",
        wc1.""U_Width"",
        wc1.""U_Length1"",
        wc1.""U_Length2"",
        wc1.""U_Pitch"",
        wc1.""U_Pcs"",
        wc1.""U_SchPcs"",
        wc1.""U_Pkts"",
        wc1.""U_PcPerPkt"",
        wc1.""U_UnitWt"",
        wc1.""U_Qty"",
        wc1.""U_SchQty"",
        wc1.""U_UOM"",
        wc1.""U_PassNum"",
        wc1.""U_PassLen"",
        wc1.""U_PassUOM"",
        wc1.""U_SplInstn"",
        wc1.""U_MinPkWt"",
        wc1.""U_MaxPkWt"",
        wc1.""U_Surface"",
        wc1.""U_Coating"",
        wc1.""U_Oiling"",
        wc1.""U_Edge"",
        wc2.""LineId"" AS ""AssgnLn"",
        wc2.""U_OPLn"" AS ""AssgnOPLn"",
        wc2.""U_SOLn"" AS ""AssgnSOLn"",
        wc2.""U_SODE"",
        wc2.""U_SODN"",
        wc1.""U_QDCDE"",
        wc1.""U_QDCNo"",
        wc1.""U_QDCObj"",
        wc1.""U_CustCode"",
        wc1.""U_CustName"",
        wc1.""U_WhseCode"",
        wc1.""U_VPDE"",
        wc1.""U_VPPrcLn"",
        COALESCE(A.""U_SeqCode"", '0') AS ""U_SeqCode"",
        CAST(wc1.""U_Rmks"" AS NVARCHAR) AS ""U_Rmks""
    FROM ""@CCO_TRNS_WRKORD_HD"" wh
    LEFT JOIN ""@CCO_TRNS_WRKORD_C1"" wc1 ON wh.""DocEntry"" = wc1.""DocEntry""
    LEFT JOIN ""@CCO_TRNS_WRKORD_C2"" wc2 ON wh.""DocEntry"" = wc2.""DocEntry"" 
        AND wc1.""LineId"" = wc2.""U_OPLn""
    LEFT JOIN ""@CCO_TRNS_PRCSCH_C1"" sc1 
        ON sc1.""DocEntry"" = wh.""U_SchId"" 
        AND sc1.""LineId"" = wh.""U_IPLn""
    LEFT JOIN (
        SELECT ""U_SeqCode"", T0.""U_WOID"", T0.""U_WOLine"" 
        FROM ""@CCO_TRNS_SCHREL_C0"" T0
    ) A ON A.""U_WOID"" = wh.""DocEntry"" 
       AND A.""U_WOLine"" = wc1.""LineId""
    INNER JOIN OITM ON OITM.""ItemCode"" = sc1.""U_ItemCode""
    WHERE wh.""U_SchNo"" = '{schNo}'
      AND wc1.""U_VPDE"" IS NOT NULL
    ORDER BY wc1.""U_Width""
) T; ";

                var result = await conn.QueryFirstOrDefaultAsync<CountDto>(query, new
                {
                    SchNo = schNo
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error fetching work order count",
                    error = ex.Message
                });
            }
        }
        [HttpGet("GetJobBatchDetails")]
        public async Task<IActionResult> GetJobBatchDetails([FromQuery]string schNo)
        {
            try
            {
                using var conn = _databaseContext.CreateConnection();
                conn.Open();

                var query = $@"
            SELECT DISTINCT 
                T0.""U_JBName"",
                a.""U_SchNo"",
                b.""U_SchQty"",
                T0.""U_CoilNo"",
                T0.""U_Grade"",
                T0.""U_Thick"",
                T0.""U_Width""
            FROM ""@CCO_TRNS_PRCSCH_HD"" a
            INNER JOIN ""@CCO_TRNS_PRCSCH_C1"" b 
                ON a.""DocEntry"" = b.""DocEntry""
            INNER JOIN ""@CCO_TRNS_WRKORD_HD"" c 
                ON a.""DocEntry"" = c.""U_SchId""
                AND a.""U_SchNo"" = c.""U_SchNo""
                AND b.""LineId"" = c.""U_IPLn""
                AND b.""U_IPBatch"" = c.""U_IPBatch""
            INNER JOIN OIBT T0 
                ON T0.""BatchNum"" = c.""U_IPBatch""
                AND T0.""ItemCode"" = c.""U_IPItem""

            WHERE a.""U_SchNo"" = '{schNo}'

            AND c.""DocEntry"" NOT IN (
                SELECT b.""U_WOId""
                FROM ""@CCO_TRNS_PRDEXE_HD"" peh
                INNER JOIN ""@CCO_TRNS_PRDEXE_C1"" b 
                    ON peh.""DocEntry"" = b.""DocEntry""
                INNER JOIN ""@CCO_TRNS_WRKORD_HD"" c2 
                    ON b.""U_WOId"" = c2.""DocEntry""
                WHERE b.""U_Select"" = 'Y'
                  AND b.""U_WOId"" IS NOT NULL
            )";

                var result = await conn.QueryAsync<JobBatchDto>(query, new
                {
                    SchNo = schNo
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error fetching job batch details",
                    error = ex.Message
                });
            }
        }
        [HttpGet("GetScheduleSizeDetails")]
      
        public async Task<IActionResult> GetScheduleSizeDetails([FromQuery] string schNo)
        {
            try
            {
                using var conn = _databaseContext.CreateConnection();
                conn.Open();

                var query = $@"
SELECT DISTINCT 
    wh.""U_IPLn"",
    COALESCE(wh.""U_Roll"", 'N') AS ""U_Roll"",
    sc1.""U_ItemCode"" AS ""U_IPItem"",
    wc1.""U_IPBatch"",
    wc1.""U_MTS"",
    wc1.""U_WIP"",
    wc1.""U_OPType"",
    wc1.""U_OPItem"",
    OITM.""U_SpecNo"" AS ""EqSpec"",
    wc1.""U_Type"",
    wc1.""U_Grade"",
    wc1.""U_Form"",
    wc1.""U_Thick"",
    wc1.""U_Width"",
    wc1.""U_Length1"",
    wc1.""U_Length2"",
    wc1.""U_Pitch"",
    wc1.""U_Pcs"",
    wc1.""U_SchPcs"",
    wc1.""U_Pkts"",
    wc1.""U_PcPerPkt"",
    wc1.""U_UnitWt"",
    wc1.""U_Qty"",
    wc1.""U_SchQty"",
    wc1.""U_UOM"",
    wc1.""U_PassNum"",
    wc1.""U_PassLen"",
    wc1.""U_PassUOM"",
    wc1.""U_SplInstn"",
    wc1.""U_MinPkWt"",
    wc1.""U_MaxPkWt"",
    wc1.""U_Surface"",
    wc1.""U_Coating"",
    wc1.""U_Oiling"",
    wc1.""U_Edge"",
    wc2.""LineId"" AS ""AssgnLn"",
    wc2.""U_OPLn"" AS ""AssgnOPLn"",
    wc2.""U_SOLn"" AS ""AssgnSOLn"",
    wc2.""U_SODE"",
    wc2.""U_SODN"",
    wc1.""U_QDCDE"",
    wc1.""U_QDCNo"",
    wc1.""U_QDCObj"",
    wc1.""U_CustCode"",
    wc1.""U_CustName"",
    wc1.""U_WhseCode"",
    wc1.""U_VPDE"",
    wc1.""U_VPPrcLn"",
    COALESCE(A.""U_SeqCode"", '0') AS ""U_SeqCode"",
    CAST(wc1.""U_Rmks"" AS NVARCHAR) AS ""U_Rmks""
FROM ""@CCO_TRNS_WRKORD_HD"" wh
LEFT JOIN ""@CCO_TRNS_WRKORD_C1"" wc1 
    ON wh.""DocEntry"" = wc1.""DocEntry""
LEFT JOIN ""@CCO_TRNS_WRKORD_C2"" wc2 
    ON wh.""DocEntry"" = wc2.""DocEntry"" 
    AND wc1.""LineId"" = wc2.""U_OPLn""
LEFT JOIN ""@CCO_TRNS_PRCSCH_C1"" sc1 
    ON sc1.""DocEntry"" = wh.""U_SchId"" 
    AND sc1.""LineId"" = wh.""U_IPLn""
LEFT OUTER JOIN (
    SELECT ""U_SeqCode"", T0.""U_WOID"", T0.""U_WOLine"" 
    FROM ""@CCO_TRNS_SCHREL_C0"" T0
) A 
    ON A.""U_WOID"" = wh.""DocEntry"" 
    AND A.""U_WOLine"" = wc1.""LineId""
INNER JOIN OITM 
    ON OITM.""ItemCode"" = sc1.""U_ItemCode""
WHERE wh.""U_SchNo"" = '{schNo}'
  AND wc1.""U_VPDE"" IS NOT NULL
ORDER BY wc1.""U_Width""";

                var result = await conn.QueryAsync<ScheduleSizeDto>(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error fetching schedule size details",
                    error = ex.Message
                });
            }
        }

    }
}
