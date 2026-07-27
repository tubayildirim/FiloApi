using System;

namespace Filo.Application.DTOs;

public class VehicleMatchPersonDto
{
    public int VehiclePersonId { get; set; }
    public int VehicleId { get; set; }
    public VehicleDto? Vehicle { get; set; }
    public int PersonId { get; set; }
    public PersonDto? Person { get; set; }
    public DateTime AssignmentDate { get; set; }
    public int AssignmentKm { get; set; }

    public class CreateRequest
    {
        public int VehicleId { get; set; }
        public int PersonId { get; set; }
        public DateTime AssignmentDate { get; set; }
        public int AssignmentKm { get; set; }
    }

    public class UpdateRequest
    {
        public int VehicleId { get; set; }
        public int PersonId { get; set; }
        public DateTime AssignmentDate { get; set; }
        public int AssignmentKm { get; set; }
    }
}
