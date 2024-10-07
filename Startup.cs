using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Dependency_Injection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

namespace EmployeeManagementAPI
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

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder.WithOrigins("http://localhost:58692") // Specify your Angular app URL
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());
            });

            services.AddControllers();

            // JWT Authentication Configuration
            var key = Encoding.ASCII.GetBytes("6mP87jTr2HnB$2p4&Zy9!qXcR5!w8qG7nF3vLk9Sx8Qy3EwJb4Pz1T7Kx6R2A8Z1B5Tj1Fq9R0v8D5Lq2Ue8C7!p9A0g6H8!m");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true; // Ensure tokens are transmitted over HTTPS in production
                options.SaveToken = true; // Save token to AuthenticationProperties

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // Validate the signing key
                    IssuerSigningKey = new SymmetricSecurityKey(key), // The key used for signing and validating the token

                    ValidateIssuer = false, // Skip issuer validation for simplicity (can set to true if you want to validate the issuer)
                    ValidateAudience = false, // Skip audience validation (can set to true for added security)

                    ValidateLifetime = true, // Validate the token's expiration time (recommended to prevent token reuse)
                    ClockSkew = TimeSpan.Zero // Optional: eliminates the 5-minute default clock skew
                };
            });

            // Swagger Configuration
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EmployeeManagementAPI", Version = "v1" });
            });

            // Database Context
            services.AddDbContext<EmployeeContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Dependency Injection for Employee Repository
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EmployeeManagementAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("AllowSpecificOrigin");

            // Order of these middlewares is important
            app.UseAuthentication(); // Authentication should be before authorization
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
