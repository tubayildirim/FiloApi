using System;
using Filo.Domain.Common;

namespace Filo.Domain.Entities;

public class VehicleToll : BaseEntity
{
    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public DateTime TransitDate { get; set; }
    public required string Location { get; set; } // Gişe veya otoyol adı
    public decimal Amount { get; set; }
    public required string Type { get; set; } // "HGS" veya "OGS"
}
