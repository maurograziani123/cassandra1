using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using cartservice.cartstore;
using cartservice.services;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Collections.Generic;

namespace cartservice
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {         

                
            string redisAddress = Configuration["REDIS_ADDR"];
            RedisCartStore cartStore = null;
            if (string.IsNullOrEmpty(redisAddress))
            {
                Console.WriteLine("Redis cache host(hostname+port) was not specified.");
                Console.WriteLine("This sample was modified to showcase OpenTelemetry RedisInstrumentation.");
                Console.WriteLine("REDIS_ADDR environment variable is required.");
                System.Environment.Exit(1);
            }
            cartStore = new RedisCartStore(redisAddress);

            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService("cartservice")
                .AddAttributes(new Dictionary<string, object> {
                    { "redis", redisAddress}
                })
                .AddTelemetrySdk();  

            // Initialize the redis store
            cartStore.InitializeAsync().GetAwaiter().GetResult();
            Console.WriteLine("Initialization completed");

            services.AddSingleton<ICartStore>(cartStore);

            services.AddOpenTelemetryTracing((builder) => builder
                .AddRedisInstrumentation(
                    cartStore.GetConnection(),
                    options => options.SetVerboseDatabaseStatements = true)
                .AddAspNetCoreInstrumentation()
                .AddGrpcClientInstrumentation()
                .AddHttpClientInstrumentation()
                .SetResourceBuilder(resourceBuilder)
                .AddSource("cartservice)")
                .AddOtlpExporter());

            services.AddGrpc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        private static void Enrich(Activity activity, string eventName, object obj)
        {
            Console.WriteLine("Enrich : " + eventName);
            if (obj is HttpRequest request)
            {
                var context = request.HttpContext;
                activity.AddTag("http.client_ip", context.Connection.RemoteIpAddress);
            }
            else if (obj is HttpResponse response)
            {
                activity.AddTag("http.response_content_length", response.ContentLength);
                activity.AddTag("http.response_content_type", response.ContentType);
            }
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<CartService>();
                endpoints.MapGrpcService<cartservice.services.HealthCheckService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}