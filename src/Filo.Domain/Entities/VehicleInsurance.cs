using System;
using Filo.Domain.Common;

namespace Filo.Domain.Entities;

public class VehicleInsurance : BaseEntity
{
    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public required string InsuranceType { get; set; } // "Kasko" or "Trafik Sigortası"
    public required string PolicyNumber { get; set; }
    public required string ProviderCompany { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Cost { get; set; }
}
