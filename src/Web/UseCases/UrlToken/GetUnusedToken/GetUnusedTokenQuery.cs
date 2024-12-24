using MediatR;
using Web.Common.Models.Endpoints;

namespace Web.UseCases.UrlToken.GetUnusedToken;

public class GetUnusedTokenQuery : IRequest<Result<GetUnusedTokenResponse>>
{
}