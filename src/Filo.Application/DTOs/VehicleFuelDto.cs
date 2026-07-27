using System;

namespace Filo.Application.DTOs;

public class VehicleFuelDto
{
    public int VehicleFuelId { get; set; }
    public int VehicleId { get; set; }
    public VehicleDto? Vehicle { get; set; }
    public DateTime RefuelingDate { get; set; }
    public int Odometer { get; set; }
    public double Liters { get; set; }
    public decimal PricePerLiter { get; set; }
    public decimal TotalPrice { get; set; }
    public string? ReceiptNumber { get; set; }

    public class CreateRequest
    {
        public int VehicleId { get; set; }
        public DateTime RefuelingDate { get; set; }
        public int Odometer { get; set; }
        public double Liters { get; set; }
        public decimal PricePerLiter { get; set; }
        public string? ReceiptNumber { get; set; }
    }

    public class UpdateRequest
    {
        public int VehicleId { get; set; }
        public DateTime RefuelingDate { get; set; }
        public int Odometer { get; set; }
        public double Liters { get; set; }
        public decimal PricePerLiter { get; set; }
        public string? ReceiptNumber { get; set; }
    }
}
