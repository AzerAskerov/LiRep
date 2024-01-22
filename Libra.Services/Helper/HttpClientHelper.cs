using Libra.Contract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Libra.Services.Helper
{
    public static class HttpClientHelper
    {
        public static async Task<OperationResult<Tout>> PostJsonAsync<T, Tout>(this HttpClient httpClient, string url, T content)
        {
            Tout output = default(Tout);
            OperationResult<Tout> operationResult = new OperationResult<Tout>();
            HttpResponseMessage response = null;

            try
            {
                var stringContent = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
                response = await httpClient.PostAsync(url, stringContent);

                if (response.IsSuccessStatusCode)
                {
                    string respStr = await response.Content.ReadAsStringAsync();

                    output = JsonConvert.DeserializeObject<Tout>(respStr);

                    operationResult.Model = output;
                }
                else
                {
                    string respStr = await response.Content.ReadAsStringAsync();
                    var listIssue = JsonConvert.DeserializeObject<List<string>>(respStr);

                    foreach (string str in listIssue)
                    {
                        operationResult.Issues.Add(new Issue(str, IssueSeverity.Error));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //operationResult.AddException(ex);
                //TechnicalLog.LogException(ex, string.Format("{0}: Exception in \"Execute\" method", "HttpClientHelper.PostJsonAsync"));
            }

            return operationResult;
        }
    }
}
    
