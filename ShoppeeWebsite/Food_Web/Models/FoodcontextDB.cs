using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Food_Web.Models
{
    public partial class FoodcontextDB : DbContext
    {
        public FoodcontextDB()
            : base("name=FoodcontextDB3")
        {
        }

        public virtual DbSet<Blog> Blogs { get; set; }
        public virtual DbSet<CartItem> CartItems { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<comment_SP> comment_SP { get; set; }
        public virtual DbSet<Discount> Discounts { get; set; }
        public virtual DbSet<extrafood> extrafoods { get; set; }
        public virtual DbSet<Heartitem> Heartitems { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Order_detail> Order_detail { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ThanhToan> ThanhToans { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>()
                .Property(e => e.Bloglong)
                .IsUnicode(false);

            modelBuilder.Entity<Blog>()
                .Property(e => e.image)
                .IsUnicode(false);

            modelBuilder.Entity<Blog>()
                .Property(e => e.Blogshort)
                .IsUnicode(false);

            modelBuilder.Entity<CartItem>()
                .Property(e => e.ProductName)
                .IsUnicode(false);

            modelBuilder.Entity<CartItem>()
                .Property(e => e.Price)
                .HasPrecision(18, 0);

            modelBuilder.Entity<CartItem>()
                .Property(e => e.Image)
                .IsUnicode(false);

            modelBuilder.Entity<CartItem>()
                .Property(e => e.Money)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Category>()
                .HasMany(e => e.Products)
                .WithRequired(e => e.Category)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<extrafood>()
                .Property(e => e.image)
                .IsUnicode(false);

            modelBuilder.Entity<Heartitem>()
                .Property(e => e.ProductName)
                .IsUnicode(false);

            modelBuilder.Entity<Heartitem>()
                .Property(e => e.Price)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Heartitem>()
                .Property(e => e.Image)
                .IsUnicode(false);

            modelBuilder.Entity<Order>()
                .Property(e => e.Od_name)
                .IsUnicode(false);

            modelBuilder.Entity<Order>()
                .HasMany(e => e.Order_detail)
                .WithRequired(e => e.Order)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Product>()
                .HasMany(e => e.extrafoods)
                .WithRequired(e => e.Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Product>()
                .HasMany(e => e.Order_detail)
                .WithRequired(e => e.Product)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ThanhToan>()
                .HasMany(e => e.Orders)
                .WithOptional(e => e.ThanhToan)
                .HasForeignKey(e => e.idthanhtoan);
        }
    }
}
