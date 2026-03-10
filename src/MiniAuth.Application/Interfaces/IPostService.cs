using MiniAuth.Application.DTOs;
using MiniAuth.Application.Requests;
using MiniAuth.Domain.QueryObjects;

namespace MiniAuth.Application.Interfaces;

public interface IPostService
{
    Task<PostDto> CreateAsync(CreatePostRequest request, CancellationToken ct = default);
    Task<PostDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaginatedResult<PostDto>> ListAsync(PostQuery query, int page, int size, CancellationToken ct = default);
    Task<PostDto> UpdateAsync(Guid id, UpdatePostRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
