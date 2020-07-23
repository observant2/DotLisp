using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;

namespace DotLispLsp
{
    internal class TextDocumentSyncHandler : ITextDocumentSyncHandler
    {
        private readonly ILanguageServer _languageServer;
        private readonly BufferManager _bufferManager;

        private SynchronizationCapability _capability;

        public TextDocumentSyncHandler(ILanguageServer languageServer,
            BufferManager bufferManager)
        {
            _languageServer = languageServer;
            _bufferManager = bufferManager;
        }

        public TextDocumentChangeRegistrationOptions GetRegistrationOptions()
        {
            return new TextDocumentChangeRegistrationOptions()
            {
                DocumentSelector = GlobalSettings.DocumentSelector,
                SyncKind = TextDocumentSyncKind.Full
            };
        }


        public Task<Unit> Handle(DidChangeTextDocumentParams request,
            CancellationToken cancellationToken)
        {
            var documentPath = request.TextDocument.Uri.ToString();
            var text = request.ContentChanges.FirstOrDefault()?.Text;

            _bufferManager.UpdateBuffer(documentPath, text);

            _languageServer.Window.LogInfo(
                $"Updated buffer for document: {documentPath}");

            return Unit.Task;
        }

        public Task<Unit> Handle(DidOpenTextDocumentParams request,
            CancellationToken cancellationToken)
        {
            _bufferManager.UpdateBuffer(request.TextDocument.Uri.ToString(),
                request.TextDocument.Text);
            return Unit.Task;
        }

        public Task<Unit> Handle(DidCloseTextDocumentParams request,
            CancellationToken cancellationToken)
        {
            return Unit.Task;
        }

        public Task<Unit> Handle(DidSaveTextDocumentParams request,
            CancellationToken cancellationToken)
        {
            return Unit.Task;
        }

        public void SetCapability(SynchronizationCapability capability)
        {
            _capability = capability;
        }

        TextDocumentRegistrationOptions
            IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions()
            {
                DocumentSelector = GlobalSettings.DocumentSelector
            };
        }

        TextDocumentSaveRegistrationOptions
            IRegistration<TextDocumentSaveRegistrationOptions>.
            GetRegistrationOptions()
        {
            return new TextDocumentSaveRegistrationOptions()
            {
                DocumentSelector = GlobalSettings.DocumentSelector,
                IncludeText = true
            };
        }

        public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
        {
            return new TextDocumentAttributes(uri, "dotlisp");
        }
    }
}