using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniAuth.Domain.Interfaces;
using MiniAuth.Infrastructure.Data;
using MiniAuth.Infrastructure.Messaging;
using MiniAuth.Infrastructure.Repositories;

namespace MiniAuth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                .UseLazyLoadingProxies());

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();

        // RabbitMQ — Singleton porque a conexão é cara e thread-safe
        services.AddSingleton<IEventPublisher, RabbitMqPublisher>();

        return services;
    }
}
