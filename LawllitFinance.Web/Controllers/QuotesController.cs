using LawllitFinance.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace LawllitFinance.Web.Controllers;

[Authorize]
public class QuotesController(IHttpClientFactory httpClientFactory, IMemoryCache cache) : BaseController
{
    private const string CacheKey      = "quotes_data";
    private const string ErrorCacheKey = "quotes_error";

    public async Task<IActionResult> Index()
    {
        if (cache.TryGetValue(CacheKey, out List<QuoteViewModel>? cachedQuotes) && cachedQuotes is not null)
        {
            if (!cachedQuotes.Any() && cache.TryGetValue(ErrorCacheKey, out string? cachedError))
                TempData["Error"] = cachedError;
            return View(cachedQuotes);
        }

        var quotes = new List<QuoteViewModel>();

        try
        {
            using var httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("LawllitFinance/1.0");

            var json = await httpClient.GetStringAsync(
                "https://economia.awesomeapi.com.br/json/last/USD-BRL,EUR-BRL,GBP-BRL,JPY-BRL,BTC-BRL,ARS-BRL");

            var deserializeOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, QuoteApiData>>(json, deserializeOptions)!;

            var configuration = new[]
            {
                new { Key = "USDBRL", Label = "Dólar",          FlagEmoji = "🇺🇸", DecimalPlaces = 4 },
                new { Key = "EURBRL", Label = "Euro",           FlagEmoji = "🇪🇺", DecimalPlaces = 4 },
                new { Key = "GBPBRL", Label = "Libra",          FlagEmoji = "🇬🇧", DecimalPlaces = 4 },
                new { Key = "JPYBRL", Label = "Iene",           FlagEmoji = "🇯🇵", DecimalPlaces = 4 },
                new { Key = "BTCBRL", Label = "Bitcoin",        FlagEmoji = "₿",   DecimalPlaces = 2 },
                new { Key = "ARSBRL", Label = "Peso Argentino", FlagEmoji = "🇦🇷", DecimalPlaces = 4 },
            };

            foreach (var item in configuration)
            {
                if (!data.TryGetValue(item.Key, out var api)) continue;
                if (string.IsNullOrEmpty(api.Bid) || string.IsNullOrEmpty(api.High) || string.IsNullOrEmpty(api.Low)) continue;

                var lastUpdated = long.TryParse(api.Timestamp, out var unixSeconds)
                    ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds).LocalDateTime
                    : DateTime.Now;

                decimal.TryParse(api.PctChange, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var pctChange);

                quotes.Add(new QuoteViewModel
                {
                    Label         = item.Label,
                    FlagEmoji     = item.FlagEmoji,
                    BuyRate       = decimal.Parse(api.Bid,  System.Globalization.CultureInfo.InvariantCulture),
                    SellRate      = decimal.Parse(api.Ask,  System.Globalization.CultureInfo.InvariantCulture),
                    DailyHigh     = decimal.Parse(api.High, System.Globalization.CultureInfo.InvariantCulture),
                    DailyLow      = decimal.Parse(api.Low,  System.Globalization.CultureInfo.InvariantCulture),
                    PctChange     = pctChange,
                    DecimalPlaces = item.DecimalPlaces,
                    LastUpdated   = lastUpdated,
                });
            }

            cache.Set(CacheKey, quotes, TimeSpan.FromHours(1));
        }
        catch (Exception exception)
        {
            var errorMsg = $"Não foi possível carregar as cotações. ({exception.GetType().Name}: {exception.Message})";
            cache.Set(CacheKey,      quotes,   TimeSpan.FromMinutes(3));
            cache.Set(ErrorCacheKey, errorMsg, TimeSpan.FromMinutes(3));
            TempData["Error"] = errorMsg;
        }

        return View(quotes);
    }
}
