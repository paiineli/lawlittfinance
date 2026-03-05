using LawllitFinance.Web.Models;

namespace LawllitFinance.Web.Services.Interfaces;

public interface IQuotesService
{
    Task<(List<QuoteViewModel> Quotes, string? ErrorTechDetails)> GetQuotesAsync();
}
