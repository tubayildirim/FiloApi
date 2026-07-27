using Filo.Domain.Common;
using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Filo.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Filo.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }

        var events = ChangeTracker.Entries<BaseEntity>()
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        foreach (var domainEvent in events)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                Type = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                Content = System.Text.Json.JsonSerializer.Serialize(domainEvent, domainEvent.GetType())
            };
            Set<OutboxMessage>().Add(outboxMessage);
        }

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.ClearDomainEvents();
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Person> Person => Set<Person>();
    public DbSet<VehicleMatchPerson> VehicleMatchPersons => Set<VehicleMatchPerson>();
    public DbSet<VehicleFuel> VehicleFuels => Set<VehicleFuel>();
    public DbSet<VehicleMaintenance> VehicleMaintenances => Set<VehicleMaintenance>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>(entity =>
        {
            entity.ToTable("Person");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Surname).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Tckn).IsRequired().HasMaxLength(11);
            entity.HasIndex(e => e.Tckn).IsUnique();
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Brand).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PlateNumber).IsRequired().HasMaxLength(15);
            entity.HasIndex(e => e.PlateNumber).IsUnique();
            entity.Property(e => e.Color).HasMaxLength(30);
            entity.Property(e => e.FuelType).HasMaxLength(30);
            entity.Property(e => e.TransmissionType).HasMaxLength(30);
            entity.Property(e => e.EngineNumber).HasMaxLength(50);
            entity.Property(e => e.ChassisNumber).HasMaxLength(50);
            entity.HasOne(e => e.Person)
                .WithMany(p => p.Vehicles)
                .HasForeignKey(e => e.PersonId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<VehicleMatchPerson>(entity =>
        {
            entity.ToTable("VehicleMatchPersons");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("VehiclePersonId");
            entity.Property(e => e.AssignmentDate).IsRequired();
            entity.Property(e => e.AssignmentKm).IsRequired();

            entity.HasOne(e => e.Vehicle)
                .WithMany(v => v.VehicleMatches)
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Person)
                .WithMany(p => p.VehicleMatches)
                .HasForeignKey(e => e.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<VehicleFuel>(entity =>
        {
            entity.ToTable("VehicleFuels");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("VehicleFuelId");
            entity.Property(e => e.RefuelingDate).IsRequired();
            entity.Property(e => e.Odometer).IsRequired();
            entity.Property(e => e.Liters).IsRequired();
            entity.Property(e => e.PricePerLiter).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.TotalPrice).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.ReceiptNumber).HasMaxLength(50);

            entity.HasOne(e => e.Vehicle)
                .WithMany(v => v.VehicleFuels)
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<VehicleMaintenance>(entity =>
        {
            entity.ToTable("VehicleMaintenances");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("VehicleMaintenanceId");
            entity.Property(e => e.MaintenanceDate).IsRequired();
            entity.Property(e => e.Odometer).IsRequired();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Cost).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.MaintenanceType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.NextMaintenanceDate);
            entity.Property(e => e.NextMaintenanceKm);

            entity.HasOne(e => e.Vehicle)
                .WithMany(v => v.VehicleMaintenances)
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Error).HasMaxLength(2000);
        });
    }
}
