using Checkme.BL;
using Checkme.BL.Abstract;
using Checkme.DAL.Abstract;
using Checkme.DAL.Azure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Checkme.API
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
            services.AddLogging(config => config.AddConsole());
            services.AddControllers();
            services.AddSingleton<IListService, ListService>();
            services.AddSingleton<IBlobStorageRepo, BlobStorageRepo>();
            services.AddSingleton<Config>(new Config()
            {
                ConnectionString = "DefaultEndpointsProtocol=https;AccountName=vbrokerblobstorage;AccountKey=+r86vEEQdyQ8NGrheZtYUb/WLbKRIfEeSVpQQ8QVH61UFrnsmtecStAUzTGBecg5BSNMJ4YrBFqU028W6SYA+Q==;EndpointSuffix=core.windows.net",
                TypeId = "checkme"
            });
            services.AddSwaggerDocument();
            services.AddCors(options => options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseCors();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseOpenApi();

            app.UseSwaggerUi3();
        }
    }
}
