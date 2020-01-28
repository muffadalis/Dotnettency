using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Piranha;
using Piranha.AspNetCore.Identity.SQLite;
using Dotnettency;
using System;
using Microsoft.Extensions.Configuration;
using Piranha.AttributeBuilder;

namespace MvcWeb
{
    public class Startup
    {
        [Obsolete]
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var defaultServices = services.Clone();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMultiTenancy<Tenant>((builder) =>
            {
                builder.IdentifyTenantsWithRequestAuthorityUri()
                       .InitialiseTenant<TenantShellFactory>()
                       .AddAspNetCore()
                       .ConfigureTenantContainers((containerOptions) =>
                       {
                           var x = 0;
                           containerOptions
                           .SetDefaultServices(defaultServices)
                           .Autofac((tenant, services) =>
                           {
                               if (tenant != null)
                               {
                                   services.AddControllersWithViews();
                                   services.AddRazorPages()
                                       .AddPiranhaManagerOptions();

                                   services.AddPiranha();
                                   services.AddPiranhaApplication();
                                   services.AddPiranhaFileStorage();
                                   services.AddPiranhaImageSharp();
                                   services.AddPiranhaManager();
                                   services.AddPiranhaTinyMCE();
                                   // services.AddPiranhaApi();

                                   services.AddPiranhaEF(options =>
                                       options.UseSqlite(tenant.ConnectionString));
                                   //services.AddPiranhaIdentityWithSeed<IdentitySQLiteDb>(options =>
                                   //    options.UseSqlite(tenant.ConnectionString));

                                   services.AddMemoryCache();
                                   services.AddPiranhaMemoryCache();
                                   //tenantServices.AddRazorPages((o) =>
                                   //{
                                   //    o.RootDirectory = $"/Pages/{tenant.Name}";
                                   //}).AddNewtonsoftJson();
                               }
                           });
                       })
                       .ConfigureTenantMiddleware((tenantOptions) =>
                       {
                           var x = tenantOptions;

                           tenantOptions.AspNetCorePipeline((context, app) =>
                           {
                               app.Use(async (c, next) =>
                               {
                                   Console.WriteLine("Entering tenant pipeline: " + context.Tenant?.Name);
                                   await next.Invoke();
                               });

                               app.UseRouting();

                               if (context.Tenant != null)
                               {
                                   var api = app.ApplicationServices.GetService<IApi>();

                                   var type = typeof(App);
                                   System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);

                                   App.Init(api);

                                   // Configure cache level
                                   App.CacheLevel = Piranha.Cache.CacheLevel.Full;

                                   var builder = new ContentTypeBuilder(api)
                                    .AddAssembly(typeof(Startup).Assembly)
                                    .Build()
                                    .DeleteOrphans();

                                   //// Build content types
                                   //var pageTypeBuilder = new Piranha.AttributeBuilder.PageTypeBuilder(api)
                                   //    .AddType(typeof(Models.BlogArchive))
                                   //    .AddType(typeof(Models.StandardPage))
                                   //    .AddType(typeof(Models.TeaserPage))
                                   //    .Build()
                                   //    .DeleteOrphans();
                                   //var postTypeBuilder = new Piranha.AttributeBuilder.PostTypeBuilder(api)
                                   //    .AddType(typeof(Models.BlogPost))
                                   //    .Build()
                                   //    .DeleteOrphans();
                                   //var siteTypeBuilder = new Piranha.AttributeBuilder.SiteTypeBuilder(api)
                                   //    .AddType(typeof(Models.StandardSite))
                                   //    .Build()
                                   //    .DeleteOrphans();

                                   // Register middleware
                                   app.UseStaticFiles();
                                   app.UsePiranha();
                                   app.UseRouting();
                                   app.UseAuthentication();
                                   app.UseAuthorization();
                                   app.UsePiranhaIdentity();
                                   app.UsePiranhaManager();
                                   app.UsePiranhaTinyMCE();
                                   app.UseEndpoints(endpoints =>
                                   {
                                       endpoints.MapControllerRoute(
                                           name: "default",
                                           pattern: "{controller=Home}/{action=Index}/{id?}");

                                       endpoints.MapPiranhaManager();
                                   });

                                   Seed.RunAsync(api, context.Tenant).GetAwaiter().GetResult();

                                   //tenantAppBuilder.UseAuthorization();

                                   //tenantAppBuilder.Use(async (c, next) =>
                                   //{
                                   //     // Demonstrates per tenant files.
                                   //     // /foo.txt exists for one tenant but not another.
                                   //     var webHostEnvironment = c.RequestServices.GetRequiredService<IWebHostEnvironment>();
                                   //    var contentFileProvider = webHostEnvironment.ContentRootFileProvider;
                                   //    var webFileProvider = webHostEnvironment.WebRootFileProvider;

                                   //    var fooTextFile = webFileProvider.GetFileInfo("/foo.txt");

                                   //    Console.WriteLine($"/Foo.txt file exists? {fooTextFile.Exists}");
                                   //    await next.Invoke();
                                   //});

                                   //tenantAppBuilder.UseEndpoints(endpoints =>
                                   //{
                                   //    endpoints.MapRazorPages();
                                   //});
                               }

                           });
                       })
                       ;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //App.Init(api);

            //// Configure cache level
            //App.CacheLevel = Piranha.Cache.CacheLevel.Full;

            //// Build content types
            //var pageTypeBuilder = new Piranha.AttributeBuilder.PageTypeBuilder(api)
            //    .AddType(typeof(Models.BlogArchive))
            //    .AddType(typeof(Models.StandardPage))
            //    .AddType(typeof(Models.TeaserPage))
            //    .Build()
            //    .DeleteOrphans();
            //var postTypeBuilder = new Piranha.AttributeBuilder.PostTypeBuilder(api)
            //    .AddType(typeof(Models.BlogPost))
            //    .Build()
            //    .DeleteOrphans();
            //var siteTypeBuilder = new Piranha.AttributeBuilder.SiteTypeBuilder(api)
            //    .AddType(typeof(Models.StandardSite))
            //    .Build()
            //    .DeleteOrphans();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMultitenancy<Tenant>((builder) =>
            {
                builder.UseTenantContainers()
                       .UsePerTenantMiddlewarePipeline(app);
            });
        }
    }
}
