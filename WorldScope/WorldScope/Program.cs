using Microsoft.EntityFrameworkCore;
using WorldScope.API.Data;
using WorldScope.API.Services;


var builder = WebApplication.CreateBuilder(args);

// إضافة Controllers
builder.Services.AddControllers();

// إعداد Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// الاتصال بقاعدة البيانات
builder.Services.AddDbContext<WorldScopeDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ربط CountryService بالواجهة ICountryService
builder.Services.AddHttpClient<ICountryService, CountryService>(client =>
{
    client.BaseAddress = new Uri(
        "https://restcountries.com/v3.1/"
    );
});

// السماح لـ Angular بالاتصال بالـ Backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// تشغيل Swagger في وضع التطوير
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AngularPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();