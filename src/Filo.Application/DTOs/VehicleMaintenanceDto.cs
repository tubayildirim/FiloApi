using System;

namespace Filo.Application.DTOs;

public class VehicleMaintenanceDto
{
    public int VehicleMaintenanceId { get; set; }
    public int VehicleId { get; set; }
    public VehicleDto? Vehicle { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public int Odometer { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public DateTime? NextMaintenanceDate { get; set; }
    public int? NextMaintenanceKm { get; set; }

    public class CreateRequest
    {
        public int VehicleId { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public int Odometer { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime? NextMaintenanceDate { get; set; }
        public int? NextMaintenanceKm { get; set; }
    }

    public class UpdateRequest
    {
        public int VehicleId { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public int Odometer { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime? NextMaintenanceDate { get; set; }
        public int? NextMaintenanceKm { get; set; }
    }
}
