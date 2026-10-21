using SanjeshP.Payeh.WebFramework.Configuration;
using SanjeshP.Payeh.WebFramework.Middlewares;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// اضافه کردن سرویس‌ها از WebFramework
builder.Services.AddMinimalServices(builder.Configuration);
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // استفاده از Scalar برای مستندسازی در .NET 10 به دلیل سادگی، سرعت و سازگاری کامل با OpenAPI داخلی
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();