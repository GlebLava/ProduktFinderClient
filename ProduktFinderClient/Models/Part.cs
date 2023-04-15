using System.Collections.Generic;

namespace ProduktFinderClient.Models
{
    public class Part
    {
        public string? Hyperlink { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Supplier { get; set; }
        public string? SupplierPartNumber { get; set; }
        public string? Manufacturer { get; set; }
        public string? ManufacturerPartNumber { get; set; }
        public int? AmountInStock { get; set; } = null;
        public List<Preorder> PreorderPossibilities { get; set; } = new List<Preorder>();
        public int? LeadTimeInDays { get; set; } = null;
        public List<Price> Prices { get; set; } = new List<Price>();

    }

    public class Price
    {
        public int FromAmount { get; set; }
        public float PricePerPiece { get; set; }
        public string? Currency { get; set; }

    }

    public class Preorder
    {
        public int Quantity { get; set; } = 0;
        public string? ExpectedArrivalDate { get; set; }
    }

}
