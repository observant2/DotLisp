using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotLisp;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;

namespace DotLispLsp
{
    public class DiagnosticsHandler : PublishDiagnosticsHandler
    {
        private readonly ILanguageServer _languageServer;
        private readonly ILogger _logger;
        private readonly BufferManager _bufferManager;

        public DiagnosticsHandler(ILanguageServer languageServer,
            ILogger<PublishDiagnosticsParams> logger, BufferManager bufferManager)
        {
            _languageServer = languageServer;
            _logger = logger;
            _bufferManager = bufferManager;
            _logger.LogCritical(
                "-----------------------DiagnosticsHandler initialized!");
        }

        public override Task<Unit> Handle(PublishDiagnosticsParams request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                $"------------------------ {request.PrettyPrint()}");

            _languageServer.Window.LogError(
                $"--------------DIAGNOSTICS! {request.PrettyPrint()}");

            var uri = request.Uri;

            var errors = _bufferManager.GetAstFor(uri.ToString()).Errors;

            var diagnostics = new List<Diagnostic>();
            foreach (var error in errors)
            {
                diagnostics.Add(new Diagnostic()
                {
                    Severity = DiagnosticSeverity.Error,
                    Message = $"Parser Error:\n{error.Message}",
                    Range = new Range(new Position(error.Line - 1, error.Column),
                        new Position(error.Line - 1, error.Column)),
                });
            }

            diagnostics.Add(new Diagnostic()
            {
                Severity = DiagnosticSeverity.Error,
                Message = $"Parser Error:\nTEST",
                Range = new Range(new Position(3, 4),
                    new Position(3, 8))
            });

            request.Diagnostics = new Container<Diagnostic>(diagnostics);
            
            _languageServer.SendNotification("textDocument/publishDiagnostics", request);

            return Unit.Task;
        }
    }
}