using SalesSystem.ViewModels;
using SalesSystem.DAL;
using SalesSystem.Entities;
using SalesSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SalesSystem.BLL
{
    public class SalesServices
    {
        private readonly SalesContext _salesContext;
        internal SalesServices(SalesContext context)
        {
            _salesContext = context;
        }
        public List<SaleRefundDetailView> SaleRefund_GetSaleDetailsRefund(int saleId)
        {
            bool saleExists = _salesContext.Sales.Where(x => x.SaleID == saleId).Any();
            if (!saleExists)
            {
                throw new Exception($"No sale with ID of {saleId} found");
            }
            else
            {
                return _salesContext.SaleDetails.Where(x => x.SaleID == saleId).Select(x => new SaleRefundDetailView
                {
                    OriginalQuantity = x.Quantity,
                    PartID = x.PartID,
                    Refundable = x.Part.Refundable == "Y" ? true : false,
                    Description = x.Part.Description,
                    Quantity = 0,
                    ReturnQuantity = _salesContext.SaleRefundDetails.Where(y => y.SaleRefund.SaleID == x.SaleID && y.PartID == x.PartID).Any() ? _salesContext.SaleRefundDetails.Where(z => z.SaleRefund.SaleID == x.SaleID && z.PartID == x.PartID).Select(a => a.Quantity).Sum() : 0,
                    SellingPrice = x.SellingPrice
                }).ToList();
            }
        }
		public bool VerifyCoupon(string couponCode)
        {
            bool couponExists = _salesContext.Coupons.Where(x => x.CouponIDValue == couponCode).Any();
			if (!couponExists)
            {
				return false;
			}
			else
            {                
				return true;
			}
        }

		public async Task<SaleRefundView> SaleRefund_GetSaleRefund(int saleId)
        {
            bool saleExists = _salesContext.Sales.Where(x => x.SaleID == saleId).Any();
            if (!saleExists)
            {
                throw new Exception($"No sale with ID of {saleId} found");
            }
            else
            {
                return _salesContext.Sales.Where(x => x.SaleID == saleId).Select(x => new SaleRefundView
                {
                    DiscountPercent = x.Coupon == null ? 0 : x.Coupon.CouponDiscount,
                    SaleId = x.SaleID,
                    SubTotal = x.SubTotal,
                    EmployeeId = x.EmployeeID,
                    TaxAmount = x.TaxAmount
                }).First();
            }
        }
        public async Task<int> Sale_GetStock(int partId)
        {
			bool partExists = _salesContext.Parts.Where(x => x.PartID == partId).Any();
			if (!partExists)
            {
				throw new Exception($"No part with ID of {partId} found");
			}
			else
            {
				return _salesContext.Parts.Where(x => x.PartID == partId).Select(x => x.QuantityOnHand).First();
			}
		}
		public int Sale_GetStockInt(int partId)
		{
			bool partExists = _salesContext.Parts.Where(x => x.PartID == partId).Any();
			if (!partExists)
			{
				throw new Exception($"No part with ID of {partId} found");
			}
			else
			{
				return _salesContext.Parts.Where(x => x.PartID == partId).Select(x => x.QuantityOnHand).First();
			}
		}

		public int SaleRefund_Refund(SaleRefundView saleRefund, List<SaleRefundDetailView> saleRefundDetails)
        {
            List<Exception> errorlist = new List<Exception>();
            SaleRefund newRefund = new SaleRefund();
            List<SaleRefundDetail> newRefundDetails = new List<SaleRefundDetail>();
            bool saleExists = _salesContext.Sales.Where(x => x.SaleID == saleRefund.SaleId).Any();
            int count = 0;
            string errString = "";
            if (saleExists)
            {
                foreach (var i in saleRefundDetails)
                {
                    if (i.Quantity < 0)
                    {
                        errorlist.Add(new Exception($"Quantity for item {i.Description} must be greater than 0"));
                    }
                    if (i.Quantity > 0 && i.Reason == "" || i.Reason == null && i.Quantity > 0)
                    {
						if (count == 0)
						{
							errString += i.Description;
						}
						else
						{
							errString += $", {i.Description}";
						}
						count++;						
                    }
                    if (i.Quantity > i.OriginalQuantity)
                    {
                        errorlist.Add(new Exception($"The quantity of {i.Description} you are trying to return exceeds the number on the original purchase"));
                    }
                    if (!i.Refundable && i.Quantity > 0)
                    {
                        errorlist.Add(new Exception($"{i.Description} is not refundable"));
                    }
                }
                if(count > 0)
                {
					errorlist.Add(new Exception($"There is no reason given for {errString}"));
				}
            }
            else
            {
                errorlist.Add(new Exception($"No sale with ID {saleRefund.SaleId} exists"));
            }

            newRefund.SaleRefundDate = DateTime.Today;
            newRefund.SaleID = saleRefund.SaleId;
            newRefund.SubTotal = saleRefund.SubTotal;
            newRefund.EmployeeID = saleRefund.EmployeeId;
            newRefund.TaxAmount = saleRefund.TaxAmount;

            foreach (var i in saleRefundDetails)
            {
                if (i.Quantity > 0)
                {
                    if(i.Quantity > i.OriginalQuantity - i.ReturnQuantity)
                    {
                        errorlist.Add(new Exception($"{i.Description} exceeds unreturned amount"));
                    }
                    else
                    {
                        Part part = new Part();
						part = _salesContext.Parts.Where(x => x.PartID == i.PartID).First();
						part.QuantityOnHand += i.Quantity;
						SaleRefundDetail newRefundDetailEntry = new SaleRefundDetail();
						newRefundDetailEntry.PartID = i.PartID;
						newRefundDetailEntry.Part = _salesContext.Parts.Where(x => x.PartID == i.PartID).First();
						newRefundDetailEntry.Quantity = i.Quantity;
						newRefundDetailEntry.Reason = i.Reason;
						newRefundDetailEntry.SaleRefund = newRefund;
						newRefundDetailEntry.SaleRefundID = newRefund.SaleRefundID;
						newRefundDetailEntry.SellingPrice = i.SellingPrice;
						newRefundDetails.Add(newRefundDetailEntry);
						_salesContext.SaleRefundDetails.Add(newRefundDetailEntry);
					}                    
                }
            }

            newRefund.SaleRefundDetails = newRefundDetails;
            _salesContext.SaleRefunds.Add(newRefund);

            if (errorlist.Count() > 0)
            {
				_salesContext.ChangeTracker.Clear();
				throw new AggregateException("Error: ", errorlist);
            }
            else
            {
                _salesContext.SaveChanges();
                return newRefund.SaleRefundID;
            }
        }
        public async Task<List<PartView>> Sale_GetParts(int categoryId)
        {
            bool categoryExists = _salesContext.Categories.Where(x => x.CategoryID == categoryId).Any();
            if (!categoryExists)
            {
                throw new ArgumentException("That category does not exist");
            }
            else
            {
                return _salesContext.Parts.Where(x => !x.Discontinued && x.CategoryID == categoryId).Select(x => new PartView
                {
                    Description = x.Description,
                    PartID = x.PartID,
                    SellingPrice = x.SellingPrice
                }).ToList();
            }
        }
        public CouponView GetCoupon(string coupon)
        {
            bool couponExists = _salesContext.Coupons.Where(x => x.CouponIDValue.Equals(coupon)).Any();
            if (!couponExists)
            {
                CouponView noCoupon = new CouponView();
                noCoupon.CouponDiscount = 0;
                noCoupon.CouponId = 0;
                return noCoupon;
            }
            else
            {
                return _salesContext.Coupons.Where(x => x.CouponIDValue.Equals(coupon)).Select(x => new CouponView
                {
                    CouponDiscount = x.CouponDiscount,
                    CouponId = x.CouponID,
                    Start = x.StartDate,
					End = x.EndDate,
                    CouponType = x.SalesOrService
                }).First();
            }
        }
        public async Task<List<CategoryView>> Sale_GetCategories()
        {
            return _salesContext.Categories.Select(x => new CategoryView
            {
                CategoryID = x.CategoryID,
                Description = x.Description
            }).ToList();
        }
        public int Checkout(SaleView sale, List<SaleDetailView> saleDetails)
        {
            List<Exception> errorlist = new List<Exception>();
            Sale newSale = new Sale();
            
            List<SaleDetail> newDetails = new List<SaleDetail>();
            if (saleDetails.Count() <= 0)
            {
                errorlist.Add(new ArgumentNullException("No sale details were submitted"));
            }
            int qoh;
            foreach (var i in saleDetails)
            {
                bool partExists = _salesContext.Parts.Where(x => x.PartID == i.PartID).Any();
                if (partExists)
                {
                    int currentStock = _salesContext.Parts.Where(x => x.PartID == i.PartID).Select(x => x.QuantityOnHand).First();
                    if (i.Quantity < 0)
                    {
                        errorlist.Add(new Exception($"Quantity for sale item {i} must be a positive value"));
                    }
                    else if (currentStock < i.Quantity)
                    {
                        errorlist.Add(new Exception($"{i} is out of stock"));
                    }
                }
                else
                {
                    errorlist.Add(new Exception($"Part with ID of {i.PartID} does not exist"));
                }
            }
            bool couponExists = _salesContext.Coupons.Where(x => x.CouponID == sale.CouponId).Any();
            if (couponExists || sale.CouponId == 0)
            {
                if (sale.CouponId != 0)
                {
                    newSale.SubTotal = sale.SubTotal - (sale.SubTotal * ((decimal)sale.DiscountPercent / 100));
                    newSale.TaxAmount = sale.SubTotal * (decimal)0.05;
                    newSale.CouponID = sale.CouponId;
                }
                else
                {
                    newSale.TaxAmount = sale.TaxAmount;
                    newSale.SubTotal = sale.SubTotal;
                }
            }
            else
            {
                if (sale.CouponId != null)
                {
                    errorlist.Add(new Exception($"No coupon with ID {sale.CouponId} exists"));
                }
                else
                {
					newSale.TaxAmount = sale.TaxAmount;
					newSale.SubTotal = sale.SubTotal;
				}
            }

            
            
            
            foreach (var item in saleDetails)
            {
				Part part = new Part();
				SaleDetail newDetailEntry = new SaleDetail();
				part = _salesContext.Parts.Where(x => x.PartID == item.PartID).First();
                part.QuantityOnHand -= item.Quantity;
				newDetailEntry.PartID = item.PartID;
				newDetailEntry.Part = _salesContext.Parts.Where(x => x.PartID == item.PartID).First();
				newDetailEntry.Quantity = item.Quantity;
				newDetailEntry.SellingPrice = item.SellingPrice;
                _salesContext.SaleDetails.Add(newDetailEntry);
				newDetails.Add(newDetailEntry);
			}

			newSale.EmployeeID = sale.EmployeeId;
			newSale.PaymentType = sale.PaymentType;
			newSale.SaleDate = DateTime.Today;
			newSale.SaleDetails = newDetails;

			_salesContext.Sales.Add(newSale);

            if (errorlist.Count() > 0)
            {
				_salesContext.ChangeTracker.Clear();
				throw new AggregateException("Error: ", errorlist);
            }
            else
            {
                _salesContext.SaveChanges();
                return newSale.SaleID;
            }
        }
    }
}
