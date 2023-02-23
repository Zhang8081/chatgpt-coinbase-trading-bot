using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitBot
{
    /// <summary>
    /// Class used to store order book data.  Uses a binary tree structure.  Seperate objects will be used for bids and asks.
    /// </summary>
    internal class OrderBookTree
    {
        private int _count = 0;
        internal int count 
        {
            get { return _count; }
            private set { _count = value; }
        }

        internal OrderBookTreeNode root { get; set; }

        internal OrderBookTree()
        {
            root = null;
        }

        internal void Insert(Order value)
        {
            if (root == null)
            {
                root = new OrderBookTreeNode(null, value);
            }
            else
            {
                root.Insert(value);
            }

            count++;
        }

        internal bool Remove(string oid, decimal amount, decimal price)
        {
            if (root == null)
            {
                return false;
            }
            else
            {
                return root.Remove(oid, amount, price);
            }
        }

        internal bool Remove(string oid, decimal price)
        {
            if (root == null)
            {
                return false;
            }
            else
            {
                return root.Remove(oid, price);
            }
        }
        

        /// <summary>
        /// Returns a DataTable with 4 columns: price, volume, totalVolume, oid.
        /// </summary>
        /// <param name="isBids">true if returning data for Bids.  False if data for Asks (aggregated backwards). </param>
        /// <returns></returns>
        internal System.Data.DataTable OutputDatatable(bool isBids)
        {
            System.Data.DataTable returnTable = new System.Data.DataTable("MarketData");
            returnTable.Columns.Add("price", typeof(decimal));
            returnTable.Columns.Add("volume", typeof(decimal));
            returnTable.Columns.Add("totalVolume", typeof(decimal));
            returnTable.Columns.Add("oid", typeof(string));
            //returnTable.PrimaryKey = new System.Data.DataColumn[]{ returnTable.Columns["oid"] }; //can't do this because we can have duplicate oid's if an order's volume changed

            if (root != null)
            {
                OrderBookTreeNode curNode = root;
                decimal runningTotal = 0;

                if (isBids)
                {
                    root.AddBids(ref returnTable, ref runningTotal);
                }
                else
                {
                    root.AddOffers(ref returnTable, ref runningTotal);
                }
            }

            return returnTable;
        }
    }

    /// <summary>
    /// Object representing an individual node in the binary tree.
    /// </summary>
    internal class OrderBookTreeNode
    {
        internal Order value { get; set; }
        internal OrderBookTreeNode parent { get; set; }
        internal OrderBookTreeNode left { get; set; }
        internal OrderBookTreeNode right { get; set; }

        internal OrderBookTreeNode(OrderBookTreeNode parent, Order newValue)
        {
            this.parent = parent;
            value = newValue;
            left = null;
            right = null;
        }

        internal void Insert(Order newValue)
        {
            if (newValue.price < value.price || (newValue.price == value.price && newValue.OrderID.CompareTo(value.OrderID) <= 0))
            {
                if (left == null)
                {
                    left = new OrderBookTreeNode(this, newValue);
                }
                else
                {
                    left.Insert(newValue);
                }
            }
            else //newValue.price > value.price
            {
                if (right == null)
                {
                    right = new OrderBookTreeNode(this, newValue);
                }
                else
                {
                    right.Insert(newValue);
                }
            }
        }

        internal OrderBookTreeNode findMin()
        {
            OrderBookTreeNode currentNode = this;
            while (currentNode.left != null)
            {
                currentNode = currentNode.left;
            }
            return currentNode;
        }

        internal OrderBookTreeNode findMax()
        {
            OrderBookTreeNode currentNode = this;
            while (currentNode.right != null)
            {
                currentNode = currentNode.right;
            }
            return currentNode;
        }

        internal void ReplaceNodeInParent(OrderBookTreeNode newChild)
        {
            if (parent != null)
            {
                if (this == this.parent.left)
                {
                    this.parent.left = newChild;
                }
                else
                {
                    this.parent.right = newChild;
                }
            }
            if (newChild != null)
            {
                newChild.parent = this.parent;
            }
        }


        /// <summary>
        /// Removes the specified amount from the specified oid at the specified price.
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="amount"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        internal bool Remove(string oid, decimal amount, decimal price)
        {
            if (this.value.price == price && this.value.OrderID == oid)
            {
                if (this.value.amount <= amount)
                {
                    this.DeleteNode();
                }
                else
                {
                    this.value.amount -= amount;
                }
                return true;
            }
            //else if (this.value.price >= price)
            else if (this.value.price > price || (this.value.price == price && this.value.OrderID.CompareTo(oid) > 0))
            {
                if (left == null)
                {
                    return false; //could not find node
                }
                else
                {
                    return left.Remove(oid, amount, price);
                }
            }
            else
            {
                if (right == null)
                {
                    return false;
                }
                else
                {
                    return right.Remove(oid, amount, price);
                }
            }
        }

        internal bool Remove(string oid, decimal price)
        {
            if (this.value.price == price && this.value.OrderID == oid)
            {
                this.DeleteNode();
                return true;
            }

            else if (this.value.price > price || (this.value.price == price && this.value.OrderID.CompareTo(oid) > 0))
            {
                if (left == null)
                {
                    return false; //could not find node
                }
                else
                {
                    return left.Remove(oid, price);
                }
            }
            else
            {
                if (right == null)
                {
                    return false;
                }
                else
                {
                    return right.Remove(oid, price);
                }
            }
        }

        internal void DeleteNode()
        {
            if (left != null && right != null)
            {
                //06/29/2015
                //OrderBookTreeNode successor = findMin();
                OrderBookTreeNode successor = right.findMin();
                value = successor.value;
                successor.DeleteNode();
            }
            else if (left != null)
            {
                ReplaceNodeInParent(left);
            }
            else if (right != null)
            {
                ReplaceNodeInParent(right);
            }
            else
            {
                ReplaceNodeInParent(null);
            }
        }

        internal void AddBids(ref System.Data.DataTable returnTable, ref decimal runningTotal) //returnTable has columns: price, volume, totalVolume, oid
        {
            if (this.right != null)
            {
                this.right.AddBids(ref returnTable, ref runningTotal);
            }

            runningTotal += this.value.amount;
            returnTable.Rows.Add(new object[] { this.value.price, this.value.amount, runningTotal, this.value.OrderID });

            if (this.left != null)
            {
                this.left.AddBids(ref returnTable, ref runningTotal);
            }
        }

        internal void AddOffers(ref System.Data.DataTable returnTable, ref decimal runningTotal) //returnTable has columns: price, volume, totalVolume, oid
        {
            if (this.left != null)
            {
                this.left.AddOffers(ref returnTable, ref runningTotal);
            }

            runningTotal += this.value.amount;
            returnTable.Rows.Add(new object[] { this.value.price, this.value.amount, runningTotal, this.value.OrderID });

            if (this.right != null)
            {
                this.right.AddOffers(ref returnTable, ref runningTotal);
            }
        }
    }

    /// <summary>
    /// Class representing an individual order in the order book.
    /// </summary>
    internal class Order
    {
        internal string OrderID { get; set; }
        internal string ClientOrderID { get; set; }
        internal decimal amount { get; set; }
        internal decimal price { get; set; }

        internal Order()
        {
            OrderID = null;
            ClientOrderID = null;
            amount = -1;
            price = -1;
        }

        internal Order(string newOrderID, string newClientOrderID, decimal newAmount, decimal newPrice)
        {
            OrderID = newOrderID;
            ClientOrderID = newClientOrderID;
            amount = newAmount;
            price = newPrice;
        }
    }
}
