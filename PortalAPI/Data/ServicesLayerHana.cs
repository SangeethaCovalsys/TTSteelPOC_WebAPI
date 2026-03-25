using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;


namespace PortalAPI.Data
{
    public class ServicesLayerHana
    {
        static string SessionID = ""; string result = "";
        string CN1, CN2, CV1, CV2;
        string strFun = string.Empty;
        public string _url = string.Empty;
        public string _password = string.Empty;
        public string _username = string.Empty;
        public string _companyName = string.Empty;
        public string _routeValue = string.Empty;
        public ServicesLayerHana(IConfiguration configuration)
        {
            _url = configuration.GetValue<string>("ServiceURL");
            _companyName = configuration.GetValue<string>("CompanyName");
            _username = configuration.GetValue<string>("UN");
            _password = configuration.GetValue<string>("PW");
        }

       
        public string Login(out string strRouteVal)
        {
            string str_Response = string.Empty;
            string ResponseMessage = string.Empty;

            try
            {
                 
                string json = "{\"CompanyDB\": \"" + _companyName + "\", \"UserName\": \"" + _username + "\", \"Password\": \"" + _password + "\"}";
                var client = new RestClient(_url+"Login");
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                var request = new RestRequest(Method.POST);                
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                //dynamic value = JsonConvert.DeserializeObject(response.Content);
                Root _result=JsonConvert.DeserializeObject<Root>(response.Content);
                //value = (value == null) ? null : value.ToString();
                if (_result.SessionId != null)
                {
                    ResponseMessage = _result.SessionId;
                }
                CookieContainer cookie = new CookieContainer();
                var cookie_1 = response.Cookies.FirstOrDefault();
                var cookie_2 = response.Cookies.LastOrDefault();
                CN1 = cookie_1.Name;
                CN2 = cookie_2.Name;
                CV1 = cookie_1.Value;
                CV2 = cookie_2.Value;
                strRouteVal = CV2;
                if (_result.SessionId != null)
                {
                    ResponseMessage = CV1;
                }
                else
                {
                    ResponseMessage = _result.error.message.value;
                }               
                return ResponseMessage;
            }
            catch
            {
                throw;
            }
        }
        public IRestResponse TransactionPosting(string pageURL, string MasterData, string str_SessionID, string TransactionType, string strRoutevalue)
        {
            string str_Response = string.Empty;
            IRestResponse response=null;
            try
            {                
                strFun = TransactionType;
                var client = new RestClient(_url+ pageURL + "?SessionId=" + str_SessionID);
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                var request = new RestRequest(Method.POST);
                //request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", MasterData, ParameterType.RequestBody);

                request.AddParameter("B1SESSION", str_SessionID, ParameterType.Cookie);
                request.AddParameter("ROUTEID", strRoutevalue, ParameterType.Cookie);
                request.AddParameter("CompanyDB", _companyName, ParameterType.Cookie);

                 response = client.Execute(request);
                dynamic value = JsonConvert.DeserializeObject(response.Content);
                value = (value == null) ? null : value.ToString();
                str_Response = value;
                if (value != null)
                {
                    //str_Response = JsonStringToDataTable(value, strFun);
                }
            }
            catch (WebException ex)
            {
                str_Response = ex.ToString();
            }
            return response;
        }
        public IRestResponse TransactionPatch(string _DocEntry,string pageURL, string MasterData, string str_SessionID, string TransactionType, string strRoutevalue)
        {
            string str_Response = string.Empty;
            IRestResponse response = null;
            try
            {
                strFun = TransactionType;
                if(TransactionType== "OSPR")
                {
                    _DocEntry = "('" +_DocEntry+ "')";
                }
                else
                {
                    _DocEntry = "(" + _DocEntry + ")";
                }
                var client = new RestClient(_url + pageURL +_DocEntry+"?SessionId=" + str_SessionID);
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                var request = new RestRequest(Method.PATCH);
                //request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", MasterData, ParameterType.RequestBody);

                request.AddParameter("B1SESSION", str_SessionID, ParameterType.Cookie);
                request.AddParameter("ROUTEID", strRoutevalue, ParameterType.Cookie);
                request.AddParameter("CompanyDB", _companyName, ParameterType.Cookie);

                response = client.Execute(request);
                dynamic value = JsonConvert.DeserializeObject(response.Content);
                value = (value == null) ? null : value.ToString();
                str_Response = value;
                if (value != null)
                {
                    //str_Response = JsonStringToDataTable(value, strFun);
                }
            }
            catch (WebException ex)
            {
                str_Response = ex.ToString();
            }
            return response;
        }
        public IRestResponse TransactionGetAllRecord(string pageURL, string MasterData, string str_SessionID, string TransactionType, string strRoutevalue)
        {
            string str_Response = string.Empty;
            IRestResponse response = null;
            try
            {
                strFun = TransactionType;
                //var client = new RestClient(_url + pageURL + "?SessionId=" + str_SessionID)  ;
                var client = new RestClient(_url + pageURL );
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                var request = new RestRequest(Method.GET);
                //request.AddHeader("cache-control", "no-cache");
                //request.AddHeader("content-type", "application/json");
                //request.AddParameter("application/json", MasterData, ParameterType.RequestBody);

                request.AddParameter("B1SESSION", str_SessionID, ParameterType.Cookie);
                request.AddParameter("ROUTEID", strRoutevalue, ParameterType.Cookie);
                request.AddParameter("CompanyDB", _companyName, ParameterType.Cookie);

                response = client.Execute(request);
                dynamic value = JsonConvert.DeserializeObject(response.Content);
                value = (value == null) ? null : value.ToString();
                str_Response = value;
                if (value != null)
                {
                    //str_Response = JsonStringToDataTable(value, strFun);
                }
            }
            catch (WebException ex)
            {
                str_Response = ex.ToString();
            }
            return response;
        }
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
