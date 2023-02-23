//Data class used for updating market Data

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Net;
using System.Threading;
using System.Data;
using Newtonsoft.Json;

using System.ComponentModel;

namespace BitBot
{
    internal static class MarketData
    {
        internal delegate void PropertyChangedDelegate(string propertyName);
        static internal event PropertyChangedDelegate PropertyChangedEvent;

        static internal ManualResetEvent workerRunningEvent = new ManualResetEvent(true); //true means other threads can proceed.
        static private ManualResetEvent waitForWebsocketConnectedEvent = new ManualResetEvent(false);
        static internal ManualResetEventSlim ProcessingMessageEvent = new ManualResetEventSlim(true);
        static internal ManualResetEventSlim ProcessingQueueEvent = new ManualResetEventSlim(true);

        static private bool initialized = false;
        static internal string apiEndpointURL = "https://api.exchange.coinbase.com";
        static private System.ComponentModel.BackgroundWorker bwMarketUpdater = new System.ComponentModel.BackgroundWorker();

        static private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>(); //used to queue messages for processing such as when updating order book

        static private FileStream logFileStream;
        static private StreamWriter logWriter;
        static private StringBuilder messageHistory = new StringBuilder();

        static private DateTime timeLastOrderBookLoadTry = new DateTime();

        static private long firstCoinbaseSequenceNumber = 0; //coinbase sequence number when order book is loaded
        static private bool bTestingAndReloadingOrderBook = false;
        static private bool bLoadingOrderBook = false;

        //static internal string COINBASE_API_ID = "";
        //static internal string COINBASE_API_SECRET = "";
        //static internal string COINBASE_API_PASSPHRASE = "";

        private const string ORDER_BOOK_OUTPUT_FILENAME = "LoadedOrderBook.txt";
        private const string DATETIME_FORMAT_STRING = "MM/dd/yyyy hh:mm:ss.fff";

        //changed to application settings
        //private const long RELOAD_COINBASE_ORDERBOOK_FREQUENCY = 10000; //reload coinbase order book every this many sequence numbers
        //static private TimeSpan MIN_TIME_TO_WAIT_BETWEEN_ORDERBOOK_POLLS = new TimeSpan(0, 7, 3); //hours, minutes, seconds

        static internal OrderBookTree bids = new OrderBookTree();
        static internal OrderBookTree asks = new OrderBookTree();

        internal enum ProcessWebsocketMessageResponse
        {
            success,
            fail,
            maxSequenceReachedForQueueProcessing,
            previousSequenceNumberEncountered,
            timeToReloadOrderBook
        };

        enum statusFlag
        {
            inOld,
            inNew,
        };

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

        private static long _sequence = 0;
        internal static long sequence
        {
            get { return _sequence; }
            private set
            {
                _sequence = value;
                if (PropertyChangedEvent != null)
                {
                    PropertyChangedEvent("sequence");
                }
            }
        }

        private static bool _bWorkerRunning = false;
        internal static bool bWorkerRunning
        {
            get { return _bWorkerRunning; }
            private set
            {
                _bWorkerRunning = value;
                if (PropertyChangedEvent != null)
                {
                    PropertyChangedEvent("bWorkerRunning");
                }
            }
        }

        private static string _response = "";
        internal static string response
        {
            get { return _response; }
            private set
            {
                _response = value;
                if (PropertyChangedEvent != null)
                {
                    PropertyChangedEvent("response");
                }
                messageHistory.Append(value + Environment.NewLine);
            }
        }

        internal static bool bConnecting { get; private set; }

        /// <summary>
        /// DO NOT CHANGE FROM UI THREAD
        /// </summary>
        private static bool _queueMessages = false;
        internal static bool queueMessages
        {

            get
            {
                return _queueMessages;
            }

            set
            {
                if (_queueMessages == false && value == true)
                {
                    _queueMessages = value;
                    ProcessingQueueEvent.Wait();
                    ProcessingMessageEvent.Wait();
                }
                else if (_queueMessages == true && value == false && messageQueue != null && messageQueue.IsEmpty == false)
                {
                    _queueMessages = value;
                    if (!ProcessMessageQueue()) //fail in one of the queued messages
                    {
                        loadOrderBook();
                    }
                }
                else
                {
                    _queueMessages = value;
                }
            }
        }

        static MarketData()
        {


        }

        static internal void OnClose()
        {
            logWriter.Close();
        }

        static internal void Initialize()
        {
            if (!initialized)
            {
                bwMarketUpdater.WorkerSupportsCancellation = true;
                bwMarketUpdater.DoWork += marketUpdater_DoWork;
                bConnecting = false;
                logFileStream = File.Open("log.txt", FileMode.Append);
                logWriter = new StreamWriter(logFileStream);
                initialized = true;
            }
        }

        /// <summary>
        /// Loads order book and connects to Websocket feed if not already connected.
        /// </summary>
        /// <returns></returns>
        static internal void Connect()
        {
            messageHistory.Clear();
            while (!CheckForInternetConnection())
            {
                errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error, no internet connection detected.  retrying in 60 seconds.";
                System.Threading.Thread.Sleep(60000);
            }

            if (!bConnecting)
            {
                bConnecting = true;
                //await Task.Run(() => loadOrderBook());
                loadOrderBook();
            }
        }


        /// <summary>
        /// gets full order book from website and puts it in the passed OrderBookJSON variable
        /// </summary>
        /// <param name="jsonOrderBook"></param>
        /// <returns></returns>
        private static bool getOrderBookJSON(out CoinbaseOrderBookJSON jsonOrderBook)
        {
            bool didItWork = true;
            string json = "";
            string orderBookRequestURL = apiEndpointURL + @"/products/BTC-USD/book?level=3";
            jsonOrderBook = new CoinbaseOrderBookJSON();

            HttpWebRequest orderBookRequest = (HttpWebRequest)WebRequest.Create(orderBookRequestURL);
            orderBookRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)";

            try
            {
                using (Stream responseStream = orderBookRequest.GetResponse().GetResponseStream())
                {
                    using (StreamReader responseReader = new StreamReader(responseStream))
                    {
                        //StreamReader responseReader = File.OpenText("coinbase order book.txt"); //TEST

                        json = responseReader.ReadToEnd();
                        System.IO.File.WriteAllText(ORDER_BOOK_OUTPUT_FILENAME, System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + Environment.NewLine + json);
                    }
                }
            }
            catch (Exception)
            {
                didItWork = false;
                errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error getting order book.";
                logWriter.WriteLine(errorMsg);
            }

            if (didItWork)
            {
                try
                {
                    //each order contains a list consisting of: price, volume, order id
                    jsonOrderBook = JsonConvert.DeserializeObject<CoinbaseOrderBookJSON>(json);
                }
                catch
                {
                    errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error deserializing order book";
                    logWriter.WriteLine(errorMsg);
                    didItWork = false;
                }
            }
            return didItWork;
        }

        static private void loadOrderBook()
        {
            bLoadingOrderBook = true;
            queueMessages = true;

            //if (timeLastOrderBookLoadTry != new System.DateTime() && System.DateTime.Now - timeLastOrderBookLoadTry < MIN_TIME_TO_WAIT_BETWEEN_ORDERBOOK_POLLS)
            //{
            //    statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Waiting " 
            //        + (MIN_TIME_TO_WAIT_BETWEEN_ORDERBOOK_POLLS - (System.DateTime.Now - timeLastOrderBookLoadTry)).TotalSeconds + " seconds to reload order book";
            //    //keep going if the timespan becomes negative in the time it takes to print this
            //    try
            //    {
            //        System.Threading.Thread.Sleep(MIN_TIME_TO_WAIT_BETWEEN_ORDERBOOK_POLLS - (System.DateTime.Now - timeLastOrderBookLoadTry));
            //    }
            //    catch
            //    { }
            //}
            if (timeLastOrderBookLoadTry != new System.DateTime() && (System.DateTime.Now - timeLastOrderBookLoadTry).TotalSeconds < coinbase_bot.Properties.Settings.Default.cbMinTimeReload)
            {
                int secondsToSleep = coinbase_bot.Properties.Settings.Default.cbMinTimeReload - (int)(System.DateTime.Now - timeLastOrderBookLoadTry).TotalSeconds;

                statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Waiting " + secondsToSleep + " seconds to reload order book";
                System.Threading.Thread.Sleep(new TimeSpan(0, 0, secondsToSleep));
            }
            timeLastOrderBookLoadTry = System.DateTime.Now;
            statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Loading order book...";
            bool didItWork;

            do
            {
                CoinbaseOrderBookJSON jsonOrderBook;
                didItWork = true;

                if (!bWorkerRunning)
                {
                    bwMarketUpdater.RunWorkerAsync();
                }

                waitForWebsocketConnectedEvent.WaitOne(); //wait until we get the first websocket message

                messageQueue = new ConcurrentQueue<string>(); //clear the message queue to avoid processing messages we don't have to

                didItWork = getOrderBookJSON(out jsonOrderBook);

                if (didItWork)
                {
                    statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Order book loading complete.  Sequence = " + jsonOrderBook.sequence;

                    firstCoinbaseSequenceNumber = jsonOrderBook.sequence;

                    buildOrderBookTree(ref jsonOrderBook);
                }

            } while (!didItWork);

            queueMessages = false; //queue is auto-processed when this is set
            bConnecting = false;
            bLoadingOrderBook = false;
        }

        static private bool testAndReloadOrderBook()
        {
            bTestingAndReloadingOrderBook = true;
            queueMessages = true;
            statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Testing and Reloading Orderbook.";
            bool retVal;

            ProcessingMessageEvent.Wait();
            ProcessingQueueEvent.Wait();
            statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Done waiting for message processing.";

            //if (timeLastOrderBookLoadTry != new System.DateTime() && System.DateTime.Now - timeLastOrderBookLoadTry < MIN_TIME_TO_WAIT_BETWEEN_ORDERBOOK_POLLS)
            //{
            //    statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Waiting " + (MIN_TIME_TO_WAIT_BETWEEN_ORDERBOOK_POLLS - (System.DateTime.Now - timeLastOrderBookLoadTry)).TotalSeconds + " seconds to reload order book";
            //    //keep going if the timespan becomes negative in the time it takes to print this
            //    try
            //    {
            //        System.Threading.Thread.Sleep(MIN_TIME_TO_WAIT_BETWEEN_ORDERBOOK_POLLS - (System.DateTime.Now - timeLastOrderBookLoadTry));
            //    }
            //    catch
            //    { }
            //}
            if (timeLastOrderBookLoadTry != new System.DateTime() && (System.DateTime.Now - timeLastOrderBookLoadTry).TotalSeconds < coinbase_bot.Properties.Settings.Default.cbMinTimeReload)
            {
                int secondsToSleep = coinbase_bot.Properties.Settings.Default.cbMinTimeReload - (int)(System.DateTime.Now - timeLastOrderBookLoadTry).TotalSeconds;

                statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Waiting " + secondsToSleep + " seconds to reload order book";
                System.Threading.Thread.Sleep(new TimeSpan(0, 0, secondsToSleep));
            }

            timeLastOrderBookLoadTry = System.DateTime.Now;

            //copy current order book to string for output to file if error occurs
            string tempOrderBookText = "";
            try
            {
                tempOrderBookText = File.ReadAllText(ORDER_BOOK_OUTPUT_FILENAME);
            }
            catch { }

            CoinbaseOrderBookJSON newOrderBookJSON;
            
            //if (!getOrderBookJSON(out newOrderBookJSON))
            //{
            //    errorMsg = System.DateTime.Now.ToString() + " Error getting new order book JSON during scheduled reload.";
            //    logWriter.WriteLine(errorMsg);
            //    return false;
            //}

            bool getOrderBookSuccess = true;
            do
            {
                getOrderBookSuccess = getOrderBookJSON(out newOrderBookJSON);

                if (getOrderBookSuccess)
                {
                    statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Order book reload complete. ";
                }
                else
                {
                    errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error getting new order book JSON during scheduled reload.  Waiting 60 seconds to try again.";
                    logWriter.WriteLine(errorMsg);

                    System.Threading.Thread.Sleep(60000);
                }

            } while (getOrderBookSuccess == false);

            bool successfulQueueProcess = ProcessMessageQueueThroughSequenceNumber(newOrderBookJSON.sequence);

            if (!successfulQueueProcess)
            {
                errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Skipping Order Book Comparison Test and Reloading Order Book.";
                buildOrderBookTree(ref newOrderBookJSON);
                retVal = false;
            }
            else
            {
                DataTable dtOldBids = bids.OutputDatatable(true);
                DataTable dtOldAsks = asks.OutputDatatable(false);

                buildOrderBookTree(ref newOrderBookJSON);

                DataTable dtNewBids = bids.OutputDatatable(true);
                DataTable dtNewAsks = asks.OutputDatatable(false);

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

                StringBuilder sbErrorReport = new StringBuilder();

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
                if (asksErrorTable.Rows.Count > 0 || bidsErrorTable.Rows.Count > 0)
                {
                    retVal = false;
                    DateTime now = System.DateTime.Now;
                    System.IO.File.WriteAllText("ErrorReport_" + now.ToString("MMddyyyy'-'HHmmss") + ".txt", System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + Environment.NewLine + sbErrorReport.ToString());
                    errorMsg = now.ToString(DATETIME_FORMAT_STRING) + ":  Old orderbook does not match new orderbook during scheduled full reload.  Details in ErrorReport_"
                        + now.ToString("MMddyyyy'-'HHmmss") + ".txt, OrderBookOld_ErrorReport_" + now.ToString("MMddyyyy'-'HHmmss") + ".txt, " + "OrderBookNew_ErrorReport_"
                        + now.ToString("MMddyyyy'-'HHmmss") + ".txt, Messages_ErrorReport_" + now.ToString("MMddyyyy'-'HHmmss") + ".txt";
                    logWriter.WriteLine(errorMsg);

                    System.IO.File.WriteAllText("Messages_ErrorReport_" + now.ToString("MMddyyyy'-'HHmmss") + ".txt", messageHistory.ToString());
                    System.IO.File.WriteAllText("OrderBookOld_ErrorReport_" + now.ToString("MMddyyyy'-'HHmmss") + ".txt", tempOrderBookText);
                    System.IO.File.Copy(ORDER_BOOK_OUTPUT_FILENAME, "OrderBookNew_ErrorReport_" + now.ToString("MMddyyyy'-'HHmmss") + ".txt", true);
                }
                else
                {
                    statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Order Book Test Complete.  Resutlt: PASS";
                    retVal = true;
                }
            }

            messageHistory.Clear(); //clear websocket messages from history

            #endregion TestOrderBook
            MarketData.queueMessages = false;

            MarketData.ProcessingMessageEvent.Wait();
            MarketData.ProcessingQueueEvent.Wait();

            bTestingAndReloadingOrderBook = false;
            return retVal;
        }


        /// <summary>
        /// takes a json order book and loads it into two new binary trees.
        /// </summary>
        /// <param name="jsonOrderBook"></param>
        static internal void buildOrderBookTree(ref CoinbaseOrderBookJSON jsonOrderBook)
        {
            sequence = jsonOrderBook.sequence;
            bids = new OrderBookTree();
            List<int> orderToInsertBids = new List<int>(jsonOrderBook.bids.Count);
            FindInsertOrder(ref orderToInsertBids, 0, jsonOrderBook.bids.Count - 1);

            foreach (int i in orderToInsertBids)
            {
                decimal price;
                decimal amount;

                if (decimal.TryParse(jsonOrderBook.bids[i][0], out price) && decimal.TryParse(jsonOrderBook.bids[i][1], out amount)) //make sure numbers are valid
                {
                    bids.Insert(new Order(jsonOrderBook.bids[i][2], null, amount, price));
                }
                else
                {
                    errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error inserting bid into binary tree.  Price: " + jsonOrderBook.bids[i][0]
                        + " Amount: " + jsonOrderBook.bids[i][1] + " OID: " + jsonOrderBook.bids[i][2];
                    logWriter.WriteLine(errorMsg);
                }
            }

            asks = new OrderBookTree();
            List<int> orderToInsertAsks = new List<int>(jsonOrderBook.asks.Count);
            FindInsertOrder(ref orderToInsertAsks, 0, jsonOrderBook.asks.Count - 1);

            foreach (int i in orderToInsertAsks)
            {
                decimal price;
                decimal amount;

                if (decimal.TryParse(jsonOrderBook.asks[i][0], out price) && decimal.TryParse(jsonOrderBook.asks[i][1], out amount)) //make sure numbers are valid
                {
                    asks.Insert(new Order(jsonOrderBook.asks[i][2], null, amount, price));
                }
                else
                {
                    errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error inserting ask into binary tree.  Price: " + jsonOrderBook.asks[i][0]
                        + " Amount: " + jsonOrderBook.asks[i][1] + " OID: " + jsonOrderBook.asks[i][2];
                    logWriter.WriteLine(errorMsg);
                }
            }
        }

        /// <summary>
        /// DO NOT CALL FROM UI THREAD
        /// </summary>
        internal static bool ProcessMessageQueue()
        {
            int messageCount = 0;

            statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Beginning Message Queue Processing";
            ProcessingMessageEvent.Wait();
            ProcessingQueueEvent.Reset();
            string message;
            while (messageQueue.TryDequeue(out message))
            {
                messageCount++;
                ProcessWebsocketMessageResponse response = ProcessWebsocketMessage(message);
                if (response == ProcessWebsocketMessageResponse.fail)
                {
                    ProcessingQueueEvent.Set();
                    statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Failed Message Processing during Complete Queue Processing.  Queue Processing Aborted. ";
                    return false;
                }
                else if (response == ProcessWebsocketMessageResponse.timeToReloadOrderBook)
                {
                    ProcessingQueueEvent.Set();
                    statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Time to reload order book encountered during Complete Queue Processing.  Queue Processing Aborted. ";
                    return false;
                }
            }
            ProcessingQueueEvent.Set();
            statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Done with Message Queue Processing.  " + messageCount + " messages processed.";
            return true;
        }

        /// <summary>
        ///  DO NOT CALL FROM UI THREAD
        /// </summary>
        /// <param name="sequenceNum"></param>
        internal static bool ProcessMessageQueueThroughSequenceNumber(long sequenceNum, bool ignoreTimeToReload = false)
        {
            bool reachedEndMessage = false;
            int messageCount = 0;

            statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Beginning Message Queue Processing through sequence number " + sequenceNum;

            ProcessingMessageEvent.Wait();
            ProcessingQueueEvent.Reset();
            string message;
            //while (messageQueue.TryDequeue(out message))
            while (messageQueue.TryPeek(out message))
            {
                ProcessWebsocketMessageResponse response = ProcessWebsocketMessage(message, sequenceNum, true);
                if (response == ProcessWebsocketMessageResponse.maxSequenceReachedForQueueProcessing)
                {
                    reachedEndMessage = true;
                    break;
                }
                else
                {
                    if (response == ProcessWebsocketMessageResponse.fail)
                    {
                        ProcessingQueueEvent.Set();
                        statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Failed Message Processing during Queue Processing to Sequence Number.  Queue Processing Aborted. ";
                        return false;
                    }
                    else if (response == ProcessWebsocketMessageResponse.timeToReloadOrderBook && ignoreTimeToReload == false)
                    {
                        ProcessingQueueEvent.Set();
                        statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Time to reload order book encountered during Queue Processing to Sequence Number.  Queue Processing Aborted. ";
                        return false;
                    }
                    messageCount++;
                    messageQueue.TryDequeue(out message);
                }
            }

            ProcessingQueueEvent.Set();

            StringBuilder sbMSG = new StringBuilder(System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Message Queue Processing complete. " + messageCount + " messages processed. ");
            if (reachedEndMessage)
            {
                sbMSG.Append("End of queue reached. ");
            }
            else
            {
                sbMSG.Append("End of queue not reached. ");
            }
            statusMsg = sbMSG.ToString();
            return true;
        }

        /// <summary>
        /// Processes a message that was received over websocket connection
        /// </summary>
        /// <param name="message"></param>
        internal static ProcessWebsocketMessageResponse ProcessWebsocketMessage(string message, long lastSeqNum = long.MaxValue, bool ignoreReload = false)
        {
            message = message.Substring(0, message.IndexOf('}') + 1); //fix for server side message transmission error adding redundant message fragments at end
            CoinbaseMessageJSON jsonMessage = null;
            try
            {
                jsonMessage = JsonConvert.DeserializeObject<CoinbaseMessageJSON>(message);
            }
            catch
            {
                errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error deserializing websocket message JSON.  Message: " + message;
                logWriter.WriteLine(errorMsg);
                return ProcessWebsocketMessageResponse.fail;
            }

            decimal amount;
            decimal price;


            //if (jsonMessage.sequence > sequence && jsonMessage.sequence <= lastSeqNum)
            if (jsonMessage.sequence <= sequence)
            {
                return ProcessWebsocketMessageResponse.previousSequenceNumberEncountered;
            }
            else if (jsonMessage.sequence > lastSeqNum)
            {
                return ProcessWebsocketMessageResponse.maxSequenceReachedForQueueProcessing;
            }

            ProcessWebsocketMessageResponse retVal;

            //TODO: how to handle missing sequence numbers...reload book for now.  reevaluate if frequent out of order messages occur
            if (jsonMessage.sequence != sequence + 1)
            {
                errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error: sequence number missing.  Number: " + jsonMessage.sequence + " expected: " + (sequence + 1);
                logWriter.WriteLine(errorMsg);
                sequence = jsonMessage.sequence;
                return ProcessWebsocketMessageResponse.fail;
            }

            sequence = jsonMessage.sequence;

            switch (jsonMessage.type)
            {
                case "received":
                    retVal = ProcessWebsocketMessageResponse.success;
                    break;

                case "done":

                    if (jsonMessage.order_type == "market")
                    {
                        retVal = ProcessWebsocketMessageResponse.success; //just ignore these market order messages.  they never get added to the order book so we don't need to do anything with them.
                    }
                    else if (decimal.TryParse(jsonMessage.price, out price))
                    {
                        if (jsonMessage.reason != "filled") //fills are also broadcasted as "match" messages.  handled there to also handle partial fills
                        {
                            if (jsonMessage.side == "buy")
                            {
                                lock (bids)
                                {
                                    if (!bids.Remove(jsonMessage.order_id, price)) //returns false if fail.
                                    {
                                        errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error removing order from \"buy\" order book.  OID: " + jsonMessage.order_id + " Price: " + price;
                                        logWriter.WriteLine(errorMsg);
                                        retVal = ProcessWebsocketMessageResponse.fail;
                                    }
                                    else
                                    {
                                        retVal = ProcessWebsocketMessageResponse.success;
                                    }
                                }
                            }
                            else
                            {
                                lock (asks)
                                {
                                    if (!asks.Remove(jsonMessage.order_id, price)) //returns false if fail.
                                    {
                                        errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error removing order from \"sell\" order book.  OID: " + jsonMessage.order_id + " Price: " + price;
                                        logWriter.WriteLine(errorMsg);
                                        retVal = ProcessWebsocketMessageResponse.fail;
                                    }
                                    else
                                    {
                                        retVal = ProcessWebsocketMessageResponse.success;
                                    }
                                }
                            }
                        }
                        else
                        {
                            retVal = ProcessWebsocketMessageResponse.success;
                        }
                    }
                    else
                    {
                        errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error parsing \"done\" websocket message price.  Message: " + message;
                        logWriter.WriteLine(errorMsg);
                        retVal = ProcessWebsocketMessageResponse.fail;
                    }
                    break;

                case "open":

                    if (decimal.TryParse(jsonMessage.remaining_size, out amount) && decimal.TryParse(jsonMessage.price, out price))
                    {
                        if (jsonMessage.side == "buy")
                        {
                            lock (bids)
                            {
                                bids.Insert(new Order(jsonMessage.order_id, null, amount, price));
                            }
                        }
                        else
                        {
                            lock (asks)
                            {
                                asks.Insert(new Order(jsonMessage.order_id, null, amount, price));
                            }
                        }
                        retVal = ProcessWebsocketMessageResponse.success;
                    }
                    else
                    {
                        errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error parsing \"open\" websocket message amount and price.  Message: " + message;
                        logWriter.WriteLine(errorMsg);
                        retVal = ProcessWebsocketMessageResponse.fail;
                    }
                    break;

                case "match":
                    if (decimal.TryParse(jsonMessage.price, out price) && decimal.TryParse(jsonMessage.size, out amount))
                    {
                        if (jsonMessage.side == "buy")
                        {
                            lock (bids)
                            {
                                if (!bids.Remove(jsonMessage.maker_order_id, amount, price)) //returns false if fail.
                                {
                                    errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error removing order from \"buy\" order book.  OID: " + jsonMessage.order_id + " Price: " + price;
                                    logWriter.WriteLine(errorMsg);
                                    retVal = ProcessWebsocketMessageResponse.fail;
                                }
                                else
                                {
                                    retVal = ProcessWebsocketMessageResponse.success;
                                }
                            }
                        }
                        else
                        {
                            lock (asks)
                            {
                                if (!asks.Remove(jsonMessage.maker_order_id, amount, price)) //returns false if fail.
                                {
                                    errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error removing order from \"sell\" order book.  OID: " + jsonMessage.order_id + " Price: " + price;
                                    logWriter.WriteLine(errorMsg);
                                    retVal = ProcessWebsocketMessageResponse.fail;
                                }
                                else
                                {
                                    retVal = ProcessWebsocketMessageResponse.success;
                                }
                            }
                        }
                    }
                    else
                    {
                        errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error parsing \"match\" websocket message price or amount.  Message: " + message;
                        logWriter.WriteLine(errorMsg);
                        retVal = ProcessWebsocketMessageResponse.fail;
                    }
                    break;

                case "change":
                    decimal oldSize;
                    decimal newSize;
                    if (decimal.TryParse(jsonMessage.old_size, out oldSize) && decimal.TryParse(jsonMessage.new_size, out newSize) && decimal.TryParse(jsonMessage.price, out price))
                    {
                        if (jsonMessage.side == "buy")
                        {
                            lock (bids)
                            {
                                //remove change in value from existing order.  
                                //This may return false during normal operation since these messages can be received for orders that have been received but do not have a corresponding "open" message yet.
                                bids.Remove(jsonMessage.order_id, oldSize - newSize, price);
                            }
                        }
                        else
                        {
                            lock (asks)
                            {
                                //remove change in value from existing order.  
                                //This may return false during normal operation since these messages can be received for orders that have been received but do not have a corresponding "open" message yet.
                                asks.Remove(jsonMessage.order_id, oldSize - newSize, price);
                            }
                        }
                        retVal = ProcessWebsocketMessageResponse.success;
                    }
                    else
                    {
                        errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error parsing \"change\" websocket message old/new size and price.  Message: " + message;
                        logWriter.WriteLine(errorMsg);
                        retVal = ProcessWebsocketMessageResponse.fail;
                    }
                    break;

                case "error":
                    errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Websocket Error message received.  Message: " + message;
                    logWriter.WriteLine(errorMsg);
                    retVal = ProcessWebsocketMessageResponse.success;
                    break;

                default:
                    errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Websocket message type not recognized.  Message: " + message;
                    logWriter.WriteLine(errorMsg);
                    retVal = ProcessWebsocketMessageResponse.fail;
                    break;
            }

            //TEST
            //if (sequence % 100 == 0)
            //    retVal = ProcessWebsocketMessageResponse.fail;
            //ENDTEST

            //reload the order book every RELOAD_COINBASE_ORDERBOOK_FREQUENCY sequence numbers
            //if (((jsonMessage.sequence - firstCoinbaseSequenceNumber) + RELOAD_COINBASE_ORDERBOOK_FREQUENCY * .80) % RELOAD_COINBASE_ORDERBOOK_FREQUENCY == 0 && (jsonMessage.sequence - firstCoinbaseSequenceNumber) != 0)  //TEST CODE.  REDUCES TIME TO FIRST RELOAD OF ORDERBOOK BY 80%
            //if ((jsonMessage.sequence - firstCoinbaseSequenceNumber) % RELOAD_COINBASE_ORDERBOOK_FREQUENCY == 0 && (jsonMessage.sequence - firstCoinbaseSequenceNumber) != 0)
            if (coinbase_bot.Properties.Settings.Default.cbMessageTrigger && (jsonMessage.sequence - firstCoinbaseSequenceNumber) % coinbase_bot.Properties.Settings.Default.cbAutoReloadMessages == 0 && (jsonMessage.sequence - firstCoinbaseSequenceNumber) != 0)
            {
                statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Order book reload triggered due to " 
                    + coinbase_bot.Properties.Settings.Default.cbAutoReloadMessages + " websocket messages received since last reload.  Sequence = " + jsonMessage.sequence;
                retVal = ProcessWebsocketMessageResponse.timeToReloadOrderBook;
            }
            else if (coinbase_bot.Properties.Settings.Default.cbTimeTrigger && (System.DateTime.Now - timeLastOrderBookLoadTry).TotalSeconds > coinbase_bot.Properties.Settings.Default.cbAutoReloadTime)
            {
                statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Order book reload triggered due to "
                    + coinbase_bot.Properties.Settings.Default.cbAutoReloadTime + " seconds elapsed since last reload.  Sequence = " + jsonMessage.sequence;
                retVal = ProcessWebsocketMessageResponse.timeToReloadOrderBook;
            }

            return retVal;
        }

        /// <summary>
        /// recursive function that takes a minimuim and maximum index of a list of orders.  Finds the order to add them to a binary tree so that it is balanced.
        /// </summary>
        /// <param name="orderToInsert"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        internal static void FindInsertOrder(ref List<int> orderToInsert, int min, int max)
        {
            if (max - min < 0)
            {
                return;
            }
            else if (max == min)
            {
                orderToInsert.Add(max);
            }
            else
            {
                int numToAdd = min + (max - min) / 2;
                orderToInsert.Add(numToAdd);
                FindInsertOrder(ref orderToInsert, min, numToAdd - 1);
                FindInsertOrder(ref orderToInsert, numToAdd + 1, max);
            }
        }

        /// <summary>
        /// Disconnects from the Websocket feed if it is connected.
        /// </summary>
        /// <returns></returns>
        static internal bool DisConnect()
        {
            if (bWorkerRunning)
            {
                bwMarketUpdater.CancelAsync();
            }
            logWriter.Flush();
            return true; //TODO: return false if fail
        }

        /// <summary>
        /// used for testing.  loads an order book from a text file.
        /// </summary>
        /// <returns></returns>
        static internal void loadOrderBookFromFile(string fileName)
        {
            StreamReader responseReader = File.OpenText(fileName);

            string inputString = responseReader.ReadToEnd();

            CoinbaseOrderBookJSON jsonOrderBook = JsonConvert.DeserializeObject<CoinbaseOrderBookJSON>(inputString);

            buildOrderBookTree(ref jsonOrderBook);
        }

        static private async void marketUpdater_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            bool firstMessage = true;
            //do this first
            bWorkerRunning = true;
            workerRunningEvent.Reset();

            //this.Invoke((MethodInvoker)delegate
            //{
            //    tbStatus.AppendText("Backround Worker Started." + Environment.NewLine + Environment.NewLine);
            //    lblCoinBaseStatus.Text = "Connected";
            //});

            ClientWebSocket coinBaseWebSocket = new ClientWebSocket();

            CancellationToken coinBaseSocketCancellationToken = new CancellationToken();

            bool success;
            
            do
            {
                success = true;
                try
                {
                    await coinBaseWebSocket.ConnectAsync(new Uri("wss://ws-feed.exchange.coinbase.com"), coinBaseSocketCancellationToken);
                }
                catch
                {
                    success = false;
                }
            } while (!success);

//Additional information: Unable to connect to the remote server on close

            string subscribeMessage = @"{
                                            ""type"": ""subscribe"",
                                            ""product_id"": ""BTC-USD""
                                        }";

            byte[] subscribeBytes = System.Text.Encoding.UTF8.GetBytes(subscribeMessage);

            await coinBaseWebSocket.SendAsync(new ArraySegment<byte>(subscribeBytes), WebSocketMessageType.Text, true, coinBaseSocketCancellationToken);

            byte[] receiveBuffer = new byte[65536];

            while (!bwMarketUpdater.CancellationPending)
            {
                ProcessWebsocketMessageResponse messageProcessingResult = ProcessWebsocketMessageResponse.success;

                if (coinBaseWebSocket.State != WebSocketState.Open)
                {
                    errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error.  Unexpected websocket reset.";

                    coinBaseWebSocket.Dispose();
                    workerRunningEvent.Set();
                    bWorkerRunning = false;
                    statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " attempting to reconnect...";
                    Connect();
                    return;
                }
                try
                {
                    WebSocketReceiveResult socketResult = await coinBaseWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), coinBaseSocketCancellationToken);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error.  Exception trying to receive websocket message. " + ex.Message;
                    }
                    else
                    {
                        errorMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " Error.  Exception trying to receive websocket message. "
                      + ex.Message + " Inner Exception: " + ex.InnerException.Message;
                    }

                    coinBaseWebSocket.Dispose();
                    workerRunningEvent.Set();
                    bWorkerRunning = false;
                    statusMsg = System.DateTime.Now.ToString(DATETIME_FORMAT_STRING) + " attempting to reconnect...";
                    Connect();
                    return;
                }

                ProcessingMessageEvent.Reset();
                response = Encoding.UTF8.GetString(receiveBuffer).TrimEnd('\0');

                if (!string.IsNullOrWhiteSpace(response))
                {

                    if (bConnecting || queueMessages || bTestingAndReloadingOrderBook || bLoadingOrderBook)
                    {
                        messageQueue.Enqueue(response);
                    }
                    else
                    {
                        messageProcessingResult = ProcessWebsocketMessage(response);
                    }

                    if (firstMessage)
                    {
                        waitForWebsocketConnectedEvent.Set();
                        firstMessage = false;
                    }
                }

                //appropriate log entries written in ProcessWebsocketMessage()
                if (messageProcessingResult == ProcessWebsocketMessageResponse.fail) 
                {
                    messageHistory.Clear();
#pragma warning disable 4014 //disable warning about not awaiting when starting new threads
                    Task.Run(() => loadOrderBook()); //fire and continue so we can keep handling websocket messages
                }
                //appropriate log entries written in ProcessWebsocketMessage()
                else if (messageProcessingResult == ProcessWebsocketMessageResponse.timeToReloadOrderBook)
                {
                    Task.Run(() => testAndReloadOrderBook()); //fire and continue so we can keep handling websocket messages
                }
#pragma warning restore 4014

                ProcessingMessageEvent.Set();
            }

            string unSubscribeMessage = @"{
                                            ""type"": ""unsubscribe"",
                                            ""product_id"": ""BTC-USD""
                                        }";

            byte[] unSubscribeBytes = System.Text.Encoding.UTF8.GetBytes(unSubscribeMessage);

            await coinBaseWebSocket.SendAsync(new ArraySegment<byte>(unSubscribeBytes), WebSocketMessageType.Text, true, coinBaseSocketCancellationToken);

            await coinBaseWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Output Close Requested", coinBaseSocketCancellationToken);

            coinBaseWebSocket.Dispose();

            //do this last
            workerRunningEvent.Set();
            bWorkerRunning = false;
        }

        static internal bool CheckForInternetConnection()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    using (Stream stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}