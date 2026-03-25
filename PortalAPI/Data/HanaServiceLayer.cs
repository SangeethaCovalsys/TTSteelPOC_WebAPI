using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Data;
using System.Net;

namespace PortalAPI.Data
{
    public class HanaServiceLayer
    {
        private readonly string _connectionString;
        private readonly string _connectionHanaString;
        private readonly IConfiguration _configuration;
        string _url = string.Empty;
        string _companyname = string.Empty;
        string _userName = string.Empty;
        string _password = string.Empty;
        public HanaServiceLayer(IConfiguration configuration)
        {            
            _configuration = configuration;   
            _url = _configuration.GetValue<string>("ServiceURL");
            _companyname = _configuration.GetValue<string>("CompanyName");
            _userName = _configuration.GetValue<string>("UN");
            _password = _configuration.GetValue<string>("PW");
        }

        public string Login(out string strRouteVal)
        {
            string str_Response = string.Empty;
            string ResponseMessage = string.Empty;
            strRouteVal = string.Empty; // Initialize the output variable

            try
            {
                // Prepare the URL and request payload
                string sURL = $"{_url}Login";
                string json = JsonConvert.SerializeObject(new
                {
                    CompanyDB = _companyname,
                    UserName = _userName,
                    Password = _password
                });

                // Set up the REST client
                var client = new RestClient(sURL);
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                // Use TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Create the request
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", json, ParameterType.RequestBody);

                // Execute the request and handle the response
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Root _result = JsonConvert.DeserializeObject<Root>(response.Content);
                    //value = (value == null) ? null : value.ToString();
                    if (_result.SessionId != null)
                    {
                        ResponseMessage = _result.SessionId;
                    }
                    CookieContainer cookie = new CookieContainer();
                    var cookie_1 = response.Cookies.FirstOrDefault();
                    var cookie_2 = response.Cookies.LastOrDefault();
                    //CN1 = cookie_1.Name;
                    //CN2 = cookie_2.Name;
                    //CV1 = cookie_1.Value;
                    //CV2 = cookie_2.Value;
                    strRouteVal = cookie_2.Value;
                    if (_result.SessionId != null)
                    {
                        ResponseMessage = cookie_1.Value;
                    }
                    else
                    {
                        ResponseMessage = _result.error.message.value;
                    }
                     
                }
                else
                {
                    ResponseMessage = $"Error: {response.StatusCode}, {response.Content}";
                }
            }
            catch (Exception ex)
            {
                // Log and throw the exception for debugging purposes
                throw new Exception("An error occurred during login.", ex);
            }

            return ResponseMessage;
        }

        public string JsonStringToDataTable(string jsonString, string strFun)
        {
            try
            {
                DataTable dt = new DataTable();
                var trgArray = new JArray();
                var cleanRow1 = new JObject();
                var cleanRow = new JObject();
                var Rows = new JObject();
                var js = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonString);
                var jsonLinq = JObject.Parse(jsonString);
                string Cnd = string.Empty;
                string errval = string.Empty;
                string DocEntry = string.Empty;
                string DocNum = string.Empty;
                string CardCode = string.Empty;
                string CardName = string.Empty;
                string DocumentEntry = string.Empty;
                string DocumentNumber = string.Empty;
                string ServiceCallId = string.Empty;
                foreach (var lin in jsonLinq)
                {
                    if (lin.Key == "error")
                    {
                        Rows.Add(lin.Key, lin.Key);
                        var srcArray = jsonLinq.Descendants().Where(d => d is JObject).First();
                        foreach (JObject row in srcArray.Last())
                        {
                            cleanRow = new JObject();
                            foreach (JProperty column in row.Properties())
                            {
                                // Only include JValue types
                                if (column.Value is JValue)
                                {
                                    if (column.Name == "value")
                                    {
                                        cleanRow.Add(column.Name, column.Value);
                                        errval = Convert.ToString(column.Value);
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        if (lin.Key == "DocEntry")
                        {
                            DocEntry = lin.Value.ToString();
                        }
                        else if (lin.Key == "ServiceCallID")
                        {
                            ServiceCallId = lin.Value.ToString();
                        }

                        else if (lin.Key == "DocNum")
                        {
                            DocNum = lin.Value.ToString();
                        }
                        else if (lin.Key == "CardCode")
                        {
                            CardCode = lin.Value.ToString();
                        }
                        else if (lin.Key == "CardName")
                        {
                            CardName = lin.Value.ToString();
                        }
                        else if (lin.Key == "AbsEntry")
                        {
                            DocEntry = lin.Value.ToString();
                        }
                        else if (lin.Key == "DepositNumber")
                        {
                            DocNum = lin.Value.ToString();
                        }
                        else if (lin.Key == "ReconNum")
                        {
                            DocNum = lin.Value.ToString();
                        }
                        else if (lin.Key == "DocumentEntry")
                        {
                            DocEntry = lin.Value.ToString();
                        }
                        else if (lin.Key == "DocumentNumber")
                        {
                            DocNum = lin.Value.ToString();
                        }
                        if (strFun == "Login")
                        {
                            errval = "Company Connected";
                            break;
                        }
                        else if (strFun == "BPMasterCreation")
                        {
                            if (CardCode != "" && CardName != "")
                            {
                                errval = CardCode + "#" + CardName + "#" + "Customer created successfully";
                                break;
                            }

                        }
                        else if (strFun == "ServiceCalls")
                        {
                            if (ServiceCallId != "" && DocNum != "")
                            {
                                errval = ServiceCallId + "#" + DocNum + "#" + "Service Call created Sucessfully";
                                break;
                            }

                        }
                        else if (strFun == "SalesInvoice")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "ARInvoice created successfully";
                                break;
                            }
                        }
                        else if (strFun == "SalesReturn")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "SalesReturn created successfully";
                                break;
                            }
                        }
                        else if (strFun == "Incoming")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "Payment created successfully";
                                break;
                            }
                            //if (DocNum != "")
                            //{
                            //    errval = DocNum + "#" + "Payment created successfully";
                            //    break;
                            //}
                        }
                        else if (strFun == "SalesOrder")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "SalesOrder Created successfully";
                                break;
                            }
                        }
                        else if (strFun == "SalesReturn")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "SalesReturn Created successfully";
                                break;
                            }
                        }
                        else if (strFun == "GRPO")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "GRPO created successfully";
                                break;
                            }
                        }
                        else if (strFun == "StockTransferRequest")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "Stocktransferrequest created successfully";
                                break;
                            }
                        }
                        else if (strFun == "Stocktransfer")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "Stocktransfer created successfully";
                                break;
                            }
                        }
                        else if (strFun == "OutgoingPayment")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "OutgoingPayment created Sucessfully";
                                break;
                            }
                        }
                        else if (strFun == "Deposit")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "Deposit created Sucessfully";
                                break;
                            }
                        }
                        else if (strFun == "InternalReconciliations")
                        {
                            errval = "Reconciliation(s) done successfully";
                            break;

                        }
                        else if (strFun == "GIS_OINC")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "InventoryCountings created Sucessfully";
                                break;
                            }

                        }
                        else if (strFun == "ODEF")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "Defective Document Created successfully";
                                break;
                            }
                        }
                        else if (strFun == "GIS_OWTS")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "Transfer Document Created successfully";
                                break;
                            }
                        }

                        else if (strFun == "GIS_OSPN")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "Shipping Note Document Created successfully";
                                break;
                            }
                        }
                        //DENOMINATION
                        else if (strFun == "DENOMINATION")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "Denomination Created successfully";
                                break;
                            }
                        }
                        else if (strFun == "PurchaseReturns")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "PurchaseReturns Created successfully";
                                break;
                            }
                        }
                        else if (strFun == "Quotations")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "Sales Quotation Created successfully";
                                break;
                            }
                        }
                        else if (strFun == "Orders")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "SalesOrder Created successfully";
                                break;
                            }
                        }
                        else if (strFun == "Draft")
                        {
                            if (DocEntry != "" && DocNum != "")
                            {
                                errval = DocEntry + "#" + DocNum + "#" + "Draft Created successfully";
                                break;
                            }
                        }
                    }
                }
                return errval;
            }
            catch
            {
                throw;
            }
        }
        //public string JsonStringToDataTable(string jsonString, string strFun)
        //{
        //    try
        //    {
        //        // Parse JSON string into a dictionary
        //        var jsonLinq = JObject.Parse(jsonString);
        //        var keyMappings = new Dictionary<string, string>
        //            {
        //                { "DocEntry", "Document Entry" },
        //                { "DocNum", "Document Number" },
        //                { "CardCode", "Customer Code" },
        //                { "CardName", "Customer Name" },

        //            };

        //        // Handle special cases for errors
        //        if (jsonLinq.ContainsKey("error"))
        //        {
        //            var error = jsonLinq["error"]?.FirstOrDefault()?["value"]?.ToString();
        //            return string.IsNullOrEmpty(error) ? "Unknown error occurred." : error;
        //        }

        //        // Extract required values from JSON
        //        var extractedValues = keyMappings.ToDictionary(
        //            mapping => mapping.Key,
        //            mapping => jsonLinq[mapping.Key]?.ToString() ?? string.Empty
        //        );

        //        string docEntry = extractedValues["DocEntry"];
        //        string docNum = extractedValues["DocNum"];
        //        string cardCode = extractedValues["CardCode"];
        //        string cardName = extractedValues["CardName"];


        //        // Define success messages for each function
        //   var successMessages = new Dictionary<string, Func<string>>
        //    {
        //    { "Login", () => "Company Connected" },
        //    { "BPMasterCreation", () => !string.IsNullOrEmpty(cardCode) && !string.IsNullOrEmpty(cardName)
        //        ? $"{cardCode}#{cardName}#Customer created successfully" : string.Empty },
        //    { "SalesInvoice", () => FormatSuccessMessage(docEntry, docNum, "ARInvoice created successfully") },
        //    { "SalesReturn", () => FormatSuccessMessage(docEntry, docNum, "SalesReturn created successfully") },
        //    { "Incoming", () => FormatSuccessMessage(docEntry, docNum, "Payment created successfully") },
        //    { "SalesOrder", () => FormatSuccessMessage(docEntry, docNum, "SalesOrder Created successfully") },
        //    { "GRPO", () => FormatSuccessMessage(docEntry, docNum, "GRPO created successfully") },
        //    { "StockTransferRequest", () => FormatSuccessMessage(docEntry, docNum, "Stocktransferrequest created successfully") },
        //    { "Stocktransfer", () => FormatSuccessMessage(docEntry, docNum, "Stocktransfer created successfully") },
        //    { "OutgoingPayment", () => FormatSuccessMessage(docEntry, docNum, "OutgoingPayment created successfully") },
        //    { "Deposit", () => FormatSuccessMessage(docEntry, docNum, "Deposit created successfully") },
        //    { "InternalReconciliations", () => "Reconciliation(s) done successfully" },
        //    { "DENOMINATION", () => FormatSuccessMessage(docEntry, docNum, "Denomination created successfully") },
        //    { "PurchaseReturns", () => FormatSuccessMessage(docEntry, docNum, "PurchaseReturns created successfully") },
        //    { "Quotations", () => FormatSuccessMessage(docEntry, docNum, "Sales Quotation Created successfully") },
        //    { "Orders", () => FormatSuccessMessage(docEntry, docNum, "SalesOrder Created successfully") }
        //};

        //        // Return the success message based on the function
        //        return successMessages.TryGetValue(strFun, out var messageFunc) ? messageFunc() : "Function not supported.";
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log exception in a production environment
        //        throw new Exception("An error occurred while processing JSON data.", ex);
        //    }
        //}

        // Helper method to format success messages
        private string FormatSuccessMessage(string docEntry, string docNum, string message)
        {
            return !string.IsNullOrEmpty(docEntry) && !string.IsNullOrEmpty(docNum)
                ? $"{docEntry}#{docNum}#{message}"
                : string.Empty;
        }


        public string TransactionPosting(string URL, string MasterData,   string TransactionType, string strRoutevalue)
        {
            string str_Response = string.Empty;
            string CV1= string.Empty;
            string strFun = string.Empty;
            try
            {
                string strRouteVal_O = string.Empty;
                
                CV1 = Login(out strRouteVal_O);
                strFun = TransactionType;
                var client = new RestClient(_url+"/"+URL + "?SessionId=" + CV1);
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                var request = new RestRequest(Method.POST);
                //request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", MasterData, ParameterType.RequestBody);

                request.AddParameter("B1SESSION", CV1, ParameterType.Cookie);
                request.AddParameter("ROUTEID", strRouteVal_O, ParameterType.Cookie);
                request.AddParameter("CompanyDB", _companyname, ParameterType.Cookie);

                IRestResponse response = client.Execute(request);
                dynamic value = JsonConvert.DeserializeObject(response.Content);
                value = (value == null) ? null : value.ToString();

                str_Response = value;

                if (value != null)
                {
                    str_Response = JsonStringToDataTable(value, strFun);
                }
            }
            catch (WebException ex)
            {
                str_Response = ex.ToString();
            }
            return str_Response;
        }

        public partial class Error
        {
            public int code { get; set; }
            public Message message { get; set; }
        }

        public partial class Message
        {
            public string lang { get; set; }
            public string value { get; set; }
        }

        public partial class Root
        {
            [JsonProperty("odata.metadata")]
            public string odatametadata { get; set; }
            public string SessionId { get; set; }
            public string Version { get; set; }
            public int SessionTimeout { get; set; }
            public Error error { get; set; }
        }
    }
}
