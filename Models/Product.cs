using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace shoppro.Models;

[Table("Products")]
public class Product
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string? Name { get; set; }
    [Column("sku")]
    public string? Sku { get; set; }
    [Column("quantity")]
    public int Quantity { get; set; }
    [Column("price")]
    public decimal Price { get; set; }
    [Column("barcode")]
    public string? Barcode { get; set; }
}
