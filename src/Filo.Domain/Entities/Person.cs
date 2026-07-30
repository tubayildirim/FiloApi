using Filo.Domain.Common;

namespace Filo.Domain.Entities;

public class Person : BaseEntity
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string Tckn { get; set; }
    public int Age { get; set; }
    public required string Gender { get; set; }

    public string Role { get; set; } = "Staff"; // Admin, Manager, Staff
    public int? ManagerId { get; set; }
    public Person? Manager { get; set; }
    public ICollection<Person> Subordinates { get; set; } = new List<Person>();

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<VehicleMatchPerson> VehicleMatches { get; set; } = new List<VehicleMatchPerson>();
}
