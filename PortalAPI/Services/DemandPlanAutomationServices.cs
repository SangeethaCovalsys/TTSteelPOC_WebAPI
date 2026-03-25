using Dapper;
using Newtonsoft.Json;
using PortalAPI.Model;
using System.Data;
using System.Globalization;

namespace PortalAPI.Services
{
    public class DemandPlanAutomationServices
    {
        private readonly string _dbType;
        private readonly ILogger<DemandPlanAutomationServices> _logger;

        public DemandPlanAutomationServices(string dbType, ILogger<DemandPlanAutomationServices> logger)
        {
            _dbType = dbType;
            _logger = logger;
        }

        #region Supplier Plan Automation
        public async Task<List<DemandPlan>> FetchDemandPlansAsync(IDbConnection dbConnection, List<CustomerActivePlans> customerPlans)
        {
            var demandPlans = new List<DemandPlan>();

            foreach (var plan in customerPlans)
            {
                try
                {
                    var headQuery = _dbType == "SQL"
                        ? "exec SP_UserCURD 'GetAllUser','','','','','',''"
                        : $"CALL COV_Kanban_SP_GetPlanDetails('PlanHeadDetails','{plan.planID}')";

                    var demandPlan = await dbConnection.QueryFirstOrDefaultAsync<DemandPlan>(headQuery);

                    if (demandPlan == null) continue;

                    var partQuery = $"CALL COV_Kanban_SP_GetPlanDetails('PlanPart','{demandPlan.planID}')";
                    var partDetails = (await dbConnection.QueryAsync<DemandPlanDetails>(partQuery)).ToList();

                    foreach (var part in partDetails)
                    {
                        string monthQuery = $"CALL COV_Kanban_SP_GetPlanDetails('PlanMonth','{part.partID}')";
                        part.planMonthsDetails = (await dbConnection.QueryAsync<DemandPlanMonthsDetails>(monthQuery)).ToList();

                        string dayQuery = $"CALL COV_Kanban_SP_GetPlanDetails('PlanDay','{part.partID}')";
                        part.daysDetails = (await dbConnection.QueryAsync<DemandPlanDaysDetails>(dayQuery)).ToList();
                    }

                    demandPlan.demandplanDetails = partDetails;
                    demandPlans.Add(demandPlan);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error fetching plan for PlanID {PlanID}: {Message}", plan.planID, ex.Message);
                }
            }

            return demandPlans;
        }

        public async Task<List<SupplierPlanData>> GetSupplierPlanListAsync(IDbConnection dbConnection, string entity, string dbName)
        {
            string query = $"CALL COV_Kanban_SP_GetAutoBOM('GetSupplierList','', '', '', '{entity}', '{dbName}')";

            var suppliers = await dbConnection.QueryAsync<SupplierPlanData>(query);

            return suppliers
                .Where(x => !string.IsNullOrWhiteSpace(x.SupplierCode))
                .GroupBy(x => x.SupplierCode)
                .Select(g => g.First())
                .ToList();
        }
        public async Task<List<AutoSupplierPlanMap>> GetSupplierPlansMappingAsync(IDbConnection dbConnection, List<DemandPlan> demandPlans,string userID,  string entity, string companyDbName)
        {
            
            try
            {
                if (demandPlans?.Count == 0)
                    return new List<AutoSupplierPlanMap>();

                await LoadAllBomDataAsync(dbConnection, demandPlans, userID, entity, companyDbName);
                return await GenerateSupplierPlans(dbConnection,demandPlans, userID, entity, companyDbName,false);
                 
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetSupplierPlansMappingAsync: {Message}", ex.Message);
                return new List<AutoSupplierPlanMap>();
            }
        }

        private async Task LoadAllBomDataAsync( IDbConnection dbConnection,List<DemandPlan> demandPlans,    string userID,    string entity,    string companyDbName)
        {
            try
            {
                // Delete old runtime BOM
                string deleteQuery = $"CALL COV_Kanban_SP_GetAutoBOM('DeleteAUTOBOM','','','','{entity}', '{companyDbName}')";
                await dbConnection.ExecuteAsync(deleteQuery);

                var tasks = new List<Task>();

                foreach (var plan in demandPlans)
                {
                    if (plan?.demandplanDetails?.Count > 0)
                    {
                        foreach (var part in plan.demandplanDetails)
                        {
                            if (string.IsNullOrWhiteSpace(part.partCode) || string.IsNullOrWhiteSpace(part.partID))
                                continue;

                            string query = $"CALL COV_Kanban_SP_GetAutoBOM('LoadBom','{part.partCode}','{part.partID}','{userID}','{entity}', '{companyDbName}')";

                            // Collect tasks to run in parallel
                            tasks.Add(dbConnection.QueryAsync<AutoSupplierPlanMap>(query));
                        }
                    }
                }
                // Await all queries in parallel to improve performance
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error loading BOM data: {Message}", ex.Message);
                throw; // Let outer method handle the fallback
            }
        }


        // Assuming this method exists and is synchronous; adapt if it's async
        private async Task<List<AutoSupplierPlanMap>> GenerateSupplierPlans(IDbConnection dbConnection, List<DemandPlan> demandPlans,  string userID,string entity,string companyDbName,bool supplierType)
        {
            try
            {
                string query = $"CALL COV_Kanban_SP_GetAutoBOM('GETSUPPLIER', '', '', '{userID}', '{entity}', '{companyDbName}')";
                var bomSupplierDetails = (await dbConnection.QueryAsync<AutoSupplierPlanMap>(query)).ToList();

                if (!bomSupplierDetails.Any())
                    return new List<AutoSupplierPlanMap>();

                var supplierCodes = bomSupplierDetails
                    .Where(x => !string.IsNullOrEmpty(x.SupplierCode))
                    .Select(x => x.SupplierCode)
                    .Distinct()
                    .ToList();

                foreach (var supplierCode in supplierCodes)
                {
                    var supplierPartsDetails = bomSupplierDetails
                        .Where(x => x.SupplierCode == supplierCode)
                        .GroupBy(x => new { x.PartCode, x.ParentCode, x.SupplierPercentage })
                        .Select(g => new
                        {
                            PartCode = g.Key.PartCode,
                            SupplierPercentage = g.Key.SupplierPercentage,
                            ParentWiseCounts = g.GroupBy(x => x.ParentCode)
                                                .ToDictionary(p => p.Key, p => p.Count())
                        })
                        .ToList();

                    var dailyPlansMap = new Dictionary<string, SupplierAutoDemandPlanData>();

                    foreach (var part in supplierPartsDetails)
                    {
                        foreach (var parent in part.ParentWiseCounts)
                        {
                            var parentCode = parent.Key;
                            var matchDetails = demandPlans
                                .SelectMany(dp => dp.demandplanDetails)
                                .FirstOrDefault(dp => dp.partCode == parentCode);

                            if (matchDetails == null)
                                continue;

                            string dayQuery = $"CALL \"COV_Kanban_SP_GetDayplansDetails\"('GetDayDetails', '{parentCode}', '{matchDetails.partID}', {parent.Value}, '', '', {part.SupplierPercentage})";
                            var dayPlans = dbConnection.Query<DemandPlanDaysDetails>(dayQuery).ToList();

                            string monthQuery = $"CALL \"COV_Kanban_SP_GetDayplansDetails\"('GetMonthDetails', '{parentCode}', '{matchDetails.partID}', {parent.Value}, '', '', {part.SupplierPercentage})";
                            var monthPlans = dbConnection.Query<DemandPlanMonthsDetails>(monthQuery).ToList();

                            if (!dailyPlansMap.ContainsKey(part.PartCode))
                            {
                                dailyPlansMap[part.PartCode] = new SupplierAutoDemandPlanData
                                {
                                    partCode = part.PartCode,
                                    DayPlans = new List<DemandPlanDaysDetails>(),
                                    MonthPlans = new List<DemandPlanMonthsDetails>()
                                };
                            }

                            // Merge and aggregate day plans
                            dailyPlansMap[part.PartCode].DayPlans.AddRange(dayPlans);
                            dailyPlansMap[part.PartCode].DayPlans = dailyPlansMap[part.PartCode].DayPlans
                                .GroupBy(dp => dp.planDate)
                                .Select(g => new DemandPlanDaysDetails
                                {
                                    planDate = g.Key,
                                    dayQty = g.Sum(dp => Convert.ToDecimal(dp.dayQty)).ToString()
                                })
                                .ToList();

                            // Merge and aggregate month plans
                            dailyPlansMap[part.PartCode].MonthPlans.AddRange(monthPlans);
                            dailyPlansMap[part.PartCode].MonthPlans = dailyPlansMap[part.PartCode].MonthPlans
                                .GroupBy(mp => mp.monthName)
                                .Select(g => new DemandPlanMonthsDetails
                                {
                                    monthName = g.Key,
                                    firmQty = g.Sum(mp => Convert.ToDecimal(mp.firmQty)).ToString(),
                                    forecastQty = g.Sum(mp => Convert.ToDecimal(mp.forecastQty)).ToString()
                                })
                                .ToList();
                        }
                    }

                     var customerPlan = JsonConvert.SerializeObject(demandPlans.First(), Formatting.Indented);
                    if (supplierType)
                    {
                        await InsertIntermnalSupplierPlanAutomationAsync(dbConnection, dailyPlansMap, customerPlan, supplierCode, userID, entity);
                    }
                    else
                    {
                        await InsertSupplierPlansDetails(dbConnection, dailyPlansMap, customerPlan, supplierCode, userID, entity);
                    }
                    
                }

                return bomSupplierDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GenerateSupplierPlans: {Message}", ex.Message);
                return new List<AutoSupplierPlanMap>();
            }
        }

        private async Task InsertSupplierPlansDetails(IDbConnection dbConnection, IDictionary<string, SupplierAutoDemandPlanData> supplierPlanDetails,    string customerPlan,string supplierCode,string userID, string entity)
        {
            try
            {
                if (supplierPlanDetails == null || supplierPlanDetails.Count == 0)
                    return;

                var planHeader = JsonConvert.DeserializeObject<DemandPlan>(customerPlan);
                var totalFirm = supplierPlanDetails.Values
                    .SelectMany(x => x.MonthPlans)
                    .Sum(x => Convert.ToDecimal(x.firmQty));

                string query = $"CALL COV_Kanaban_SP_AutoSupplierGeneratePlanversion('GetSupplierVersion','{supplierCode}','')";
                int supplierDocEntry = dbConnection.QueryFirstOrDefault<int>(query);

                string versionNumber;
                if (supplierDocEntry == 0)
                {
                    query = $"CALL COV_Kanban_SP_SupplierExcelPlan('PlanHead','','{supplierCode}','','','','O',{userID},'','','{entity}')";
                    var result = dbConnection.QueryFirst<ReslutResponse>(query);
                    supplierDocEntry = Convert.ToInt32(result.DocEntry);
                    versionNumber = "1";
                }
                else
                {
                    versionNumber = await GenerateSupPlanVersionAsync(dbConnection,supplierDocEntry.ToString());
                }

                string planVersion = $"{supplierCode}{DateTime.Now.Year}{versionNumber}";
                string issueDate = ParseIssueDate(planHeader.issueDate);

                query = $"CALL COV_Kanban_SP_SupplierPlanHead('PlanHeadData','','{supplierDocEntry}','{planVersion}','{supplierCode}-{planVersion}','{planHeader.partType}','{planHeader.productionMonth}','{planHeader.revisionType}','{totalFirm}','{issueDate}','{planHeader.remarks}','O','{planHeader.revisionNumber}','{planHeader.wocWeek}','{planHeader.forecastType}')";
                var planHead = dbConnection.QueryFirst<PlanRes>(query);

                foreach (var item in supplierPlanDetails)
                {
                    query = $"CALL COV_Kanban_SP_SupplierPlanPartDetails('PlanPart','','{supplierDocEntry}','{planHead.PlanID}','{planVersion}','{item.Key}','')";
                    var partDoc = dbConnection.QueryFirst<PlanPartRes>(query);

                    int currentYear = DateTime.Now.Year;

                    foreach (var month in item.Value.MonthPlans)
                    {
                        int forecastQty = SafeToInt(month.forecastQty);
                        int firmQty = SafeToInt(month.firmQty);

                        query = $"CALL COV_Kanban_SP_SupplierPlanPartMonthDetails('PlanMonth','','{supplierDocEntry}','{planHead.PlanID}','{partDoc.PartID}','{planVersion}','{month.monthName}',{forecastQty},{firmQty},'{currentYear}')";
                        dbConnection.QueryFirstOrDefault<PlanMonthRes>(query);

                        if (int.Parse(month.monthName) == 12)
                            currentYear++;
                    }

                    foreach (var day in item.Value.DayPlans)
                    {
                        int qty = SafeToInt(day.dayQty);
                        query = $"CALL COV_Kanban_SP_SupplierPlanPartDayDetails('PlanDayFirm','','{supplierDocEntry}','{planHead.PlanID}','{partDoc.PartID}',0,'{planVersion}','{day.planDate}',{qty})";
                        dbConnection.QueryFirstOrDefault<PlanDayRes>(query);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("InsertSupplierPlansDetails Error: {Message}", ex.Message);
            }
        }

        public async Task<string> GenerateSupPlanVersionAsync(IDbConnection dbConnection,string docEntry)
        {
            string _planVer = "0";

            try
            {
                if (!string.IsNullOrEmpty(docEntry))
                {
                    string _query = $"CALL COV_Kanaban_SP_AutoSupplierGeneratePlanversion('NewVersion', '{docEntry}', '')";

                    var versionList = (await dbConnection.QueryAsync<string>(_query)).ToList();

                    if (versionList.Any())
                    {
                        _planVer = "1." + versionList.First(); // Format: 1.X
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("DemandPlan GenerateSupPlanVersionAsync Ex: {Message}", ex.Message);
            }

            return _planVer;
        }

        #endregion

        #region Internal Supplier Plan Automation

        public async Task IntermnalSupplierPlanAutomationAsync(
    IDbConnection dbConnection,
    List<DemandPlan> demandPlans,
    string userID,
    string entity,
    string companyDbName)
        {
            try
            {
                // Step 1: Delete existing internal FG plans
                string deleteQuery = $"CALL COV_Kanban_SP_InternalSupplierPlans('DeleteFG','','','','{entity}','{companyDbName}')";
                await dbConnection.QueryAsync(deleteQuery);

                if (demandPlans == null || demandPlans.Count == 0)
                    return;

                var internalSuppliers = new List<InternalSupplier>();

                foreach (var plan in demandPlans)
                {
                    if (plan?.demandplanDetails == null || plan.demandplanDetails.Count == 0)
                        continue;

                    foreach (var detail in plan.demandplanDetails)
                    {
                        if (string.IsNullOrWhiteSpace(detail.partCode))
                            continue;

                        // Step 2: Fetch Internal Supplier Details
                        string getSupplierQuery = $"CALL COV_Kanban_SP_InternalSupplierPlans('GetSuppleir','{detail.partCode}','{plan.planID}','{userID}','{entity}','{companyDbName}')";
                        var supplierDetails = (await dbConnection.QueryAsync<InternalSupplierDetails>(getSupplierQuery)).ToList();

                        internalSuppliers.Add(new InternalSupplier
                        {
                            planID = plan.planID,
                            partCodes = detail.partCode,
                            internalSupplierDetails = supplierDetails
                        });

                        // Step 3: Insert Internal Supplier Data
                        string insertSupplierQuery = $"CALL COV_Kanban_SP_InternalSupplierPlans('ALLFG','{detail.partCode}','{plan.planID}','{userID}','{entity}','{companyDbName}')";
                        await dbConnection.QueryAsync(insertSupplierQuery);
                    }
                }

                // You can return `internalSuppliers` if you need to use it later

                await GenerateSupplierPlans(dbConnection, demandPlans, userID, entity, companyDbName, true);
            }
            catch (Exception ex)
            {
                _logger.LogError("IntermnalSupplierPlanAutomationAsync Ex: {Message}", ex.Message);
            }
        }

        public async Task InsertIntermnalSupplierPlanAutomationAsync(
     IDbConnection dbConnection,
     IDictionary<string, SupplierAutoDemandPlanData> supplierPlanDetails,
     string customerPlan,
     string supplierCode,
     string userID,
     string entity)
        {
            try
            {
                if (supplierPlanDetails == null || supplierPlanDetails.Count == 0)
                    return;

                var planHeader = JsonConvert.DeserializeObject<DemandPlan>(customerPlan);
                var totalFirm = supplierPlanDetails.Values
                    .SelectMany(x => x.MonthPlans)
                    .Sum(x => Convert.ToDecimal(x.firmQty));

                string currentYear = DateTime.Now.Year.ToString();

                string query = $"CALL COV_Kanaban_SP_AutoSupplierGeneratePlanversion('GetInternalSupplierVersion', '{supplierCode}', '')";
                string supplierDocEntry = await dbConnection.QueryFirstOrDefaultAsync<string>(query);

                string versionSuffix;
                if (supplierDocEntry == "0" || supplierDocEntry == "1")
                {
                    query = $"CALL COV_Kanaban_SP_InteralSupplierHeaderPlan('PlanHead','','{supplierCode}','','EX001','','','O',{userID},{entity})";
                    var docEntry = await dbConnection.QueryFirstAsync<ReslutResponse>(query);
                    supplierDocEntry = docEntry.DocEntry;
                    versionSuffix = "1";
                }
                else
                {
                    versionSuffix = await InternalGenerateSupPlanVersionAsync(dbConnection, supplierDocEntry);
                }

                string planVersion = $"{supplierCode}{currentYear}{versionSuffix}";
                string issueDate = ParseIssueDate(planHeader.issueDate);

                query = $"CALL COV_Kanban_SP_InteralSupplierPlanVesion('PlanHeadData','','{supplierDocEntry}','{planVersion}','{supplierCode}-{planVersion}','{planHeader.partType}','{planHeader.productionMonth}','{planHeader.revisionType}','{totalFirm}','{issueDate}','{planHeader.remarks}','O','{planHeader.revisionNumber}','{planHeader.wocWeek}','{planHeader.forecastType}')";
                var planHead = dbConnection.QueryFirst<PlanRes>(query);
                int baseYear = DateTime.Now.Year;

                foreach (var item in supplierPlanDetails)
                {
                    query = $"CALL COV_Kanban_SP_InteralSupplierPlanPartDetails('PlanPart','','{supplierDocEntry}','{planHead.PlanID}','{planVersion}','{item.Key}','')";
                    var partDoc = dbConnection.QueryFirst<PlanPartRes>(query);

                    int currentYearInt = baseYear;
                    int itemCount = supplierPlanDetails.Count;

                    foreach (var month in item.Value.MonthPlans)
                    {
                        int forecastQty = Convert.ToInt32(string.IsNullOrWhiteSpace(month.forecastQty) ? "0" : month.forecastQty) * itemCount;
                        int firmQty = Convert.ToInt32(string.IsNullOrWhiteSpace(month.firmQty) ? "0" : month.firmQty) * itemCount;

                        query = $"CALL COV_Kanban_SP_InteralSupplierPlanPartMonthDetails('PlanMonth','','{supplierDocEntry}','{planHead.PlanID}','{partDoc.PartID}','{planVersion}','{month.monthName}',{forecastQty},{firmQty},'{currentYearInt}')";
                        dbConnection.QueryFirstOrDefault<PlanMonthRes>(query);

                        if (int.TryParse(month.monthName, out int monthVal) && monthVal == 12)
                            currentYearInt++;
                    }

                    foreach (var day in item.Value.DayPlans)
                    {
                        int dayQty = int.TryParse(day.dayQty, out int qty) ? qty : 0;
                        query = $"CALL COV_Kanban_SP_InteralSupplierPlanPartDayDetails('PlanDayFirm','','{supplierDocEntry}','{planHead.PlanID}','{partDoc.PartID}',0,'{planVersion}','{day.planDate}',{dayQty})";
                        dbConnection.QueryFirstOrDefault<PlanDayRes>(query);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("InsertIntermnalSupplierPlanAutomationAsync Ex: {Message}", ex.Message);
            }
        }

        public async Task<string> InternalGenerateSupPlanVersionAsync(IDbConnection dbConnection, string docEntry)
        {
            string planVer = "0";

            try
            {
                if (!string.IsNullOrWhiteSpace(docEntry))
                {
                    string query = $"CALL COV_Kanaban_SP_AutoSupplierGeneratePlanversion('InternalCustNewVersion', '{docEntry}', '')";

                    var versionList = (await dbConnection.QueryAsync<string>(query)).ToList();

                    if (versionList.Any())
                    {
                        planVer = "1." + versionList.First();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("DemandPlan InternalGenerateSupPlanVersionAsync Ex: {Message}", ex.Message);
            }

            return planVer;
        }

        #endregion
        private int SafeToInt(string value)
        {
            return int.TryParse(value?.Trim(), out var result) ? result : 0;
        }
        private string ParseIssueDate(string issueDateRaw)
        {
            string[] formats = {
                                "dd/MM/yyyy",
                                "d/M/yyyy h:mm:ss tt",
                                "dd/MM/yyyy h:mm:ss tt",
                                "MM/dd/yyyy hh:mm:ss tt"
                            };

            if (DateTime.TryParseExact(issueDateRaw, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate.ToString("yyyy-MM-dd");
            }

            _logger.LogWarning("ParseIssueDate: Invalid date format for value: {IssueDateRaw}", issueDateRaw);
            return string.Empty; // or throw an exception, depending on your use case
        }

    }
}
