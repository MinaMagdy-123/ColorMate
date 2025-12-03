using ColorMate.BL.EmailService;
using ColorMate.BL.FacebookService;
using ColorMate.BL.UserService;
using ColorMate.Core.Models;
using ColorMate.EF;
using ColorMate.EF.Repositories.Base;
using ColorMate.EF.Repositories.User;
using ColorMate.EF.UnitOfWork;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

namespace ColorMate.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });


            builder.Services.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
            builder.Services.AddTransient<IUserRepository, UserRepository>();
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IFacebookAuthService, FacebookAuthService>();

            builder.Services.AddHttpClient();


            //-----------------------Authentication----------------------------

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
                options =>
                {
                    options.SignIn.RequireConfirmedEmail= true;
                }
                ).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();


            builder.Services.AddAuthentication(options =>
            {
                //check jwt token in header
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; //unauthorize response
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddGoogle(options =>
            {
                var ClientId = builder.Configuration["Authentication:Google:ClientId"];
                var ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];



                options.ClientId = ClientId;
                options.ClientSecret = ClientSecret;

            }).AddFacebook(facebookOptions =>
            {
                var AppId = builder.Configuration["Authentication:Facebook:AppId"];
                var AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
                facebookOptions.AppId = AppId;
                facebookOptions.AppSecret = AppSecret;

            })
            .AddJwtBearer(options => //verified key
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:IssuerURL"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:AudienceURL"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecritKey"] ?? string.Empty))
                };
            });
          

            //-----------------------Cors Policy------------------------

            builder.Services.AddCors(options =>
            {

                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(builder.Configuration.GetSection("Domains").Get<string[]>() ?? [])
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });

            });

            //var Google = builder.Configuration.GetSection("Authentication:Google");
            //builder.Services.AddAuthentication().AddGoogle(options =>
            //{
            //    options.ClientId = Google["ClientId"]!;
            //    options.ClientSecret = Google["ClientSecret"]!;
            //    options.CallbackPath = "/signin-google";
            //});

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            //builder.Services.AddOpenApi();


            //------------------- Swagger -------------------

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            #region Swagger Region


            //builder.Services.AddSwaggerGen(swagger =>
            //{
            //    //This is to generate the Default UI of Swagger Documentation    
            //    swagger.SwaggerDoc("v1", new OpenApiInfo
            //    {
            //        Version = "v1",
            //        Title = "ASP.NET 10 Web API",
            //        Description = "ColorMate Project"
            //    });
            //    // To Enable authorization using Swagger (JWT)    
            //    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            //    {
            //        Name = "Authorization",
            //        Type = SecuritySchemeType.ApiKey,
            //        Scheme = "Bearer",
            //        BearerFormat = "JWT",
            //        In = ParameterLocation.Header,
            //        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
            //    });
            //    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
            //    {
            //        {
            //        new OpenApiSecurityScheme
            //        {
            //        Reference = new OpenApiReference
            //        {
            //        Type = ReferenceType.SecurityScheme,
            //        Id = "Bearer"
            //        }
            //        },
            //        new string[] {}
            //        }
            //        });
            //});
            #endregion




            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
