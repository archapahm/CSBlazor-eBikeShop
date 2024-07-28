using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReceivingSystem.BLL;
using ReceivingSystem.DAL;

namespace ReceivingSystem
{
    public static class ReceivingExtension
    {
        public static void ReceivingDependencies(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<ReceivingContext>(options);
            services.AddTransient<ReceivingService>((serviceProvider) =>
            {
                var context = serviceProvider.GetRequiredService<ReceivingContext>();
                return new ReceivingService(context);
            });
        }
    }
}
