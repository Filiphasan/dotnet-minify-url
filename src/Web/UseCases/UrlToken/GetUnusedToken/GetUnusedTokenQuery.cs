using MediatR;
using Web.Models.Endpoints;

namespace Web.UseCases.UrlToken.GetUnusedToken;

public class GetUnusedTokenQuery : IRequest<Result<GetUnusedTokenResponse>>
{
}