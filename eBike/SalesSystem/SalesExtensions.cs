using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SalesSystem.BLL;
using SalesSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem
{
    public static class SalesExtensions
    {
        public static void SalesDependencies(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<SalesContext>(options);
            services.AddTransient<SalesServices>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<SalesContext>();
                return new SalesServices(context);
            });
        }
    }
}
