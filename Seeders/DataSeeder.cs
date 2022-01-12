using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebShopInventory.Data;
using WebShopInventory.Models;

namespace WebShopInventory.Seeders
{
    public static class DataSeeder
    {
        public async static Task Seed(IApplicationBuilder app)
        {
            var DummyData = new List<Product>()
            {
                new Product() {
                    Code="MD-55501",
                    Title = "WD-40 200ml",
                    Description="Etkili pas sökücü",
                    Stock=50, Price=44.10,
                    ImagePath="MD-55501.jpg",
                    Timestamp=BitConverter.GetBytes(DateTime.Now.Ticks)
                },

                new Product() {
                    Code="MD-55502",
                    Title = "Einhell Tc-Gg 30",
                    Description="Silikon Mum Tabancası 30 Watt",
                    Stock=10, Price=149.00, ImagePath="MD-55502.jpg",
                    Timestamp=BitConverter.GetBytes(DateTime.Now.Ticks)
                },
                new Product() { Code="MD-55503",
                    Title = "Ersa Proalet",
                    Description="Silikon Tabancası Plastik",
                    Stock=30, Price=15.98,
                    ImagePath="MD-55503.jfif",
                    Timestamp=BitConverter.GetBytes(DateTime.Now.Ticks)
                },
            };

            var context = app
                .ApplicationServices
                .CreateScope()
                .ServiceProvider
                .GetService<ApplicationDbContext>();

            await context.Database.MigrateAsync();

            if (!await context.Products.AnyAsync())
            {
                await context.Products.AddRangeAsync(DummyData);
                await context.SaveChangesAsync();
            }
        }
    }
}
