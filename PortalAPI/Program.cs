using PortalAPI.Data;
using PortalAPI.Model;
using PortalAPI.Utilites;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Mvc;
using PortalAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<DbConectionContext>();
builder.Services.AddScoped<HanaServiceLayer>();
builder.Services.AddScoped<UserModal>();
builder.Services.AddScoped<AuditLogClass>();
builder.Services.AddScoped<ISupplierDocumentsServices,SupplierDocumentsServices>();
//builder.Services.AddScoped<DemandPlanAutomationServices>();
builder.Services.AddScoped<IDbConectionContext>();
builder.Services.AddCors();
builder.Services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Error() // Log only errors
            .WriteTo.Console()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day) // Log to a file
            .CreateLogger();
var dbType = builder.Configuration["DbType"]; // e.g., "HANA"
builder.Services.AddScoped<DemandPlanAutomationServices>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<DemandPlanAutomationServices>>();
    return new DemandPlanAutomationServices(dbType, logger);
});

builder.Services.AddLogging(builder =>
{
    builder.AddSerilog();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(builder => builder
     .AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader());
 
app.UseAuthorization();
app.UseExceptionHandler("/error");
app.UseHsts();
app.MapControllers();

app.Run();
