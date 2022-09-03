using System.Net.Http.Headers;
using Gateway.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

public class Program
{
    public static async Task Main(string[] args)
    {
        var app = GetApp(args);

        await app.RunAsync();
    }

    public static WebApplication GetApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

        builder.Services.AddControllers().PartManager.ApplicationParts.Add(new AssemblyPart(typeof(Program).Assembly));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpClient("SubService",opt =>
        {
            opt.BaseAddress = new Uri("http://localhost:3000/");
        });
        builder.WebHost.UseUrls("http://localhost:5289");
        

        var app = builder.Build();

// Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();

        return app;
    }
}