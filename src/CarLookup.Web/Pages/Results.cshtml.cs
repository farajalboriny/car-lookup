using System.ComponentModel.DataAnnotations;
using CarLookup.Core.Abstractions;
using CarLookup.Core.Clients;
using CarLookup.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarLookup.Web.Pages;

public sealed class ResultsModel : PageModel
{
    private readonly IVehicleCatalogService _catalog;
    private readonly IYearProvider _yearProvider;
    private readonly ILogger<ResultsModel> _logger;

    public ResultsModel(
        IVehicleCatalogService catalog,
        IYearProvider yearProvider,
        ILogger<ResultsModel> logger)
    {
        _catalog = catalog;
        _yearProvider = yearProvider;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true), Range(1, int.MaxValue, ErrorMessage = "Invalid make.")]
    public int MakeId { get; set; }

    [BindProperty(SupportsGet = true), Required, StringLength(100)]
    public string MakeName { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true), Range(1900, 2100)]
    public int Year { get; set; }

    [BindProperty(SupportsGet = true), Required, StringLength(50)]
    public string VehicleType { get; set; } = string.Empty;

    public IReadOnlyList<VehicleModel> Models { get; private set; } = Array.Empty<VehicleModel>();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || Year < _yearProvider.EarliestYear || Year > _yearProvider.LatestYear)
        {
            TempData["Error"] = "Please enter valid search criteria.";
            return RedirectToPage("/Index");
        }

        try
        {
            Models = await _catalog.GetModelsAsync(MakeId, Year, VehicleType, cancellationToken);
            _logger.LogInformation(
                "Search performed: makeId={MakeId} year={Year} type={Type} results={Count}",
                MakeId, Year, VehicleType, Models.Count);
            return Page();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid search input");
            TempData["Error"] = "Please enter valid search criteria.";
            return RedirectToPage("/Index");
        }
        catch (NhtsaApiException ex)
        {
            _logger.LogError(ex, "NHTSA error while fetching models");
            TempData["Error"] = "We couldn't reach the vehicle database right now. Please try again in a moment.";
            return RedirectToPage("/Error");
        }
    }
}
