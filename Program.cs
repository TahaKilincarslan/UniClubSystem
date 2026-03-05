using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;
using UniversityClubSystem.McpTools;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// 1. SERVİS KAYITLARI
// =============================================

// PostgreSQL veritabanı bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MCP Ajanı – AppDbContext'e bağımlı, Scoped olarak kaydediliyor
builder.Services.AddScoped<ClubAgent>();

// Controller'lar
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // JSON serileştirmede enum'ları string olarak döndür
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

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
}

// =============================================
// 4. HTTP PIPELINE YAPILANDIRMASI
// =============================================

// CORS middleware'ini en başta etkinleştir
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
app.MapControllers();

app.Run();
