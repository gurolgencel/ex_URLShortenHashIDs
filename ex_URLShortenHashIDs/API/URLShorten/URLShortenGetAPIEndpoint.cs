using Carter;
using ex_URLShortenHashIDs.Data;
using HashidsNet;
using Microsoft.EntityFrameworkCore;

namespace ex_URLShortenHashIDs.API.URLShorten
{
    public sealed class URLShortenGetAPIEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
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

                var url = await context.ShortenedURLs.FirstOrDefaultAsync(x => x.Id == urlID);
                if (url is null)
                {
                    return Results.NotFound("The URL not found");
                }

                return Results.Redirect(url.URL);
            });


        }
    }
}
