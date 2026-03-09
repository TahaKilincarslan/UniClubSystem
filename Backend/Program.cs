using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;
using UniversityClubSystem.McpTools;
using UniversityClubSystem.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UniversityClubSystem.Services;
using FluentValidation.AspNetCore;
using FluentValidation;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// 1. SERVİS KAYITLARI
// =============================================

// PostgreSQL veritabanı bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MCP Ajanı – AppDbContext'e bağımlı, Scoped olarak kaydediliyor
builder.Services.AddScoped<ClubAgent>();

// Token Servisi
builder.Services.AddScoped<ITokenService, TokenService>();

// AutoMapper Kaydı
builder.Services.AddAutoMapper(typeof(Program));

// JWT Kimlik Doğrulama (Authentication)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
    });

// Controller'lar
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // JSON serileştirmede enum'ları string olarak döndür
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// FluentValidation Kaydı
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Swagger / OpenAPI yapılandırması
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "University Club System API",
        Version = "v1",
        Description = "Üniversite kulüp yönetim sistemi backend API'si"
    });

    // XML yorumlarını Swagger'a dahil etme
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Swagger'a JWT desteği ekleme
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// =============================================
// 2. CORS YAPILANDIRMASI
// Frontend'in API'ye istek atabilmesi için çok önemli!
// =============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()    // Tüm origin'lere izin ver
              .AllowAnyMethod()    // GET, POST, PUT, DELETE vs.
              .AllowAnyHeader();   // Tüm header'lara izin ver
    });
});

var app = builder.Build();

// =============================================
// 3. VERİTABANI OTOMATİK MIGRATION
// Uygulama başlarken veritabanını ve tabloları oluşturur
// =============================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    
    // Seed data if empty
    if (!db.Universities.Any())
    {
        var ostim = new University { Name = "Ostim Technical", City = "Ankara", LogoUrl = "images/universities/ostimteknik.png" };
        var itu = new University { Name = "Istanbul Technical", City = "Istanbul", LogoUrl = "images/universities/itu.jpg" };
        var odtu = new University { Name = "Middle East Technical", City = "Ankara", LogoUrl = "images/universities/odtu.png" };
        
        db.Universities.AddRange(ostim, itu, odtu);
        db.SaveChanges();

        var admin = new User 
        { 
            FirstName = "Admin", 
            LastName = "User", 
            Email = "admin@example.com", 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), 
            Role = UserRole.SystemAdmin 
        };
        db.Users.Add(admin);
        db.SaveChanges();

        db.Clubs.AddRange(
            new Club { Name = "Robotics Club", Category = "Technology", UniversityId = ostim.Id, ManagerId = admin.Id, Description = "Building robots.", ImageUrl = "images/clubs/robotics.jpg" },
            new Club { Name = "Entrepreneurship Club", Category = "Business", UniversityId = ostim.Id, ManagerId = admin.Id, Description = "Startups and more.", ImageUrl = "images/clubs/entrepreneurship.jpg" },
            new Club { Name = "AI Club", Category = "Technology", UniversityId = itu.Id, ManagerId = admin.Id, Description = "Intelligence in software.", ImageUrl = "images/clubs/ai.jpg" }
        );
        db.SaveChanges();
    }
}
// Global Hata Yönetimi - En üstte olmalı!
app.UseMiddleware<UniversityClubSystem.Middlewares.ExceptionMiddleware>();

// CORS middleware'ini etkinleştir
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "University Club System API v1");
        options.DocumentTitle = "UniClub API - Swagger";
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles(); // Resimlerin (uploads) dışarıdan erişilebilir olması için

app.UseAuthentication(); // Önce Login kontrolü
app.UseAuthorization();  // Sonra Yetki kontrolü

app.MapControllers();

app.Run();
