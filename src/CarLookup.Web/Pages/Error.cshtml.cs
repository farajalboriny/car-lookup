using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarLookup.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class ErrorModel : PageModel
{
    public string Message { get; private set; } = "Something went wrong.";

    public void OnGet()
    {
        if (TempData["Error"] is string tempDataMessage && !string.IsNullOrWhiteSpace(tempDataMessage))
        {
            Message = tempDataMessage;
        }
    }
}
