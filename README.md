### About
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
- Clone this repository
- Install the required dependencies using NuGet package manager
- Build the project as x86 (important!)
- Configure `config.json` file to specify your Coinbase Pro API keys
- Run the application
