using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
//using DiffMatchPatch;

namespace BitBot
{
    public partial class TestControl : UserControl
    {
        private bool bStepping = false;

        StreamReader testWebsocketReader;

        //used for highlighting differences
        //diff_match_patch diffFinder = new diff_match_patch();

        DataTable oldbids;
        DataTable oldasks;
        DataTable newbids;
        DataTable newasks;

        struct TextSelectChunk
        {
            public int start;
            public int length;
            public Color color;

            public TextSelectChunk(int start, int length, Color color)
            {
                this.start = start;
                this.length = length;
                this.color = color;
            }
        }

        enum statusFlag
        {
            inCurrent,
            inReference,
            inBoth
        };

        public TestControl()
        {
            InitializeComponent();
            MarketData.PropertyChangedEvent += MarketData_PropertyChangedEvent;

        }

        string oldText = "";
        string newText = "";

        //[ReadOnly(true)]
        //[Browsable(false)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

        private void MarketData_PropertyChangedEvent(string propertyName)
        {
            switch (propertyName)
            {
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
                    if (tbStatusMessages.InvokeRequired)
                    {
                        tbStatusMessages.Invoke((MethodInvoker)delegate { tbStatusMessages.Text += MarketData.errorMsg + Environment.NewLine + Environment.NewLine; });
                    }
                    else
                    {
                        tbStatusMessages.Text += MarketData.errorMsg + Environment.NewLine + Environment.NewLine;
                    }
                    break;
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (bStepping)
            {
                Step();
            }
            else
            {
                Run();
            }
        }

        private void Run()
        {

            //load end state order book into the binary tree and export back to text 
            MarketData.loadOrderBookFromFile("OrderBookNew.txt");

            //DataTable bidsTable = MarketData.bids.OutputDatatable(true);
            //DataTable asksTable = MarketData.asks.OutputDatatable(false);

            newbids = MarketData.bids.OutputDatatable(true);
            newasks = MarketData.asks.OutputDatatable(false);

            tbNewBook.Text = "-------BIDS-------" + Environment.NewLine + Environment.NewLine;
            foreach (DataRow row in newbids.Rows)
            {
                tbNewBook.AppendText("BID PRICE: " + row["price"].ToString().TrimEnd('0') + " VOLUME: " + row["volume"].ToString().TrimEnd('0') + " OID: " + row["oid"].ToString()
                    + Environment.NewLine + Environment.NewLine);
            }

            tbNewBook.Text += "-------ASKS-------" + Environment.NewLine + Environment.NewLine;
            foreach (DataRow row in newasks.Rows)
            {
                tbNewBook.AppendText("ASK PRICE: " + row["price"].ToString().TrimEnd('0') + " VOLUME: " + row["volume"].ToString().TrimEnd('0') + " OID: " + row["oid"].ToString()
                    + Environment.NewLine + Environment.NewLine);
            }


            //load start state order book into the binary tree and export back to text
            MarketData.loadOrderBookFromFile("OrderBookOld.txt");

            oldbids = MarketData.bids.OutputDatatable(true);
            oldasks = MarketData.asks.OutputDatatable(false);

            tbOldBook.Text = "-------BIDS-------" + Environment.NewLine + Environment.NewLine;
            foreach (DataRow row in oldbids.Rows)
            {
                tbOldBook.AppendText("BID PRICE: " + row["price"].ToString().TrimEnd('0') + " VOLUME: " + row["volume"].ToString().TrimEnd('0') + " OID: " + row["oid"].ToString()
                    + Environment.NewLine + Environment.NewLine);
            }

            tbOldBook.Text += "-------ASKS-------" + Environment.NewLine + Environment.NewLine;
            foreach (DataRow row in oldasks.Rows)
            {
                tbOldBook.AppendText("ASK PRICE: " + row["price"].ToString().TrimEnd('0') + " VOLUME: " + row["volume"].ToString().TrimEnd('0') + " OID: " + row["oid"].ToString()
                    + Environment.NewLine + Environment.NewLine);
            }

            //process test websocket messages and append to text box
            testWebsocketReader = File.OpenText("TestWebsocketMessagesFile.txt");

            oldText = tbOldBook.Text;
            newText = tbNewBook.Text;

            if (cbStepThrough.Checked)
            {
                btnRun.Text = "Step";
                bStepping = true;

                Step();
            }
            else
            {
                while (!testWebsocketReader.EndOfStream)
                {
                    Step();
                }
            }
        }

        //step through another websocket message
        private void Step()
        {
            string msg = testWebsocketReader.ReadLine();
            if (msg == null) //end of file
            {
                return;
            }
            else
            {
                while (string.IsNullOrWhiteSpace(msg))
                {
                    msg = testWebsocketReader.ReadLine();
                    if (msg == null) //end of file
                    {
                        return;
                    }
                }

                tbStatusMessages.Text += msg + Environment.NewLine + Environment.NewLine;

                MarketData.ProcessWebsocketMessage(msg);

                DataTable dtBids = MarketData.bids.OutputDatatable(true);
                DataTable dtAsks = MarketData.asks.OutputDatatable(false);

                StringBuilder newCurrentText = new StringBuilder();

                newCurrentText.Append("-------BIDS-------" + Environment.NewLine + Environment.NewLine);
                
                foreach (DataRow row in dtBids.Rows)
                {
                    newCurrentText.Append("BID PRICE: " + row["price"].ToString().TrimEnd('0') + " VOLUME: " + row["volume"].ToString().TrimEnd('0') + " OID: " + row["oid"].ToString()
                        + Environment.NewLine + Environment.NewLine);
                }

                newCurrentText.Append("-------ASKS-------" + Environment.NewLine + Environment.NewLine);
                foreach (DataRow row in dtAsks.Rows)
                {
                    newCurrentText.Append("ASK PRICE: " + row["price"].ToString().TrimEnd('0') + " VOLUME: " + row["volume"].ToString().TrimEnd('0') + " OID: " + row["oid"].ToString()
                        + Environment.NewLine + Environment.NewLine);
                }

                tbCurrentBook.Text = newCurrentText.ToString();



                //get differences between current/old and current/new

                DataTable dtOldBidsOutput = dtBids.Clone();
                dtOldBidsOutput.Columns.Add("statusFlag", typeof(statusFlag));

                int curIdx = 0;
                bool reachedEnd = dtBids.Rows.Count == 0;
                foreach (DataRow row in oldbids.Rows)
                {
                    //add rows only in current till we get to the right price and oid
                    while (!reachedEnd && ((decimal)dtBids.Rows[curIdx]["price"] > (decimal)row["price"] || 
                        ((decimal)dtBids.Rows[curIdx]["price"] == (decimal)row["price"] && ((string)dtBids.Rows[curIdx]["oid"]).CompareTo((string)row["oid"]) > 0)))
                    {
                        DataRow newRow = dtOldBidsOutput.NewRow();
                        newRow["price"] = (decimal)dtBids.Rows[curIdx]["price"];
                        newRow["volume"] = (decimal)dtBids.Rows[curIdx]["volume"];
                        newRow["totalVolume"] = (decimal)dtBids.Rows[curIdx]["totalVolume"];
                        newRow["oid"] = (string)dtBids.Rows[curIdx]["oid"];
                        newRow["statusFlag"] = statusFlag.inCurrent;
                        dtOldBidsOutput.Rows.Add(newRow);

                        if (curIdx < dtBids.Rows.Count - 1)
                        {
                            curIdx++;
                        }
                        else
                        {
                            reachedEnd = true;
                            break;
                        }
                    }

                    //looks like this row isn't in the current version at all
                    if (reachedEnd || (decimal)dtBids.Rows[curIdx]["price"] < (decimal)row["price"] || 
                        ((decimal)dtBids.Rows[curIdx]["price"] == (decimal)row["price"] && ((string)dtBids.Rows[curIdx]["oid"]).CompareTo((string)row["oid"]) < 0))
                    {
                        DataRow newRow = dtOldBidsOutput.NewRow();
                        newRow["price"] = (decimal)row["price"];
                        newRow["volume"] = (decimal)row["volume"];
                        newRow["totalVolume"] = (decimal)row["totalVolume"];
                        newRow["oid"] = (string)row["oid"];
                        newRow["statusFlag"] = statusFlag.inReference;
                        dtOldBidsOutput.Rows.Add(newRow);
                    }

                    //we got a row with the same price and oid
                    else
                    {
                        if ((decimal)dtBids.Rows[curIdx]["volume"] == (decimal)row["volume"])
                        {
                            DataRow newRow = dtOldBidsOutput.NewRow();
                            newRow["price"] = (decimal)row["price"];
                            newRow["volume"] = (decimal)row["volume"];
                            newRow["totalVolume"] = (decimal)row["totalVolume"];
                            newRow["oid"] = (string)row["oid"];
                            newRow["statusFlag"] = statusFlag.inBoth;
                            dtOldBidsOutput.Rows.Add(newRow);
                        }
                        else //same oid but different volume so add both
                        {
                            DataRow newRow = dtOldBidsOutput.NewRow();
                            newRow["price"] = (decimal)dtBids.Rows[curIdx]["price"];
                            newRow["volume"] = (decimal)dtBids.Rows[curIdx]["volume"];
                            newRow["totalVolume"] = (decimal)dtBids.Rows[curIdx]["totalVolume"];
                            newRow["oid"] = (string)dtBids.Rows[curIdx]["oid"];
                            newRow["statusFlag"] = statusFlag.inCurrent;
                            dtOldBidsOutput.Rows.Add(newRow);

                            DataRow newRow2 = dtOldBidsOutput.NewRow();
                            newRow2["price"] = (decimal)row["price"];
                            newRow2["volume"] = (decimal)row["volume"];
                            newRow2["totalVolume"] = (decimal)row["totalVolume"];
                            newRow2["oid"] = (string)row["oid"];
                            newRow2["statusFlag"] = statusFlag.inReference;
                            dtOldBidsOutput.Rows.Add(newRow2);
                        }
                        if (curIdx < dtBids.Rows.Count - 1)
                        {
                            curIdx++;
                        }
                        else
                        {
                            reachedEnd = true;
                        }
                    }
                }
                if (!reachedEnd)
                {
                    while (curIdx < dtBids.Rows.Count) //there are some rows at the end that are in the old orderbook but not the new orderbook
                    {
                        DataRow newRow = dtOldBidsOutput.NewRow();
                        newRow["price"] = (decimal)dtBids.Rows[curIdx]["price"];
                        newRow["volume"] = (decimal)dtBids.Rows[curIdx]["volume"];
                        newRow["totalVolume"] = (decimal)dtBids.Rows[curIdx]["totalVolume"];
                        newRow["oid"] = (string)dtBids.Rows[curIdx]["oid"];
                        newRow["statusFlag"] = statusFlag.inReference;
                        dtOldBidsOutput.Rows.Add(newRow);

                        curIdx++;
                    }
                }

                DataTable dtOldAsksOutput = dtAsks.Clone();
                dtOldAsksOutput.Columns.Add("statusFlag", typeof(statusFlag));

                curIdx = 0;
                reachedEnd = dtAsks.Rows.Count == 0;
                foreach (DataRow row in oldasks.Rows)
                {
                    //add rows only in current till we get to the right price and oid
                    while (!reachedEnd && ((decimal)dtAsks.Rows[curIdx]["price"] < (decimal)row["price"] ||
                        ((decimal)dtAsks.Rows[curIdx]["price"] == (decimal)row["price"] && ((string)dtAsks.Rows[curIdx]["oid"]).CompareTo((string)row["oid"]) < 0)))
                    {
                        DataRow newRow = dtOldAsksOutput.NewRow();
                        newRow["price"] = (decimal)dtAsks.Rows[curIdx]["price"];
                        newRow["volume"] = (decimal)dtAsks.Rows[curIdx]["volume"];
                        newRow["totalVolume"] = (decimal)dtAsks.Rows[curIdx]["totalVolume"];
                        newRow["oid"] = (string)dtAsks.Rows[curIdx]["oid"];
                        newRow["statusFlag"] = statusFlag.inCurrent;
                        dtOldAsksOutput.Rows.Add(newRow);

                        if (curIdx < dtAsks.Rows.Count - 1)
                        {
                            curIdx++;
                        }
                        else
                        {
                            reachedEnd = true;
                            break;
                        }
                    }

                    //looks like this row isn't in the current version at all
                    if (reachedEnd || (decimal)dtAsks.Rows[curIdx]["price"] > (decimal)row["price"] ||
                        ((decimal)dtAsks.Rows[curIdx]["price"] == (decimal)row["price"] && ((string)dtAsks.Rows[curIdx]["oid"]).CompareTo((string)row["oid"]) > 0))
                    {
                        DataRow newRow = dtOldAsksOutput.NewRow();
                        newRow["price"] = (decimal)row["price"];
                        newRow["volume"] = (decimal)row["volume"];
                        newRow["totalVolume"] = (decimal)row["totalVolume"];
                        newRow["oid"] = (string)row["oid"];
                        newRow["statusFlag"] = statusFlag.inReference;
                        dtOldAsksOutput.Rows.Add(newRow);
                    }

                    //we got a row with the same price and oid
                    else
                    {
                        if ((decimal)dtAsks.Rows[curIdx]["volume"] == (decimal)row["volume"])
                        {
                            DataRow newRow = dtOldAsksOutput.NewRow();
                            newRow["price"] = (decimal)row["price"];
                            newRow["volume"] = (decimal)row["volume"];
                            newRow["totalVolume"] = (decimal)row["totalVolume"];
                            newRow["oid"] = (string)row["oid"];
                            newRow["statusFlag"] = statusFlag.inBoth;
                            dtOldAsksOutput.Rows.Add(newRow);
                        }
                        else //same oid but different volume so add both
                        {
                            DataRow newRow = dtOldAsksOutput.NewRow();
                            newRow["price"] = (decimal)dtAsks.Rows[curIdx]["price"];
                            newRow["volume"] = (decimal)dtAsks.Rows[curIdx]["volume"];
                            newRow["totalVolume"] = (decimal)dtAsks.Rows[curIdx]["totalVolume"];
                            newRow["oid"] = (string)dtAsks.Rows[curIdx]["oid"];
                            newRow["statusFlag"] = statusFlag.inCurrent;
                            dtOldAsksOutput.Rows.Add(newRow);

                            DataRow newRow2 = dtOldAsksOutput.NewRow();
                            newRow2["price"] = (decimal)row["price"];
                            newRow2["volume"] = (decimal)row["volume"];
                            newRow2["totalVolume"] = (decimal)row["totalVolume"];
                            newRow2["oid"] = (string)row["oid"];
                            newRow2["statusFlag"] = statusFlag.inReference;
                            dtOldAsksOutput.Rows.Add(newRow2);
                        }
                        if (curIdx < dtAsks.Rows.Count - 1)
                        {
                            curIdx++;
                        }
                        else
                        {
                            reachedEnd = true;
                        }
                    }
                }
                if (!reachedEnd)
                {
                    while (curIdx < dtAsks.Rows.Count) //there are some rows at the end that are in the old orderbook but not the new orderbook
                    {
                        DataRow newRow = dtOldAsksOutput.NewRow();
                        newRow["price"] = (decimal)dtAsks.Rows[curIdx]["price"];
                        newRow["volume"] = (decimal)dtAsks.Rows[curIdx]["volume"];
                        newRow["totalVolume"] = (decimal)dtAsks.Rows[curIdx]["totalVolume"];
                        newRow["oid"] = (string)dtAsks.Rows[curIdx]["oid"];
                        newRow["statusFlag"] = statusFlag.inReference;
                        dtOldAsksOutput.Rows.Add(newRow);

                        curIdx++;
                    }
                }

                DataTable dtNewBidsOutput = dtBids.Clone();
                dtNewBidsOutput.Columns.Add("statusFlag", typeof(statusFlag));

                curIdx = 0;
                reachedEnd = dtBids.Rows.Count == 0;
                foreach (DataRow row in newbids.Rows)
                {
                    //add rows only in current till we get to the right price and oid
                    while (!reachedEnd && ((decimal)dtBids.Rows[curIdx]["price"] > (decimal)row["price"] ||
                        ((decimal)dtBids.Rows[curIdx]["price"] == (decimal)row["price"] && ((string)dtBids.Rows[curIdx]["oid"]).CompareTo((string)row["oid"]) > 0)))
                    {
                        DataRow newRow = dtNewBidsOutput.NewRow();
                        newRow["price"] = (decimal)dtBids.Rows[curIdx]["price"];
                        newRow["volume"] = (decimal)dtBids.Rows[curIdx]["volume"];
                        newRow["totalVolume"] = (decimal)dtBids.Rows[curIdx]["totalVolume"];
                        newRow["oid"] = (string)dtBids.Rows[curIdx]["oid"];
                        newRow["statusFlag"] = statusFlag.inCurrent;
                        dtNewBidsOutput.Rows.Add(newRow);

                        if (curIdx < dtBids.Rows.Count - 1)
                        {
                            curIdx++;
                        }
                        else
                        {
                            reachedEnd = true;
                            break;
                        }
                    }

                    //looks like this row isn't in the current version at all
                    if (reachedEnd || (decimal)dtBids.Rows[curIdx]["price"] < (decimal)row["price"] ||
                        ((decimal)dtBids.Rows[curIdx]["price"] == (decimal)row["price"] && ((string)dtBids.Rows[curIdx]["oid"]).CompareTo((string)row["oid"]) < 0))
                    {
                        DataRow newRow = dtNewBidsOutput.NewRow();
                        newRow["price"] = (decimal)row["price"];
                        newRow["volume"] = (decimal)row["volume"];
                        newRow["totalVolume"] = (decimal)row["totalVolume"];
                        newRow["oid"] = (string)row["oid"];
                        newRow["statusFlag"] = statusFlag.inReference;
                        dtNewBidsOutput.Rows.Add(newRow);
                    }

                    //we got a row with the same price and oid
                    else
                    {
                        if ((decimal)dtBids.Rows[curIdx]["volume"] == (decimal)row["volume"])
                        {
                            DataRow newRow = dtNewBidsOutput.NewRow();
                            newRow["price"] = (decimal)row["price"];
                            newRow["volume"] = (decimal)row["volume"];
                            newRow["totalVolume"] = (decimal)row["totalVolume"];
                            newRow["oid"] = (string)row["oid"];
                            newRow["statusFlag"] = statusFlag.inBoth;
                            dtNewBidsOutput.Rows.Add(newRow);
                        }
                        else //same oid but different volume so add both
                        {
                            DataRow newRow = dtNewBidsOutput.NewRow();
                            newRow["price"] = (decimal)dtBids.Rows[curIdx]["price"];
                            newRow["volume"] = (decimal)dtBids.Rows[curIdx]["volume"];
                            newRow["totalVolume"] = (decimal)dtBids.Rows[curIdx]["totalVolume"];
                            newRow["oid"] = (string)dtBids.Rows[curIdx]["oid"];
                            newRow["statusFlag"] = statusFlag.inCurrent;
                            dtNewBidsOutput.Rows.Add(newRow);

                            DataRow newRow2 = dtNewBidsOutput.NewRow();
                            newRow2["price"] = (decimal)row["price"];
                            newRow2["volume"] = (decimal)row["volume"];
                            newRow2["totalVolume"] = (decimal)row["totalVolume"];
                            newRow2["oid"] = (string)row["oid"];
                            newRow2["statusFlag"] = statusFlag.inReference;
                            dtNewBidsOutput.Rows.Add(newRow2);
                        }
                        if (curIdx < dtBids.Rows.Count - 1)
                        {
                            curIdx++;
                        }
                        else
                        {
                            reachedEnd = true;
                        }
                    }
                }
                if (!reachedEnd)
                {
                    while (curIdx < dtBids.Rows.Count) //there are some rows at the end that are in the old orderbook but not the new orderbook
                    {
                        DataRow newRow = dtNewBidsOutput.NewRow();
                        newRow["price"] = (decimal)dtBids.Rows[curIdx]["price"];
                        newRow["volume"] = (decimal)dtBids.Rows[curIdx]["volume"];
                        newRow["totalVolume"] = (decimal)dtBids.Rows[curIdx]["totalVolume"];
                        newRow["oid"] = (string)dtBids.Rows[curIdx]["oid"];
                        newRow["statusFlag"] = statusFlag.inReference;
                        dtNewBidsOutput.Rows.Add(newRow);

                        curIdx++;
                    }
                }

                DataTable dtNewAsksOutput = dtAsks.Clone();
                dtNewAsksOutput.Columns.Add("statusFlag", typeof(statusFlag));

                curIdx = 0;
                reachedEnd = dtAsks.Rows.Count == 0;
                foreach (DataRow row in newasks.Rows)
                {
                    //add rows only in current till we get to the right price and oid
                    while (!reachedEnd && ((decimal)dtAsks.Rows[curIdx]["price"] < (decimal)row["price"] ||
                        ((decimal)dtAsks.Rows[curIdx]["price"] == (decimal)row["price"] && ((string)dtAsks.Rows[curIdx]["oid"]).CompareTo((string)row["oid"]) < 0)))
                    {
                        DataRow newRow = dtNewAsksOutput.NewRow();
                        newRow["price"] = (decimal)dtAsks.Rows[curIdx]["price"];
                        newRow["volume"] = (decimal)dtAsks.Rows[curIdx]["volume"];
                        newRow["totalVolume"] = (decimal)dtAsks.Rows[curIdx]["totalVolume"];
                        newRow["oid"] = (string)dtAsks.Rows[curIdx]["oid"];
                        newRow["statusFlag"] = statusFlag.inCurrent;
                        dtNewAsksOutput.Rows.Add(newRow);

                        if (curIdx < dtAsks.Rows.Count - 1)
                        {
                            curIdx++;
                        }
                        else
                        {
                            reachedEnd = true;
                            break;
                        }
                    }

                    //looks like this row isn't in the current version at all
                    if (reachedEnd || (decimal)dtAsks.Rows[curIdx]["price"] > (decimal)row["price"] ||
                        ((decimal)dtAsks.Rows[curIdx]["price"] == (decimal)row["price"] && ((string)dtAsks.Rows[curIdx]["oid"]).CompareTo((string)row["oid"]) > 0))
                    {
                        DataRow newRow = dtNewAsksOutput.NewRow();
                        newRow["price"] = (decimal)row["price"];
                        newRow["volume"] = (decimal)row["volume"];
                        newRow["totalVolume"] = (decimal)row["totalVolume"];
                        newRow["oid"] = (string)row["oid"];
                        newRow["statusFlag"] = statusFlag.inReference;
                        dtNewAsksOutput.Rows.Add(newRow);
                    }

                    //we got a row with the same price and oid
                    else
                    {
                        if ((decimal)dtAsks.Rows[curIdx]["volume"] == (decimal)row["volume"])
                        {
                            DataRow newRow = dtNewAsksOutput.NewRow();
                            newRow["price"] = (decimal)row["price"];
                            newRow["volume"] = (decimal)row["volume"];
                            newRow["totalVolume"] = (decimal)row["totalVolume"];
                            newRow["oid"] = (string)row["oid"];
                            newRow["statusFlag"] = statusFlag.inBoth;
                            dtNewAsksOutput.Rows.Add(newRow);
                        }
                        else //same oid but different volume so add both
                        {
                            DataRow newRow = dtNewAsksOutput.NewRow();
                            newRow["price"] = (decimal)dtAsks.Rows[curIdx]["price"];
                            newRow["volume"] = (decimal)dtAsks.Rows[curIdx]["volume"];
                            newRow["totalVolume"] = (decimal)dtAsks.Rows[curIdx]["totalVolume"];
                            newRow["oid"] = (string)dtAsks.Rows[curIdx]["oid"];
                            newRow["statusFlag"] = statusFlag.inCurrent;
                            dtNewAsksOutput.Rows.Add(newRow);

                            DataRow newRow2 = dtNewAsksOutput.NewRow();
                            newRow2["price"] = (decimal)row["price"];
                            newRow2["volume"] = (decimal)row["volume"];
                            newRow2["totalVolume"] = (decimal)row["totalVolume"];
                            newRow2["oid"] = (string)row["oid"];
                            newRow2["statusFlag"] = statusFlag.inReference;
                            dtNewAsksOutput.Rows.Add(newRow2);
                        }
                        if (curIdx < dtAsks.Rows.Count - 1)
                        {
                            curIdx++;
                        }
                        else
                        {
                            reachedEnd = true;
                        }
                    }
                }
                if (!reachedEnd)
                {
                    while (curIdx < dtAsks.Rows.Count) //there are some rows at the end that are in the old orderbook but not the new orderbook
                    {
                        DataRow newRow = dtNewAsksOutput.NewRow();
                        newRow["price"] = (decimal)dtAsks.Rows[curIdx]["price"];
                        newRow["volume"] = (decimal)dtAsks.Rows[curIdx]["volume"];
                        newRow["totalVolume"] = (decimal)dtAsks.Rows[curIdx]["totalVolume"];
                        newRow["oid"] = (string)dtAsks.Rows[curIdx]["oid"];
                        newRow["statusFlag"] = statusFlag.inReference;
                        dtNewAsksOutput.Rows.Add(newRow);

                        curIdx++;
                    }
                }

                List<TextSelectChunk> oldChunks = new List<TextSelectChunk>();

                StringBuilder sbOldBook = new StringBuilder();

                sbOldBook.Append("-------BIDS-------" + Environment.NewLine + Environment.NewLine);
                int newLines = 2; // when selecting text in a textbox, newlines are counted as one character, however in a string they are counted as two characters

                bool bExcludeSame = false; //skip matches in output textbox
                if (cbExcludeSame.Checked)
                {
                    bExcludeSame = true;
                }

                foreach (DataRow row in dtOldBidsOutput.Rows)
                {
                    if (!bExcludeSame || (statusFlag)row["statusFlag"] != statusFlag.inBoth) //don't do anything if it is a match and the skip matches textbox is checked
                    {
                        string textToAdd = "BID PRICE: " + row["price"].ToString().TrimEnd('0') + " VOLUME: " + row["volume"].ToString().TrimEnd('0') + " OID: " + row["oid"].ToString()
                            + Environment.NewLine + Environment.NewLine;

                        if ((statusFlag)row["statusFlag"] == statusFlag.inReference)
                        {
                            oldChunks.Add(new TextSelectChunk(sbOldBook.Length - newLines, textToAdd.Length - 2, Color.LightSalmon));
                        }
                        else if ((statusFlag)row["statusFlag"] == statusFlag.inCurrent)
                        {
                            oldChunks.Add(new TextSelectChunk(sbOldBook.Length - newLines, textToAdd.Length - 2, Color.LightGreen));
                        }
                        newLines += 2;
                        sbOldBook.Append(textToAdd);
                    }
                }

                sbOldBook.Append("-------ASKS-------" + Environment.NewLine + Environment.NewLine);
                newLines += 2;
                foreach (DataRow row in dtOldAsksOutput.Rows)
                {
                    if (!bExcludeSame || (statusFlag)row["statusFlag"] != statusFlag.inBoth) //don't do anything if it is a match and the skip matches textbox is checked
                    {
                        string textToAdd = "ASK PRICE: " + row["price"].ToString().TrimEnd('0') + " VOLUME: " + row["volume"].ToString().TrimEnd('0') + " OID: " + row["oid"].ToString()
                            + Environment.NewLine + Environment.NewLine;

                        if ((statusFlag)row["statusFlag"] == statusFlag.inReference)
                        {
                            oldChunks.Add(new TextSelectChunk(sbOldBook.Length - newLines, textToAdd.Length - 2, Color.LightSalmon));
                        }
                        else if ((statusFlag)row["statusFlag"] == statusFlag.inCurrent)
                        {
                            oldChunks.Add(new TextSelectChunk(sbOldBook.Length - newLines, textToAdd.Length - 2, Color.LightGreen));
                        }
                        newLines += 2;
                        sbOldBook.Append(textToAdd);
                    }
                }

                List<TextSelectChunk> newChunks = new List<TextSelectChunk>();

                StringBuilder sbNewBook = new StringBuilder();

                sbNewBook.Append("-------BIDS-------" + Environment.NewLine + Environment.NewLine);
                newLines = 2;
                foreach (DataRow row in dtNewBidsOutput.Rows)
                {
                    if (!bExcludeSame || (statusFlag)row["statusFlag"] != statusFlag.inBoth) //don't do anything if it is a match and the skip matches textbox is checked
                    {
                        string textToAdd = "BID PRICE: " + row["price"].ToString().TrimEnd('0') + " VOLUME: " + row["volume"].ToString().TrimEnd('0') + " OID: " + row["oid"].ToString()
                            + Environment.NewLine + Environment.NewLine;

                        if ((statusFlag)row["statusFlag"] == statusFlag.inReference)
                        {
                            newChunks.Add(new TextSelectChunk(sbNewBook.Length - newLines, textToAdd.Length - 2, Color.LightSalmon));
                        }
                        else if ((statusFlag)row["statusFlag"] == statusFlag.inCurrent)
                        {
                            newChunks.Add(new TextSelectChunk(sbNewBook.Length - newLines, textToAdd.Length - 2, Color.LightGreen));
                        }
                        newLines += 2;
                        sbNewBook.Append(textToAdd);
                    }
                }

                sbNewBook.Append("-------ASKS-------" + Environment.NewLine + Environment.NewLine);
                newLines += 2;
                foreach (DataRow row in dtNewAsksOutput.Rows)
                {
                    if (!bExcludeSame || (statusFlag)row["statusFlag"] != statusFlag.inBoth) //don't do anything if it is a match and the skip matches textbox is checked
                    {
                        string textToAdd = "ASK PRICE: " + row["price"].ToString().TrimEnd('0') + " VOLUME: " + row["volume"].ToString().TrimEnd('0') + " OID: " + row["oid"].ToString()
                            + Environment.NewLine + Environment.NewLine;

                        if ((statusFlag)row["statusFlag"] == statusFlag.inReference)
                        {
                            newChunks.Add(new TextSelectChunk(sbNewBook.Length - newLines, textToAdd.Length - 2, Color.LightSalmon));
                        }
                        else if ((statusFlag)row["statusFlag"] == statusFlag.inCurrent)
                        {
                            newChunks.Add(new TextSelectChunk(sbNewBook.Length - newLines, textToAdd.Length - 2, Color.LightGreen));
                        }
                        newLines += 2;
                        sbNewBook.Append(textToAdd);
                    }
                }


                tbOldBook.Text = sbOldBook.ToString();
                tbNewBook.Text = sbNewBook.ToString();

                foreach (TextSelectChunk chunk in oldChunks)
                {
                    tbOldBook.Select(chunk.start, chunk.length);
                    tbOldBook.SelectionBackColor = chunk.color;
                }
                foreach (TextSelectChunk chunk in newChunks)
                {
                    tbNewBook.Select(chunk.start, chunk.length);
                    tbNewBook.SelectionBackColor = chunk.color;
                }

                //List<Diff> OldToCurrentDiffs = diffFinder.diff_main(oldText, tbCurrentBook.Text);
                //diffFinder.diff_cleanupSemanticLossless(OldToCurrentDiffs);

                //List<Diff> NewToCurrentDiffs = diffFinder.diff_main(newText, tbCurrentBook.Text);
                //diffFinder.diff_cleanupSemanticLossless(NewToCurrentDiffs);

                ////highlight differences in old and new textboxes
                ////light salmon = missing in current version
                ////light green = added in current version (missing from old/new version)

                //tbOldBook.Text = "";
                //int totalLength = 0;
                //foreach (Diff diff in OldToCurrentDiffs)
                //{
                //    tbOldBook.AppendText(diff.text);
                //    if (diff.operation == Operation.DELETE) //in old version but missing in current version
                //    {
                //        tbOldBook.Select(totalLength, diff.text.Length);
                //        tbOldBook.SelectionBackColor = Color.LightSalmon;
                //    }
                //    else if (diff.operation == Operation.INSERT) //in current version but missing in old version
                //    {
                //        tbOldBook.Select(totalLength, diff.text.Length);
                //        tbOldBook.SelectionBackColor = Color.LightGreen;
                //    }
                //    totalLength += diff.text.Length;
                //}

                //tbNewBook.Text = "";
                //totalLength = 0;
                //foreach (Diff diff in NewToCurrentDiffs)
                //{
                //    tbNewBook.AppendText(diff.text);
                //    if (diff.operation == Operation.DELETE) //in old version but missing in current version
                //    {
                //        tbNewBook.Select(totalLength, diff.text.Length);
                //        tbNewBook.SelectionBackColor = Color.LightSalmon;
                //    }
                //    else if (diff.operation == Operation.INSERT)
                //    {
                //        tbNewBook.Select(totalLength, diff.text.Length);
                //        tbNewBook.SelectionBackColor = Color.LightGreen;
                //    }
                //    totalLength += diff.text.Length;
                //}
            }
        }
    }
}
