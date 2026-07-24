namespace Filo.Application.DTOs;

public class PersonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Tckn { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public ICollection<VehicleDto> Vehicles { get; set; } = new List<VehicleDto>();

    public class CreateRequest
    {
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string Tckn { get; set; }
        public int Age { get; set; }
        public required string Gender { get; set; }
    }

    public class UpdateRequest
    {
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string Tckn { get; set; }
        public int Age { get; set; }
        public required string Gender { get; set; }
    }

    public class DeleteRequest
    {
        public int Id { get; set; }
    }
}
