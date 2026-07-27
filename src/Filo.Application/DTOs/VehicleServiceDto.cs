using System;

namespace Filo.Application.DTOs;

public class VehicleServiceDto
{
    public int VehicleServiceId { get; set; }
    public int VehicleId { get; set; }
    public VehicleDto? Vehicle { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime? ExitDate { get; set; }
    public int Odometer { get; set; }
    public string ServiceCompany { get; set; } = string.Empty;
    public string FailureDescription { get; set; } = string.Empty;
    public decimal? Cost { get; set; }
    public string Status { get; set; } = "Aktif";
    public string? InvoiceNumber { get; set; }

    public class CreateRequest
    {
        public int VehicleId { get; set; }
        public DateTime EntryDate { get; set; }
        public int Odometer { get; set; }
        public string ServiceCompany { get; set; } = string.Empty;
        public string FailureDescription { get; set; } = string.Empty;
    }

    public class UpdateRequest
    {
        public int VehicleId { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime? ExitDate { get; set; }
        public int Odometer { get; set; }
        public string ServiceCompany { get; set; } = string.Empty;
        public string FailureDescription { get; set; } = string.Empty;
        public decimal? Cost { get; set; }
        public string Status { get; set; } = "Aktif";
        public string? InvoiceNumber { get; set; }
    }
}
