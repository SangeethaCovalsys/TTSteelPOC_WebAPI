using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using TTSteelWebAPI.Data;
using TTSteelWebAPI.Interface;
using TTSteelWebAPI.Service;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ICurrentUserInterface, CurrentUserService>();

// db name dynamically change based on user login db name.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DbConectionContext>();
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var baseConnection = configuration.GetConnectionString("HanaCon");

    //var dbName = httpContextAccessor.HttpContext?.User?.FindFirst("Database")?.Value;
    //var builderConn = new SqlConnectionStringBuilder(baseConnection);

    //if (!string.IsNullOrEmpty(dbName))
    //{
    //    builderConn.InitialCatalog = dbName;   // change database safely 
    //    options.UseSqlServer(builderConn.ConnectionString);
    //}
    //else
    //{
    //    options.UseSqlServer(baseConnection);
    //}
});

//builder.Services.AddDbContext<SboCommonContext>(options =>
//options.UseSqlServer(builder.Configuration.GetConnectionString("SboCommonConnection")));

var sapBaseUrl = builder.Configuration["SapSettings:BaseUrl"];


if (string.IsNullOrEmpty(sapBaseUrl))
    throw new Exception("Missing configuration: SapSettings:BaseUrl in appsettings.json");

builder.Services.AddHttpClient<SapService>(client =>
{
    client.BaseAddress = new Uri(sapBaseUrl);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("*")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<DbConectionContext>();
var app = builder.Build();
app.UseCors("AllowFrontend");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();