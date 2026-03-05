using LawllitFinance.Web.Models;

namespace LawllitFinance.Web.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> BuildDashboardAsync(Guid userId, int? requestedMonth, int? requestedYear);
}
