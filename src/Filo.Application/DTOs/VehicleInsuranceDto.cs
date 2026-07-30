using System;

namespace Filo.Application.DTOs;

public class VehicleInsuranceDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public VehicleDto? Vehicle { get; set; }
    public string InsuranceType { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string ProviderCompany { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Cost { get; set; }

    public class CreateRequest
    {
        public int VehicleId { get; set; }
        public string InsuranceType { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public string ProviderCompany { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Cost { get; set; }
    }

    public class UpdateRequest : CreateRequest
    {
    }
}
