/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using QuantConnect.Securities.Interfaces;

namespace QuantConnect.Securities 
{
    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// SecurityHolding is a base class for purchasing and holding a market item which manages the asset portfolio
    /// </summary>
    public class SecurityHolding
    {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        //Working Variables
        private decimal _averagePrice = 0;
        private int     _quantity = 0;
        private decimal _price = 0;
        private readonly string  _symbol = "";
        private readonly SecurityType _securityType;
        private decimal _totalSaleVolume = 0;
        private decimal _profit = 0;
        private decimal _lastTradeProfit = 0;
        private decimal _totalFees = 0;
        private ISecurityTransactionModel _model;

        /******************************************************** 
        * CONSTRUCTOR DEFINITION
        *********************************************************/

        /// <summary>
        /// Create a new holding class instance setting the initial properties to $0.
        /// </summary>
        public SecurityHolding(string symbol, ISecurityTransactionModel transactionModel)
            : this(symbol, SecurityType.Equity, transactionModel)
        {
        }

        /// <summary>
        /// Create a new holding class instance setting the initial properties to $0.
        /// </summary>
        public SecurityHolding(string symbol, SecurityType type, ISecurityTransactionModel transactionModel)
        {
            _model = transactionModel;
            _symbol = symbol;
            _securityType = type;

            //Total Sales Volume for the day
            _totalSaleVolume = 0;
            _lastTradeProfit = 0;
        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        /// <summary>
        /// Average price of the security holdings.
        /// </summary>
        public decimal AveragePrice
        {
            get
            {
                return _averagePrice;
            }
        }


        /// <summary>
        /// Quantity of the security held.
        /// </summary>
        /// <remarks>Positive indicates long holdings, negative quantity indicates a short holding</remarks>
        /// <seealso cref="AbsoluteQuantity"/>
        public int Quantity
        {
            get
            {
                return _quantity;
            }
        }


        /// <summary>
        /// Symbol identifier of the underlying security.
        /// </summary>
        public string Symbol
        {
            get
            {
                return _symbol;
            }
        }
        

        /// <summary>
        /// Acquisition cost of the security total holdings.
        /// </summary>
        public virtual decimal HoldingsCost 
        {
            get 
            {
                return AveragePrice * Convert.ToDecimal(Quantity);
            }
        }

        /// <summary>
        /// Current market price of the security.
        /// </summary>
        public virtual decimal Price
        {
            get
            {
                return _price;
            }
        }

        /// <summary>
        /// Absolute unlevered holdings cost for current holdings.
        /// </summary>
        /// <seealso cref="HoldingsCost"/>
        public virtual decimal AbsoluteHoldingsCost 
        {
            get 
            {
                return Math.Abs(HoldingsCost);
            }
        }

        /// <summary>
        /// Market value of our holdings.
        /// </summary>
        public virtual decimal HoldingsValue
        {
            get
            {
                return _price * Convert.ToDecimal(Quantity);
            }
        }

        /// <summary>
        /// Absolute of the market value of our holdings.
        /// </summary>
        /// <seealso cref="HoldingsValue"/>
        public virtual decimal AbsoluteHoldingsValue
        {
            get
            {
                return Math.Abs(HoldingsValue);
            }
        }
        
        /// <summary>
        /// Boolean flat indicating if we hold any of the security
        /// </summary>
        public virtual bool HoldStock 
        {
            get 
            {
                return (AbsoluteQuantity > 0);
            }
        }


        /// <summary>
        /// Boolean flat indicating if we hold any of the security
        /// </summary>
        /// <remarks>Alias of HoldStock</remarks>
        /// <seealso cref="HoldStock"/>
        public virtual bool Invested
        {
            get
            {
                return HoldStock;
            }
        }

        /// <summary>
        /// The total transaction volume for this security since the algorithm started.
        /// </summary>
        public virtual decimal TotalSaleVolume 
        {
            get 
            {
                return _totalSaleVolume;
            }
        }

        /// <summary>
        /// Total fees for this company since the algorithm started.
        /// </summary>
        public virtual decimal TotalFees 
        {
            get 
            {
                return _totalFees;
            }
        }

        /// <summary>
        /// Boolean flag indicating we have a net positive holding of the security.
        /// </summary>
        /// <seealso cref="IsShort"/>
        public virtual bool IsLong 
        {
            get 
            {
                return Quantity > 0;
            }
        }

        /// <summary>
        /// BBoolean flag indicating we have a net negative holding of the security.
        /// </summary>
        /// <seealso cref="IsLong"/>
        public virtual bool IsShort 
        {
            get 
            {
                return Quantity < 0;
            }
        }

        /// <summary>
        /// Absolute quantity of holdings of this security
        /// </summary>
        /// <seealso cref="Quantity"/>
        public virtual decimal AbsoluteQuantity 
        {
            get 
            {
                return Math.Abs(Quantity);
            }
        }

        /// <summary>
        /// Record of the closing profit from the last trade conducted.
        /// </summary>
        public virtual decimal LastTradeProfit 
        {
            get 
            {
                return _lastTradeProfit;
            }
        }

        /// <summary>
        /// Calculate the total profit for this security.
        /// </summary>
        /// <seealso cref="NetProfit"/>
        public virtual decimal Profit 
        {
            get 
            {
                return _profit;
            }
        }

        /// <summary>
        /// Return the net for this company measured by the profit less fees.
        /// </summary>
        /// <seealso cref="Profit"/>
        /// <seealso cref="TotalFees"/>
        public virtual decimal NetProfit 
        {
            get 
            {
                return Profit - TotalFees;
            }
        }

        /// <summary>
        /// Unrealized profit of this security when absolute quantity held is more than zero.
        /// </summary>
        public virtual decimal UnrealizedProfit 
        {
            get 
            {
                return TotalCloseProfit();
            }
        }

        /******************************************************** 
        * CLASS METHODS 
        *********************************************************/
        /// <summary>
        /// Adds a fee to the running total of total fees.
        /// </summary>
        /// <param name="newFee"></param>
        public void AddNewFee(decimal newFee) 
        {
            _totalFees += newFee;
        }

        /// <summary>
        /// Adds a profit record to the running total of profit.
        /// </summary>
        /// <param name="profitLoss">The cash change in portfolio from closing a position</param>
        public void AddNewProfit(decimal profitLoss) 
        {
            _profit += profitLoss;
        }

        /// <summary>
        /// Adds a new sale value to the running total trading volume.
        /// </summary>
        /// <param name="saleValue"></param>
        public void AddNewSale(decimal saleValue) 
        {
            _totalSaleVolume += saleValue;
        }

        /// <summary>
        /// Set the last trade profit for this security from a Portfolio.ProcessFill call.
        /// </summary>
        /// <param name="lastTradeProfit">Value of the last trade profit</param>
        public void SetLastTradeProfit(decimal lastTradeProfit) 
        {
            _lastTradeProfit = lastTradeProfit;
        }
            
        /// <summary>
        /// Set the quantity of holdings and their average price after processing a portfolio fill.
        /// </summary>
        public virtual void SetHoldings(decimal averagePrice, int quantity) 
        {
            _averagePrice = averagePrice;
            _quantity = quantity;
        }

        /// <summary>
        /// Update local copy of closing price value.
        /// </summary>
        /// <param name="closingPrice">Price of the underlying asset to be used for calculating market price / portfolio value</param>
        public virtual void UpdatePrice(decimal closingPrice)
        {
            _price = closingPrice;
        }

        /// <summary>
        /// Profit if we closed the holdings right now including the approximate fees.
        /// </summary>
        /// <remarks>Does not use the transaction model for market fills but should.</remarks>
        public virtual decimal TotalCloseProfit() 
        {
            decimal gross = 0, net = 0;
            decimal orderFee = 0;

            if (AbsoluteQuantity > 0) 
            {
                orderFee = _model.GetOrderFee(AbsoluteQuantity, _price);
            }

            if (IsLong) 
            {
                //if we're long on a position, profit from selling off $10,000 stock:
                gross = (_price - AveragePrice) * AbsoluteQuantity;
            } 
            else if (IsShort) 
            {
                //if we're short on a position, profit from buying $10,000 stock:
                gross = (AveragePrice - _price) * AbsoluteQuantity;
            } 
            else 
            {
                //no holdings, 0 profit.
                return 0;
            }

            net = gross - orderFee;

            return net;
        }
    }
} //End Namespace