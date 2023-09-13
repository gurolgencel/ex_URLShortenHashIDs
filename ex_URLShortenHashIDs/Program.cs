using Carter;
using ex_URLShortenHashIDs.Data;
using ex_URLShortenHashIDs.Entities;
using ex_URLShortenHashIDs.Models;
using HashidsNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCarter();
IConfiguration configurationFile = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
builder.Services.AddDbContextFactory<dbContext>(opt =>
opt.UseNpgsql(configurationFile.GetConnectionString("connectionString")), ServiceLifetime.Transient);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSingleton<IHashids>(_ => new Hashids("SecretSalt"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.MapCarter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
{
    dbContext context = (dbContext)serviceScope.ServiceProvider.GetRequiredService<dbContext>();
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        Console.WriteLine($"You have {pendingMigrations.Count()} pending migrations to apply.");
        Console.WriteLine("Applying pending migrations now");
        await context.Database.MigrateAsync();
    }
     
}
app.UseHttpsRedirection();
app.Run();

