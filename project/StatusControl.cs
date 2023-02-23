using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace BitBot
{
    public partial class StatusControl : UserControl
    {
        //private AutoResetEvent workerFinishedEvent = new AutoResetEvent(true);

        public StatusControl()
        {
            InitializeComponent();
        }

        //do when drop down selection changes
        private void cbSelectedExchange_SelectionChangeCommitted(object sender, EventArgs e)
        {
            doSelectionChanged();
        }

        private void doSelectionChanged()
        {

        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            DataTable marketData1;
            DataTable marketData2;

            lock (MarketData.bids)
            {
                marketData1 = MarketData.bids.OutputDatatable(true);
            }

            lock (MarketData.asks)
            {
                marketData2 = MarketData.asks.OutputDatatable(false);
            }

            decimal marketPrice = ((decimal)marketData1.Rows[0]["price"] + (decimal)marketData2.Rows[0]["price"]) / 2;
            decimal minimumPrice = marketPrice / 1.5m;
            decimal maximumPrice = marketPrice * 1.5m;


            //Remove redundant rows from multiple orders at the same price and orders > 50% away from market price
            decimal tempPrice = (decimal)marketData1.Rows[0]["price"];
            for (int i = 1; i < marketData1.Rows.Count; i++ )
            {
                if ((decimal)marketData1.Rows[i]["price"] < minimumPrice)
                {
                    marketData1.Rows.RemoveAt(i);
                    i--;
                }
                else if ((decimal)marketData1.Rows[i]["price"] == tempPrice)
                {
                    marketData1.Rows.RemoveAt(i-1);
                    i--;
                }
                else
                {
                    tempPrice = (decimal)marketData1.Rows[i]["price"];
                }
            }

            //insert a row with 0 for graphing purposes
            if (marketData1.Rows.Count > 0)
            {
                DataRow rowToInsert = marketData1.NewRow();
                rowToInsert["price"] = (decimal)marketData1.Rows[0]["price"] + 0.001m;
                rowToInsert["volume"] = 0;
                rowToInsert["totalVolume"] = 0m;

                marketData1.Rows.InsertAt(rowToInsert, 0);
            }

            //Remove redundant rows from multiple orders at the same price and orders > 50% away from market price
            tempPrice = (decimal)marketData2.Rows[0]["price"];
            for (int i = 1; i < marketData2.Rows.Count; i++)
            {
                if ((decimal)marketData2.Rows[i]["price"] > maximumPrice)
                {
                    marketData2.Rows.RemoveAt(i);
                    i--;
                }
                else if ((decimal)marketData2.Rows[i]["price"] == tempPrice)
                {
                    marketData2.Rows.RemoveAt(i - 1);
                    i--;
                }
                else
                {
                    tempPrice = (decimal)marketData2.Rows[i]["price"];
                }
            }

            //insert a row with 0 for graphing purposes
            if (marketData2.Rows.Count > 0)
            {
                DataRow rowToInsert = marketData2.NewRow();
                rowToInsert["price"] = (decimal)marketData2.Rows[0]["price"] - 0.001m;
                rowToInsert["totalVolume"] = 0m;

                marketData2.Rows.InsertAt(rowToInsert, 0);
            }

            marketData1.Merge(marketData2);

            chart1.Series.Clear();
            chart1.Series.Add("Market Depth");
            chart1.Series["Market Depth"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area;
            chart1.Series["Market Depth"].XValueMember = "price";
            chart1.Series["Market Depth"].YValueMembers = "totalVolume";

            chart1.DataSource = marketData1;
            chart1.DataBind();
        }
    }
}
