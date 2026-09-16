using Microsoft.EntityFrameworkCore;
using MesLite.Data;

var builder = WebApplication.CreateBuilder(args);

// EF Core + MSSQL
builder.Services.AddDbContext<MesLiteContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers + JSON（避免循環參照序列化爆炸）
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS：開放給 Vue3 開發伺服器 (vite 預設 5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 啟動時自動套用 migration，資料庫不存在會自動建立，不用手動下 dotnet ef database update
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MesLiteContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowVueDev");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
