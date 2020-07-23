using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DotLispLsp
{
    internal class CompletionHandler : ICompletionHandler
    {
        private readonly ILanguageServer _router;
        private readonly BufferManager _bufferManager;

        private CompletionCapability _capability;

        public CompletionHandler(ILanguageServer router, BufferManager bufferManager)
        {
            _router = router;
            _bufferManager = bufferManager;
        }

        public CompletionRegistrationOptions GetRegistrationOptions()
        {
            return new CompletionRegistrationOptions
            {
                DocumentSelector = GlobalSettings.DocumentSelector,
                ResolveProvider = true
            };
        }

        public async Task<CompletionList> Handle(CompletionParams request,
            CancellationToken cancellationToken)
        {
            var documentPath = request.TextDocument.Uri.ToString();
            var buffer = _bufferManager.GetBuffer(documentPath);

            var cList = new[]
            {
                new CompletionItem()
                {
                    Label = "(map proc list)",
                    Kind = CompletionItemKind.Function,
                    Detail = "maps the proc over the list returning the transformed list",
                    Documentation = "Documentation... Examples... Markup...",
                    SortText = "map",
                    FilterText = "map",
                    InsertText = "(map $1 $2)",
                    InsertTextFormat = InsertTextFormat.Snippet
                }
            };
            return new CompletionList(cList);
        }

        public void SetCapability(CompletionCapability capability)
        {
            _capability = capability;
        }
    }
}