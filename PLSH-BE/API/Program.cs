using System.Text;
using API.Common;
using API.Configs;
using API.Middlewares;
using BU.Extensions;
using Common.Library;
using Data.DatabaseContext;
using Data.Repository.Implementation;
using Data.UnitOfWork;
using DotNetEnv;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
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
             .WriteTo.File("Logs/pl-book-hive-.log", rollingInterval: RollingInterval.Day) // Ghi log vào file
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
              // .WithOrigins(
              //   "https://www.book-hive.space",
              //   "http://104.197.134.164",
              //   "http://localhost:5281",
              //   "http://localhost:3000",
              //   "https://book-hive.space")
              .AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              // .AllowCredentials()
              );
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
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
//DI
builder.Services.AddHttpClient();
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
           OnAuthenticationFailed = context => { return Task.CompletedTask; },
           OnTokenValidated = context =>
           {
             Console.WriteLine("Token is valid!");
             return Task.CompletedTask;
           }
         };
       });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
builder.Services.AddHttpClient<HttpClientWrapper>(c => c.BaseAddress = new Uri("https://localhost:44353/"));
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
       .AddJsonOptions(options =>
       {
         options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
       });
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureEmailService();
var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseCors("AllowSpecificOrigins");
app.UseSwagger();
app.UseSwaggerUI();
// }

// app.UseRouting();
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
app.UseHttpsRedirection();
app.MapControllers();
app.Run();