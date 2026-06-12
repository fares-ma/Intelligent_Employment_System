using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Presentation.Controllers;

/// <summary>
/// Multipart form for <c>POST /api/company</c> (create company + optional brand image).
/// </summary>
public sealed class CreateCompanyForm
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Industry { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Website { get; set; }

    [StringLength(50)]
    public string? TaxNumber { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>Optional logo / brand image (.jpg, .jpeg, .png).</summary>
    public IFormFile? BrandAsset { get; set; }
}
