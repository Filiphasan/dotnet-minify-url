using Carter;
using Web.Common.Models.Options;
using Web.Helpers;

namespace Web.Endpoints;

public class TestEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/test", (AppSettingModel setting) =>
        {
            var tokenBuilder = new TokenBuilder()
                .WithEpoch(setting.UrlToken.EpochDate)
                .WithAdditionalCharLength(3);

            var token = tokenBuilder.Build();

            return Results.Ok(token);
        });
    }
}