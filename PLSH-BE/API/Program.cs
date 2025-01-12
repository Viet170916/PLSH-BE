using System;
using API.Common;
using API.Configs;
using API.Middlewares;
using BU.Mappings;
using Common.Library;
using Data.DatabaseContext;
using Data.Repository.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Model.Entity.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add builder.Services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<IISOptions>(options => { options.AutomaticAuthentication = true; });
builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);
builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromHours(Constants.StartUp.TimeSpanHours); });
builder.Services.AddMvc(config =>
{
  var policy = new AuthorizationPolicyBuilder()
               .RequireAuthenticatedUser()
               .Build();
  config.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.Configure<IISServerOptions>(options => { options.MaxRequestBodySize = int.MaxValue; });
builder.Services.Configure<FormOptions>(options =>
{
  options.ValueLengthLimit = int.MaxValue;
  options.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
  options.MultipartHeadersLengthLimit = int.MaxValue;
});
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.ConfigureApplicationCookie(options =>
{
  options.ExpireTimeSpan = TimeSpan.FromDays(Constants.StartUp.TimeSpanDays);
  options.SlidingExpiration = true;
});
builder.Services.AddCors(option =>
{
  option.AddPolicy(Constants.CorsPolicy,
    buidler => buidler.SetIsOriginAllowed(_ => true)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials());
});
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString(Constants.ConnStr)));
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(Constants.ConnStr)),
  ServiceLifetime.Scoped);
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
       .AddEntityFrameworkStores<AppDbContext>()
       .AddDefaultTokenProviders();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<HttpClientWrapper>(c => c.BaseAddress = new Uri("https://localhost:44353/"));
builder.Services.AddSwaggerGen(swagger =>
{
  swagger.SwaggerDoc(Constants.ApiVersion,
    new OpenApiInfo { Title = Constants.ApiTitle, Version = Constants.ApiVersion });
  swagger.OperationFilter<SwaggerFileOperationFilter>();
});
// builder.Services.AddSwaggerGenNewtonsoftSupport();
builder.Services.AddMvc()
       .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new DateTimeConverter()); });
DiConfiguration.Initialize(builder.Services);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddBusinessLayer();
// builder.Services.AddLockBusinessLayer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseCorsMiddleware();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();