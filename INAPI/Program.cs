using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Services;
using Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Подключаем DbContext с конфигурацией строки подключения из appsettings.json
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Регистрируем сервисы
builder.Services.AddScoped<IAiService>(provider => new AiService(  provider.GetRequiredService<IConfiguration>()));
builder.Services.AddScoped<IUserService>(provider => 
    new UserService(
        provider.GetRequiredService<ApplicationContext>(),
        provider.GetRequiredService<IConfiguration>()
    ));
builder.Services.AddScoped<INoteService>(provider => 
    new NoteService(
        provider.GetRequiredService<ApplicationContext>(),
        provider.GetRequiredService<IAiService>(),
        provider.GetRequiredService<IUserService>()
    ));



// Настройка JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
// Добавляем контроллеры
builder.Services.AddControllers();

// Добавляем поддержку Swagger для API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notes API", Version = "v1" });

    // Добавляем JWT аутентификацию в Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Введите JWT токен (без 'Bearer ' в начале)",
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
            new string[] { }
        }
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin() 
                  .AllowAnyMethod() 
                  .AllowAnyHeader(); 
        });
});

var app = builder.Build();

// Включаем Swagger только в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notes API v1");
        c.RoutePrefix = string.Empty; // Открывать Swagger по умолчанию
    });
}

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();
// Подключаем контроллеры к маршрутам
app.MapControllers();

// Настройка слушания на всех интерфейсах и порту 5000
app.Urls.Add("http://0.0.0.0:5000");
// Подключаем статические файлы из папки wwwroot/files
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files")),
    RequestPath = "/files"  // URL путь, по которому будут доступны файлы
});

// Запуск приложения
app.Run();