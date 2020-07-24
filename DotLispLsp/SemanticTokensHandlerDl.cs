using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotLisp;
using DotLisp.Parsing;
using DotLisp.Types;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document.Proposals;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Models.Proposals;

namespace DotLispLsp
{
#pragma warning disable 618
    public class SemanticTokensHandlerDl : SemanticTokensHandler
    {
        private readonly ILogger _logger;
        private BufferManager _bufferManager;

        public SemanticTokensHandlerDl(ILogger<SemanticTokens> logger,
            BufferManager bufferManager) : base(
            new SemanticTokensRegistrationOptions()
            {
                DocumentSelector = DocumentSelector.ForLanguage("dotlisp"),
                Legend = new SemanticTokensLegend(),
                DocumentProvider =
                    new Supports<SemanticTokensDocumentProviderOptions>(true,
                        new SemanticTokensDocumentProviderOptions()
                        {
                            Edits = true
                        }),
                RangeProvider = true
            })
        {
            _logger = logger;
            _bufferManager = bufferManager;
        }

        public override async Task<SemanticTokens> Handle(
            SemanticTokensParams request, CancellationToken cancellationToken)
        {
            var result = await base.Handle(request, cancellationToken);
            return result;
        }

        public override async Task<SemanticTokens> Handle(
            SemanticTokensRangeParams request, CancellationToken cancellationToken)
        {
            var result = await base.Handle(request, cancellationToken);
            return result;
        }

        public override async Task<SemanticTokensOrSemanticTokensEdits> Handle(
            SemanticTokensEditsParams request,
            CancellationToken cancellationToken)
        {
            var result = await base.Handle(request, cancellationToken);
            return result;
        }

        protected override async Task Tokenize(SemanticTokensBuilder builder,
            ITextDocumentIdentifierParams identifier,
            CancellationToken cancellationToken)
        {
            using var typesEnumerator =
                RotateEnum(SemanticTokenType.Defaults).GetEnumerator();
            using var modifiersEnumerator =
                RotateEnum(SemanticTokenModifier.Defaults).GetEnumerator();
            // TODO: Get this from the BufferManager

            var content =
                _bufferManager.GetBuffer(identifier.TextDocument.Uri.ToString());

            _logger.LogInformation(
                $"content: {content}\nuri: {identifier.TextDocument.Uri}");

            var inPort = new InPort("");

            // TODO: Read() fails, when comments are present in the file...
            DotExpression ast = null;
            try
            {
                ast = Expander.Expand(inPort.Read(content));
            }
            catch (Exception e)
            {
                _logger.LogError("SemanticTokensHandler error:\n" + e.Message);
                return;
            }

            await Task.Yield();

            if (ast is DotList l)
            {
                var symbols = ExtractTypes<DotSymbol>(l);
                _logger.LogInformation("symbols:\n" + symbols.PrettyPrint());
                // TODO: Make this parallel
                foreach (var symbol in symbols)
                {
                    builder.Push(symbol.Line - 1, symbol.Column,
                        symbol.Name.Length,
                        SemanticTokenType.Function, SemanticTokenModifier.Static);
                }

                var strings = ExtractTypes<DotString>(l);
                _logger.LogInformation("strings:\n" + strings.PrettyPrint());
                foreach (var str in strings)
                {
                    builder.Push(str.Line - 1, str.Column, str.Value.Length + 2,
                        SemanticTokenType.Class, SemanticTokenModifier.Static);
                }
            }
        }

        private List<T> ExtractTypes<T>(DotList l) where T : DotExpression
        {
            var ret = new List<T>();
            foreach (var exp in l.Expressions)
            {
                if (exp is T s)
                {
                    ret.Add(s);
                }

                if (exp is DotList innerList)
                {
                    ret.AddRange(ExtractTypes<T>(innerList));
                }
            }

            return ret;
        }

        protected override Task<SemanticTokensDocument>
            GetSemanticTokensDocument(ITextDocumentIdentifierParams @params,
                CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new SemanticTokensDocument(GetRegistrationOptions().Legend));
        }


        private IEnumerable<T> RotateEnum<T>(IEnumerable<T> values)
        {
            while (true)
            {
                foreach (var item in values)
                    yield return item;
            }
        }
    }
#pragma warning restore 618
}