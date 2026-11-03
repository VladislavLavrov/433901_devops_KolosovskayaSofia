using Microsoft.EntityFrameworkCore;
using Calculator.Data;
using Calculator.Services;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using static System.Net.WebRequestMethods;
using System.Runtime.Intrinsics.X86;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();
string mariadbCS = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<CalculatorContext>(options =>
{
    options.UseMySql(mariadbCS, new MySqlServerVersion(new Version(10, 5, 15)));
});

builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddSingleton<KafkaProducerHandler>();
builder.Services.AddSingleton<KafkaProducerService<Null,string>>(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Calculator/Error");
    // The default HSTS value is 30 days. You may wantto change this for production scenarios, see https://aka.ms / aspnetcore - hsts.
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