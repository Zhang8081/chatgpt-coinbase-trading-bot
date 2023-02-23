//JSON.cs
//
//Classes used in Deserialization of JSON messages.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BitBot
{
    class CoinbaseOrderBookJSON
    {
        [JsonProperty("sequence")]
        internal long sequence { get; set; }

        [JsonProperty("bids")]
        internal List<List<string>> bids { get; set; }

        [JsonProperty("asks")]
        internal List<List<string>> asks { get; set; }
    }

    class CoinbaseMessageJSON
    {
        [JsonProperty("type")]
        internal string type { get; set; }

        [JsonProperty("order_type")]
        internal string order_type { get; set; }

        [JsonProperty("sequence")]
        internal long sequence { get; set; }

        [JsonProperty("order_id")]
        internal string order_id { get; set; }

        [JsonProperty("size")]
        internal string size { get; set; }

        [JsonProperty("price")]
        internal string price { get; set; }

        [JsonProperty("side")]
        internal string side { get; set; }

        [JsonProperty("time")]
        internal string time { get; set; }

        [JsonProperty("remaining_size")]
        internal string remaining_size { get; set; }

        [JsonProperty("trade_id")]
        internal string trade_id { get; set; }

        [JsonProperty("maker_order_id")]
        internal string maker_order_id { get; set; }

        [JsonProperty("taker_order_id")]
        internal string taker_order_id { get; set; }

        [JsonProperty("reason")]
        internal string reason { get; set; }

        [JsonProperty("new_size")]
        internal string new_size { get; set; }

        [JsonProperty("old_size")]
        internal string old_size { get; set; }
    }
}
