using ex_URLShortenHashIDs.Data;
using ex_URLShortenHashIDs.Entities;
using ex_URLShortenHashIDs.Models;
using HashidsNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

var builder = WebApplication.CreateBuilder(args);

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("api/shorten", async (ShortenUrlRequest request
    ,IHashids hashIds, IDbContextFactory<dbContext> contextFactory,
    HttpContext httpContext) =>
{
    if (!Uri.TryCreate(request.URL, UriKind.Absolute, out _))
    {
        return Results.BadRequest("The URL is invalid");
    }

    using var context = contextFactory.CreateDbContext();
    ShortenedURL url = new ShortenedURL
    {
        URL = request.URL,
        CreatedOnUTC = DateTime.UtcNow
    };
    context.ShortenedURLs.Add(url);
    await context.SaveChangesAsync();

    return Results.Ok(hashIds.Encode(url.Id));

});
app.MapGet("api/{code}", async (string code, 
    IDbContextFactory<dbContext> contextFactory,
    IHashids hashIds) =>
{
    using var context = contextFactory.CreateDbContext();
    int urlID;
    try
    {
        urlID = hashIds.DecodeSingle(code);
    }
    catch (Exception)
    {

        return Results.NotFound("The URL not found");
    }
    
    var url = await context.ShortenedURLs.FirstOrDefaultAsync(x=>x.Id == urlID);
    if (url is null)
    {
        return Results.NotFound("The URL not found");
    }

    return Results.Redirect(url.URL);
});


using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
{
    dbContext context = (dbContext)serviceScope.ServiceProvider.GetRequiredService<dbContext>();
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        Console.WriteLine($"You have {pendingMigrations.Count()} pending migrations to apply.");
        Console.WriteLine("Applying pending migrations now");
        await context.Database.MigrateAsync();
        //context.ApplyHypertables();
    }
     



}
app.UseHttpsRedirection();



app.Run();

