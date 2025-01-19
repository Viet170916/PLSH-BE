using System;
using System.Text;
using API.Middlewares;
using BU.Extensions;
using BU.Mappings;
using Common.Library;
using Data.DatabaseContext;
using Data.Repository.Implementation;
using Data.UnitOfWork;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
Env.Load();
var environment = builder.Environment.EnvironmentName;
var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? "";
var googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? "";
var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "";
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "";
Log.Logger = new LoggerConfiguration()
             .WriteTo.Console() // Ghi log ra console
             .WriteTo.File("Logs/pl-book-hive.log", rollingInterval: RollingInterval.Day) // Ghi log vào file
             .CreateLogger();
builder.Host.UseSerilog();

// builder.Services.AddAuthentication(options =>
//        {
//          options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//          options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
//        })
//        .AddCookie()
//        .AddGoogle(options =>
//        {
//          options.ClientId = googleClientId;
//          options.ClientSecret = googleClientSecret!;
//          options.CallbackPath = "/signin-google";
//        });
builder.Services.AddAuthentication(options =>
       {
         options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
         options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
       })
       .AddJwtBearer(options =>
       {
         options.TokenValidationParameters = new TokenValidationParameters
         {
           ValidateIssuer = true,
           ValidateAudience = true,
           ValidateLifetime = true,
           ValidateIssuerSigningKey = true,
           ValidIssuer = "your_issuer",
           ValidAudience = "your_audience",
           IssuerSigningKey =
             new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
         };
       });
builder.Services.AddAuthorization(options =>
{
  options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
});

// Add builder.Services to the container.
// builder.Services.Configure<IISOptions>(options => { options.AutomaticAuthentication = true; });
// builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);
// builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromHours(Constants.StartUp.TimeSpanHours); });
// builder.Services.AddMvc(config =>
// {
//   var policy = new AuthorizationPolicyBuilder()
//                .RequireAuthenticatedUser()
//                .Build();
//   config.Filters.Add(new AuthorizeFilter(policy));
// });
// builder.Services.Configure<IISServerOptions>(options => { options.MaxRequestBodySize = int.MaxValue; });
// builder.Services.Configure<FormOptions>(options =>
// {
//   options.ValueLengthLimit = int.MaxValue;
//   options.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
//   options.MultipartHeadersLengthLimit = int.MaxValue;
// });
// builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
// builder.Services.AddMemoryCache();
// builder.Services.AddDistributedMemoryCache();
// builder.Services.ConfigureApplicationCookie(options =>
// {
//   options.ExpireTimeSpan = TimeSpan.FromDays(Constants.StartUp.TimeSpanDays);
//   options.SlidingExpiration = true;
// });
builder.Services.AddCors(option =>
{
  option.AddPolicy(Constants.CorsPolicy,
    policyBuilder => policyBuilder.SetIsOriginAllowed(_ => true)
                                  .AllowAnyMethod()
                                  .AllowAnyHeader()
                                  .AllowCredentials());
});
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseMySql(dbConnectionString,
    ServerVersion.AutoDetect(dbConnectionString)));
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseMySql(dbConnectionString,
      ServerVersion.AutoDetect(dbConnectionString)),
  ServiceLifetime.Scoped);
// builder.Services.AddIdentity<Account, Role>()
//        .AddEntityFrameworkStores<AppDbContext>()
//        .AddDefaultTokenProviders();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
//DI
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddBusinessLayer();
builder.Services.AddLockBusinessLayer();

//
builder.Services.AddHttpClient<HttpClientWrapper>(c => c.BaseAddress = new Uri("https://localhost:44353/"));
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseCorsMiddleware();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers()
   .WithMetadata(new RouteAttribute("/api/v1/[controller]"));
app.Run();