//todo: listen to websocket stream for owned orders

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.WebSockets;
using System.Threading;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace BitBot
{
    internal partial class ControlControl : UserControl
    {
        [ReadOnly(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        static private FileStream websocketFileStream;

        [ReadOnly(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        static private StreamWriter websocketMessageWriter;

        private const string DATETIME_FORMAT_STRING = "MM/dd/yyyy hh:mm:ss.fff";

        internal ControlControl()
        {
            InitializeComponent();
            websocketFileStream = File.Open("websocket messages.txt", FileMode.Create);
            websocketMessageWriter = new StreamWriter(websocketFileStream);
            MarketData.PropertyChangedEvent += MarketData_PropertyChangedEvent;
            TradingEngine.PropertyChangedEvent += TradingEngine_PropertyChangedEvent;
        }

        ~ControlControl()
        {
            try
            {
                websocketMessageWriter.Close();
            }
            catch
            { }

            MarketData.PropertyChangedEvent -= MarketData_PropertyChangedEvent;
            TradingEngine.PropertyChangedEvent -= TradingEngine_PropertyChangedEvent;
        }

        private void MarketData_PropertyChangedEvent(string propertyName)
        {
            switch (propertyName)
            {
                case "response":
                    if (rbDisplay.Checked)
                    {
                        if (tbStatus.InvokeRequired)
                        {
                            tbStatus.Invoke((MethodInvoker)delegate { tbStatus.Text += MarketData.response + Environment.NewLine + Environment.NewLine; });
                        }
                        else
                        {
                            tbStatus.Text += MarketData.response + Environment.NewLine + Environment.NewLine;
                        }
                    }
                    else
                    {
                        websocketMessageWriter.WriteLine(MarketData.response);
                    }
                    break;

                case "sequence":
                    if (lblSequence.InvokeRequired)
                    {
                        lblSequence.Invoke((MethodInvoker)delegate { lblSequence.Text = MarketData.sequence.ToString(); });
                    }
                    else
                    {
                        lblSequence.Text = MarketData.sequence.ToString();
                    }
                    break;

                case "errorMsg":
                    if (tbStatus.InvokeRequired)
                    {
                        tbStatus.Invoke((MethodInvoker)delegate { tbStatus.Text += MarketData.errorMsg + Environment.NewLine + Environment.NewLine; });
                    }
                    else
                    {
                        tbStatus.Text += MarketData.errorMsg + Environment.NewLine + Environment.NewLine;
                    }
                    break;

                case "statusMsg":
                    if (tbStatus.InvokeRequired)
                    {
                        tbStatus.Invoke((MethodInvoker)delegate { tbStatus.Text += MarketData.statusMsg + Environment.NewLine + Environment.NewLine; });
                    }
                    else
                    {
                        tbStatus.Text += MarketData.statusMsg + Environment.NewLine + Environment.NewLine;
                    }
                    break;
            }
        }

        void TradingEngine_PropertyChangedEvent(string propertyName)
        {
            switch (propertyName)
            {
                case "errorMsg":
                    if (tbStatus.InvokeRequired)
                    {
                        tbStatus.Invoke((MethodInvoker)delegate { tbStatus.Text += TradingEngine.errorMsg + Environment.NewLine + Environment.NewLine; });
                    }
                    else
                    {
                        tbStatus.Text += TradingEngine.errorMsg + Environment.NewLine + Environment.NewLine;
                    }
                    break;

                case "statusMsg":
                    if (tbStatus.InvokeRequired)
                    {
                        tbStatus.Invoke((MethodInvoker)delegate { tbStatus.Text += TradingEngine.statusMsg + Environment.NewLine + Environment.NewLine; });
                    }
                    else
                    {
                        tbStatus.Text += TradingEngine.statusMsg + Environment.NewLine + Environment.NewLine;
                    }
                    break;
            }
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            bool loadedAccounts = await Task.Run<bool>(() => TradingEngine.loadAccounts());
            if (loadedAccounts == false)
            {
                MessageBox.Show("ERROR LOADING ACCOUNT INFORMATION.  CONNECT ABORTED.", "BitBot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            //todo: get current orders

            tbStatus.Text += System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Connecting to Market Data Websocket Stream..." + Environment.NewLine + Environment.NewLine;

            await Task.Run(() => MarketData.Connect());

            btnDisconnect.Enabled = true;
            btnConnect.Enabled = false;

            tbStatus.Text += "Connected. " + Environment.NewLine + Environment.NewLine;

            lblCoinBaseStatus.Text = "Connected";

            TradingEngine.Start();
        }

        private async void btnDisconnect_Click(object sender, EventArgs e)
        {
            tbStatus.Text += "Disconnecting..." + Environment.NewLine + Environment.NewLine;

            await Task.Run(() => MarketData.DisConnect());

            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;

            tbStatus.Text += "Disconnected." + Environment.NewLine + Environment.NewLine;

            lblCoinBaseStatus.Text = "Disconnected";
        }

        private async void btnTestBook_Click(object sender, EventArgs e)
        {
            await Task.Run(() => doTest());
            System.IO.File.WriteAllText("TestingWebsocketMessages.txt", System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + Environment.NewLine + tbStatus.Text);
        }

        private void doTest()
        {
            if (tbStatus.InvokeRequired)
            {
                tbStatus.Invoke((MethodInvoker)delegate { tbStatus.Text += System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Order book test started manually." + Environment.NewLine; });
            }
            else
            {
                tbStatus.Text += System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Order book test started manually." + Environment.NewLine;
            }

            MarketData.queueMessages = true;
            string json = "";

            string orderBookRequestURL = "https://api.exchange.coinbase.com/products/BTC-USD/book?level=3";

            System.Net.HttpWebRequest orderBookRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(orderBookRequestURL);
            orderBookRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)";


            using (System.IO.Stream responseStream = orderBookRequest.GetResponse().GetResponseStream())
            {
                using (System.IO.StreamReader responseReader = new System.IO.StreamReader(responseStream))
                {
                    //StreamReader responseReader = File.OpenText("coinbase order book.txt"); //TEST

                    json = responseReader.ReadToEnd();
                    System.IO.File.WriteAllText("TestingNewOrderBook.txt", System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + Environment.NewLine + json);
                }
            }
            
            //each order contains a list consisting of: price, volume, order id
            CoinbaseOrderBookJSON jsonOrderBook = Newtonsoft.Json.JsonConvert.DeserializeObject<CoinbaseOrderBookJSON>(json);

            long sequence = jsonOrderBook.sequence;

            OrderBookTree newBids = new OrderBookTree();
            List<int> orderToInsertBids = new List<int>(jsonOrderBook.bids.Count);
            MarketData.FindInsertOrder(ref orderToInsertBids, 0, jsonOrderBook.bids.Count - 1);

            foreach (int i in orderToInsertBids)
            {
                decimal price;
                decimal amount;

                if (decimal.TryParse(jsonOrderBook.bids[i][0], out price) && decimal.TryParse(jsonOrderBook.bids[i][1], out amount)) //make sure numbers are valid
                {
                    newBids.Insert(new Order(jsonOrderBook.bids[i][2], null, amount, price));
                }
                else
                {
                    //errorMsg = System.DateTime.Now.ToString() + ":  Error inserting bid into binary tree.  Price: " + jsonOrderBook.bids[i][0]
                    //    + " Amount: " + jsonOrderBook.bids[i][1] + " OID: " + jsonOrderBook.bids[i][2];
                    //logWriter.WriteLine(errorMsg);
                }
            }

            OrderBookTree newAsks = new OrderBookTree();
            List<int> orderToInsertAsks = new List<int>(jsonOrderBook.asks.Count);
            MarketData.FindInsertOrder(ref orderToInsertAsks, 0, jsonOrderBook.asks.Count - 1);

            foreach (int i in orderToInsertAsks)
            {
                decimal price;
                decimal amount;

                if (decimal.TryParse(jsonOrderBook.asks[i][0], out price) && decimal.TryParse(jsonOrderBook.asks[i][1], out amount)) //make sure numbers are valid
                {
                    newAsks.Insert(new Order(jsonOrderBook.asks[i][2], null, amount, price));
                }
                else
                {
                    //errorMsg = System.DateTime.Now.ToString() + ":  Error inserting ask into binary tree.  Price: " + jsonOrderBook.asks[i][0]
                    //    + " Amount: " + jsonOrderBook.asks[i][1] + " OID: " + jsonOrderBook.asks[i][2];
                    //logWriter.WriteLine(errorMsg);
                }
            }

            DataTable dtNewBids = newBids.OutputDatatable(true);
            DataTable dtNewAsks = newAsks.OutputDatatable(false);

            bool successfulQueueProcess = MarketData.ProcessMessageQueueThroughSequenceNumber(sequence, true);

            StringBuilder sbErrorReport = new StringBuilder();

            if (!successfulQueueProcess)
            {
                sbErrorReport.Append("Error Processing Message Queue.  Test Aborted.");
            }
            else
            {
                DataTable dtOldBids = MarketData.bids.OutputDatatable(true);
                DataTable dtOldAsks = MarketData.asks.OutputDatatable(false);

                //MarketData.DisConnect();

                #region TestOrderBook

                DataTable bidsErrorTable = dtNewBids.Clone();
                bidsErrorTable.Columns.Add("statusFlag", typeof(statusFlag));
                DataTable asksErrorTable = bidsErrorTable.Clone();

                //run twice to compare both bids and asks.
                for (int i = 0; i < 2; i++)
                {
                    DataTable oldTable;
                    DataTable newTable;
                    DataTable errorTable;

                    if (i == 0) //bids
                    {
                        oldTable = dtOldBids;
                        newTable = dtNewBids;
                        errorTable = bidsErrorTable;
                    }
                    else //asks
                    {
                        oldTable = dtOldAsks;
                        newTable = dtNewAsks;
                        errorTable = asksErrorTable;
                    }

                    int oldindex = 0;
                    bool reachedEnd = oldTable.Rows.Count == 0;
                    foreach (DataRow row in newTable.Rows)
                    {
                        //get all the orders in the old table that precede the orders in the new table 
                        while (!reachedEnd
                            && (i == 0 /*bids*/ && ((decimal)oldTable.Rows[oldindex]["price"] > (decimal)row["price"]
                                || ((decimal)oldTable.Rows[oldindex]["price"] == (decimal)row["price"] && ((string)oldTable.Rows[oldindex]["oid"]).CompareTo(row["oid"]) > 0))
                                    || (i == 1 /*asks*/ && ((decimal)oldTable.Rows[oldindex]["price"] < (decimal)row["price"]
                                || ((decimal)oldTable.Rows[oldindex]["price"] == (decimal)row["price"] && ((string)oldTable.Rows[oldindex]["oid"]).CompareTo(row["oid"]) < 0)))))
                        {
                            DataRow newRow = errorTable.NewRow();
                            newRow["price"] = (decimal)oldTable.Rows[oldindex]["price"];
                            newRow["volume"] = (decimal)oldTable.Rows[oldindex]["volume"];
                            newRow["totalVolume"] = (decimal)oldTable.Rows[oldindex]["totalVolume"];
                            newRow["oid"] = (string)oldTable.Rows[oldindex]["oid"];
                            newRow["statusFlag"] = statusFlag.inOld;
                            errorTable.Rows.Add(newRow);

                            if (oldindex < oldTable.Rows.Count - 1)
                            {
                                oldindex++;
                            }
                            else
                            {
                                reachedEnd = true;
                                break;
                            }
                        }

                        //row is in new but not old
                        if (reachedEnd
                            || (i == 0 /*bids*/ && (((decimal)oldTable.Rows[oldindex]["price"] < (decimal)row["price"])
                                                       || (decimal)oldTable.Rows[oldindex]["price"] == (decimal)row["price"]
                                                                        && ((string)oldTable.Rows[oldindex]["oid"]).CompareTo((string)row["oid"]) < 0)
                            || (i == 1 /*asks*/ && (((decimal)oldTable.Rows[oldindex]["price"] > (decimal)row["price"])
                                                       || (decimal)oldTable.Rows[oldindex]["price"] == (decimal)row["price"]
                                                                        && ((string)oldTable.Rows[oldindex]["oid"]).CompareTo((string)row["oid"]) > 0))))
                        {
                            DataRow newRow = errorTable.NewRow();
                            newRow["price"] = (decimal)row["price"];
                            newRow["volume"] = (decimal)row["volume"];
                            newRow["totalVolume"] = (decimal)row["totalVolume"];
                            newRow["oid"] = (string)row["oid"];
                            newRow["statusFlag"] = statusFlag.inNew;
                            errorTable.Rows.Add(newRow);
                        }

                        else //row with same price and oid
                        {
                            if ((decimal)oldTable.Rows[oldindex]["volume"] != (decimal)row["volume"])
                            {
                                //volume is different so still an error.  add both rows to table
                                DataRow newRow = errorTable.NewRow();
                                newRow["price"] = (decimal)oldTable.Rows[oldindex]["price"];
                                newRow["volume"] = (decimal)oldTable.Rows[oldindex]["volume"];
                                newRow["totalVolume"] = (decimal)oldTable.Rows[oldindex]["totalVolume"];
                                newRow["oid"] = (string)oldTable.Rows[oldindex]["oid"];
                                newRow["statusFlag"] = statusFlag.inOld;
                                errorTable.Rows.Add(newRow);

                                DataRow newRow2 = errorTable.NewRow();
                                newRow2["price"] = (decimal)row["price"];
                                newRow2["volume"] = (decimal)row["volume"];
                                newRow2["totalVolume"] = (decimal)row["totalVolume"];
                                newRow2["oid"] = (string)row["oid"];
                                newRow2["statusFlag"] = statusFlag.inNew;
                                errorTable.Rows.Add(newRow2);
                            }
                            if (oldindex < oldTable.Rows.Count - 1)
                            {
                                oldindex++;
                            }
                            else
                            {
                                reachedEnd = true;
                            }
                        }
                    }

                    if (!reachedEnd)
                    {
                        while (oldindex < oldTable.Rows.Count) //there are some rows at the end that are in the old orderbook but not the new orderbook
                        {
                            DataRow newRow = errorTable.NewRow();
                            newRow["price"] = (decimal)oldTable.Rows[oldindex]["price"];
                            newRow["volume"] = (decimal)oldTable.Rows[oldindex]["volume"];
                            newRow["totalVolume"] = (decimal)oldTable.Rows[oldindex]["totalVolume"];
                            newRow["oid"] = (string)oldTable.Rows[oldindex]["oid"];
                            newRow["statusFlag"] = statusFlag.inOld;
                            errorTable.Rows.Add(newRow);

                            oldindex++;
                        }
                    }

                    if (i == 0)
                    {
                        bidsErrorTable = errorTable;
                    }
                    else
                    {
                        asksErrorTable = errorTable;
                    }
                }

                sbErrorReport.Append("-------BIDS-------" + Environment.NewLine + Environment.NewLine);
                if (bidsErrorTable.Rows.Count == 0)
                {
                    sbErrorReport.Append("NONE" + Environment.NewLine + Environment.NewLine);
                }
                else
                {
                    foreach (DataRow row in bidsErrorTable.Rows)
                    {
                        sbErrorReport.Append("PRICE: " + row["price"] + " VOLUME: " + row["volume"] + " TOTAL VOLUME: " + row["totalVolume"] + " ORDER ID: " + row["oid"] + " ");
                        if ((statusFlag)row["statusFlag"] == statusFlag.inOld)
                        {
                            sbErrorReport.Append("SHOULD NOT BE THERE" + Environment.NewLine);
                        }
                        else
                        {
                            sbErrorReport.Append("IS MISSING" + Environment.NewLine);
                        }
                    }
                }

                sbErrorReport.Append("-------ASKS-------" + Environment.NewLine + Environment.NewLine);
                if (asksErrorTable.Rows.Count == 0)
                {
                    sbErrorReport.Append("NONE" + Environment.NewLine + Environment.NewLine);
                }
                else
                {
                    foreach (DataRow row in asksErrorTable.Rows)
                    {
                        sbErrorReport.Append("PRICE: " + row["price"] + " VOLUME: " + row["volume"] + " TOTAL VOLUME: " + row["totalVolume"] + " ORDER ID: " + row["oid"] + " ");
                        if ((statusFlag)row["statusFlag"] == statusFlag.inOld)
                        {
                            sbErrorReport.Append("SHOULD NOT BE THERE" + Environment.NewLine);
                        }
                        else
                        {
                            sbErrorReport.Append("IS MISSING" + Environment.NewLine);
                        }
                    }
                }

                if (asksErrorTable.Rows.Count == 0 && bidsErrorTable.Rows.Count == 0)
                {
                    if (tbStatus.InvokeRequired)
                    {
                        tbStatus.Invoke((MethodInvoker)delegate { tbStatus.Text += System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Order Book Test Passed." + Environment.NewLine; });
                    }
                    else
                    {
                        tbStatus.Text += System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Order Book Test Passed." + Environment.NewLine;
                    }
                }
                else
                {
                    string nowFileSafe = System.DateTime.Now.ToString("MMddyyyy_hhmmss");

                    try
                    {
                        File.Copy("LoadedOrderBook.txt", "ManualTestOldOrderBook_" + nowFileSafe);
                        File.Copy("TestingNewOrderBook.txt", "ManualTestNewOrderBook_" + nowFileSafe);
                        File.Copy("websocket messages.txt", "ManualTestWebsocketMessages_" + nowFileSafe);
                    }
                    catch
                    { }

                    if (tbStatus.InvokeRequired)
                    {
                        tbStatus.Invoke((MethodInvoker)delegate
                        {
                            tbStatus.Text += System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Order book test failed.  Details in ManualTestOldOrderBook_" + nowFileSafe
                                + " , ManualTestNewOrderBook_" + nowFileSafe + ", ManualTestWebsocketMessages_" + nowFileSafe + "." + Environment.NewLine;
                        });
                    }
                    else
                    {
                        tbStatus.Text += System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Order book test failed.  Details in ManualTestOldOrderBook_" + nowFileSafe
                      + " , ManualTestNewOrderBook_" + nowFileSafe + ", ManualTestWebsocketMessages_" + nowFileSafe + "." + Environment.NewLine;
                    }
                }
            }

            System.IO.File.WriteAllText("ErrorReport.txt", System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + Environment.NewLine + sbErrorReport.ToString());

            #endregion TestOrderBook
            MarketData.queueMessages = false;

            MarketData.workerRunningEvent.WaitOne();
            MarketData.ProcessingMessageEvent.Wait();
            MarketData.ProcessingQueueEvent.Wait();
            websocketMessageWriter.Flush();
        }

        enum statusFlag
        {
            inOld,
            inNew,
        };

        private void btnTestMessage_Click(object sender, EventArgs e)
        {
            string timeStamp = ((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString(); //timestamp must be number of seconds since unix epoch.
            string method = "POST";
            string requestPath = @"/orders";
            string body = @"{
                                ""size"": ""0.01"",
                                ""price"": ""123.21"",
                                ""side"": ""buy"",
                                ""product_id"": ""BTC-USD""
                            }";

            byte[] data = Encoding.ASCII.GetBytes(body);
                                
            string testURL = MarketData.apiEndpointURL + requestPath;

            HttpWebRequest testRequest = (HttpWebRequest)WebRequest.Create(testURL);

            //byte[] secretDecoded = Convert.FromBase64String(MarketData.COINBASE_API_SECRET);
            byte[] secretDecoded = Convert.FromBase64String(coinbase_bot.Properties.Settings.Default.tbAPISecret);
            string encodeString = timeStamp + method + requestPath + body;
            string hashString;

            using (System.Security.Cryptography.HMACSHA256 hmac = new System.Security.Cryptography.HMACSHA256(secretDecoded))
            {
                byte[] output = hmac.ComputeHash(Encoding.UTF8.GetBytes(encodeString));
                hashString = Convert.ToBase64String(output);
            }

            testRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)";
            testRequest.ContentType = "application/json";
            testRequest.Method = "POST";
            //testRequest.Headers.Add("CB-ACCESS-KEY", MarketData.COINBASE_API_ID);
            testRequest.Headers.Add("CB-ACCESS-KEY", coinbase_bot.Properties.Settings.Default.tbAPIID);
            testRequest.Headers.Add("CB-ACCESS-SIGN", hashString);
            testRequest.Headers.Add("CB-ACCESS-TIMESTAMP", timeStamp);
            //testRequest.Headers.Add("CB-ACCESS-PASSPHRASE", MarketData.COINBASE_API_PASSPHRASE);
            testRequest.Headers.Add("CB-ACCESS-PASSPHRASE", coinbase_bot.Properties.Settings.Default.tbAPIPassphrase);
            testRequest.ContentLength = data.Length;
            using (Stream stream = testRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }


            string response = "";
            bool itWorked = false;

            try
            {
                using (WebResponse webResponse = testRequest.GetResponse())
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
                            response = "EXCEPTION: " + reader.ReadToEnd();
                        }
                    }
                }
            }

            if (itWorked)
            {
               
            }

            //todo: handle json
            tbStatus.Text += response + Environment.NewLine;
        }
    }
}
