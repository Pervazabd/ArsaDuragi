using ArsaDuragi.API.Common;
using ArsaDuragi.Application.Common;
using ArsaDuragi.Application.DTOs.PropertyAd;
using ArsaDuragi.Application.DTOs.PropertyImage;
using ArsaDuragi.Application.Interfaces.Services;
using ArsaDuragi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArsaDuragi.API.Controllers;

[Route("api/properties")]
public class PropertyAdController : ApiControllerBase
{
    private const long MaxRequestBytes = 60_000_000; // 10 images x 5 MB + overhead

    private readonly IPropertyAdService _properties;
    private readonly IPropertyImageService _images;

    public PropertyAdController(IPropertyAdService properties, IPropertyImageService images)
    {
        _properties = properties;
        _images = images;
    }

    // ----------------------------------------------------------- public

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PropertyAdFilterDto filter, CancellationToken ct)
        => Success(await _properties.GetPublicAsync(filter, ct), "Properties retrieved successfully");

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured([FromQuery] int take = 6, CancellationToken ct = default)
        => Success(await _properties.GetFeaturedAsync(take, ct), "Featured properties retrieved successfully");

    /// <summary>The id can be the numeric id (12) or the slug (merdakan-konut-arsasi).</summary>
    [HttpGet("{idOrSlug}")]
    public async Task<IActionResult> GetByIdOrSlug(string idOrSlug, CancellationToken ct)
        => Success(await _properties.GetByIdOrSlugAsync(idOrSlug, ct), "Property retrieved successfully");

    // ----------------------------------------------------------- authenticated

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PropertyAdCreateDto dto, CancellationToken ct)
    {
        var created = await _properties.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetByIdOrSlug), new { idOrSlug = created.Slug },
            ApiResponse<PropertyAdGetDto>.Ok(created, "Property created successfully"));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PropertyAdUpdateDto dto, CancellationToken ct)
        => Success(await _properties.UpdateAsync(id, dto, ct), "Property updated successfully");

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _properties.DeleteAsync(id, ct);
        return NoContent();
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] PropertyAdStatusChangeDto dto, CancellationToken ct)
        => Success(await _properties.ChangeStatusAsync(id, dto, ct), "Property status updated successfully");

    // ----------------------------------------------------------- images

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{id:int}/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<IActionResult> UploadImages(int id, IFormFileCollection files, CancellationToken ct)
    {
        var uploads = files.Select(f => new UploadFile(f.OpenReadStream(), f.FileName, f.Length)).ToList();
        var result = await _images.AddImagesAsync(id, uploads, ct);
        return CreatedResponse(result, "Images uploaded successfully");
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}/images/{imageId:int}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    public async Task<IActionResult> ReplaceImage(int id, int imageId, IFormFile file, CancellationToken ct)
    {
        var result = await _images.ReplaceImageAsync(id, imageId, new UploadFile(file.OpenReadStream(), file.FileName, file.Length), ct);
        return Success<PropertyImageDto>(result, "Image replaced successfully");
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}/images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId, CancellationToken ct)
    {
        await _images.DeleteImageAsync(id, imageId, ct);
        return NoContent();
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}/images/{imageId:int}/main")]
    public async Task<IActionResult> SetMainImage(int id, int imageId, CancellationToken ct)
        => Success(await _images.SetMainImageAsync(id, imageId, ct), "Main image updated successfully");
}
