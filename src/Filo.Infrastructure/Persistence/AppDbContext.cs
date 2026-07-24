using Filo.Domain.Common;
using Filo.Domain.Entities;
using Filo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Filo.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly IEventBus _eventBus;

    public AppDbContext(DbContextOptions<AppDbContext> options, IEventBus eventBus) : base(options)
    {
        _eventBus = eventBus;
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

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in events)
        {
            await _eventBus.PublishAsync(domainEvent.GetType().Name, domainEvent);
        }

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Person> Person => Set<Person>();

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
    }
}
