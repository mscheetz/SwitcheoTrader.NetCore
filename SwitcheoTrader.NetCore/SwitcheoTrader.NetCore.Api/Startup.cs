using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using SwitcheoTrader.NetCore.Business;
using SwitcheoTrader.NetCore.Business.Interfaces;
using SwitcheoTrader.NetCore.Core;
using SwitcheoTrader.NetCore.Data;
using SwitcheoTrader.NetCore.Data.Interfaces;
using SwitcheoTrader.NetCore.Manager;

namespace SwitcheoTrader.NetCore.Api
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
            services.AddMvc();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddTransient<IOrderBookTradeBuilder, OrderBookTradeBuilder>();
            services.AddTransient<ITradeBuilder, TradeBuilder>();
            services.AddTransient<ISwitcheoTraderService, SwitcheoTraderManager>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "SwitcheoTrader.NetCore API",
                    Description = "RESTful API endpoints for Switcheo Exchagne",
                    Contact = new Contact { Name = "CryptoBitfolio", Email = "cryptoBitfolio@gmail.com", Url = "https://twitter.com/CBitfolio" },
                    Version = "1.0"

                });
                var filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "project.xml");
                c.IncludeXmlComments(filePath);
                c.DescribeAllEnumsAsStrings();
                //c.DocumentFilter<SwaggerAddEnumDescriptions>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            app.UseMiddleware<ExceptionHandler>();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SwitcheoTrader.NetCore API V1");
            });

            app.UseMvc();
        }
    }
}
