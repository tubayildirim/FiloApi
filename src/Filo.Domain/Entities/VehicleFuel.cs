using System;
using Filo.Domain.Common;

namespace Filo.Domain.Entities;

public class VehicleFuel : BaseEntity
{
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int VehicleFuelId
    {
        get => Id;
        set => Id = value;
    }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public DateTime RefuelingDate { get; set; }
    public int Odometer { get; set; }
    public double Liters { get; set; }
    public decimal PricePerLiter { get; set; }
    public decimal TotalPrice { get; set; }
    public string? ReceiptNumber { get; set; }
}
