using MediatR;
using Web.Constants;
using Web.Models.Endpoints;
using Web.Services.Interfaces;
using Web.UseCases.UrlToken.SetTokenUsed;

namespace Web.UseCases.UrlToken.GetUnusedToken;

public class GetUnusedTokenHandler(ISender sender, ICacheService cacheService) : IRequestHandler<GetUnusedTokenQuery, Result<GetUnusedTokenResponse>>
{
    public async Task<Result<GetUnusedTokenResponse>> Handle(GetUnusedTokenQuery request, CancellationToken cancellationToken)
    {
        var token = await cacheService.ListLeftPopAsync<string>(RedisConstant.Key.TokenSeedList);
        if (string.IsNullOrEmpty(token))
        {
            return Result<GetUnusedTokenResponse>.Error(404, "No token available");
        }

        await sender.Send(new SetTokenUsedCommand { Token = token }, cancellationToken);
        return Result<GetUnusedTokenResponse>.Success(new GetUnusedTokenResponse { Token = token });
    }
}