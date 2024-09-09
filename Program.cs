
using APICatalogo.DTOs.Mappings;
using APICatalogo.Extensions;
using APICatalogo.Filters;
using APICatalogo.Models;
using APICatalogo.Repositorio;
using APICatalogo.Repositorio;
using APICatalogo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
  options.Filters.Add(typeof(ApiExceptionFilter));
})
  .AddJsonOptions(options =>
  {
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
  });
builder.Services.AddAuthorization();
//builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddRateLimiter(options =>
{
  options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

  options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpcontext =>
                          RateLimitPartition.GetFixedWindowLimiter(
                                             partitionKey: httpcontext.User.Identity?.Name ??
                                                           httpcontext.Request.Headers.Host.ToString(),
                          factory: partition => new FixedWindowRateLimiterOptions
                          {
                            AutoReplenishment = true,
                            PermitLimit = 2,
                            QueueLimit = 0,
                            Window = TimeSpan.FromSeconds(10)
                          }));
});

builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "apicatalogo", Version = "v1" });

  c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
  {
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Bearer JWT ",
  });
  c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                          {
                              Reference = new OpenApiReference
                              {
                                  Type = ReferenceType.SecurityScheme,
                                  Id = "Bearer"
                              }
                          },
                         new string[] {}
                    }
                });
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().
  AddEntityFrameworkStores<CatalogoDbEfPowerContext>().
  AddDefaultTokenProviders();

var secretKey = builder.Configuration["JWT:SecretKey"]
                   ?? throw new ArgumentException("Invalid secret key!!");

builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
  options.SaveToken = true;
  options.RequireHttpsMetadata = false;
  options.TokenValidationParameters = new TokenValidationParameters()
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ClockSkew = TimeSpan.Zero,
    ValidAudience = builder.Configuration["JWT:ValidAudience"],
    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
    IssuerSigningKey = new SymmetricSecurityKey(
                         Encoding.UTF8.GetBytes(secretKey))
  };
});


builder.Services.AddAuthorization(options =>
{
  options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

  options.AddPolicy("SuperAdminOnly", policy =>
                     policy.RequireRole("Admin").RequireClaim("id", "etienne.lima"));

  options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));

  options.AddPolicy("ExclusiveOnly", policy =>
                    policy.RequireAssertion(context =>
                    context.User.HasClaim(claim =>
                                         claim.Type == "id" && claim.Value == "etienne.lima")
                                         || context.User.IsInRole("SuperAdmin")));
});

//string SqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.Services.AddDbContext<CatalogoDbEfPowerContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<CatalogoDbEfPowerContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("LocalEfPower")));

builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IUnitOfwork, UnitOfwork>();
builder.Services.AddScoped<ITokenService, TokenService>();


builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddAutoMapper(typeof(ProdutoDTOMappingProfile));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();
