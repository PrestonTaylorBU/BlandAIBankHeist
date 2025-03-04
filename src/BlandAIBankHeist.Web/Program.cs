using BlandAIBankHeist.Web.Installers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogWithConfiguration(builder.Configuration);

builder.Services.AddControllersWithViews();

builder.Services.AddAntiforgery();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseRouting();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Call}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
