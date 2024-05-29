using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

using Swashbuckle.AspNetCore.Swagger;
using SSSCalApp.Infrastructure.DataContext;
using SSSCalApp.Infrastructure.Repositories;
using SSSCalApp.Core.DomainService;
using SSSCalApp.Core.ApplicationService;
using SSSCalApp.Core.ApplicationService.Services;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SSSCalAppWebAPI
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
             var cfgbuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

           var audienceConfig = Configuration.GetSection("Audience");


        var workingconnvalue = cfgbuilder.GetConnectionString("DefaultConnection");
        var envAudConfig = cfgbuilder.GetSection("Audience").GetChildren();
        var Secret = envAudConfig.Where(v=>v.Key=="Secret").FirstOrDefault().Value;
        

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret));
            var authKey = audienceConfig["ocelotkey"];
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = audienceConfig["Iss"],
                ValidateAudience = true,
                ValidAudience = audienceConfig["Aud"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,
            };
    services.AddAuthentication(sharedOptions =>
{
    sharedOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    sharedOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
            .AddJwtBearer("Bearer", options =>
            {
                //options.Authority = "http://localhost:3600";
                options.RequireHttpsMetadata = false;

                options.Audience = "Family";
                options.TokenValidationParameters = tokenValidationParameters;
             });
/*
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = authKey;
                o.DefaultChallengeScheme = authKey;
            })
            .AddJwtBearer(authKey, x =>
             {
                 x.RequireHttpsMetadata = false;
                 x.TokenValidationParameters = tokenValidationParameters;
             });
*/


           // services.AddCors(); // Make sure you call this previous to AddMvc
            services.AddCors(options => options.AddPolicy("ApiCorsPolicy", builder =>
                {
                    builder.WithOrigins("http://localhost:52293").AllowAnyMethod().AllowAnyHeader();
                    builder.WithOrigins("http://localhost:5000").AllowAnyMethod().AllowAnyHeader();
                    builder.WithOrigins("https://localhost:5001").AllowAnyMethod().AllowAnyHeader();
                    builder.WithOrigins("http://localhost:5020").AllowAnyMethod().AllowAnyHeader();
                    builder.WithOrigins("https://localhost:5021").AllowAnyMethod().AllowAnyHeader();
                    builder.WithOrigins("http://localhost:8080").AllowAnyMethod().AllowAnyHeader();
                    builder.WithOrigins("http://localhost:803").AllowAnyMethod().AllowAnyHeader();
                    builder.WithOrigins("http://blz.schuebelsoftware.com").AllowAnyMethod().AllowAnyHeader();
                    builder.WithOrigins("http://vue.schuebelsoftware.com").AllowAnyMethod().AllowAnyHeader();
                    builder.WithOrigins("https://www.schuebelsoftware.com").AllowAnyMethod().AllowAnyHeader();
                }));

          
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.MaxDepth = 3;
                });
/*
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options => {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.MaxDepth = 3;
                });
*/
//bad practice : use user secrets or environment vars             options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))

            services.AddDbContext<PersonContext>(options =>
             options.UseSqlServer(cfgbuilder.GetConnectionString("DefaultConnection"))
           );

           services.AddScoped<IPersonRepository, PersonRepository>();
           services.AddScoped<IEventRepository, EventRepository>();
           services.AddScoped<IGroupRepository, GroupRepository>();
           services.AddScoped<IPersonService, PersonService>();
           services.AddScoped<IEventService, EventService>();
           services.AddScoped<IGroupService, GroupService>();
           services.AddScoped<IAddressRepository, AddressRepository>();
           services.AddScoped<IAddressService, AddressService>();
          

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {

             if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
             //   app.UseHsts();
            }

        // Make sure you call this before calling app.UseMvc()
        app.UseCors("ApiCorsPolicy");
//            app.UseCors(
//                options => options.WithOrigins("http://localhost:52293/").AllowAnyMethod()
//            );
            app.UseRouting();
           // app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            //2.1core   app.UseMvc();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
        });


        }
    }
}
