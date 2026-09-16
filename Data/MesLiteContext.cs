using Microsoft.EntityFrameworkCore;
using MesLite.Models;

namespace MesLite.Data
{
    public class MesLiteContext : DbContext
    {
        public MesLiteContext(DbContextOptions<MesLiteContext> options) : base(options) { }

        public DbSet<Material> Materials => Set<Material>();
        public DbSet<Bom> Boms => Set<Bom>();
        public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
        public DbSet<WorkReport> WorkReports => Set<WorkReport>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 避免 Bom 兩個 FK 都指到 Material 造成級聯刪除衝突
            modelBuilder.Entity<Bom>()
                .HasOne(b => b.ParentItem)
                .WithMany(m => m.AsParentBoms)
                .HasForeignKey(b => b.ParentItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bom>()
                .HasOne(b => b.ChildItem)
                .WithMany(m => m.AsChildBoms)
                .HasForeignKey(b => b.ChildItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkOrder>()
                .HasOne(w => w.Product)
                .WithMany()
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(t => t.WorkOrder)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(t => t.Material)
                .WithMany()
                .HasForeignKey(t => t.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkReport>()
                .HasOne(r => r.WorkOrder)
                .WithMany()
                .HasForeignKey(r => r.WoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Material>()
                .HasIndex(m => m.ItemCode)
                .IsUnique();

            modelBuilder.Entity<WorkOrder>()
                .HasIndex(w => w.WoNumber)
                .IsUnique();
        }
    }
}
