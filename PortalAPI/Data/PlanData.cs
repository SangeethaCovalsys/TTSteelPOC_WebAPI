using PortalAPI.Model;

namespace PortalAPI.Data
{
    public class PlanData
    {
        public string PlanJSON(CustomerPlanDetails customerPlan)
        {
            string planJSON = string.Empty;
            HanaCustomerPlan hanaCustomerPlan = new HanaCustomerPlan();
            try
            {
                if (customerPlan != null)
                {
                    hanaCustomerPlan.DocEntry=customerPlan.DocEntry;
                    hanaCustomerPlan.U_CardCode = customerPlan.cardCode;
                    hanaCustomerPlan.U_CardName = customerPlan.cardCode;
                    hanaCustomerPlan.U_PartCode = customerPlan.partCode;
                    hanaCustomerPlan.U_PartName = customerPlan.partName;
                    hanaCustomerPlan.U_ModelName = customerPlan.modelName;
                    hanaCustomerPlan.U_PlanVersion= customerPlan.planVersion;
                    hanaCustomerPlan.U_PlanDesc= customerPlan.planDesc;
                    hanaCustomerPlan.U_ForcastQty = customerPlan.yearForcastQty;
                    hanaCustomerPlan.U_EffectiveFrom=customerPlan.effectiveFrom;
                    hanaCustomerPlan.U_EffectiveTo=customerPlan.effectiveTo;
                    if (customerPlan.months.Count > 0)
                    {
                        
                        List<COVPLN1Collection> _finalPLN1 = new List<COVPLN1Collection>();
                        customerPlan.months.Where(x => x.forecastQty != "").ToList();
                        for(int i = 0;i< customerPlan.months.Count;i++)
                        {
                            if (!string.IsNullOrEmpty(customerPlan.months[i].firmQty))
                            {
                                COVPLN1Collection _PLN1 = new COVPLN1Collection();
                                _PLN1.DocEntry= customerPlan.months[i].DocEntry!=null? customerPlan.months[i].DocEntry:customerPlan.DocEntry;
                                _PLN1.LineId= customerPlan.months[i].LineId;
                                _PLN1.U_FirmQty = customerPlan.months[i].firmQty;
                                _PLN1.U_ForcastQty = customerPlan.months[i].forecastQty;
                                _PLN1.U_currentMonth = customerPlan.months[i].monthName;
                                _finalPLN1.Add(_PLN1);
                            }
                        }
                        hanaCustomerPlan.COV_PLN1Collection = _finalPLN1;
                    }
                    if (customerPlan.days.Count > 0)
                    {
                        List<COVPLN2Collection> _finalPLN2 = new List<COVPLN2Collection>();
                        for (int i = 0; i < customerPlan.days.Count; i++)
                        {
                            COVPLN2Collection _PLN2 = new COVPLN2Collection();
                            _PLN2.DocEntry= customerPlan.days[i].DocEntry!=""? customerPlan.days[i].DocEntry: customerPlan.DocEntry;
                            _PLN2.LineId = customerPlan.days[i].LineId != ""? customerPlan.days[i].LineId:(i+1).ToString();
                            _PLN2.U_currentDay = customerPlan.days[i].dayName;
                            if(customerPlan.days[i].cDay!=null)
                            {
                                string[] _cD = customerPlan.days[i].cDay.Split('-');

                                _PLN2.U_CDay = _cD[2]+"-"+ _cD[1]+ "-" + _cD[0];
                            }
                            
                            _PLN2.U_PDSNo = customerPlan.days[i].pdsNo;
                            _PLN2.U_PDSQty = customerPlan.days[i].pdsQty;
                            _PLN2.U_PDSDispatchQty = customerPlan.days[i].dayDispatchQty;
                            _finalPLN2.Add(_PLN2);
                        }
                        hanaCustomerPlan.COV_PLN2Collection = _finalPLN2;
                    }

                }
                planJSON = Newtonsoft.Json.JsonConvert.SerializeObject(hanaCustomerPlan);
            }
            catch {
            }
            return planJSON;
        }

        public string SupplierPlanJSON(SupplierDemandPlan supplierPlan,string location)
        {
            string planJSON = string.Empty;
            HanaSupplierPlan hanaCustomerPlan = new HanaSupplierPlan();
            try
            {
                if (supplierPlan != null)
                {
                    hanaCustomerPlan.DocEntry = supplierPlan.DocEntry;
                    hanaCustomerPlan.U_SupplierCode = supplierPlan.supplierCode;
                    hanaCustomerPlan.U_CardCode = supplierPlan.cardCode;
                    //hanaCustomerPlan.U_CardName = supplierPlan.cardCode;
                    hanaCustomerPlan.U_PartCode = supplierPlan.partCode;
                    hanaCustomerPlan.U_PartName = supplierPlan.partName;
                    hanaCustomerPlan.U_ModelName = supplierPlan.modelName;
                    hanaCustomerPlan.U_PlanVersion = supplierPlan.planVersion;
                    hanaCustomerPlan.U_PlanDesc = supplierPlan.planDesc;
                    hanaCustomerPlan.U_ForcastQty = supplierPlan.yearForcastQty;
                    hanaCustomerPlan.U_EffectiveFrom = supplierPlan.effectiveFrom;
                    hanaCustomerPlan.U_EffectiveTo = supplierPlan.effectiveTo;
                    if (supplierPlan.months.Count > 0)
                    {

                        List<COVSPN1Collection> _finalPLN1 = new List<COVSPN1Collection>();
                        supplierPlan.months.Where(x => x.forecastQty != "").ToList();
                        for (int i = 0; i < supplierPlan.months.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(supplierPlan.months[i].firmQty))
                            {
                                COVSPN1Collection _PLN1 = new COVSPN1Collection();
                                _PLN1.DocEntry = supplierPlan.months[i].DocEntry != null ? supplierPlan.months[i].DocEntry : supplierPlan.DocEntry;
                                _PLN1.LineId = supplierPlan.months[i].LineId;
                                _PLN1.U_FirmQty = supplierPlan.months[i].firmQty;
                                _PLN1.U_ForcastQty = supplierPlan.months[i].forecastQty;
                                _PLN1.U_currentMonth = supplierPlan.months[i].monthName;
                                _finalPLN1.Add(_PLN1);
                            }
                        }
                        hanaCustomerPlan.COV_SPN1Collection = _finalPLN1;
                    }
                    if (supplierPlan.days.Count > 0)
                    {
                        List<COVSPN2Collection> _finalPLN2 = new List<COVSPN2Collection>();
                        for (int i = 0; i < supplierPlan.days.Count; i++)
                        {
                            COVSPN2Collection _PLN2 = new COVSPN2Collection();
                            _PLN2.DocEntry = supplierPlan.days[i].DocEntry != "" ? supplierPlan.days[i].DocEntry : supplierPlan.DocEntry;
                            _PLN2.LineId = supplierPlan.days[i].LineId != "" ? supplierPlan.days[i].LineId : "";
                            _PLN2.U_currentDay = supplierPlan.days[i].dayName;
                            if (supplierPlan.days[i].cDay != null)
                            {
                                string[] _cD = supplierPlan.days[i].cDay.Split('-');

                                _PLN2.U_CDay = _cD[2] + "-" + _cD[1] + "-" + _cD[0];
                            }

                            _PLN2.U_PDSNo = supplierPlan.days[i].pdsNo;
                            _PLN2.U_PDSQty = supplierPlan.days[i].pdsQty;
                            _PLN2.U_PDSDispatchQty = supplierPlan.days[i].dayDispatchQty;
                            _finalPLN2.Add(_PLN2);
                        }
                        hanaCustomerPlan.COV_SPN2Collection = _finalPLN2;
                    }
                    if (supplierPlan.documentFiles.Count > 0)
                    {
                        List<COV_SPN3Collection> _finalPLN3 = new List<COV_SPN3Collection>();
                        for (int i = 0; i < supplierPlan.documentFiles.Count; i++)
                        {if (supplierPlan.documentFiles[i].uFileName != null)
                            {
                                COV_SPN3Collection _PLN3 = new COV_SPN3Collection();
                                _PLN3.DocEntry = supplierPlan.documentFiles[i].docEntry != "" ? supplierPlan.documentFiles[i].docEntry : supplierPlan.DocEntry;
                                _PLN3.LineId = supplierPlan.documentFiles[i].lineId != "" ? supplierPlan.documentFiles[i].lineId : "";
                                _PLN3.U_PDSNo = supplierPlan.documentFiles[i].uPDSNo;
                                //if (supplierPlan.days[i].cDay != null)
                                //{
                                //    string[] _cD = supplierPlan.days[i].cDay.Split('-');

                                //    _PLN2.U_CDay = _cD[2] + "-" + _cD[1] + "-" + _cD[0];
                                //}

                                _PLN3.U_PDSDate = supplierPlan.documentFiles[i].uPDSDate;
                                _PLN3.U_FileName = supplierPlan.documentFiles[i].uFileName;
                                _PLN3.U_FileType = supplierPlan.documentFiles[i].uFileType;
                                _PLN3.U_FileSize = supplierPlan.documentFiles[i].uFileSize;
                                if (supplierPlan.documentFiles[i].uFileDate != null)
                                {
                                    string[] _cD = supplierPlan.documentFiles[i].uFileDate.Split('-');

                                    _PLN3.U_FileuploadedDate = _cD[2] + "-" + _cD[1] + "-" + _cD[0];
                                }
                                //_PLN3.U_FileuploadedDate = supplierPlan.documentFiles[i].uFileDate;
                                _PLN3.U_FilePath = location;
                                _finalPLN3.Add(_PLN3);
                            }
                        }
                        hanaCustomerPlan.COV_SPN3Collection = _finalPLN3;
                    }

                }
                planJSON = Newtonsoft.Json.JsonConvert.SerializeObject(hanaCustomerPlan);
            }
            catch
            {
            }
            return planJSON;
        }

    }
}
