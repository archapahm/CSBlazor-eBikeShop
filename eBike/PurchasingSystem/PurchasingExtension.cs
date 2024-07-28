using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PurchasingSystem.BLL;
using PurchasingSystem.DAL;

namespace PurchasingSystem
{
    public static class PurchasingExtension
    {
        public static void PurchasingDependencies(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<PurchasingContext>(options);
            services.AddTransient<PurchasingService>((serviceProvider) =>
            {
                var context = serviceProvider.GetRequiredService<PurchasingContext>();
                return new PurchasingService(context);
            });
        }
    }
}
