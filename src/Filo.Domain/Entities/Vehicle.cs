using Filo.Domain.Common;

namespace Filo.Domain.Entities;

public class Vehicle : BaseEntity
{
    public required string Brand { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public required string PlateNumber { get; set; }
    public string? Color { get; set; }
    public string? FuelType { get; set; }
    public string? TransmissionType { get; set; }
    public string? EngineNumber { get; set; }
    public string? ChassisNumber { get; set; }
    public DateTime? RegistrationDate { get; set; }

    public int? PersonId { get; set; }
    public Person? Person { get; set; }
    public ICollection<VehicleMatchPerson> VehicleMatches { get; set; } = new List<VehicleMatchPerson>();
    public ICollection<VehicleFuel> VehicleFuels { get; set; } = new List<VehicleFuel>();
}
