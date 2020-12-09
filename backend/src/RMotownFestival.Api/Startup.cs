
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RMotownFestival.Api.Common;
using RMotownFestival.Api.Domain;
using RMotownFestival.Api.Options;
using RMotownFestival.DAL;
using System;
using System.Linq;

namespace RMotownFestival.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettingsOptions>(Configuration);

            services.AddCors();
            services.AddControllers();

            services.AddDbContext<MotownDbContext>(
                options => options.UseSqlServer("name=ConnectionStrings:DefaultConnection")
            );
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

            services.AddSingleton(p => new StorageSharedKeyCredential(
                Configuration.GetValue<string>("Storage:AccountName"),
                Configuration.GetValue<string>("Storage:AccountKey")));

            services.AddSingleton(p => new BlobServiceClient(Configuration.GetValue<string>("Storage:ConnectionString")));
            services.AddSingleton<BlobUtility>();

            services.Configure<BlobSettingsOptions>(Configuration.GetSection("Storage"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            SeedData(app);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // THIS IS NOT A SECURE CORS POLICY, DO NOT USE IN PRODUCTION
            app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void SeedData(IApplicationBuilder app)
        {
            using IServiceScope serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            MotownDbContext dbContext = serviceScope.ServiceProvider.GetRequiredService<MotownDbContext>();

            if (!dbContext.Artists.Any())
            {
                dbContext.Artists.AddRange(
                    new Artist[] {
                      new Artist { Name = "Diana Ross", ImagePath = "dianaross.jpg", Website = new Uri("http://www.dianaross.co.uk/indexb.html") },
                      new Artist { Name = "The Commodores", ImagePath = "thecommodores.jpg", Website = new Uri("http://en.wikipedia.org/wiki/Commodores") },
                      new Artist { Name = "Stevie Wonder", ImagePath = "steviewonder.jpg", Website = new Uri("http://www.steviewonder.net/") },
                      new Artist { Name = "Lionel Richie", ImagePath = "lionelrichie.jpg", Website = new Uri("http://lionelrichie.com/") },
                      new Artist { Name = "Marvin Gaye", ImagePath = "marvingaye.jpg", Website = new Uri("http://www.marvingayepage.net/") }
                    }
                );

                dbContext.SaveChanges();
            }
        }
    }
}
