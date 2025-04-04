using PloomesCRM.ERP.Proxy;
using PloomesCRM.ERP.Proxy.Entities;
using PloomesCRM.ERP.Proxy.Helpers;

namespace Ploomes.ERP.Proxy
{
    public static class Program
    {
        public static IConfigurationSection LogWriterSection { get; set; }
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHttpClient();
            builder.Services.AddLogging();

            LogWriterSection = builder.Configuration.GetSection("LogWriter");

            var app = builder.Build();

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
        }
    }
}

