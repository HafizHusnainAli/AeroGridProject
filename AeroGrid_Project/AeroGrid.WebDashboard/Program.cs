using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AeroGrid.WebDashboard.Components;
using AeroGrid.WebDashboard.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MODIFIED BY AI (Sprint 6 - API Task): BaseAddress now points at Open-Meteo instead of the
// old OpenWeatherMap host. WeatherApiService.cs actually issues fully-qualified URLs for both
// the geocoding host and the forecast host it needs, so this BaseAddress isn't strictly load
// -bearing today -- it's kept set (rather than removed) for discoverability/documentation and
// as a safety net for any future relative-URL calls added to the forecast host.
builder.Services.AddHttpClient<WeatherApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com/");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<AeroGrid.WebDashboard.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
