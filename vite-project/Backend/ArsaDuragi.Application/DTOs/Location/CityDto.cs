namespace ArsaDuragi.Application.DTOs.Location;

public class CityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CityUpsertDto
{
    public string Name { get; set; } = string.Empty;
}
