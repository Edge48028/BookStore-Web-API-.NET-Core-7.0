namespace ModelAndRequest.Order
{
    public class OrderDetailViewModel
    {
        public int OrderId { get; set; }
        public int BookId { get; set; }
        public string BookImageUrl { get; set; }
        public string BookName { get; set; }
        public int Quantity { get; set; }
        public float TotalPrice { get; set; }
    }
}