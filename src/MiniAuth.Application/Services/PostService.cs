using MiniAuth.Application.DTOs;
using MiniAuth.Application.Interfaces;
using MiniAuth.Application.Requests;
using MiniAuth.Domain.Entities;
using MiniAuth.Domain.Exceptions;
using MiniAuth.Domain.Interfaces;
using MiniAuth.Domain.QueryObjects;

namespace MiniAuth.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly ICurrentUser _currentUser;

    public PostService(IPostRepository postRepository, ICurrentUser currentUser)
    {
        _postRepository = postRepository;
        _currentUser = currentUser;
    }

    public async Task<PostDto> CreateAsync(CreatePostRequest request, CancellationToken ct = default)
    {
        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            AuthorId = _currentUser.UserId
        };

        _postRepository.Add(post);
        await _postRepository.SaveChangesAsync(ct);

        return PostDto.FromEntity(post);
    }

    public async Task<PostDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var post = await _postRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Post), id);

        return PostDto.FromEntity(post);
    }

    public async Task<PaginatedResult<PostDto>> ListAsync(PostQuery query, int page, int size, CancellationToken ct = default)
    {
        var result = await _postRepository.GetPaginatedAsync(query, page, size, ct);

        return new PaginatedResult<PostDto>
        {
            Data = result.Data.Select(PostDto.FromEntity),
            TotalCount = result.TotalCount,
            PageIndex = result.PageIndex,
            PageSize = result.PageSize
        };
    }

    public async Task<PostDto> UpdateAsync(Guid id, UpdatePostRequest request, CancellationToken ct = default)
    {
        var post = await _postRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Post), id);

        if (post.AuthorId != _currentUser.UserId)
            throw new DomainException("You can only edit your own posts.");

        post.Title = request.Title;
        post.Content = request.Content;
        post.IsPublished = request.IsPublished;

        if (request.IsPublished && post.PublishedAt is null)
            post.PublishedAt = DateTime.UtcNow;

        _postRepository.Update(post);
        await _postRepository.SaveChangesAsync(ct);

        return PostDto.FromEntity(post);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var post = await _postRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Post), id);

        if (post.AuthorId != _currentUser.UserId)
            throw new DomainException("You can only delete your own posts.");

        _postRepository.Remove(post);
        await _postRepository.SaveChangesAsync(ct);
    }
}
