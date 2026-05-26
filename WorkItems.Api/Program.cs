using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WorkItems.Api.Data;
using WorkItems.Api.Intefaces;
using WorkItems.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var usersServiceUrl = builder.Configuration["ServiceUrls:UsersService"]
    ?? throw new InvalidOperationException("La URL del servicio de usuarios no está configurada.");

builder.Services.AddDbContext<WorkItemsDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient("UsersServiceClient", client =>
{
    client.BaseAddress = new Uri(usersServiceUrl);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
}); ;

builder.Services.AddScoped<IDistributionService, DistributionService>();
builder.Services.AddScoped<IWorkItemApplicationService, WorkItemApplicationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<WorkItemsDbContext>();
        context.Database.EnsureCreated();
        WorkItemSeeder.Seed(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error inicializando la base de datos de WorkItems.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
