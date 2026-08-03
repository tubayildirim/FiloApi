using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Filo.Infrastructure.Persistence;

public static class AppDbContextSeed
{
    public static void SeedData(AppDbContext context)
    {
        try
        {
            context.Database.Migrate();

            if (!context.Person.Any())
            {
                var persons = new List<Filo.Domain.Entities.Person>
                {
                    new() { Name = "Ahmet", Surname = "Yıldırım", Tckn = "12345678901", Age = 34, Gender = "Erkek", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                    new() { Name = "Ayşe", Surname = "Demir", Tckn = "12345678902", Age = 29, Gender = "Kadın", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                    new() { Name = "Mehmet", Surname = "Kara", Tckn = "12345678903", Age = 41, Gender = "Erkek", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                    new() { Name = "Zeynep", Surname = "Aydın", Tckn = "12345678904", Age = 27, Gender = "Kadın", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                    new() { Name = "Emre", Surname = "Çelik", Tckn = "12345678905", Age = 36, Gender = "Erkek", CreatedBy = "System", CreatedAt = DateTime.UtcNow }
                };

                context.Person.AddRange(persons);
                context.SaveChanges();

                Console.WriteLine("Database successfully seeded with initial person data.");
            }

            // Apply RBAC seed to existing/new data
            var tuba = context.Person.FirstOrDefault(p => p.Name == "Tuba" && p.Surname == "Yıldırım");
            if (tuba == null)
            {
                tuba = new Filo.Domain.Entities.Person { Name = "Tuba", Surname = "Yıldırım", Tckn = "11111111111", Age = 30, Gender = "Kadın", Role = "Admin", CreatedBy = "System", CreatedAt = DateTime.UtcNow };
                context.Person.Add(tuba);
                context.SaveChanges();
            }
            else if (tuba.Role != "Admin")
            {
                tuba.Role = "Admin";
                context.SaveChanges();
            }

            var ahmet = context.Person.FirstOrDefault(p => p.Id == 1);
            if (ahmet != null && ahmet.Role != "Manager")
            {
                ahmet.Role = "Manager";
                context.SaveChanges();
            }

            var otherStaff = context.Person.Where(p => p.Id != tuba.Id && p.Id != 1).ToList();
            bool staffUpdated = false;
            foreach (var staff in otherStaff)
            {
                if (staff.Role != "Staff" || staff.ManagerId != 1)
                {
                    staff.Role = "Staff";
                    staff.ManagerId = 1;
                    staffUpdated = true;
                }
            }
            if (staffUpdated)
            {
                context.SaveChanges();
            }

            if (!context.Vehicles.Any())
            {
                var persons = context.Person.ToList();
                context.Vehicles.AddRange(new List<Filo.Domain.Entities.Vehicle>
                {
                    new() { Brand = "BMW", Model = "320i", Year = 2022, PlateNumber = "34BMW123", Color = "Black", FuelType = "Gasoline", TransmissionType = "Automatic", PersonId = persons[0].Id, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                    new() { Brand = "Audi", Model = "A4", Year = 2021, PlateNumber = "34AUD456", Color = "White", FuelType = "Diesel", TransmissionType = "Automatic", PersonId = persons[1].Id, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                    new() { Brand = "Mercedes-Benz", Model = "C200", Year = 2023, PlateNumber = "34MER789", Color = "Gray", FuelType = "Gasoline", TransmissionType = "Automatic", PersonId = persons[2].Id, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                    new() { Brand = "Toyota", Model = "Corolla", Year = 2020, PlateNumber = "34TOY012", Color = "Blue", FuelType = "Hybrid", TransmissionType = "Automatic", PersonId = persons[3].Id, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                    new() { Brand = "Volkswagen", Model = "Golf", Year = 2019, PlateNumber = "34VW345", Color = "Red", FuelType = "Gasoline", TransmissionType = "Manual", PersonId = persons[4].Id, CreatedBy = "System", CreatedAt = DateTime.UtcNow }
                });
                context.SaveChanges();
                Console.WriteLine("Database successfully seeded with initial vehicle data.");
            }

            if (!context.VehicleMatchPersons.Any())
            {
                var vehicles = context.Vehicles.ToList();
                var persons = context.Person.ToList();
                if (vehicles.Count >= 2 && persons.Count >= 4)
                {
                    context.VehicleMatchPersons.AddRange(new List<Filo.Domain.Entities.VehicleMatchPerson>
                    {
                        new() { VehicleId = vehicles[0].Id, PersonId = persons[1].Id, AssignmentDate = DateTime.UtcNow, AssignmentKm = 1000, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                        new() { VehicleId = vehicles[1].Id, PersonId = persons[3].Id, AssignmentDate = DateTime.UtcNow, AssignmentKm = 1000, CreatedBy = "System", CreatedAt = DateTime.UtcNow }
                    });
                    context.SaveChanges();
                    Console.WriteLine("Database successfully seeded with initial vehicle-match-person data.");
                }
            }

            if (!context.VehicleFuels.Any())
            {
                var vehicles = context.Vehicles.ToList();
                if (vehicles.Count >= 2)
                {
                    context.VehicleFuels.AddRange(new List<Filo.Domain.Entities.VehicleFuel>
                    {
                        new() { VehicleId = vehicles[0].Id, RefuelingDate = DateTime.UtcNow.AddDays(-3), Odometer = 1200, Liters = 50, PricePerLiter = 42.50m, TotalPrice = 2125m, ReceiptNumber = "RCP-001", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                        new() { VehicleId = vehicles[1].Id, RefuelingDate = DateTime.UtcNow.AddDays(-2), Odometer = 1500, Liters = 45, PricePerLiter = 43.10m, TotalPrice = 1939.50m, ReceiptNumber = "RCP-002", CreatedBy = "System", CreatedAt = DateTime.UtcNow }
                    });
                    context.SaveChanges();
                    Console.WriteLine("Database successfully seeded with initial fuel data.");
                }
            }

            if (!context.VehicleMaintenances.Any())
            {
                var vehicles = context.Vehicles.ToList();
                if (vehicles.Count >= 1)
                {
                    context.VehicleMaintenances.AddRange(new List<Filo.Domain.Entities.VehicleMaintenance>
                    {
                        new() { VehicleId = vehicles[0].Id, MaintenanceDate = DateTime.UtcNow.AddDays(-10), Odometer = 1000, Description = "15.000 KM periyodik bakımı yapıldı.", Cost = 7500m, MaintenanceType = "Periyodik", NextMaintenanceDate = DateTime.UtcNow.AddMonths(12), NextMaintenanceKm = 15000, CreatedBy = "System", CreatedAt = DateTime.UtcNow }
                    });
                    context.SaveChanges();
                    Console.WriteLine("Database successfully seeded with initial maintenance data.");
                }
            }

            if (!context.VehicleServices.Any())
            {
                var vehicles = context.Vehicles.ToList();
                if (vehicles.Count >= 4)
                {
                    context.VehicleServices.AddRange(new List<Filo.Domain.Entities.VehicleService>
                    {
                        new() { VehicleId = vehicles[2].Id, EntryDate = DateTime.UtcNow.AddDays(-5), ExitDate = DateTime.UtcNow.AddDays(-3), Odometer = 3000, ServiceCompany = "Maslak Doğuş Oto", FailureDescription = "Far değişimi ve lokal boya yapıldı.", Cost = 4500m, Status = "Tamamlandı", InvoiceNumber = "INV-001", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                        new() { VehicleId = vehicles[3].Id, EntryDate = DateTime.UtcNow, ExitDate = null, Odometer = 5000, ServiceCompany = "Oto Teknik Servis", FailureDescription = "Şanzıman arıza tespiti ve kontrolü.", Cost = null, Status = "Aktif", InvoiceNumber = null, CreatedBy = "System", CreatedAt = DateTime.UtcNow }
                    });
                    context.SaveChanges();
                    Console.WriteLine("Database successfully seeded with initial service data.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while migrating or seeding the database. {ex.Message}");
        }
    }
}
