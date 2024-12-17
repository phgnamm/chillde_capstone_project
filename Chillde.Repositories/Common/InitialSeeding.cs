using Chillde.Repositories.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Chillde.Repositories.Common;

/// <summary>
///     This class is used to insert initial data
/// </summary>
public static class InitialSeeding
{
    private static readonly List<Role> Roles =
    [
        new() { Name = Enums.Role.Admin.ToString() },
        new() { Name = Enums.Role.Customer.ToString() },
        new() { Name = Enums.Role.Artist.ToString() }
    ];

    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        foreach (var role in Roles)
            if (!context.Roles.Any(r => r.Name == role.Name))
            {
                role.CreationDate = DateTime.UtcNow;
                context.Roles.Add(role);
            }

        await context.SaveChangesAsync();
    }
}