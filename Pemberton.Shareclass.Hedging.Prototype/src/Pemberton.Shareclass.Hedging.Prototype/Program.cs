using Pemberton.Shareclass.Hedging.Prototype.Services;
using Pemberton.Shareclass.Hedging.Prototype.ViewModels;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

builder.Services.AddTransient<IHedgingEngine, HedgingEngine>();
builder.Services.AddTransient<IApiService, MockApiService>();
builder.Services.AddTransient<IHedgingWizardViewModel, HedgingWizardViewModel>();
builder.Services.AddTransient<IHedgingDiagramViewModel, HedgingDiagramViewModel>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
