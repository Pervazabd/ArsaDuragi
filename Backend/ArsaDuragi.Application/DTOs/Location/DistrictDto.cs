namespace ArsaDuragi.Application.DTOs.Location;

public class DistrictDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
}

public class DistrictUpsertDto
{
    public string Name { get; set; } = string.Empty;
    public int CityId { get; set; }
}
