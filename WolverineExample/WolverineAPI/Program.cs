using Oakton;
using Wolverine;
using Wolverine.Transports.Tcp;
using WolverineAPI.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Setup wolverine in the Host
builder.Host.UseWolverine(opt =>
    opt.PublishMessage<ItemCreated>().ToPort(5580)
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunOaktonCommands(args);
