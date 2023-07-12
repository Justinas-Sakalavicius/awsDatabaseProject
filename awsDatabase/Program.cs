using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using awsDatabase.Data;
using awsDatabase.Repositories;
using awsDatabase.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;

namespace awsDatabase
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddDbContext<ImageDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            builder.Services.AddScoped<IImageRepository, ImageRepository>();
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
            builder.Services.AddAWSService<IAmazonS3>();
            builder.Services.AddAWSService<IAmazonSQS>();
            builder.Services.AddAWSService<IAmazonSimpleNotificationService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IS3Service, S3Service>();
            builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
            builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ImageDbContext>();
                    // This line applies the migrations
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                    // You may choose to exit the program if database migration fails, depending on your needs
                }
            }

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}