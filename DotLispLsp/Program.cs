using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;

namespace DotLispLsp
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                .MinimumLevel.Verbose()
                .CreateLogger();

            Log.Logger.Information("This only goes file...");

            var server = await LanguageServer.From(options =>
                options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .ConfigureLogging(x => x
                        .AddSerilog()
                        .AddLanguageProtocolLogging()
                        .SetMinimumLevel(LogLevel.Information))
                    .WithHandler<TextDocumentSyncHandler>()
                    .WithHandler<CompletionHandler>()
                    // .WithHandler<DidChangeWatchedFilesHandler>()
                    // .WithHandler<FoldingRangeHandler>()
                    // .WithHandler<SemanticTokens>()
                    .WithServices(x =>
                        x.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace)))
                    .WithServices(services =>
                    {
                        services.AddSingleton(provider =>
                        {
                            var loggerFactory =
                                provider.GetService<ILoggerFactory>();
                            var logger = loggerFactory.CreateLogger<Foo>();

                            logger.LogInformation("Configuring");

                            return new Foo(logger);
                        });
                        services.AddSingleton<BufferManager>();
                    })
                    // .OnDidChangeTextDocument(async (param, capability, cancellationToken) =>
                    //     {
                    //         Log.Logger.Information("My own did change message");
                    //     },
                    //     new TextDocumentChangeRegistrationOptions()
                    //     {
                    //         DocumentSelector =
                    //             DocumentSelector.ForLanguage("dotlisp"),
                    //         SyncKind = TextDocumentSyncKind.Full
                    //     })
            );

            await server.WaitForExit;
        }
    }

    internal class Foo
    {
        private readonly ILogger<Foo> _logger;

        public Foo(ILogger<Foo> logger)
        {
            _logger = logger;
        }

        public void Log(string message)
        {
            _logger.LogInformation(message);
        }
    }
}