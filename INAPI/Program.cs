using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Services;
using Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
});

// Загрузка .env (если нужно)
builder.Configuration.AddEnvironmentVariables();

// Настройка базы данных
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")));

// Сервисы
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<IUserService>(provider =>
    new UserService(provider.GetRequiredService<ApplicationContext>()));
builder.Services.AddScoped<INoteService>(provider =>
    new NoteService(
        provider.GetRequiredService<ApplicationContext>(),
        provider.GetRequiredService<IAiService>(),
        provider.GetRequiredService<IUserService>()));

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")))
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Контроллеры и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notes API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Введите JWT токен",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notes API v1");
    c.RoutePrefix = string.Empty;
});

// ВАЖНО: CORS до Routing
app.UseCors("AllowAll");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Слушать на всех интерфейсах и порту 8080 (для Docker)
app.Urls.Add("http://0.0.0.0:5000");

// Если хранишь файлы вне wwwroot — явно укажи путь к ним
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider("/root/inapi/files"),  // или свой путь к директории
    RequestPath = "/files"
});

app.Run();
