using MediatR;
using Web.Models.Endpoints;

namespace Web.UseCases.UrlToken.SetTokenUsed;

public class SetTokenUsedCommand : IRequest<Result<SetTokenUsedResponse>>
{
    public string Token { get; set; } = null!;
}