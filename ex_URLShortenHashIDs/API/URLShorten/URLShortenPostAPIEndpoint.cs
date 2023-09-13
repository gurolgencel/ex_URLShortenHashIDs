using Carter;
using ex_URLShortenHashIDs.Data;
using ex_URLShortenHashIDs.Entities;
using ex_URLShortenHashIDs.Models;
using HashidsNet;
using Microsoft.EntityFrameworkCore;

namespace ex_URLShortenHashIDs.API.URLShorten
{
    public sealed class URLShortenPostAPIEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/shorten", async (ShortenUrlRequest request
                , IHashids hashIds, IDbContextFactory<dbContext> contextFactory,
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
        }
    }
}
