using LawllitFinance.Data.Entities;
using LawllitFinance.Data.Repositories.Interfaces;
using LawllitFinance.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LawllitFinance.Web.Controllers;

[Authorize]
public class DashboardController(ITransactionRepository transactionRepository) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(int? month, int? year)
    {
        var userId = GetUserId();
        var now = DateTime.Now;
        var selectedMonth = month ?? now.Month;
        var selectedYear = year ?? now.Year;

        var summary = await transactionRepository.GetSummaryAsync(userId, selectedMonth, selectedYear);
        var trend = await transactionRepository.GetMonthlyTrendAsync(userId, selectedMonth, selectedYear);
        var upcomingExpenses = await transactionRepository.GetUpcomingExpensesAsync(userId, selectedMonth, selectedYear);

        bool isCurrentMonth = selectedMonth == now.Month && selectedYear == now.Year;
        int daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);
        int daysDone = isCurrentMonth ? now.Day : daysInMonth;

        decimal dailyAverage = summary.TotalExpenses > 0 ? summary.TotalExpenses / daysDone : 0;
        decimal? monthlyProjection = isCurrentMonth && summary.TotalExpenses > 0
            ? (summary.TotalExpenses / daysDone) * daysInMonth
            : null;

        int previousMonth = selectedMonth == 1 ? 12 : selectedMonth - 1;
        int previousYear = selectedMonth == 1 ? selectedYear - 1 : selectedYear;
        int nextMonth = selectedMonth == 12 ? 1 : selectedMonth + 1;
        int nextYear = selectedMonth == 12 ? selectedYear + 1 : selectedYear;

        var viewModel = new DashboardViewModel
        {
            Month = selectedMonth,
            Year = selectedYear,
            TotalIncome = summary.TotalIncome,
            TotalExpenses = summary.TotalExpenses,
            Balance = summary.Balance,
            ExpensesByCategory = summary.ExpensesByCategory,
            MonthlyTrend = trend,
            UpcomingExpenses = upcomingExpenses,
            IsCurrentMonth = isCurrentMonth,
            DaysInMonth = daysInMonth,
            DaysDone = daysDone,
            DailyAverage = dailyAverage,
            MonthlyProjection = monthlyProjection,
            PreviousMonth = previousMonth,
            PreviousYear = previousYear,
            NextMonth = nextMonth,
            NextYear = nextYear,
            CanGoToNextMonth = !isCurrentMonth,
        };

        return View(viewModel);
    }
}
