using HomelabDashboard.Components;
using HomelabDashboard.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Mocked data services — swap these out for real API/exporter calls later
// (Pi-hole API, df/du stats, Docker socket, etc.) without touching the UI.
builder.Services.AddSingleton<IDashboardStatsProvider, MockDashboardStatsProvider>();
builder.Services.AddSingleton<IServiceCatalog, ServiceCatalog>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for
    // production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
