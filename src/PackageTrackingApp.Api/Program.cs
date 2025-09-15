using PackageTrackingApp.Api;
using PackageTrackingApp.Api.Middlewares;
using PackageTrackingApp.Data.Context;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configure your custom services
        builder.Services.ConfigureServices(builder.Configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Package Tracking API V1");
                c.RoutePrefix = "swagger";
            });
        }
        else
        {
            app.UseHsts();
        }

        // Initialize database
        using (var serviceScope = app.Services.CreateScope())
        {
            var loggerFactory = serviceScope.ServiceProvider.GetService<ILoggerFactory>();
            try
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                var logger = loggerFactory?.CreateLogger<Program>();
                logger?.LogError(ex, "Error occurred while creating database");
            }
        }

        app.UseMiddleware<ExceptionMiddleware>();
        app.UseRouting();
        app.UseCors();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Swagger available at: /swagger");
        });

        app.Run();
    }
}