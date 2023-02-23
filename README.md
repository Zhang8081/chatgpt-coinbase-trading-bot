### About
![alt text](https://github.com/Zhang8081/chatgpt-coinbase-trading-bot/blob/main/main.png?raw=true)

I asked ChatGPT to program a simple crypto trading strategy using Moving Averages (MA), Relative Strength Index (RSI) and Dollar Cost Averaging (DCA). The bot uses Coinbase Pro API to trade.

### Advanced Trading Strategy
ChatGPT built the following strategy, which it called "Advanced Trading Strategy". The full ChatGPT code is at the bottom of the page.

The algorithm works as follows:
- Calculate the 50-MA and the 200-MA.
- When the 50-MA crosses above the 200-MA, buy the asset.
- When the RSI is greater than 70, sell the asset.
- If the asset price drops more than 5% from the purchase price, apply the DCA algorithm and buy more of the asset at a lower price to lower the average cost of the asset.
- If the asset price drops more than 10% from the purchase price, sell the asset to cut losses.

### Dependencies
- Coinbase API library
- .NET framework 4.8
- Newtonsoft.Json

### Installation
This repository contains both PROJECT and BINARY files (windows only).
- [Download](https://github.com/Zhang8081/chatgpt-coinbase-trading-bot/archive/refs/heads/main.zip) this repository
- Install the required dependencies using NuGet package manager
- Build the project as x86 (important!)
- Run the application and specify your Coinbase Pro API keys

### Algorithm
```
// Calculate moving averages
var shortMA = candleData.GetSimpleMovingAverage(50);
var longMA = candleData.GetSimpleMovingAverage(200);

// Calculate RSI
var rsi = candleData.GetRelativeStrengthIndex(14);

// Retrieve account balance
var balance = client.GetAccountBalance("USD");

// Check if short MA crossed above long MA
if (shortMA.Last() > longMA.Last() && shortMA.ElementAt(shortMA.Count - 2) <= longMA.ElementAt(longMA.Count - 2))
{
    // Buy asset
    var price = client.GetPrice(product);
    var size = balance.Available / price;
    var buyOrder = client.PlaceLimitOrder(OrderSide.Buy, product, size, price);

    // Check for successful order placement
    if (buyOrder != null)
    {
        // Save purchase price and quantity
        var purchasePrice = price;
        var purchaseQuantity = size;
    }
}

// Check if RSI is above 70
if (rsi.Last() > 70)
{
    // Sell asset
    var price = client.GetPrice(product);
    var sellOrder = client.PlaceLimitOrder(OrderSide.Sell, product, purchaseQuantity, price);

    // Check for successful order placement
    if (sellOrder != null)
    {
        // Calculate profit/loss
        var pnl = (price - purchasePrice) * purchaseQuantity;
    }
}

// Check for price drop of more than 5% and apply DCA algorithm
if (price < purchasePrice * 0.95)
{
    // Calculate new purchase price and quantity
    var newPurchasePrice = (purchasePrice * purchaseQuantity + balance.Available) / (purchaseQuantity + balance.Available / price);
    var newPurchaseQuantity = purchaseQuantity + balance.Available / price;

    // Buy more of the asset at a lower price
    var buyOrder = client.PlaceLimitOrder(OrderSide.Buy, product, balance.Available / price, price);

    // Check for successful order placement
    if (buyOrder != null)
    {
        // Save new purchase price and quantity
        purchasePrice = newPurchasePrice;
        purchaseQuantity = newPurchaseQuantity;
    }
}

// Check for price drop of more than 10%
if (price < purchasePrice * 0.90)
{
    // Sell asset to cut losses
    var sellOrder = client.PlaceLimitOrder(OrderSide.Sell, product, purchaseQuantity, price);

    // Check for successful order placement
    if (sellOrder != null)
    {
        // Calculate profit/loss
        var pnl = (price - purchasePrice) * purchaseQuantity;
    }
}
```
