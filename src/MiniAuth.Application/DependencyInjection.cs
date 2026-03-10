using Microsoft.Extensions.DependencyInjection;
using MiniAuth.Application.Interfaces;
using MiniAuth.Application.Services;

namespace MiniAuth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICommentService, CommentService>();

        return services;
    }
}
