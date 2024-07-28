namespace SalesSystem.ViewModels
{
	public class CouponView
	{
		public int CouponId { get; set; }
		public int CouponDiscount { get; set; }
		public int CouponType { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
	}
}