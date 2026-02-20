using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SimplePOS.Models
{
    public partial class Product : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        [ObservableProperty]
        private decimal _price;

        public string Category { get; set; } = "General";
        // Hex color for the button
        public string Color { get; set; } = "#2563eb";
    }

    public partial class TicketItem : ObservableObject
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Subtotal))]
        private int _quantity;

        public decimal Subtotal => Price * Quantity;

        public string PaymentMethod { get; set; } = "Efectivo";
    }

    public class Withdrawal
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Amount { get; set; }
    }

    public class CashClosing
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal TotalSales { get; set; }
    }

    public partial class PosDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public Microsoft.EntityFrameworkCore.DbSet<CashClosing> CashClosings { get; set; } = null!;
        public Microsoft.EntityFrameworkCore.DbSet<Product> Products { get; set; } = null!;

        protected override void OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder)
        {
            // Windows Authentication connection string for local SQL Server
            // Change Server name if using a full SQL Server instance (e.g., . or (local))
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=POS-TICKET;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}
