using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace BitBot
{
    internal static class TradingEngine
    {
        internal delegate void PropertyChangedDelegate(string propertyName);
        static internal event PropertyChangedDelegate PropertyChangedEvent;

        static internal CoinbaseAccount CoinbaseBTCAccount = null;
        static internal CoinbaseAccount CoinbaseUSDAccount = null;

        static private System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();

        private static string _errorMsg = "";
        internal static string errorMsg
        {
            get { return _errorMsg; }
            private set
            {
                _errorMsg = value;
                if (PropertyChangedEvent != null)
                {
                    PropertyChangedEvent("errorMsg");
                }
            }
        }

        private static string _statusMsg = "";
        internal static string statusMsg
        {
            get { return _statusMsg; }
            private set
            {
                _statusMsg = value;
                if (PropertyChangedEvent != null)
                {
                    PropertyChangedEvent("statusMsg");
                }
            }
        }

        private static bool bRunning = false;

        static TradingEngine()
        {
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += worker_DoWork;
        }

        static void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // todo:
        }

        /// <summary>
        /// loads USD and BTC trading account info
        /// </summary>
        /// <returns>true if accounts are loaded successfully, false if load failed. </returns>
        static internal bool loadAccounts()
        {
            bool retVal;

            string timeStamp = ((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString(); //timestamp must be number of seconds since unix epoch.
            string method = "GET";
            string requestPath = @"/accounts";
            string body = "";

            string requestURL = MarketData.apiEndpointURL + requestPath;
            HttpWebRequest accountsRequest = (HttpWebRequest)WebRequest.Create(requestURL);

            //byte[] secretDecoded = Convert.FromBase64String(MarketData.COINBASE_API_SECRET);
            byte[] secretDecoded = Convert.FromBase64String(coinbase_bot.Properties.Settings.Default.tbAPISecret);
            string encodeString = timeStamp + method + requestPath + body;
            string hashString;

            using (System.Security.Cryptography.HMACSHA256 hmac = new System.Security.Cryptography.HMACSHA256(secretDecoded))
            {
                byte[] output = hmac.ComputeHash(Encoding.UTF8.GetBytes(encodeString));
                hashString = Convert.ToBase64String(output);
            }

            accountsRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)";
            accountsRequest.ContentType = "application/json";
            //accountsRequest.Headers.Add("CB-ACCESS-KEY", MarketData.COINBASE_API_ID);
            accountsRequest.Headers.Add("CB-ACCESS-KEY", coinbase_bot.Properties.Settings.Default.tbAPIID);
            accountsRequest.Headers.Add("CB-ACCESS-SIGN", hashString);
            accountsRequest.Headers.Add("CB-ACCESS-TIMESTAMP", timeStamp);
            //accountsRequest.Headers.Add("CB-ACCESS-PASSPHRASE", MarketData.COINBASE_API_PASSPHRASE);
            accountsRequest.Headers.Add("CB-ACCESS-PASSPHRASE", coinbase_bot.Properties.Settings.Default.tbAPIPassphrase);

            string response = "";
            bool itWorked = false;

            try
            {
                using (WebResponse webResponse = accountsRequest.GetResponse())
                {
                    using (Stream responseStream = webResponse.GetResponseStream())
                    {
                        using (StreamReader responseReader = new StreamReader(responseStream))
                        {
                            itWorked = true;
                            response = responseReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    using (HttpWebResponse errorResponse = (HttpWebResponse)wex.Response)
                    {
                        using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            errorMsg = "EXCEPTION LOADING COINBASE ACCOUNT INFO: " + reader.ReadToEnd();
                        }
                    }
                }
            }

            if (itWorked)
            {
                List<CoinbaseAccount> accounts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CoinbaseAccount>>(response); //this works!
                if (accounts.Count != 2)
                {
                    errorMsg = "ERROR LOADING COINBASE ACCOUNT INFO.  INVALID NUMBER OF ACCOUNTS.  2 ACCOUNTS EXPECTED (BTC AND USD).  " + accounts.Count + " ACCOUNTS FOUND.";
                    retVal = false;
                }
                else
                {
                    bool foundUSD = false;
                    bool foundBTC = false;

                    foreach (CoinbaseAccount account in accounts)
                    {
                        if (account.currency == "USD")
                        {
                            foundUSD = true;
                            CoinbaseUSDAccount = account;
                        }
                        else if (account.currency == "BTC")
                        {
                            foundBTC = true;
                            CoinbaseBTCAccount = account;
                        }
                    }

                    if (foundUSD && foundBTC)
                    {
                        statusMsg = "Coinbase account info loaded successfully.  USD Balance: $" + CoinbaseUSDAccount.balance.ToString("G29") + "   BTC Balance: " + CoinbaseBTCAccount.balance.ToString("G29");
                        retVal = true;
                    }
                    else
                    {
                        retVal = false;
                        if (!foundUSD)
                        {
                            errorMsg = "ERROR LOADING COINBASE ACCOUNT INFO.  USD ACCOUNT NOT FOUND.";
                        }
                        if (!foundBTC)
                        {
                            errorMsg = "ERROR LOADING COINBASE ACCOUNT INFO.  BTC ACCOUNT NOT FOUND.";
                        }
                    }
                }
            }
            else
            {
                retVal = false;
            }
            return retVal;
        }

        /// <summary>
        /// posts a limit order and retruns the deserialized JSON data.  returns null if post failed.
        /// </summary>
        /// <param name="price"></param>
        /// <param name="size"></param>
        /// <param name="isBuyOrder">true if buying BTC, false if selling BTC</param>
        /// <returns>deserialized response</returns>
        static internal CoinbaseOrderPostResponse PostOrder(decimal size, decimal price, bool isBuyOrder)
        {
            string timeStamp = ((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString(); //timestamp must be number of seconds since unix epoch.
            string method = "POST";
            string requestPath = @"/orders";


            StringBuilder bodyBuilder = new StringBuilder(@"{
                                  ""size"": ");
            bodyBuilder.Append(size.ToString("G29"));
            bodyBuilder.Append(Environment.NewLine + "\"price\": ");
            bodyBuilder.Append(price.ToString("G29"));
            bodyBuilder.Append(Environment.NewLine + "\"side\": ");
            bodyBuilder.Append(isBuyOrder ? "buy" : "sell");
            bodyBuilder.Append(Environment.NewLine + "\"product_id\": \"BTC-USD\"");

            string body = bodyBuilder.ToString();

            byte[] data = Encoding.ASCII.GetBytes(body);

            string requestURL = MarketData.apiEndpointURL + requestPath;

            HttpWebRequest postRequest = (HttpWebRequest)WebRequest.Create(requestURL);

            //byte[] secretDecoded = Convert.FromBase64String(MarketData.COINBASE_API_SECRET);
            byte[] secretDecoded = Convert.FromBase64String(coinbase_bot.Properties.Settings.Default.tbAPISecret);
            string encodeString = timeStamp + method + requestPath + body;
            string hashString;

            using (System.Security.Cryptography.HMACSHA256 hmac = new System.Security.Cryptography.HMACSHA256(secretDecoded))
            {
                byte[] output = hmac.ComputeHash(Encoding.UTF8.GetBytes(encodeString));
                hashString = Convert.ToBase64String(output);
            }

            postRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)";
            postRequest.ContentType = "application/json";
            postRequest.Method = "POST";
            //postRequest.Headers.Add("CB-ACCESS-KEY", MarketData.COINBASE_API_ID);
            postRequest.Headers.Add("CB-ACCESS-KEY", coinbase_bot.Properties.Settings.Default.tbAPIID);
            postRequest.Headers.Add("CB-ACCESS-SIGN", hashString);
            postRequest.Headers.Add("CB-ACCESS-TIMESTAMP", timeStamp);
            //postRequest.Headers.Add("CB-ACCESS-PASSPHRASE", MarketData.COINBASE_API_PASSPHRASE);
            postRequest.Headers.Add("CB-ACCESS-PASSPHRASE", coinbase_bot.Properties.Settings.Default.tbAPIPassphrase);
            postRequest.ContentLength = data.Length;
            
            using (Stream stream = postRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            string response = "";
            bool itWorked = false;

            try
            {
                using (WebResponse webResponse = postRequest.GetResponse())
                {
                    using (Stream responseStream = webResponse.GetResponseStream())
                    {
                        using (StreamReader responseReader = new StreamReader(responseStream))
                        {
                            itWorked = true;
                            response = responseReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    using (HttpWebResponse errorResponse = (HttpWebResponse)wex.Response)
                    {
                        using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            errorMsg = "Error message received from server while posting order.  Message body: " + Environment.NewLine + body + Environment.NewLine + "Server Response: "  + reader.ReadToEnd();
                        }
                    }
                }
            }

            CoinbaseOrderPostResponse retVal = null;

            if (itWorked)
            {
                try
                {
                    retVal = Newtonsoft.Json.JsonConvert.DeserializeObject<CoinbaseOrderPostResponse>(response);
                }
                catch
                {
                    errorMsg = "Error posting order. Error deserializing server response.  Server response: " + Environment.NewLine + response;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Starts trading engine
        /// </summary>
        static internal void Start()
        {
            if (!bRunning)
            {
                bRunning = true;
                worker.RunWorkerAsync();
            }
        }

        static internal void Stop()
        {
            if (bRunning)
            {
                worker.CancelAsync();
            }
        }
    }

    internal class CoinbaseAccount
    {
        [JsonProperty("id")]
        internal string id { get; set; }

        [JsonProperty("balance")]
        internal decimal balance { get; set; }

        /// <summary>
        /// funds on hold and not availiable for use
        /// </summary>
        [JsonProperty("hold")]
        internal decimal hold { get; set; }

        /// <summary>
        /// funds available to withdraw or trade
        /// </summary>
        [JsonProperty("available")]
        internal decimal available { get; set; }

        /// <summary>
        /// ex: USD or BTC
        /// </summary>
        [JsonProperty("currency")]
        internal string currency { get; set; }
    }

    internal class CoinbaseOrderPostResponse
    {
        /// <summary>
        /// order id
        /// </summary>
        [JsonProperty("id")]
        internal string id { get; set; }

        [JsonProperty("price")]
        internal string price { get; set; }

        [JsonProperty("size")]
        internal string size { get; set; }

        [JsonProperty("product_id")]
        internal string product_id { get; set; }
        
        /// <summary>
        /// "buy" or "sell"
        /// </summary>
        [JsonProperty("side")]
        internal string side { get; set; }

        /// <summary>
        /// self trade prevention behavior.  
        /// "dc" - decrease and cancel (default)
        /// "co" - cancel oldest
        /// "cn" - cancel newest
        /// "cb" - cancel both
        /// </summary>
        [JsonProperty("stp")]
        internal string stp { get; set; }

        /// <summary>
        /// "market" or "limit"
        /// </summary>
        [JsonProperty("type")]
        internal string type { get; set; }

        /// <summary>
        /// "GTC" - good until cancelled (default)
        /// "IOC" - immediate or cancel (allows partial fills)
        /// "FOK" - fill or kill (no partial fills)
        /// </summary>
        [JsonProperty("time_in_force")]
        internal string time_in_force { get; set; }

        /// <summary>
        /// if true, order will only make liquidity, if the order would hit an existing order, it will be rejected
        /// </summary>
        [JsonProperty("post_only")]
        internal bool post_only { get; set; }

        [JsonProperty("created_at")]
        internal string created_at { get; set; }

        [JsonProperty("fill_fees")]
        internal string fill_fess { get; set; }

        [JsonProperty("filled_size")]
        internal string filled_size { get; set; }

        /// <summary>
        /// "pending", "open", "done"
        /// </summary>
        [JsonProperty("status")]
        internal string status { get; set; }

        [JsonProperty("settled")]
        internal bool settled { get; set; }
    }
}
