using Microsoft.EntityFrameworkCore;
using MTCCore.Entities;
using MTCCore.Repositories;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MTCCore.DataBase
{
    public class ApplicationDbContext : DbContext, IUnitOfWork
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string? exePath = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
                string dir = Path.GetDirectoryName(exePath);

                var connectionString = $"DataSource={dir}\\mtc.db";
                optionsBuilder.UseSqlite(connectionString);
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            return result;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NodeEntity>()
                .HasOne(e => e.Position)
                .WithOne(ed => ed.Node)
                .HasForeignKey<PositionEntity>(ed => ed.NodeId);
        }

        public DbSet<NodeEntity> Nodes { get; set; }
        public DbSet<PositionEntity> Positions { get; set; }
    }
}
