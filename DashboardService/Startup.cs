using System;
using System.Collections.Generic;
using DashboardService.DataAccess.Elastic;
using DashboardService.Domain;
using DashboardService.Init;
using DashboardService.Messaging.RabbitMq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PolicyService.Api.Events;
using Steeltoe.Discovery.Client;

namespace DashboardService;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDiscoveryClient(Configuration);

        services.AddMvc()
            .AddNewtonsoftJson();

        services.AddMediatR(opts => opts.RegisterServicesFromAssemblyContaining<Startup>());

        services.AddElasticSearch(Configuration.GetConnectionString("ElasticSearchConnection"));

        services.AddSingleton<IPolicyRepository, ElasticPolicyRepository>();

        services.AddRabbitListeners(Configuration.GetSection("RabbitMqOptions").Get<RabbitMqOptions>());

        services.AddInitialSalesData();

        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseRabbitListeners(new List<Type> { typeof(PolicyCreated) });

        // 👉 QUAN TRỌNG (thiếu là không register được)
        app.UseDiscoveryClient();
    }
}