using System.ComponentModel.DataAnnotations;

namespace AuthServer.Models;

public sealed class CompanySelectionViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a company.")]
    [Display(Name = "Company")]
    public int CompanyId { get; set; }

    public string? ReturnUrl { get; set; }

    public List<CompanyOptionViewModel> Companies { get; set; } = [];
}

public sealed class CompanyOptionViewModel
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Code { get; set; }
}
