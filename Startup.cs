using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
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

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(audienceConfig["Secret"]));
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



            services.AddCors(); // Make sure you call this previous to AddMvc
            services.AddCors(options => options.AddPolicy("ApiCorsPolicy", builder =>
                {
                    builder.WithOrigins("http://localhost:52293").AllowAnyMethod().AllowAnyHeader();
                }));

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(
                    options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );

//bad practice : use user secrets or environment vars             options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))

            services.AddDbContext<PersonContext>(options =>
             options.UseSqlServer(cfgbuilder.GetConnectionString("DefaultConnection"))
           );

           services.AddScoped<IPersonRepository, PersonRepository>();
           services.AddScoped<IEventRepository, EventRepository>();
           services.AddScoped<IPersonService, PersonService>();
           services.AddScoped<IEventService, EventService>();
          
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

           // app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
