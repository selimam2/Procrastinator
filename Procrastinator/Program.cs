using Microsoft.EntityFrameworkCore;
using Procrastinator;
using Procrastinator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Entity Framework
builder.Services.AddDbContext<ProcrastinatorContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Message Service (currently Twilio, but easily extensible)
builder.Services.AddScoped<IMessageService, TwilioService>();

// Add App Configuration
builder.Services.AddSingleton<IAppConfiguration, AppConfiguration>();

// Add Background Service for dispatching reminders
builder.Services.AddHostedService<ReminderDispatchService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swagger/opena
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
