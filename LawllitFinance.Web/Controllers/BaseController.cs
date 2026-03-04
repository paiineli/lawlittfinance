using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LawllitFinance.Web.Controllers;

public abstract class BaseController : Controller
{
    protected Guid GetUserId()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            throw new InvalidOperationException("Authenticated user has no valid ID claim.");
        return id;
    }
}
