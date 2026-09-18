namespace Library.Application.Categories.Queries;

using Library.Domain.Interfaces;
using MediatR;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryCacheService _categoryCacheService;
    public GetAllCategoriesQueryHandler(ICategoryCacheService categoryCacheService) => _categoryCacheService = categoryCacheService;

    public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryCacheService.GetAllCategoriesAsync(cancellationToken);

        return categories.Select(c => new CategoryDto(c.Id, c.Name)).ToList();
    }
}

public record GetAllCategoriesQuery : IRequest<List<CategoryDto>>;
public record CategoryDto(Guid Id, string Name);
