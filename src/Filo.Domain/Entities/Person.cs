using Filo.Domain.Common;

namespace Filo.Domain.Entities;

public class Person : BaseEntity
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string Tckn { get; set; }
    public int Age { get; set; }
    public required string Gender { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<VehicleMatchPerson> VehicleMatches { get; set; } = new List<VehicleMatchPerson>();
}
