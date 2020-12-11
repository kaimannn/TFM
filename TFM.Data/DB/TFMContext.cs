using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TFM.Data.DB
{
    public partial class TFMContext : DbContext
    {
        public TFMContext()
        {
        }

        public TFMContext(DbContextOptions<TFMContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Games> Games { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Games>(entity =>
            {
                entity.HasIndex(e => new { e.PlatformName, e.Name })
                    .HasName("IX_Games_PlatformNames")
                    .IsUnique();

                entity.HasIndex(e => new { e.PlatformName, e.Position })
                    .HasName("IX_Games_PlatformPositions");

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.PlatformName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ReleaseDate).HasColumnType("datetime");

                entity.Property(e => e.Thumbnail).HasMaxLength(200);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
