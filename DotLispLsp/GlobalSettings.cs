using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace DotLispLsp
{
    public class GlobalSettings
    {
        public static DocumentSelector DocumentSelector = new DocumentSelector(
            DocumentFilter.ForLanguage("dotlisp")
        );

        public static TextDocumentSyncKind SyncKind = TextDocumentSyncKind.Full;
    }
}