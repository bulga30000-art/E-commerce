namespace E_commerce.DTOs.Products;

// DTO response model for product GET requests
public class ProductReadDto
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public int QuantityInStock { get; set; }

    public decimal UnitPrice { get; set; }

    public string? ImageUrl { get; set; }

    public byte CategoryId { get; set; }

    // Flattened Category name projection from Category navigation property
    public string CategoryName { get; set; } = null!;
}