namespace ServicingSystem.ViewModels

{
	public class JobDetailView
	{
	    public string Description {get; set;}
	    public decimal JobHour {get; set;}
	    public string Comment {get; set;}
	    public int CouponId {get; set;}
	    public int DiscountPercent {get; set;}
		public int JobDetailID { get; set; }
	}
}
