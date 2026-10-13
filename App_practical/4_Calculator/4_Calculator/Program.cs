using Calculator.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using System.Runtime.Intrinsics.X86;
using static System.Net.WebRequestMethods;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
string mariadbCS = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<CalculatorContext>(options =>
    {
        options.UseMySql(mariadbCS, new MySqlServerVersion(new Version(10, 5, 15)));
    });

builder.Services.AddRazorPages();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Calculator/Error");
        app.UseHsts();
    }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
        pattern: "{controller=Calculator}/{action=Index}/{ id ?}");
app.Run();