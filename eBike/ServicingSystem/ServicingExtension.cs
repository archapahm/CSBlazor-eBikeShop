using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServicingSystem.BLL;
using ServicingSystem.DAL;

namespace ServicingSystem
{
    public static class ServicingExtension
    {
        public static void ServicingDependencies(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<ServicingContext>(options);
            services.AddTransient<ServicingService>((serviceProvider) =>
            {
                var context = serviceProvider.GetRequiredService<ServicingContext>();
                return new ServicingService(context);
            });
        }
    }
}
