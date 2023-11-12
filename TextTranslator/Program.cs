using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TextTranslator.Areas.Identity.Data;
using TextTranslator.Data;
using TextTranslator.Models;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("TextTranslatorDbContextConnection") ?? throw new InvalidOperationException("Connection string 'TextTranslatorDbContextConnection' not found.");

builder.Services.AddDbContext<TextTranslatorDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<TextTranslatorDBContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<TextTranslatorDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();;

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
