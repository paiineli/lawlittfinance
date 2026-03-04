using LawllitFinance.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LawllitFinance.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
}
