using System;

namespace Filo.Application.DTOs;

public class VehicleTollDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public VehicleDto? Vehicle { get; set; }
    public DateTime TransitDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;

    public class CreateRequest
    {
        public int VehicleId { get; set; }
        public DateTime TransitDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class UpdateRequest : CreateRequest
    {
    }
}
