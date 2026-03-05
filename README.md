<p align="center">
  <img src=".github/banner.svg" alt="lawllitfinance" width="800" />
</p>

---

## Stack

- **Backend:** ASP.NET Core MVC (.NET 10), C#
- **Database:** PostgreSQL (Supabase) via Npgsql + EF Core 10
- **Auth:** Cookie-based (BCrypt) + Google OAuth 2.0 + e-mail confirmation
- **Email:** MailKit/MimeKit via Brevo SMTP
- **Frontend:** Razor Views, Bootstrap 5.3.3 (CDN), Bootstrap Icons 1.11.3 (CDN), Chart.js 4.4.4 (CDN), vanilla JS
- **Pattern:** Repository Pattern + Service Layer + MVC

---

## Project structure

```
lawlittfinance/
├── LawllitFinance.slnx
├── LawllitFinance.Data/
│   ├── AppDbContext.cs
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Category.cs
│   │   ├── Transaction.cs
│   │   └── Summaries.cs
│   ├── Repositories/
│   │   ├── Interfaces/
│   │   │   ├── IUserRepository.cs
│   │   │   ├── ICategoryRepository.cs
│   │   │   └── ITransactionRepository.cs
│   │   ├── UserRepository.cs
│   │   ├── CategoryRepository.cs
│   │   └── TransactionRepository.cs
│   └── Migrations/
└── LawllitFinance.Web/
    ├── Program.cs
    ├── appsettings.json
    ├── Constants.cs
    ├── StringHelpers.cs
    ├── SharedResource.cs
    ├── Controllers/
    │   ├── BaseController.cs
    │   ├── HomeController.cs
    │   ├── AuthController.cs
    │   ├── DashboardController.cs
    │   ├── TransactionController.cs
    │   ├── CategoryController.cs
    │   ├── ProfileController.cs
    │   └── QuotesController.cs
    ├── Models/
    │   ├── LoginViewModel.cs
    │   ├── RegisterViewModel.cs
    │   ├── ForgotPasswordViewModel.cs
    │   ├── ResetPasswordViewModel.cs
    │   ├── DashboardViewModel.cs
    │   ├── TransactionFormViewModel.cs
    │   ├── TransactionListViewModel.cs
    │   ├── CategoryFormViewModel.cs
    │   ├── CategoryListViewModel.cs
    │   ├── ProfileViewModel.cs
    │   ├── SaveThemeViewModel.cs
    │   ├── SaveFontSizeViewModel.cs
    │   ├── SaveLanguageViewModel.cs
    │   ├── EditNameViewModel.cs
    │   ├── EditEmailViewModel.cs
    │   ├── ChangePasswordViewModel.cs
    │   └── QuoteViewModel.cs
    ├── Services/
    │   ├── Interfaces/
    │   │   ├── IAuthService.cs
    │   │   ├── IEmailService.cs
    │   │   ├── IDashboardService.cs
    │   │   ├── ITransactionService.cs
    │   │   ├── ICategoryService.cs
    │   │   ├── IProfileService.cs
    │   │   └── IQuotesService.cs
    │   ├── AuthService.cs
    │   ├── EmailService.cs
    │   ├── DashboardService.cs
    │   ├── TransactionService.cs
    │   ├── CategoryService.cs
    │   ├── ProfileService.cs
    │   └── QuotesService.cs
    ├── Resources/
    │   ├── SharedResource.resx        (pt-BR — default)
    │   └── SharedResource.en-US.resx  (en-US)
    ├── Views/
    │   ├── Auth/         (Login, Register, ForgotPassword, ResetPassword)
    │   ├── Home/         (Index — landing page)
    │   ├── Dashboard/    (Index)
    │   ├── Transaction/  (Index)
    │   ├── Category/     (Index)
    │   ├── Profile/      (Index)
    │   ├── Quotes/       (Index)
    │   └── Shared/       (_Layout, _AuthLayout, _Notification)
    └── wwwroot/
        ├── css/app.css
        └── js/
            ├── app.js        (global event delegation)
            ├── dashboard.js  (charts + ranking bars)
            ├── home.js       (typewriter animation)
            └── profile.js    (theme / font size / language AJAX)
```

## Deploy

**Environment variables:**
```
ConnectionStrings__DefaultConnection
Authentication__Google__ClientId
Authentication__Google__ClientSecret
Email__SmtpHost
Email__SmtpPort
Email__SmtpUser
Email__SmtpPass
Email__From
```
