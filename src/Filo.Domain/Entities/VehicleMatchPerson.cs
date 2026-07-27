using System;
using Filo.Domain.Common;

namespace Filo.Domain.Entities;

public class VehicleMatchPerson : BaseEntity
{
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int VehiclePersonId
    {
        get => Id;
        set => Id = value;
    }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int PersonId { get; set; }
    public Person? Person { get; set; }

    public DateTime AssignmentDate { get; set; }
    public int AssignmentKm { get; set; }
}
