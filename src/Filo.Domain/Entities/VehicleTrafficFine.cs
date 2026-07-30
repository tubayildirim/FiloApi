using System;
using Filo.Domain.Common;

namespace Filo.Domain.Entities;

public class VehicleTrafficFine : BaseEntity
{
    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int? PersonId { get; set; }
    public Person? Person { get; set; }

    public DateTime FineDate { get; set; }
    public decimal Amount { get; set; }
    public decimal? DiscountedAmount { get; set; }
    public bool IsPaid { get; set; }
    public string? Description { get; set; } // Hız ihlali, hatalı park vs.
}
