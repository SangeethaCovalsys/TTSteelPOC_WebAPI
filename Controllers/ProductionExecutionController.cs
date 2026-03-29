using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Data;
using TTSteelWebAPI.Data;
using TTSteelWebAPI.Model;
using TTSteelWebAPI.Service;
using static TTSteelWebAPI.Model.ProductionExceutionPost;
using static TTSteelWebAPI.Model.ProductionExecutionClass;

namespace TTSteelWebAPI.Controllers
{
    [Route("api/")]
    [ApiController]
    public class ProductionExecutionController : ControllerBase
    {
        private readonly DbConectionContext _databaseContext;
        private readonly SapService _sapService;
        // private readonly ILogger<LoginController> _logger;
        string _dbType = string.Empty;
        string _query = string.Empty;
        IDbConnection dbConnection = null;

        string _CompanyDbName = string.Empty;
        string _location = string.Empty;

        public ProductionExecutionController(DbConectionContext databaseContext, IConfiguration configuration,SapService sapService)
        {
            _databaseContext = databaseContext;
            _sapService = sapService;
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
    d.""U_Grade"",
    a.""U_TIPSQty"",
    d.""U_CoilNo"" 
FROM ""@CCO_TRNS_PRCSCH_HD"" a  
INNER JOIN ""@CCO_TRNS_PRCSCH_C2"" b ON a.""DocEntry"" = b.""DocEntry""
INNER JOIN ""@CCO_TRNS_PRCSCH_C1"" c ON c.""DocEntry"" = b.""DocEntry""
INNER JOIN OBTN d ON d.""DistNumber"" = c.""U_IPBatch"" 
                AND d.""ItemCode"" = c.""U_ItemCode""
WHERE 
    a.""U_SchSts"" NOT IN ('Cancelled','Completed','Planned','Completed-old') 
    AND a.""U_Source"" = '{source}' 
    AND a.""U_UnitCode"" = '{unitCode}'
ORDER BY a.""U_SchNo"" DESC
";

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
Select Distinct 
    C.""U_IPBatch"",
    C.""U_IPLn"",
    b.""U_ItemCode"",
    b.""U_Level"",
    b.""U_SchPcs"",
    b.""U_SchQty"",
    b.""U_Pkts"",
    b.""U_PcPerPkt"",
    b.""U_UOM"",
    b.""U_RcptDate"",
    b.""U_TrimWid"",
    b.""U_CLen"",
    b.""U_CLenUOM"",
    b.""U_WhseCode"",
    b.""U_MillCode"",
    b.""U_MillName"",
    T0.""U_JBCode"",
    b.""U_MaxYield"",
    b.""U_Surface"",
    b.""U_Coating"",
    b.""U_Edge"",
    b.""U_Oiling"",
    T0.""U_JBName"",
    a.""U_SchNo"",
    b.""U_SchQty"",
    T0.""U_CoilNo"",
    T0.""U_Grade"",
    T0.""U_Thick"",
    T0.""U_Width"",
    T0.""U_Form"",
    T0.""U_Grade"",
    T0.""U_Type"",
    c.""DocNum"",
    CAST('S' AS VARCHAR(50)) ""BarcdSts"",
    CAST('Open' AS VARCHAR(50)) ""Sts""

from ""@CCO_TRNS_PRCSCH_HD"" a,
     ""@CCO_TRNS_PRCSCH_C1"" b,
     ""@CCO_TRNS_WRKORD_HD"" c

INNER JOIN OIBT T0 
    on T0.""BatchNum"" = C.""U_IPBatch"" 
   AND T0.""ItemCode"" = c.""U_IPItem""

Where 
    a.""DocEntry"" = b.""DocEntry"" 
and A.""DocEntry"" = c.""U_SchId"" 
and a.""U_SchNo"" = C.""U_SchNo"" 
and b.""LineId"" = c.""U_IPLn"" 
and b.""U_IPBatch"" = c.""U_IPBatch"" 
and a.""U_SchNo"" = '{schNo}'  

AND C.""DocEntry"" not in (
    Select B.""U_WOId"" 
    from ""@CCO_TRNS_PRDEXE_HD"" peh,
         ""@CCO_TRNS_PRDEXE_C1"" b,
         ""@CCO_TRNS_WRKORD_HD"" c 

    Where a.""DocEntry"" = b.""DocEntry"" 
      and b.""U_WOId"" = c.""DocEntry"" 
      and b.""U_Select"" = 'Y' 
      and B.""U_WOId"" is not null 
      and B.""U_WOId"" IS NOT NULL 
      and a.""U_SchNo"" = '{schNo}'
);";

                var result = await conn.QueryAsync<JobBatchDto>(query);

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
    wh.""DocNum"",
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
        [HttpPost("AddProductionExecution")]
        public async Task<IActionResult> AddProductionExecution([FromBody] CCO_TRNS_PRDEXE_HD payload)
        {
            using var conn = _databaseContext.CreateConnection();
            conn.Open();

            using var trans = conn.BeginTransaction();

            try
            {
                // ================= 1. DocEntry =================
                var docEntry = await conn.ExecuteScalarAsync<int>(
                    @"SELECT IFNULL(MAX(""DocEntry""),0)+1 FROM ""@CCO_TRNS_PRDEXE_HD""",
                    transaction: trans
                );

                payload.DocEntry = docEntry;

                // ================= 2. Series & DocNum =================
                var seriesData = await conn.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT T0.""Series"", T0.""NextNumber"" 
              FROM NNM1 T0 
              INNER JOIN ONNM T1 ON T0.""Series"" = T1.""DfltSeries""  
              WHERE T0.""ObjectCode"" = 'PRDEXE'",
                    transaction: trans
                );
                var FgWhs = await conn.QueryFirstOrDefaultAsync<dynamic>(
                    $@"SELECT  T0.""U_FGWhs"", T0.""U_WhsCode"", T0.""U_Planner"" FROM ""@CCO_TRNS_PRCSCH_HD""  T0 WHERE T0.""U_SchNo"" = '{payload.U_SchNo}'",
                    transaction: trans
                    );
                
                payload.Series = seriesData?.Series;
                payload.DocNum = seriesData?.NextNumber;
                payload.U_FGWhs = FgWhs.U_FGWhs;
                payload.U_WhsCode = FgWhs.U_WhsCode;
                payload.U_Operator = FgWhs.U_Planner;

                // ================= 3. HEADER INSERT =================
                var headerQuery = $@"
INSERT INTO ""@CCO_TRNS_PRDEXE_HD""
(
    ""DocEntry"", ""DocNum"", ""Series"", ""Object"",
    ""Status"", ""CreateDate"", ""CreateTime"",
    ""U_DocEntry"", ""U_Manual"", ""U_ExecRmks"", ""U_Source"",
    ""U_WhsCode"", ""U_FGWhs"", ""U_SchId"", ""U_SchNo"",
    ""U_SchDt"", ""U_ProdType"", ""U_UnitCode"", ""U_ProdDt"",
    ""U_SchSts"", ""U_ProcCode"", ""U_OpID"", ""U_Operator"",
    ""U_StartHrs"", ""U_EndHrs"", ""U_StartDate"", ""U_EndDate"", ""U_ShiftA""
)
VALUES
(
    {payload.DocEntry},
    {payload.DocNum},
    {payload.Series},
    '{payload.Object}',
    '{payload.Status}',
    '{payload.CreateDate:yyyy-MM-dd}',
    {payload.CreateTime},

    '{payload.U_DocEntry ?? ""}',
    '{payload.U_Manual ?? ""}',
    '{payload.U_ExecRmks ?? ""}',
    '{payload.U_Source ?? ""}',

    '{payload.U_WhsCode ?? ""}',
    '{payload.U_FGWhs ?? ""}',
    '{payload.U_SchId ?? ""}',
    '{payload.U_SchNo ?? ""}',

    {(payload.U_SchDt == null
        ? "NULL"
        : $"'{payload.U_SchDt:yyyy-MM-dd HH:mm:ss}'")},

    '{payload.U_ProdType ?? ""}',
    '{payload.U_UnitCode ?? ""}',

    {(payload.U_ProdDt == null
        ? "NULL"
        : $"'{payload.U_ProdDt:yyyy-MM-dd HH:mm:ss}'")},

    '{payload.U_SchSts ?? ""}',
    '{payload.U_ProcCode ?? ""}',
    '{payload.U_OpID ?? ""}',
    '{payload.U_Operator ?? ""}',

    {(payload.U_StartHrs == null ? "NULL" : payload.U_StartHrs.ToString())},
    {(payload.U_EndHrs == null ? "NULL" : payload.U_EndHrs.ToString())},

    {(payload.U_StartDate == null
        ? "NULL"
        : $"'{payload.U_StartDate:yyyy-MM-dd HH:mm:ss}'")},

    {(payload.U_EndDate == null
        ? "NULL"
        : $"'{payload.U_EndDate:yyyy-MM-dd HH:mm:ss}'")},

    '{payload.U_ShiftA ?? ""}'
)";

                await conn.ExecuteAsync(headerQuery, transaction: trans);

                // ================= 4. C1 INSERT =================
                if (payload.CCO_TRNS_PRDEXE_C1 != null)
                {
                    for (int i = 0; i < payload.CCO_TRNS_PRDEXE_C1.Count; i++)
                    {
                        var item = payload.CCO_TRNS_PRDEXE_C1[i];

                        item.DocEntry = payload.DocEntry;
                        item.LineId = i + 1;

                        var c1Query = $@"
INSERT INTO ""@CCO_TRNS_PRDEXE_C1""
(
    ""DocEntry"", ""LineId"", ""Object"",
    ""U_Select"",
    ""U_RcptDate"",
    ""U_CoilNo"",
    ""U_ActlQty"",
    ""U_SchQty"",
    ""U_JBName"",
    ""U_WONo"", ""U_WOId"", ""U_ItemCode"",
    ""U_IPLn"", ""U_IPItem"", ""U_IPBatch"",
    ""U_BarCdSts"", ""U_IPLevel"",
    ""U_SchPcs"", ""U_Pkts"", ""U_PcPerPkt"",
    ""U_UOM"",
    ""U_TrimWid"", ""U_CLen"", ""U_WhseCode"",
    ""U_MillCode"", ""U_MillName"", ""U_JBCode"",
    ""U_Thick"", ""U_Width"", ""U_Length"",
    ""U_Form"", ""U_Type"", ""U_Grade"",
    ""U_MaxYield"",
    ""U_Surface"", ""U_Coating"", ""U_Edge"", ""U_Oiling"",
    ""U_BarScn"",
    ""U_Status""
)
VALUES
(
    {item.DocEntry},
    {item.LineId},
    '{item.Object}',

    '{item.U_Select ?? "Y"}',
    {(item.U_RcptDate == null ? "NULL" : $"'{item.U_RcptDate:yyyy-MM-dd}'")},
    '{item.U_CoilNo ?? ""}',

    {(item.U_ActlQty ?? 0)},
    {(item.U_SchQty ?? 0)},

    '{item.U_JBName ?? ""}',

    '{item.U_WONo ?? ""}',
    '{item.U_WOId ?? ""}',
    '{item.U_ItemCode ?? ""}',

    '{item.U_IPLn ?? ""}',
    '{item.U_IPItem ?? ""}',
    '{item.U_IPBatch ?? ""}',

    '{item.U_BarCdSts ?? ""}',
    '{item.U_IPLevel ?? ""}',

    {(item.U_SchPcs ?? 0)},
    {(item.U_Pkts ?? 0)},
    {(item.U_PcPerPkt ?? 0)},

    '{item.U_UOM ?? ""}',

    {(item.U_TrimWid ?? 0)},
    {(item.U_CLen ?? 0)},
    '{item.U_WhseCode ?? ""}',

    '{item.U_MillCode ?? ""}',
    '{item.U_MillName ?? ""}',
    '{item.U_JBCode ?? ""}',

    {(item.U_Thick ?? 0)},
    {(item.U_Width ?? 0)},
    {(item.U_Length ?? 0)},

    '{item.U_Form ?? ""}',
    '{item.U_Type ?? ""}',
    '{item.U_Grade ?? ""}',

    '{item.U_MaxYield ?? ""}',

    '{item.U_Surface ?? ""}',
    '{item.U_Coating ?? ""}',
    '{item.U_Edge ?? ""}',
    '{item.U_Oiling ?? ""}',

    '{item.U_BarScn ?? "S"}',

    '{item.U_Status ?? ""}'
)";

                        await conn.ExecuteAsync(c1Query, transaction: trans);
                    }
                }

                // ================= 5. C2 INSERT =================
                if (payload.CCO_TRNS_PRDEXE_C2 != null)
                {
                    for (int i = 0; i < payload.CCO_TRNS_PRDEXE_C2.Count; i++)
                    {
                        var item = payload.CCO_TRNS_PRDEXE_C2[i];

                        item.DocEntry = payload.DocEntry;
                        item.LineId = i + 1;

                        var c2Query = $@"
INSERT INTO ""@CCO_TRNS_PRDEXE_C2""
(
    ""DocEntry"", ""LineId"", ""Object"",
    ""U_Select"",
    ""U_WONo"", ""U_WOId"", ""U_Roll"",
    ""U_IPLn"", ""U_IPItem"", ""U_IPBatch"",
    ""U_MTS"", ""U_WIP"",
    ""U_OPType"", ""U_OPItem"",
    ""U_CustCode"", ""U_CustName"",
    ""U_SODN"", ""U_SODE"", ""U_SOLn"",
    ""U_SOSchQty"", ""U_Qty"", ""U_UOM"",
    ""U_Thick"", ""U_Width"", ""U_Length1"", ""U_Length2"",
    ""U_Form"", ""U_Type"", ""U_Grade"",
    ""U_Pitch"", ""U_UnitWt"",
    ""U_Pcs"", ""U_SchPcs"", ""U_Pkts"", ""U_PcPerPkt"",
    ""U_SchQty"", ""U_ActPkt"",
    ""U_PlanOP"", ""U_OpenQty"", ""U_Produced"",
    ""U_PassNum"", ""U_PassLen"", ""U_PassUOM"",
    ""U_SplInstn"",
    ""U_MinPkWt"", ""U_MaxPkWt"",
    ""U_Surface"", ""U_Coating"", ""U_Oiling"", ""U_Edge"",
    ""U_VPDE"", ""U_VPPrcLn"", ""U_VPRmks"",
    ""U_Edgebur"", ""U_Pinhole"", ""U_surfscrh"",
    ""U_Status"", ""U_Remark"", ""U_MachCode""
)
VALUES
(
    {item.DocEntry},
    {item.LineId},
    '{item.Object}',

    '{item.U_Select ?? "Y"}',

    '{item.U_WONo ?? ""}',
    '{item.U_WOId ?? ""}',
    '{item.U_Roll ?? ""}',

    '{item.U_IPLn ?? ""}',
    '{item.U_IPItem ?? ""}',
    '{item.U_IPBatch ?? ""}',

    '{item.U_MTS ?? ""}',
    '{item.U_WIP ?? ""}',

    '{item.U_OPType ?? ""}',
    '{item.U_OPItem ?? ""}',

    '{item.U_CustCode ?? ""}',
    '{item.U_CustName ?? ""}',

    '{item.U_SODN ?? ""}',
    {(item.U_SODE ?? 0)},
    '{item.U_SOLn ?? ""}',

    {(item.U_SOSchQty ?? 0)},
    {(item.U_Qty ?? 0)},
    '{item.U_UOM ?? ""}',

    {(item.U_Thick ?? 0)},
    {(item.U_Width ?? 0)},
    {(item.U_Length1 ?? 0)},
    {(item.U_Length2 ?? 0)},

    '{item.U_Form ?? ""}',
    '{item.U_Type ?? ""}',
    '{item.U_Grade ?? ""}',

    {(item.U_Pitch ?? 0)},
    {(item.U_UnitWt ?? 0)},

    {(item.U_Pcs ?? 0)},
    {(item.U_SchPcs ?? 0)},
    {(item.U_Pkts ?? 0)},
    {(item.U_PcPerPkt ?? 0)},

    {(item.U_SchQty ?? 0)},
    {(item.U_ActPkt ?? 0)},

    {(item.U_PlanOP ?? 0)},
    {(item.U_OpenQty ?? 0)},
    {(item.U_Produced ?? 0)},

    '{item.U_PassNum ?? ""}',
    {(item.U_PassLen ?? 0)},
    '{item.U_PassUOM ?? ""}',

    '{item.U_SplInstn ?? ""}',

    {(item.U_MinPkWt ?? 0)},
    {(item.U_MaxPkWt ?? 0)},

    '{item.U_Surface ?? ""}',
    '{item.U_Coating ?? ""}',
    '{item.U_Oiling ?? ""}',
    '{item.U_Edge ?? ""}',

    '{item.U_VPDE ?? ""}',
    '{item.U_VPPrcLn ?? ""}',
    '{item.U_VPRmks ?? ""}',

    '{item.U_Edgebur ?? "N"}',
    '{item.U_Pinhole ?? "N"}',
    '{item.U_surfscrh ?? "N"}',

    '{item.U_Status ?? ""}',
    '{item.U_Remark ?? ""}',
    '{item.U_MachCode ?? ""}'
)";

                        await conn.ExecuteAsync(c2Query, transaction: trans);
                    }
                }

                // ================= 6. UPDATE NextNumber =================
              //  await conn.ExecuteAsync(
              //      @"UPDATE NNM1 
              //SET ""NextNumber"" = ""NextNumber"" + 1 
              //WHERE ""ObjectCode"" = 'PRDEXE' 
              //AND ""Series"" = @Series",
              //      new { Series = payload.Series },
              //      transaction: trans
              //  );

                // ================= 7. COMMIT =================
                trans.Commit();

                return Ok(new
                {
                    message = "Production Execution Saved Successfully",
                    docEntry = payload.DocEntry,
                    docNum = payload.DocNum
                });
            }
            catch (Exception ex)
            {
                trans.Rollback();

                return StatusCode(500, new
                {
                    message = "Error inserting production execution",
                    error = ex.Message
                });
            }
        }

        [HttpPost("AddProductionExecutionC3")]
        public async Task<IActionResult> AddProductionExecutionC3([FromBody] ProductionExecutionC3Post payload)
        {
            using var conn = _databaseContext.CreateConnection();
            conn.Open();

            using var trans = conn.BeginTransaction();

            try
            {
                if (payload.C3List != null)
                {
                    for (int i = 0; i < payload.C3List.Count; i++)
                    {
                        var item = payload.C3List[i];

                        item.DocEntry = payload.DocEntry;
                        item.LineId = i + 1;

                        // ================= CALL SP =================
                        var formattedDate = payload.PostDate.ToString("yyyyMMdd");

                        var batchNum = await conn.ExecuteScalarAsync<string>(
                            $@"CALL ""BatchGeneration_PRDEXE""('{formattedDate}', '{payload.MachineCode}')",
                            transaction: trans,
                             commandTimeout: 120
                        );
                        // assign batch to item
                        item.U_OPBatch = batchNum;

                        // ================= INSERT =================
                        var query = $@"
INSERT INTO ""@CCO_TRNS_PRDEXE_C3""
(
    ""DocEntry"", ""LineId"", ""Object"",
 
    ""U_Select"", ""U_selctlbl"", ""U_PWt"",
    ""U_ItemCode"", ""U_IPItem"", ""U_OPItem"",
    ""U_IPBatch"", ""U_OPBatch"",
 
    ""U_PartNo"", ""U_Seq"", ""U_OPType"", ""U_VPUser"",
    ""U_VPPCust"", ""U_OPPartNoRef"",
 
    ""U_SOPcs"", ""U_OPSchPcs"", ""U_Pkts"", ""U_PcPerPkt"",
    ""U_UnitWt"", ""U_PckgWt"", ""U_GrossWt"", ""U_NetWt"",
    ""U_TheoWt"",
 
    ""U_OBThick1"", ""U_OBThick2"",
    ""U_OBWidth1"", ""U_OBWidth2"",
    ""U_OBLen1"", ""U_OBLen2"",
 
    ""U_IPLevel"", ""U_IPSchPcs"", ""U_IPUOM"",
    ""U_CustCode"", ""U_CustName"",
 
    ""U_MTS"", ""U_OPLn"", ""U_OPLevel"",
    ""U_OPForm"", ""U_OPGrade"",
    ""U_OPThick"", ""U_OPWidth"",
    ""U_Type"", ""U_EqSpec"",
 
    ""U_Length1"", ""U_Length2"",
    ""U_Pitch"",
 
    ""U_PurPrc"", ""U_PrmVal"", ""U_ScrPrc"", ""U_ScrVal"", ""U_UnitPrc"",
 
    ""U_UOM"", ""U_Whse"",
    ""U_FGRef"", ""U_FGWhs"",
    ""U_WIP"",
 
    ""U_PackID"", ""U_GrpNo"",
    ""U_SODN"", ""U_SOLn"", ""U_SODE"",
    ""U_QDCNo"", ""U_QDCDE"", ""U_QDCObj"",
 
    ""U_GIDE"", ""U_GRDE"", ""U_GRRevNo"", ""U_GIRevNo"",
 
    ""U_QCSts"", ""U_ExeSts"",
    ""U_MinPkWt"", ""U_MaxPkWt"",
 
    ""U_Surface"", ""U_Coating"", ""U_Oiling"", ""U_Edge"",
 
    ""U_VPDE"", ""U_VPPrcLn"",
    ""U_MachCode"", ""U_Status"",
 
    ""U_Edgebur"", ""U_OilStain"", ""U_Coilset"",
    ""U_Telescop"", ""U_Scalmark"", ""U_surfscrh"",
    ""U_RustOxd"", ""U_crosbow"", ""U_Pinhole"", ""U_dentgoug"",
 
    ""U_StartDate"", ""U_EndDate"",
    ""U_Remark"", ""U_Operator""
)
VALUES
(
    {item.DocEntry},
    {item.LineId},
    'PRDEXE',
 
    '{item.U_Select ?? "Y"}',
    NULL,
    {(item.U_PWt ?? 0)},
 
    '{item.U_ItemCode ?? ""}',
    '{item.U_IPItem ?? ""}',
    '{item.U_OPItem ?? ""}',
 
    '{item.U_IPBatch ?? ""}',
    '{item.U_OPBatch ?? ""}',
 
    {(item.U_PartNo ?? "")},
    {(item.U_Seq ?? "")},
    '{item.U_OPType ?? ""}',
    '{item.U_VPUser ?? ""}',
 
    '{item.U_VPPCust ?? ""}',
    '{item.U_OPPartNoRef ?? ""}',
 
    {(item.U_SOPcs ?? 0)},
    {(item.U_OPSchPcs ?? 0)},
    {(item.U_Pkts ?? 0)},
    {(item.U_PcPerPkt ?? 0)},
 
    {(item.U_UnitWt ?? 0)},
    {(item.U_PckgWt ?? 0)},
    {(item.U_GrossWt ?? 0)},
    {(item.U_NetWt ?? 0)},
    {(item.U_TheoWt ?? 0)},
 
    {(item.U_OBThick1 ?? 0)},
    {(item.U_OBThick2 ?? 0)},
    {(item.U_OBWidth1 ?? 0)},
    {(item.U_OBWidth2 ?? 0)},
    {(item.U_OBLen1 ?? 0)},
    {(item.U_OBLen2 ?? 0)},
 
    '{item.U_IPLevel ?? ""}',
    {(item.U_IPSchPcs ?? 0)},
    '{item.U_IPUOM ?? ""}',
 
    '{item.U_CustCode ?? ""}',
    '{item.U_CustName ?? ""}',
 
    '{item.U_MTS ?? ""}',
    {(item.U_OPLn ?? "")},
    '{item.U_OPLevel ?? ""}',
 
    '{item.U_OPForm ?? ""}',
    '{item.U_OPGrade ?? ""}',
 
    {(item.U_OPThick ?? 0)},
    {(item.U_OPWidth ?? 0)},
 
    '{item.U_Type ?? ""}',
    '{item.U_EqSpec ?? ""}',
 
    {(item.U_Length1 ?? 0)},
    {(item.U_Length2 ?? 0)},
 
    {(item.U_Pitch ?? 0)},
 
    {(item.U_PurPrc ?? 0)},
    {(item.U_PrmVal ?? 0)},
    {(item.U_ScrPrc ?? 0)},
    {(item.U_ScrVal ?? 0)},
    {(item.U_UnitPrc ?? 0)},
 
    '{item.U_UOM ?? ""}',
    '{item.U_Whse ?? ""}',
 
    '{item.U_FGRef ?? ""}',
    '{item.U_FGWhs ?? ""}',
 
    '{item.U_WIP ?? ""}',
 
    NULL,
    NULL,
 
    '{item.U_SODN ?? ""}',
    '{item.U_SOLn ?? ""}',
    {(item.U_SODE ?? 0)},
 
    '{item.U_QDCNo ?? ""}',
    '{item.U_QDCDE ?? ""}',
    '{item.U_QDCObj ?? ""}',
 
    NULL, NULL, NULL, NULL,
 
    '{item.U_QCSts ?? ""}',
    '{item.U_ExeSts ?? ""}',
 
    {(item.U_MinPkWt ?? 0)},
    {(item.U_MaxPkWt ?? 0)},
 
    '{item.U_Surface ?? ""}',
    '{item.U_Coating ?? ""}',
    '{item.U_Oiling ?? ""}',
    '{item.U_Edge ?? ""}',
 
    '{item.U_VPDE ?? ""}',
    '{item.U_VPPrcLn ?? ""}',
 
    '{payload.MachineCode ?? ""}',
    '{item.U_Status ?? "Open"}',
 
    '{item.U_Edgebur ?? "N"}',
    '{item.U_OilStain ?? "N"}',
    '{item.U_Coilset ?? "N"}',
    '{item.U_Telescop ?? "N"}',
    '{item.U_Scalmark ?? "N"}',
    '{item.U_surfscrh ?? "N"}',
    '{item.U_RustOxd ?? "N"}',
    '{item.U_crosbow ?? "N"}',
    '{item.U_Pinhole ?? "N"}',
    '{item.U_dentgoug ?? "N"}',
 
    '{payload.PostDate:yyyy-MM-dd}',
    '{payload.PostDate:yyyy-MM-dd}',
 
    '{item.U_Remark ?? ""}',
    '{item.U_Operator ?? ""}'
)";

                        await conn.ExecuteAsync(query, transaction: trans);
                    }
                }

                trans.Commit();

                return Ok(new
                {
                    message = "C3 Data Inserted Successfully with Batch",
                    docEntry = payload.DocEntry
                });
            }
            catch (Exception ex)
            {
                trans.Rollback();

                return StatusCode(500, new
                {
                    message = "Error inserting C3 data",
                    error = ex.Message
                });
            }
        }

        [HttpGet("GetProductionExecution")]
        public async Task<IActionResult> GetProductionExecution([FromQuery]int docEntry)
        {
            using var conn = _databaseContext.CreateConnection();
            conn.Open();

            // 1. Retrieve header
            string headerSql = $@"SELECT * FROM ""@CCO_TRNS_PRDEXE_HD"" WHERE ""DocEntry"" = {docEntry}";
            var header = await conn.QueryFirstOrDefaultAsync<CCO_TRNS_PRDEXE_HD>(
                headerSql);

            if (header == null)
                return NotFound($"No production execution found with DocEntry = {docEntry}");

            // 2. Retrieve C1 lines (input materials)
             string c1Sql = $@"SELECT * FROM ""@CCO_TRNS_PRDEXE_C1""  WHERE ""DocEntry"" ={docEntry}  ORDER BY ""LineId""";
            var c1Lines = await conn.QueryAsync<CCO_TRNS_PRDEXE_C1>(
                c1Sql);

            // 3. Retrieve C2 lines (output products / coils)
             string c2Sql = $@" SELECT * FROM ""@CCO_TRNS_PRDEXE_C2""  WHERE ""DocEntry"" = {docEntry}  ORDER BY ""LineId""";
            var c2Lines = await conn.QueryAsync<CCO_TRNS_PRDEXE_C2>(
                c2Sql);

            // 4. Assign child collections
            header.CCO_TRNS_PRDEXE_C1 = c1Lines.ToList();
            header.CCO_TRNS_PRDEXE_C2 = c2Lines.ToList();

            return Ok(header);
        }

        [HttpPost("CreateGoodsIssue")]
        public async Task<IActionResult> CreateGoodsIssue([FromQuery] int docEntry)
        {
            using var conn = _databaseContext.CreateConnection();
            conn.Open();

            using var trans = conn.BeginTransaction();

            try
            {
                // ================= 1. FETCH DATA =================
                var sql = $@"
SELECT 
    t.""DocEntry"",
    t.""Object"",
    t.""U_Source"",
    t.""U_SchNo"",
    t.""U_SchId"",
    t.""U_WhsCode"",
    t.""U_IPItem"" AS ""ItemCode"",
    SUM(IFNULL(t.""U_IPQty"",0)) AS ""Quantity"",
    t.""U_IPBatch"" AS ""BatchNum""
FROM 
(
    SELECT  
        T0.""DocEntry"",
        T0.""Object"",
        T0.""U_Source"",
        T0.""U_SchNo"",
        T0.""U_SchId"",
        T0.""U_WhsCode"",
        T1.""U_ItemCode"" AS ""U_IPItem"",
        SUM(IFNULL(T1.""U_SchQty"",0)) AS ""U_IPQty"",
        T1.""U_IPBatch""
    FROM ""@CCO_TRNS_PRDEXE_HD"" T0
    INNER JOIN ""@CCO_TRNS_PRDEXE_C1"" T1  
        ON T0.""DocEntry"" = T1.""DocEntry""
    WHERE 
        T0.""DocEntry"" = {docEntry}
        AND T1.""U_Select"" = 'Y'
        AND IFNULL(T1.""U_Status"",'Open') = 'Open'
    GROUP BY 
        T0.""DocEntry"",
        T0.""Object"",
        T0.""U_Source"",
        T0.""U_SchNo"",
        T0.""U_SchId"",
        T0.""U_WhsCode"",
        T1.""U_ItemCode"",
        T1.""U_IPBatch""
) t
GROUP BY 
    t.""DocEntry"",
    t.""Object"",
    t.""U_Source"",
    t.""U_SchNo"",
    t.""U_SchId"",
    t.""U_WhsCode"",
    t.""U_IPItem"",
    t.""U_IPBatch"";
";

                var data = (await conn.QueryAsync(sql, transaction: trans)).ToList();

                if (!data.Any())
                    return BadRequest("No data found for Goods Issue");

                // ================= 2. BUILD SAP PAYLOAD =================
                var payload = new SapReceiptOIGN
                {
                    DocDate = DateTime.Now,
                    Comments = $"Auto Goods Issue from PRDEXE - {docEntry}",

                    U_DocType =
    (data.First().U_Source == "Jobwork" || data.First().U_Source == "Job-Work")
        ? "J"
        : (data.First().U_Source == "Own" || data.First().U_Source == "OWN")
            ? "N"
            : "",
                    U_SrcObj = data.First().Object,
                    U_SchNo = data.First().U_SchNo,

                    DocumentLines = data
                        .GroupBy(x => new { x.ItemCode, x.U_WhsCode })
                        .Select(g => new SapReceiptLine
                        {
                            ItemCode = g.Key.ItemCode,
                            WarehouseCode = g.Key.U_WhsCode,
                            Quantity = g.Sum(x => (decimal)x.Quantity),

                            BatchNumbers = g.Select(x => new SapBatch
                            {
                                BatchNumber = x.BatchNum,
                                Quantity = x.Quantity
                            }).ToList()
                        }).ToList()
                };

                // ================= 3. CALL SAP =================
                var sapResult = await _sapService.PostAsync("InventoryGenExits", payload);
                var resultJson = JObject.Parse(sapResult);

                var sapDocEntry = resultJson["DocEntry"]?.Value<int>();

                if (sapDocEntry == null)
                    throw new Exception("SAP Goods Issue failed");

                // ================= 4. UPDATE C3 =================
                var updateSql = $@"
UPDATE ""@CCO_TRNS_PRDEXE_C3"" 
SET ""U_GIDE"" = {sapDocEntry} 
WHERE ""DocEntry"" = {docEntry}";

                await conn.ExecuteAsync(updateSql, transaction: trans);

                // ================= COMMIT =================
                trans.Commit();

                return Ok(new
                {
                    message = "Goods Issue created & C3 updated successfully",
                    sapDocEntry = sapDocEntry
                });
            }
            catch (Exception ex)
            {
                trans.Rollback();

                return StatusCode(500, new
                {
                    message = "Error creating Goods Issue",
                    error = ex.Message
                });
            }
        }
        [HttpPost("CreateGoodsReceipt")]
        public async Task<IActionResult> CreateGoodsReceipt([FromQuery] int docEntry)
        {
            using var conn = _databaseContext.CreateConnection();
            conn.Open();

            using var trans = conn.BeginTransaction();

            try
            {
                // ================= 1. FETCH DATA =================
                var sql = $@"
SELECT 
    t.""DocEntry"",
    t.""Object"",
    t.""U_Source"",
    t.""U_SchNo"",
    t.""U_SchId"",
    t.""U_WhsCode"",
    t.""U_IPItem"" AS ""ItemCode"",
    SUM(IFNULL(t.""U_IPQty"",0)) AS ""Quantity"",
    t.""U_IPBatch"" AS ""BatchNum""
FROM 
(
    SELECT  
        T0.""DocEntry"",
        T0.""Object"",
        T0.""U_Source"",
        T0.""U_SchNo"",
        T0.""U_SchId"",
        T0.""U_WhsCode"",
        T1.""U_ItemCode"" AS ""U_IPItem"",
        SUM(IFNULL(T1.""U_SchQty"",0)) AS ""U_IPQty"",
        T1.""U_IPBatch""
    FROM ""@CCO_TRNS_PRDEXE_HD"" T0
    INNER JOIN ""@CCO_TRNS_PRDEXE_C1"" T1  
        ON T0.""DocEntry"" = T1.""DocEntry""
    WHERE 
        T0.""DocEntry"" = {docEntry}
        AND T1.""U_Select"" = 'Y'
        AND IFNULL(T1.""U_Status"",'Open') = 'Open'
    GROUP BY 
        T0.""DocEntry"",
        T0.""Object"",
        T0.""U_Source"",
        T0.""U_SchNo"",
        T0.""U_SchId"",
        T0.""U_WhsCode"",
        T1.""U_ItemCode"",
        T1.""U_IPBatch""
) t
GROUP BY 
    t.""DocEntry"",
    t.""Object"",
    t.""U_Source"",
    t.""U_SchNo"",
    t.""U_SchId"",
    t.""U_WhsCode"",
    t.""U_IPItem"",
    t.""U_IPBatch"";
";

                var data = (await conn.QueryAsync(sql, transaction: trans)).ToList();

                if (!data.Any())
                    return BadRequest("No data found for Goods Receipt");

                // ================= 2. BUILD PAYLOAD =================
                var payload = new SapReceiptOIGN
                {
                    DocDate = DateTime.Now,
                    Comments = $"Auto GR from PRDEXE - {docEntry}",

                    U_DocType =
    (data.First().U_Source == "Jobwork" || data.First().U_Source == "Job-Work")
        ? "J"
        : (data.First().U_Source == "Own" || data.First().U_Source == "OWN")
            ? "N"
            : "", U_SrcObj = data.First().Object,
                    U_SchNo = data.First().U_SchNo,

                    DocumentLines = data
                        .GroupBy(x => new { x.ItemCode, x.U_WhsCode })
                        .Select(g => new SapReceiptLine
                        {
                            ItemCode = g.Key.ItemCode,
                            WarehouseCode = g.Key.U_WhsCode,
                            Quantity = g.Sum(x => (decimal)x.Quantity),

                            BatchNumbers = g.Select(x => new SapBatch
                            {
                                BatchNumber = x.BatchNum,
                                Quantity = x.Quantity
                            }).ToList()
                        }).ToList()
                };

                // ================= 3. CALL SAP =================
                var sapResult = await _sapService.PostAsync("InventoryGenEntries", payload);
                var resultJson = JObject.Parse(sapResult);

                var sapDocEntry = resultJson["DocEntry"]?.Value<int>();

                if (sapDocEntry == null)
                    throw new Exception("SAP Goods Receipt failed");

                // ================= 4. UPDATE C3 =================
                var updateSql = $@"
UPDATE ""@CCO_TRNS_PRDEXE_C3"" 
SET ""U_GRDE"" = {sapDocEntry} 
WHERE ""DocEntry"" = {docEntry}";

                await conn.ExecuteAsync(updateSql, transaction: trans);

                // ================= COMMIT =================
                trans.Commit();

                return Ok(new
                {
                    message = "Goods Receipt created & C3 updated successfully",
                    sapDocEntry = sapDocEntry
                });
            }
            catch (Exception ex)
            {
                trans.Rollback();

                return StatusCode(500, new
                {
                    message = "Error creating Goods Receipt",
                    error = ex.Message
                });
            }
        }
        [HttpGet("GetDbList")]

        public async Task<IActionResult> GetDbList()

        {

            try

            {

                using var conn = _databaseContext.CreateConnection();

                conn.Open();

                var query = @"

                    SELECT SCHEMA_NAME ""DBNAME""

                    FROM SYS.SCHEMAS

                    WHERE SCHEMA_NAME ='PHI_LIVE_04032026'";

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

        [HttpGet("GetWorkOrderLableDetails")]

        public async Task<IActionResult> GetWorkOrderLableDetails([FromQuery] string? DocEntry)

        {

            try

            {

                using var conn = _databaseContext.CreateConnection();

                conn.Open();

                var query = $@"

                    SELECT T0.""U_SchQty"",T0.""U_CoilNo"",

T1.""U_OBThick1"",T1.""U_OBWidth1"",T1.""U_SOPcs"",T1.""U_Pkts"" 

FROM ""@CCO_TRNS_PRDEXE_C1"" T0 inner join ""@CCO_TRNS_PRDEXE_C3"" T1

on T0.""DocEntry"" = T1.""DocEntry""  Where T0.""DocEntry""='{DocEntry}'";

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
        [HttpGet("GetLast7DaysProduction")]
        public async Task<IActionResult> GetLast7DaysProduction()
        {
            using var conn = _databaseContext.CreateConnection();
            conn.Open();

            try
            {
                var sql = @"
SELECT 
    T0.""U_SchNo""        AS ""SchNo"",
    
   CASE 
    WHEN T0.""U_UnitCode"" LIKE 'STL%' THEN 'Slitting'
    WHEN T0.""U_UnitCode"" LIKE 'CTL%' THEN 'Cut to Length'
    ELSE T0.""U_UnitCode""
END AS ""U_UnitCode"",

    T1.""U_JBName""      AS ""CustName"",
    T1.""U_Grade""       AS ""Grade"",

    SUM(IFNULL(T1.""U_ActlQty"", 0)) AS ""OrderQty"",
    MAX(T0.""CreateDate"") AS ""CreateDate""

FROM ""@CCO_TRNS_PRDEXE_HD"" T0
INNER JOIN ""@CCO_TRNS_PRDEXE_C1"" T1 
    ON T0.""DocEntry"" = T1.""DocEntry""

WHERE 
    T0.""CreateDate"" >= ADD_DAYS(CURRENT_DATE, -7)
    AND T1.""U_Select"" = 'Y'

GROUP BY 
    T0.""U_SchNo"",
    T0.""U_UnitCode"",
    T1.""U_JBName"",
    T1.""U_Grade""

ORDER BY 
    MAX(T0.""CreateDate"") DESC
";

                var result = await conn.QueryAsync(sql);

                return Ok(new
                {
                    message = "Last 7 days production data fetched successfully",
                    count = result.Count(),
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error fetching last 7 days data",
                    error = ex.Message
                });
            }
        }

    }
}
