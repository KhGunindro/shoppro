using shoppro.Data;
using shoppro.Models;
using System.Collections.Generic;
using System.Linq;
using System;
public class ProductService
{
    public List<Product> GetAll()
    {
        using var db = new AppDbContext();
        return db.Products.OrderBy(p => p.Name).ToList();
    }

    public void Add(Product product)
    {
        using var db = new AppDbContext();
        db.Products.Add(product);
        db.SaveChanges();
    }

    public void Update(Product product)
    {
        using var db = new AppDbContext();
        db.Products.Update(product);
        db.SaveChanges();
    }

    public void Delete(int id)
    {
        using var db = new AppDbContext();
        var prod = db.Products.Find(id);
        if (prod != null)
        {
            db.Products.Remove(prod);
            db.SaveChanges();
        }
    }
}
