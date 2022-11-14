using AspNetCore.Proxy;
using AuthAPI.DB.DBContext;
using AuthAPI.Extensions;
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

#region CORS setup
builder.Services.AddCors(p => p.AddPolicy("loose-CORS", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));
#endregion

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

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
