using System;

namespace Filo.Application.DTOs;

public class VehicleTrafficFineDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public VehicleDto? Vehicle { get; set; }
    public int? PersonId { get; set; }
    public PersonDto? Person { get; set; }
    public DateTime FineDate { get; set; }
    public decimal Amount { get; set; }
    public decimal? DiscountedAmount { get; set; }
    public bool IsPaid { get; set; }
    public string? Description { get; set; }

    public class CreateRequest
    {
        public int VehicleId { get; set; }
        public int? PersonId { get; set; }
        public DateTime FineDate { get; set; }
        public decimal Amount { get; set; }
        public decimal? DiscountedAmount { get; set; }
        public bool IsPaid { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateRequest : CreateRequest
    {
    }
}
