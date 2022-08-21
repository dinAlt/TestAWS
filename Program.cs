using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Amazon.Extensions.NETCore;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TestAWS.Models;
using TestAWS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidateActor = false,
        ValidateAudience = false,
        ValidateIssuer = false,
        // ValidIssuer = "https://localhost:7039/",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is a strong security key")),
      };
    });
builder.Services.AddDbContext<MultipartUploadsContext>(opt => opt.UseInMemoryDatabase("TodoList"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MultipartUploadsStore>();
builder.Services.AddScoped<S3UploadService>();
builder.Services.AddAWSService<IAmazonS3>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapPost("/security/createToken", () =>
{
  var key = Encoding.UTF8.GetBytes("this is a strong security key");
  var tokenDescriptor = new SecurityTokenDescriptor
  {
    Subject = new ClaimsIdentity(new[] {
            new Claim("Id", Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        }),
    Expires = DateTime.UtcNow.AddMinutes(10),
    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
  };
  var tokenHandler = new JwtSecurityTokenHandler();
  var token = tokenHandler.CreateToken(tokenDescriptor);
  var stringToken = tokenHandler.WriteToken(token);
  return Results.Json(new { token = stringToken },
    statusCode: StatusCodes.Status200OK);
});

app.Run();
