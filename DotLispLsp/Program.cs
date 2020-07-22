using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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

            IObserver<WorkDoneProgressReport> workDone = null;

            var server = await LanguageServer.From(options =>
                options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .ConfigureLogging(x => x
                        .AddSerilog()
                        .AddLanguageProtocolLogging()
                        .SetMinimumLevel(LogLevel.Debug))
                    .WithHandler<TextDocumentHandler>()
                    .WithHandler<DidChangeWatchedFilesHandler>()
                    .WithHandler<FoldingRangeHandler>()
                    .WithHandler<MyWorkspaceSymbolsHandler>()
                    .WithHandler<MyDocumentSymbolHandler>()
                    .WithHandler<SemanticTokens>()
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
                        services.AddSingleton(new ConfigurationItem()
                        {
                            Section = "typescript",
                        }).AddSingleton(new ConfigurationItem()
                        {
                            Section = "terminal",
                        });
                    })
                    .OnInitialize(async (server, request, token) =>
                    {
                        server.Services.GetRequiredService<Foo>()
                            .Log(request.ToString());

                        await new Task<InitializeResult>(() => new InitializeResult
                        {
                            Capabilities = new ServerCapabilities(),
                            ServerInfo = new ServerInfo()
                        });
                    })
                    .OnInitialized(async (server, request, response, token) =>
                    {
                    })
                    .OnStarted(async (languageServer, result, token) =>
                    {
                        // using var manager =
                        //     await languageServer.WorkDoneManager.Create(
                        //         new WorkDoneProgressBegin()
                        //         { Title = "Doing some work..." });
                        //
                        //
                        // var logger =
                        //     languageServer.Services.GetService<ILogger<Foo>>();
                        // var configuration =
                        //     await languageServer.Configuration.GetConfiguration(
                        //         new ConfigurationItem()
                        //         {
                        //             Section = "typescript",
                        //         }, new ConfigurationItem()
                        //         {
                        //             Section = "terminal",
                        //         });
                        //
                        // var baseConfig = new JObject();
                        // foreach (var config in languageServer.Configuration
                        //     .AsEnumerable())
                        // {
                        //     baseConfig.Add(config.Key, config.Value);
                        // }
                        //
                        // logger.LogInformation("Base Config: {Config}", baseConfig);
                        //
                        // var scopedConfig = new JObject();
                        // foreach (var config in configuration.AsEnumerable())
                        // {
                        //     scopedConfig.Add(config.Key, config.Value);
                        // }
                        //
                        // logger.LogInformation("Scoped Config: {Config}",
                        //     scopedConfig);
                    })
            );

            // await server.WaitForExit;
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