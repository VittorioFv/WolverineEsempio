using JasperFx.Core;
using Microsoft.EntityFrameworkCore;
using Oakton;
using Oakton.Resources;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.SqlServer;
using Wolverine.Transports.Tcp;
using WolverineAPI.Data;
using WolverineAPI.Messages;



var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("sqlserver")!;

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseWolverine(opt =>
{
    opt.Policies.AllLocalQueues(x => x.UseDurableInbox());

    /*opt.PublishMessage<ItemCreated>()
        .ToPort(5580)
        .UseDurableOutbox();*/
    
    opt.UseEntityFrameworkCoreTransactions();
    opt.PersistMessagesWithSqlServer(connectionString);
});

builder.Services.AddDbContextWithWolverineIntegration<ItemDbContext>(x =>
{
    x.UseSqlServer(connectionString);
});

builder.Host.UseResourceSetupOnStartup();

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
