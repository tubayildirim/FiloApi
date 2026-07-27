using System;
using Filo.Domain.Common;

namespace Filo.Domain.Entities;

public class VehicleService : BaseEntity
{
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int VehicleServiceId
    {
        get => Id;
        set => Id = value;
    }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public DateTime EntryDate { get; set; }
    public DateTime? ExitDate { get; set; }
    public int Odometer { get; set; }
    public string ServiceCompany { get; set; } = string.Empty;
    public string FailureDescription { get; set; } = string.Empty;
    public decimal? Cost { get; set; }
    public string Status { get; set; } = "Aktif"; // Aktif, Tamamlandı
    public string? InvoiceNumber { get; set; }
}
