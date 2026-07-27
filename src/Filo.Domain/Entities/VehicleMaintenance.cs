using System;
using Filo.Domain.Common;

namespace Filo.Domain.Entities;

public class VehicleMaintenance : BaseEntity
{
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int VehicleMaintenanceId
    {
        get => Id;
        set => Id = value;
    }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public DateTime MaintenanceDate { get; set; }
    public int Odometer { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;

    public DateTime? NextMaintenanceDate { get; set; }
    public int? NextMaintenanceKm { get; set; }
}
