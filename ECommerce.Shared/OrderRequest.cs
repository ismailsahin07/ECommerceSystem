namespace ECommerce.Shared
{
    public class OrderRequest
    {
        public string OrderId { get; set; } = Guid.NewGuid().ToString();
        public string CustomerEmail { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow; 
    }
}
