using Final_POE.Data;
using Microsoft.EntityFrameworkCore;
using Final_POE.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Configure Services ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register the ClaimService for dependency injection
builder.Services.AddScoped<ClaimService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- Configure Application Pipeline ---
// ... (omitted boilerplate)

app.UseRouting();

app.UseAuthorization();

// Set the default controller and action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=ClaimMvc}/{action=Submit}/{id?}");

app.Run();