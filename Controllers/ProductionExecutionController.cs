using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAPI.Data;
using System.Data;
using TTSteelWebAPI.Model;
using static TTSteelWebAPI.Model.ProductionExceutionPost;
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
                d.""U_Grade"",
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
                // ================= DocEntry =================
                var docEntry = await conn.ExecuteScalarAsync<int>(
                    @"SELECT IFNULL(MAX(""DocEntry""),0)+1 FROM ""@CCO_TRNS_PRDEXE_HD""",
                    transaction: trans
                );

                payload.DocEntry = docEntry;

                // ================= HEADER INSERT =================
                var headerQuery = $@"
INSERT INTO ""@CCO_TRNS_PRDEXE_HD""
(
    ""DocEntry"", ""DocNum"", ""Series"", ""Object"",
    ""Status"", ""CreateDate"", ""CreateTime"",
    ""U_DocEntry"", ""U_Manual"", ""U_ExecRmks"", ""U_Source"",
    ""U_WhsCode"", ""U_FGWhs"", ""U_SchId"", ""U_SchNo"",
    ""U_SchDt"", ""U_ProdType"", ""U_UnitCode"", ""U_ProdDt"",
    ""U_SchSts"", ""U_ProcCode"", ""U_OpID"", ""U_Operator"",
    ""U_StartHrs"", ""U_EndHrs""
)
VALUES
(
    {payload.DocEntry},
    '{payload.DocNum}',
    '{payload.Series}',
    '{payload.Object}',
    '{payload.Status}',
    '{payload.CreateDate:yyyy-MM-dd}',
    '{payload.CreateTime}',
    '{payload.U_DocEntry ?? ""}',
    '{payload.U_Manual ?? ""}',
    '{payload.U_ExecRmks ?? ""}',
    '{payload.U_Source ?? ""}',
    '{payload.U_WhsCode ?? ""}',
    '{payload.U_FGWhs ?? ""}',
    '{payload.U_SchId ?? ""}',
    '{payload.U_SchNo ?? ""}',
    '{payload.U_SchDt:yyyy-MM-dd}',
    '{payload.U_ProdType ?? ""}',
    '{payload.U_UnitCode ?? ""}',
    '{payload.U_ProdDt:yyyy-MM-dd}',
    '{payload.U_SchSts ?? ""}',
    '{payload.U_ProcCode ?? ""}',
    '{payload.U_OpID ?? ""}',
    '{payload.U_Operator ?? ""}',
    '{payload.U_StartHrs}',
    '{payload.U_EndHrs}'
)";

                await conn.ExecuteAsync(headerQuery, transaction: trans);

                // ================= C1 INSERT =================
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
    ""DocEntry"", ""LineId"", ""U_WONo"", ""U_WOId"",
    ""U_ItemCode"", ""U_IPLn"", ""U_IPItem"", ""U_IPBatch"",
    ""U_SchQty"", ""U_UOM"", ""U_WhseCode"",
    ""U_Thick"", ""U_Width"", ""U_Length"",
    ""U_Form"", ""U_Type"", ""U_Grade""
)
VALUES
(
    {item.DocEntry},
    {item.LineId},
    '{item.U_WONo ?? ""}',
    '{item.U_WOId ?? ""}',
    '{item.U_ItemCode ?? ""}',
    '{item.U_IPLn ?? ""}',
    '{item.U_IPItem ?? ""}',
    '{item.U_IPBatch ?? ""}',
    '{item.U_SchQty}',
    '{item.U_UOM ?? ""}',
    '{item.U_WhseCode ?? ""}',
    '{item.U_Thick}',
    '{item.U_Width}',
    '{item.U_Length}',
    '{item.U_Form ?? ""}',
    '{item.U_Type ?? ""}',
    '{item.U_Grade ?? ""}'
)";

                        await conn.ExecuteAsync(c1Query, transaction: trans);
                    }
                }

                // ================= C2 INSERT =================
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
    ""DocEntry"", ""LineId"", ""U_WONo"", ""U_WOId"",
    ""U_OPItem"", ""U_CustCode"", ""U_CustName"",
    ""U_SODN"", ""U_SODE"", ""U_SOLn"",
    ""U_SOSchQty"", ""U_Qty"", ""U_UOM"",
    ""U_Thick"", ""U_Width"", ""U_Length1"",
    ""U_Form"", ""U_Type"", ""U_Grade""
)
VALUES
(
    {item.DocEntry},
    {item.LineId},
    '{item.U_WONo ?? ""}',
    '{item.U_WOId ?? ""}',
    '{item.U_OPItem ?? ""}',
    '{item.U_CustCode ?? ""}',
    '{item.U_CustName ?? ""}',
    '{item.U_SODN ?? ""}',
    '{item.U_SODE}',
    '{item.U_SOLn ?? ""}',
    '{item.U_SOSchQty}',
    '{item.U_Qty}',
    '{item.U_UOM ?? ""}',
    '{item.U_Thick}',
    '{item.U_Width}',
    '{item.U_Length1}',
    '{item.U_Form ?? ""}',
    '{item.U_Type ?? ""}',
    '{item.U_Grade ?? ""}'
)";

                        await conn.ExecuteAsync(c2Query, transaction: trans);
                    }
                }

                // ================= COMMIT =================
                trans.Commit();

                return Ok(new
                {
                    message = "Production Execution Saved Successfully",
                    docEntry = payload.DocEntry
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
                        var batchNum = await conn.ExecuteScalarAsync<string>(
                            $@"CALL ""BatchGeneration_PRDEXE""('{payload.PostDate}', '{payload.MachineCode}')",
                            
                            transaction: trans
                        );

                        // assign batch to item
                        item.U_OPBatch = batchNum;

                        // ================= INSERT =================
                        var query = $@"
INSERT INTO ""@CCO_TRNS_PRDEXE_C3""
(
    ""DocEntry"", ""LineId"",
    ""U_ItemCode"", ""U_IPItem"", ""U_OPItem"",
    ""U_IPBatch"", ""U_OPBatch"",
    ""U_QCSts"", ""U_ExeSts"",
    ""U_MachCode"",
    ""U_StartDate"", ""U_EndDate"",
    ""U_Remark"",
    ""U_Operator""
)
VALUES
(
    {item.DocEntry},
    {item.LineId},
    '{item.U_ItemCode ?? ""}',
    '{item.U_IPItem ?? ""}',
    '{item.U_OPItem ?? ""}',
    '{item.U_IPBatch ?? ""}',
    '{item.U_OPBatch ?? ""}',   -- 🔥 generated batch
    '{item.U_QCSts ?? ""}',
    '{item.U_ExeSts ?? ""}',
    '{payload.MachineCode ?? ""}',
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
    }
}
