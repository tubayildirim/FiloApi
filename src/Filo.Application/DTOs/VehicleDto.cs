namespace Filo.Application.DTOs;

public class VehicleDto
{
    public int Id { get; set; }
    public required string Brand { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public required string PlateNumber { get; set; }
    public string? Color { get; set; }
    public string? FuelType { get; set; }
    public string? TransmissionType { get; set; }
    public string? EngineNumber { get; set; }
    public string? ChassisNumber { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public int? PersonId { get; set; }
    public DateTime CreatedAt { get; set; }

    public class CreateRequest
    {
        public required string Brand { get; set; }
        public required string Model { get; set; }
        public int Year { get; set; }
        public required string PlateNumber { get; set; }
        public string? Color { get; set; }
        public string? FuelType { get; set; }
        public string? TransmissionType { get; set; }
        public string? EngineNumber { get; set; }
        public string? ChassisNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public int? PersonId { get; set; }
    }

    public class UpdateRequest
    {
        public required string Brand { get; set; }
        public required string Model { get; set; }
        public int Year { get; set; }
        public required string PlateNumber { get; set; }
        public string? Color { get; set; }
        public string? FuelType { get; set; }
        public string? TransmissionType { get; set; }
        public string? EngineNumber { get; set; }
        public string? ChassisNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public int? PersonId { get; set; }
    }

    public class DeleteRequest
    {
        public int Id { get; set; }
    }
}

