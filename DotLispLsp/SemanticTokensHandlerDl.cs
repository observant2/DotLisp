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
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
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
            var content =
                _bufferManager.GetBuffer(identifier.TextDocument.Uri.ToString());

            _logger.LogInformation(
                $"content: {content}\nuri: {identifier.TextDocument.Uri}");

            var inPort = new Parser(content);

            // TODO: Read() fails, when comments are present in the file...
            var expressions = new List<DotExpression>();
            DotExpression expression;
            try
            {
                do
                {
                    expression = inPort.Read();
                    expressions.Add(expression);
                    _logger.LogInformation(
                        $"read expression:\n{expression.PrettyPrint()}");
                } while (expression != null);
            }
            catch (Exception e)
            {
                _logger.LogError("SemanticTokensHandler error:\n" + e.Message);
                return;
            }

            await Task.Yield();

            var ast = expressions.ToDotList();

            _logger.LogInformation(
                $"-----------------original ast:\n{ast.PrettyPrint()}");

            var symbols = ExtractTypes<DotSymbol>(ast);
            _logger.LogInformation(
                $"-----------------extracted Symbols:\n{symbols.PrettyPrint()}");
            // TODO: Make this parallel
            foreach (var symbol in symbols)
            {
                _logger.LogInformation(
                    $"({symbol.Line}:{symbol.Column}) symbol: {symbol.Name}");
                builder.Push(symbol.Line - 1, symbol.Column,
                    symbol.Name.Length,
                    SemanticTokenType.Function, SemanticTokenModifier.Static, SemanticTokenModifier.Documentation);
            }

            var strings = ExtractTypes<DotString>(ast);
            _logger.LogInformation(
                $"-----------------extracted Strings:\n{strings.PrettyPrint()}");
            foreach (var str in strings)
            {
                _logger.LogInformation(
                    $"({str.Line}:{str.Column}) string: {str.Value}");
                builder.Push(str.Line - 1, str.Column, str.Value.Length + 2,
                    SemanticTokenType.Class, SemanticTokenModifier.Static, SemanticTokenModifier.Readonly);
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
    }
#pragma warning restore 618
}