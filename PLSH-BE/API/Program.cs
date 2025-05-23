using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using API.Common;
using API.Configs;
using API.Middlewares;
using API.Provider;
using BU.Extensions;
using BU.Hubs;
using BU.Services.Implementation;
using BU.Services.Interface;
using Common.Library;
using Data.DatabaseContext;
using Data.Repository.Implementation;
using Data.Repository.Interfaces;
using Data.UnitOfWork;
using DotNetEnv;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
Env.Load();
var environment = builder.Environment.EnvironmentName;
var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? "";
var googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? "";
var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "";
var secretKey = Environment.GetEnvironmentVariable(Constants.JWT_SECRET) ?? "";
// var googleCloudConfig = builder.Configuration.GetSection("GoogleCloud");
// var credentialsPath = googleCloudConfig["CredentialsPath"];
Log.Logger = new LoggerConfiguration()
             .WriteTo.Console() // Ghi log ra console
             .WriteTo.File("Logs/pl-Book-hive-.log", rollingInterval: RollingInterval.Day) // Ghi log vào file
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
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowSpecificOrigins",
    policy => policy
              .WithOrigins("https://www.book-hive.space",
                "https://librarian.book-hive.space",
                "http://localhost:5281",
                "http://localhost:3000",
                "http://localhost:3001",
                "https://book-hive.space")
              // .AllowAnyOrigin()
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
// builder.Services.AddIdentity<AccountControllers, Role>()
//        .AddEntityFrameworkStores<AppDbContext>()
//        .AddDefaultTokenProviders();
//DI
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IUserIdProvider, AppUserIdProvider>();
builder.Services.AddSingleton(StorageClient.Create());
builder.Services.AddSingleton<GoogleCloudStorageHelper>();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddBusinessLayer();
builder.Services.AddLockBusinessLayer();

//
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
         options.TokenValidationParameters = new TokenValidationParameters
         {
           ValidateIssuer = true,
           ValidateAudience = true,
           ValidateLifetime = true,
           ValidateIssuerSigningKey = true,
           ValidIssuer = Constants.Issuer,
           ValidAudience = Constants.Audience,
           IssuerSigningKey =
             new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
         };
         options.Events = new JwtBearerEvents
         {
           OnMessageReceived = context =>
           {
             var accessToken = context.Request.Query["access_token"];
             var path = context.HttpContext.Request.Path;
             if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/bookHiveHub"))
             {
               context.Token = accessToken;
             }

             return Task.CompletedTask;
           }
         };
       });
builder.Services.AddAuthorizationBuilder()
       .AddPolicy("AdminPolicy", policy => policy.RequireRole("admin"))
       .AddPolicy("BorrowerPolicy", policy => policy.RequireRole("student", "teacher"))
       .AddPolicy("LibrarianPolicy", policy => policy.RequireRole("librarian", "admin"))
       .AddPolicy("NotVerifiedPolicy", policy => policy.RequireRole("notVerifiedUser"));
builder.Services.AddSignalR();
builder.Services.AddHttpClient<HttpClientWrapper>(c => c.BaseAddress = new Uri("https://localhost:44353/"));
builder.Services.AddHostedService<ReturnDateNotificationService>();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
       .AddJsonOptions(options =>
       {
         options.JsonSerializerOptions.ReferenceHandler =
           System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
       });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient("VietQR", client =>
{
    client.BaseAddress = new Uri("https://api.vietqr.io/v2/");
    client.DefaultRequestHeaders.Add("x-client-id", builder.Configuration["VietQR:ClientId"]);
    client.DefaultRequestHeaders.Add("x-api-key", builder.Configuration["VietQR:ApiKey"]);
    client.Timeout = TimeSpan.FromSeconds(300);
});
builder.Services.ConfigureEmailService();

builder.Services.AddScoped<FineAutoCreationService>();
builder.Services.AddScoped<IOTPService, OTPService>();

var app = builder.Build();
app.UseCors("AllowSpecificOrigins");
app.UseSwagger();
app.UseSwaggerUI();
app.Use(async (context, next) =>
{
  if (context.Request.Path == "/")
  {
    context.Response.Redirect("/swagger");
    return;
  }

  await next();
});
// app.UseCorsMiddleware();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
  var path = context.Request.Path;
  if (path.StartsWithSegments("/api/v1/account/b-change-password")
      || path.StartsWithSegments("/api/v1/account/b-login/google")
      || path.StartsWithSegments("/api/v1/account/b-login")
      || path.StartsWithSegments("/api/v1/account/is-verified")
      || path.StartsWithSegments("/api/v1/account/login")
      || path.StartsWithSegments("/api/v1/account/login/google")
     )
  {
    await next();
    return;
  }

  if (context.User.IsInRole("notVerifiedUser"))
  {
    context.Response.StatusCode = 403;
    context.Response.ContentType = "application/json";
    var response = new
    {
      status = "notChangePasswordYet",
      message = "Tài khoản của bạn chưa được đổi mật khẩu. Vui lòng đổi mật khẩu để tiếp tục."
    };
    await context.Response.WriteAsJsonAsync(response);
    return;
  }

  await next();
});
app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<ReviewHub>("/bookHiveHub");
app.Run();
