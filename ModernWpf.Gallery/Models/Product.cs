namespace ModernWpf.Gallery.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public int ProductCode { get; set; }

        public string ProductName { get; set; }

        public string QuantityPerUnit { get; set; }

        public double UnitPrice { get; set; }

        public int UnitsInStock { get; set; }
    }
}
