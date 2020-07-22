using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotLisp.Parsing;
using DotLisp.Types;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Progress;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.WorkDone;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace DotLispLsp
{
    class TextDocumentHandler : ITextDocumentSyncHandler
    {
        private readonly ILogger<TextDocumentHandler> _logger;
        private readonly ILanguageServerConfiguration _configuration;

        private readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.dl"
            }
        );

        private SynchronizationCapability _capability;

        public TextDocumentHandler(ILogger<TextDocumentHandler> logger, Foo foo,
            ILanguageServerConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full;

        public Task<Unit> Handle(DidChangeTextDocumentParams notification,
            CancellationToken token)
        {
            // _logger.LogCritical("Critical");
            // _logger.LogDebug("Debug");
            // _logger.LogTrace("Trace");
            // _logger.LogInformation("Hello world!");
            _logger.LogDebug($"SOMETHING DID CHANGE!!!! \n" +
                             $"{notification.ContentChanges.Select(cc => $"{cc.Text}")}");
            return Unit.Task;
        }

        TextDocumentChangeRegistrationOptions
            IRegistration<TextDocumentChangeRegistrationOptions>.
            GetRegistrationOptions()
        {
            return new TextDocumentChangeRegistrationOptions()
            {
                DocumentSelector = _documentSelector,
                SyncKind = Change
            };
        }

        public void SetCapability(SynchronizationCapability capability)
        {
            _capability = capability;
        }

        public async Task<Unit> Handle(DidOpenTextDocumentParams notification,
            CancellationToken token)
        {
            await Task.Yield();
            _logger.LogInformation("Hello world!");
            await _configuration.GetScopedConfiguration(
                notification.TextDocument.Uri);
            return Unit.Value;
        }

        TextDocumentRegistrationOptions
            IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions()
            {
                DocumentSelector = _documentSelector,
            };
        }

        public Task<Unit> Handle(DidCloseTextDocumentParams notification,
            CancellationToken token)
        {
            if (_configuration.TryGetScopedConfiguration(
                notification.TextDocument.Uri, out var disposable))
            {
                disposable.Dispose();
            }

            return Unit.Task;
        }

        public Task<Unit> Handle(DidSaveTextDocumentParams notification,
            CancellationToken token)
        {
            return Unit.Task;
        }

        TextDocumentSaveRegistrationOptions
            IRegistration<TextDocumentSaveRegistrationOptions>.
            GetRegistrationOptions()
        {
            return new TextDocumentSaveRegistrationOptions()
            {
                DocumentSelector = _documentSelector,
                IncludeText = true
            };
        }

        public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
        {
            return new TextDocumentAttributes(uri, "dotlisp");
        }
    }

    class MyDocumentSymbolHandler : DocumentSymbolHandler
    {
        public MyDocumentSymbolHandler() : base(
            new DocumentSymbolRegistrationOptions()
            {
                DocumentSelector = DocumentSelector.ForLanguage("dotlisp")
            })
        {
        }

        private List<SymbolInformationOrDocumentSymbol> ExtractSymbols(
            List<SymbolInformationOrDocumentSymbol> container, DotExpression tree)
        {
            if (!(tree is DotList l))
            {
                return container;
            }

            foreach (var exp in l.Expressions)
            {
                if (exp is DotSymbol s)
                {
                    var range = new Range(new Position(s.Line, s.Column),
                        new Position(s.Line,
                            s.Column + s.Name.Length));
                    container.Add(new DocumentSymbol()
                    {
                        Detail = "detail?",
                        Deprecated = false,
                        Kind = SymbolKind.Variable,
                        Range = range,
                        SelectionRange = range,
                        Name = s.Name
                    });
                }

                if (exp is DotList innerList)
                {
                    container.AddRange(ExtractSymbols(container, innerList));
                }
            }

            return container;
        }

        public override async Task<SymbolInformationOrDocumentSymbolContainer>
            Handle(DocumentSymbolParams request,
                CancellationToken cancellationToken)
        {
            // you would normally get this from a common source that is managed by current open editor, current active editor, etc.
            var content =
                await File.ReadAllTextAsync(DocumentUri.GetFileSystemPath(request),
                    cancellationToken);

            var inPort = new InPort();
            var parsedExpressions = Expander.Expand(inPort.Read(content));

            var symbols = new List<SymbolInformationOrDocumentSymbol>();

            symbols = ExtractSymbols(symbols, parsedExpressions);

            return symbols;
        }
    }

    class MyWorkspaceSymbolsHandler : WorkspaceSymbolsHandler
    {
        private readonly IServerWorkDoneManager _manager;
        private readonly IServerWorkDoneManager _serverWorkDoneManager;
        private readonly IProgressManager _progressManager;
        private readonly ILogger<MyWorkspaceSymbolsHandler> logger;

        public MyWorkspaceSymbolsHandler(
            IServerWorkDoneManager serverWorkDoneManager,
            IProgressManager progressManager,
            ILogger<MyWorkspaceSymbolsHandler> logger) :
            base(new WorkspaceSymbolRegistrationOptions() { })
        {
            _serverWorkDoneManager = serverWorkDoneManager;
            _progressManager = progressManager;
            this.logger = logger;
        }

        public override async Task<Container<SymbolInformation>> Handle(
            WorkspaceSymbolParams request,
            CancellationToken cancellationToken)
        {
            using var reporter = _serverWorkDoneManager.For(request,
                new WorkDoneProgressBegin()
                {
                    Cancellable = true,
                    Message = "This might take a while...",
                    Title = "Some long task....",
                    Percentage = 0
                });
            using var partialResults =
                _progressManager.For(request, cancellationToken);
            if (partialResults != null)
            {
                await Task.Delay(2000, cancellationToken);

                reporter.OnNext(new WorkDoneProgressReport()
                {
                    Cancellable = true,
                    Percentage = 20
                });
                await Task.Delay(500, cancellationToken);

                reporter.OnNext(new WorkDoneProgressReport()
                {
                    Cancellable = true,
                    Percentage = 40
                });
                await Task.Delay(500, cancellationToken);

                reporter.OnNext(new WorkDoneProgressReport()
                {
                    Cancellable = true,
                    Percentage = 50
                });
                await Task.Delay(500, cancellationToken);

                partialResults.OnNext(new[]
                {
                    new SymbolInformation()
                    {
                        ContainerName = "Partial Container",
                        Deprecated = true,
                        Kind = SymbolKind.Constant,
                        Location = new Location()
                        {
                            Range =
                                new OmniSharp.Extensions.LanguageServer.Protocol.
                                    Models.Range(new Position(2, 1),
                                        new Position(2, 10)) { }
                        },
                        Name = "Partial name"
                    }
                });

                reporter.OnNext(new WorkDoneProgressReport()
                {
                    Cancellable = true,
                    Percentage = 70
                });
                await Task.Delay(500, cancellationToken);

                reporter.OnNext(new WorkDoneProgressReport()
                {
                    Cancellable = true,
                    Percentage = 90
                });

                partialResults.OnCompleted();
                return new SymbolInformation[] { };
            }

            try
            {
                return new[]
                {
                    new SymbolInformation()
                    {
                        ContainerName = "Container",
                        Deprecated = true,
                        Kind = SymbolKind.Constant,
                        Location = new Location()
                        {
                            Range =
                                new OmniSharp.Extensions.LanguageServer.Protocol.
                                    Models.Range(new Position(1, 1),
                                        new Position(1, 10)) { }
                        },
                        Name = "name"
                    }
                };
            }
            finally
            {
                reporter.OnNext(new WorkDoneProgressReport()
                {
                    Cancellable = true,
                    Percentage = 100
                });
            }
        }
    }
}