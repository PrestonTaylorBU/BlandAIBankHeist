using BlandAIBankHeist.Web.Installers;
using BlandAIBankHeist.Web.Options;
using BlandAIBankHeist.Web.Endpoints.v1;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogWithConfiguration(builder.Configuration);

builder.Services.AddControllersWithViews();

builder.Services.AddAntiforgery();

builder.Services.Configure<BlandApiOptions>(builder.Configuration.GetSection(BlandApiOptions.Section));

builder.Services.AddBlandApiServices();

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

app.MapScheduleRecallEndpoint();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Call}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
