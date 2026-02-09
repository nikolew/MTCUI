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

            modelBuilder.Entity<GroupEntity>().HasData(new[]
            {
                new GroupEntity{ Id = 1,  Color = "#FF0000"},
                new GroupEntity{ Id = 2,  Color = "#00FF00"},
                new GroupEntity{ Id = 3,  Color = "#0000FF"}
            });

            var times = new[]
            {
                new TimeEntity{ Id = 1, Time = "00:05", GroupEntityId = 1 },
                new TimeEntity{ Id = 2, Time = "00:10", GroupEntityId = 1 },
                new TimeEntity{ Id = 3, Time = "00:15", GroupEntityId = 2 },
                new TimeEntity{ Id = 4, Time = "00:20", GroupEntityId = 2 }
            };

            modelBuilder.Entity<TimeEntity>()
               .HasData(times);
        }

        public DbSet<NodeEntity> Nodes { get; set; }
        public DbSet<PositionEntity> Positions { get; set; }
        public DbSet<GroupEntity> Groups { get; set; }
        public DbSet<TimeEntity> Times { get; set; }
    }
}
