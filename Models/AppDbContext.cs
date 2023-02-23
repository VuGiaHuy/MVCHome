using App.Models.Blog;
using App.Models.Contact;
using App.Models.Product;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.Models;
public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);    
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if(tableName.StartsWith("AspNet"))
            {
                entityType.SetTableName(tableName.Substring(6));
            }
        }
        modelBuilder.Entity<Category>(entity=>{
            entity.HasIndex(category=>category.Slug).IsUnique();
        });
        modelBuilder.Entity<PostCategory>(entity=>{
            entity.HasKey(c=>new {c.CategoryId,c.PostId});
        });
        modelBuilder.Entity<Post>(entity=>{
            entity.HasIndex(p=>p.Slug).IsUnique();
        });
        modelBuilder.Entity<PCategory>(entity=>{
            entity.HasIndex(category=>category.Slug).IsUnique();
        });
        modelBuilder.Entity<ProductPCategory>(entity=>{
            entity.HasKey(c=>new {c.ProductId,c.PCategoryId});
        });
        modelBuilder.Entity<ProductModel>(entity=>{
            entity.HasIndex(p=>p.Slug).IsUnique();
        });
    }
    public DbSet<Category> Categories {get;set;}
    public DbSet<ContactModels> ContactModels {get;set;}
    public DbSet<PostCategory> PostCategories {get;set;}
    public DbSet<Post> Posts {get;set;}
    public DbSet<ProductModel> Products {get;set;}
    public DbSet<PCategory> PCategories {get;set;}
    public DbSet<ProductPCategory> ProductPCategories {get;set;}
}