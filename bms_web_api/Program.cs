using bms_web_api.Data;
using bms_web_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MyDBContext>
    (options => options.UseSqlServer(builder.Configuration.GetConnectionString("MyDB")));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Config lại example của Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My BMS_WEB_API", Version = "v1" });
    c.MapType<OrderItemCreateModel>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
           { "book_id", new OpenApiSchema { Type = "integer", Format = "int64", Nullable = true } },
            { "quantity", new OpenApiSchema { Type = "integer", Format = "int32", Example = new OpenApiInteger(1) } } // Cho mặc định không nhập là 1
        }
    });
});

// Sửa lỗi Json limited 32
builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
// Verify Email
builder.Services.Configure<IdentityOptions>(
    options => options.SignIn.RequireConfirmedEmail = true);
// Map JWT trong appsetting với UserController
builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));
// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
    };
});
//Authorization
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminRole", policy =>
//       policy.RequireRole("Quản trị viên"));
//    options.AddPolicy("SalesRole", policy =>
//       policy.RequireRole("Nhân viên bán hàng"));
//    options.AddPolicy("StockRole", policy =>
//       policy.RequireRole("Quản lý kho"));
//    options.AddPolicy("ShipRole", policy =>
//       policy.RequireRole("Vận chuyển"));
//});
//Add CORS để truy cập API từ web client
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigin",
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});
var app = builder.Build();
app.UseCors("AllowSpecificOrigin");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
