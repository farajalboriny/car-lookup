using CarLookup.Core.Abstractions;
using CarLookup.Core.Clients;
using CarLookup.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarLookup.Web.Pages;

public sealed class IndexModel : PageModel
{
    private readonly IVehicleCatalogService _catalog;
    private readonly IYearProvider _yearProvider;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IVehicleCatalogService catalog,
        IYearProvider yearProvider,
        ILogger<IndexModel> logger)
    {
        _catalog = catalog;
        _yearProvider = yearProvider;
        _logger = logger;
    }

    public IReadOnlyList<VehicleMake> Makes { get; private set; } = Array.Empty<VehicleMake>();

    public IReadOnlyList<int> Years { get; private set; } = Array.Empty<int>();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Makes = await _catalog.GetAllMakesAsync(cancellationToken);
            Years = _yearProvider.GetAllowedYears();
            return Page();
        }
        catch (NhtsaApiException ex)
        {
            _logger.LogError(ex, "Failed to load makes for Index page");
            TempData["Error"] = "We couldn't reach the vehicle database right now. Please try again in a moment.";
            return RedirectToPage("/Error");
        }
    }

    public async Task<IActionResult> OnGetVehicleTypesAsync(int makeId, CancellationToken cancellationToken)
    {
        if (makeId <= 0)
        {
            return BadRequest(new { error = "makeId must be a positive integer." });
        }

        try
        {
            IReadOnlyList<VehicleType> types = await _catalog.GetVehicleTypesAsync(makeId, cancellationToken);
            var payload = types.Select(t => new { id = t.Id, name = t.Name });
            return new JsonResult(payload);
        }
        catch (NhtsaApiException ex)
        {
            _logger.LogWarning(ex, "Failed to load vehicle types for makeId {MakeId}", makeId);
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Upstream error." });
        }
    }
}
