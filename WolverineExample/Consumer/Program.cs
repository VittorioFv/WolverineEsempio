using Microsoft.Extensions.Hosting;
using Oakton;
using Wolverine;
using Wolverine.Transports.Tcp;

var builder = Host.CreateDefaultBuilder(args)
    .UseWolverine(opts =>
    {
        // listen to incoming messages at port 5580
        opts.ListenAtPort(5580);
    });

await builder.RunOaktonCommands(args);
