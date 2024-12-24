using MediatR;
using MongoDB.Driver;
using Web.Common.Models.Endpoints;
using Web.Data;

namespace Web.UseCases.UrlToken.SetTokenUsed;

public class SetTokenUsedHandler(MongoDbContext dbContext) : IRequestHandler<SetTokenUsedCommand, Result<SetTokenUsedResponse>>
{
    public async Task<Result<SetTokenUsedResponse>> Handle(SetTokenUsedCommand request, CancellationToken cancellationToken)
    {
        var filter = Builders<Data.Entities.UrlToken>.Filter
            .Eq(x => x.Token, request.Token);
        var updateFilter = Builders<Data.Entities.UrlToken>.Update
            .Set(x => x.IsUsed, true)
            .Set(x => x.UsedAt, DateTime.UtcNow);
        await dbContext.UrlTokens.UpdateOneAsync(filter, updateFilter, cancellationToken: cancellationToken);
        return Result<SetTokenUsedResponse>.Success(new SetTokenUsedResponse());
    }
}