using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAuth.Application.DTOs;
using MiniAuth.Application.Interfaces;
using MiniAuth.Application.Requests;

namespace MiniAuth.Api.Controllers;

[ApiController]
[Route("api/posts/{postId:guid}/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CommentDto>), 200)]
    public async Task<IActionResult> ListByPost(Guid postId, CancellationToken ct)
    {
        var comments = await _commentService.ListByPostAsync(postId, ct);
        return Ok(comments);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CommentDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(Guid postId, [FromBody] CreateCommentRequest request, CancellationToken ct)
    {
        var comment = await _commentService.CreateAsync(postId, request, ct);
        return Created("", comment);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid postId, Guid id, CancellationToken ct)
    {
        await _commentService.DeleteAsync(id, ct);
        return NoContent();
    }
}
