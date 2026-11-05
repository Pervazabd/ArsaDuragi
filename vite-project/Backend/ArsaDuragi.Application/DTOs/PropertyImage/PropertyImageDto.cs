namespace ArsaDuragi.Application.DTOs.PropertyImage;

public class PropertyImageDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public DateTime CreatedAt { get; set; }
}
