using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.EntityFrameworkCore;
using PodcastApp.Data;
using PodcastApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// AWS setup
// ============================================================
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSimpleSystemsManagement>();
builder.Services.AddSingleton<ParameterStore>();

// Determine AWS region (default to us-east-1)
var region = RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"] ?? "us-east-1");

// ============================================================
// Choose SQL connection string (local or AWS Parameter Store)
// ============================================================
string connStr;

if (bool.TryParse(builder.Configuration["AWS:UseParameterStore"], out var usePS) && usePS)
{
    // Build a temporary provider to fetch parameter
    var sp = builder.Services.BuildServiceProvider();
    var ps = sp.GetRequiredService<ParameterStore>();
    connStr = await ps.GetSqlConnectionAsync();
    Console.WriteLine("✅ Loaded SQL connection from AWS Parameter Store");
}
else
{
    connStr = builder.Configuration.GetConnectionString("RDSConnection")!;
    Console.WriteLine("⚙️ Using RDS connection string from appsettings.json");
}

// ============================================================
// Configure EF Core (SQL Server + retry logic)
// ============================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connStr, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        sqlOptions.CommandTimeout(60);
    }));

// ============================================================
// Register your AWS and app services
// ============================================================
builder.Services.AddSingleton<S3Service>();
builder.Services.AddSingleton<DynamoCommentsService>();

// ============================================================
// Add MVC + Session Support
// ============================================================
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor(); // ✅ allows session access in views

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session expires after 30 mins
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ============================================================
// Build the app
// ============================================================
var app = builder.Build();

// ============================================================
// Middleware pipeline
// ============================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Enable session BEFORE endpoints
app.UseSession();

// ============================================================
// Routing
// ============================================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
