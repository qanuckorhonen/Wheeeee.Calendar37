using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using Wheeeee.Calendar37.Repositories;
using Wheeeee.Calendar37.Repositories.Interfaces;

namespace Wheeeee.Calendar37.WebView
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            CalenderRepository repository = new(builder.Configuration.GetConnectionString("DefaultConnection"));
            repository.ExceptionOccurred += (sender, e) =>
            {
                // Log the exception or handle it as needed
                Debug.WriteLine($"An error occurred: {e.Exception.Message}");
            };
            builder.Services.AddSingleton<ICalenderRepository>(repository);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}