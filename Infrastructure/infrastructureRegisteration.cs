using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.IServices;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Infrastructure
{
    public static class infrastructureRegisteration
    {
        public static IServiceCollection infrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the DashboardService as the implementation for IDashboardService
            services.AddScoped<IDashboardService, DashboardService>();
            // Register the ProjectTaskService as the implementation for IProjectTaskService
            services.AddScoped<IProjectTaskService, ProjectTaskService>();

            // Register the ProjectMembers service as the implementation for IProjectMembers
            services.AddScoped<IProjectMembers, ProjectMembers>();
            // Register the ProjectService as the implementation for IProjectRepository
            services.AddScoped<IProjectService, ProjectService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            //register email sender
            services.AddScoped<IEmailService, EmailService>();

            //register token
            services.AddScoped<IGenerateToken, GenerateToken>();

            services.AddDbContext<AppDbContext>(op =>
            {
                op.UseSqlServer(configuration.GetConnectionString("TaskManagementDatabase"));
            });


            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
          .AddCookie(x =>
         {
           x.Cookie.Name = "token";
           x.Events.OnRedirectToLogin = context =>
           {
               context.Response.StatusCode = StatusCodes.Status401Unauthorized;
               return Task.CompletedTask;
           };
         })
       .AddJwtBearer(x =>
       {
           x.RequireHttpsMetadata = false;
           x.SaveToken = true;
           x.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuerSigningKey = true,

               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Token:Secret"])),
               ValidateIssuer = true,
               ValidIssuer = configuration["Token:Issuer"],
               ValidateAudience = false,
               ClockSkew = TimeSpan.Zero
           };
           x.Events = new JwtBearerEvents
           {

               OnMessageReceived = context =>
               {
                   var token = context.Request.Cookies["token"];
                   context.Token = token;
                   return Task.CompletedTask;
               }
           };
       });


            return services;
        }
    }
}
