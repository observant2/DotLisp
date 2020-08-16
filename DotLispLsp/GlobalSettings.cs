using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Models.Proposals;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace DotLispLsp
{
    public class GlobalSettings
    {
        public static DocumentSelector DocumentSelector = new DocumentSelector(
            DocumentFilter.ForLanguage("dotlisp")
        );

        public static TextDocumentSyncKind SyncKind = TextDocumentSyncKind.Full;

        public static SemanticTokensRegistrationOptions
            SemanticTokensRegistrationOptions =
                new SemanticTokensRegistrationOptions()
                {
                    DocumentSelector = DocumentSelector.ForLanguage("dotlisp"),
                    Legend = new SemanticTokensLegend(),
                    Full = new SemanticTokensCapabilityRequestFull
                    {
                        Delta = true
                    },
                    Range = true
                };
    }
}