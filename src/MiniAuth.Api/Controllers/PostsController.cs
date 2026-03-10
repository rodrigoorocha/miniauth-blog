using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAuth.Application.DTOs;
using MiniAuth.Application.Interfaces;
using MiniAuth.Application.Requests;
using MiniAuth.Domain.QueryObjects;

namespace MiniAuth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedResult<PostDto>), 200)]
    public async Task<IActionResult> List(
        [FromQuery] PostQuery query,
        [FromQuery] int page = 0,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        var result = await _postService.ListAsync(query, page, size, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PostDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var post = await _postService.GetByIdAsync(id, ct);
        return Ok(post);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PostDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest request, CancellationToken ct)
    {
        var post = await _postService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PostDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequest request, CancellationToken ct)
    {
        var post = await _postService.UpdateAsync(id, request, ct);
        return Ok(post);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _postService.DeleteAsync(id, ct);
        return NoContent();
    }
}
