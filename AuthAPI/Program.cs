using AspNetCore.Proxy;
using AuthAPI.DB.DBContext;
using AuthAPI.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddDbContext<AuthContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder
                          .AllowAnyOrigin()
                          .AllowAnyHeader();
                      });
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication With JWT",
        Type = SecuritySchemeType.Http,
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddProxies();
//Используем ICryptographyHelper
builder.Services.UseCryptographyHelper();
//Пока нет БД с юзерами, используем фейковый провайдер юзеров
//builder.Services.UseFakeUserProvider();
builder.Services.UseUserProvider();
//Используем генератор JWT IJwtService
builder.Services.UseJWTGenerator();
//Используем класс-валидатор для логина и пароля
builder.Services.UseUserCredentialsValidator();
builder.Services.AddAuthorization();

builder.Services.AddRazorPages(opts =>
{
    // we don't care about antiforgery in the demo
    opts.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
});

// Use the in-memory implementation of IDistributedCache.
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
        {
            // Set a short timeout for easy testing.
            options.IdleTimeout = TimeSpan.FromMinutes(2);
            options.Cookie.HttpOnly = true;
            // Strict SameSite mode is required because the default mode used
            // by ASP.NET Core 3 isn't understood by the Conformance Tool
            // and breaks conformance testing
            options.Cookie.SameSite = SameSiteMode.Unspecified;
        });

builder.Services.AddFido2(options =>
{
    options.ServerDomain = builder.Configuration["fido2:serverDomain"];
    options.ServerName = "FIDO2 Test";
    options.Origins = builder.Configuration.GetSection("fido2:origins").Get<HashSet<string>>();
    options.TimestampDriftTolerance = builder.Configuration.GetValue<int>("fido2:timestampDriftTolerance");
    options.MDSCacheDirPath = builder.Configuration["fido2:MDSCacheDirPath"];
});

#region CORS setup
builder.Services.AddCors(p => p.AddPolicy("loose-CORS", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));
#endregion

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);
app.UseSession();

app.UseAuthorization();
app.UseAuthentication();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Используем определённую ранее политику CORS
app.UseCors("loose-CORS");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
