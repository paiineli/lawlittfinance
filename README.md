# lawllitfinance

---

## Stack

- **Backend:** ASP.NET Core MVC (.NET 10), C#
- **Database:** PostgreSQL (Supabase) via Npgsql + EF Core 10
- **Auth:** Cookie-based (BCrypt) + Google OAuth 2.0 + e-mail confirmation
- **Email:** MailKit/MimeKit via Brevo SMTP
- **Frontend:** Razor Views, Bootstrap 5.3.3 (CDN), Bootstrap Icons 1.11.3 (CDN), Chart.js 4.4.4 (CDN), vanilla JS
- **Pattern:** Repository Pattern + MVC

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
    │   └── QuoteViewModel.cs
    ├── Services/
    │   ├── IEmailService.cs
    │   └── EmailService.cs
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
            └── home.js       (typewriter animation)
```

---

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
