using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DotLisp.Parsing;
using DotLisp.Types;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DotLispLsp
{
    public class BufferManager
    {
        public class Buffer
        {
            public List<DotExpression> Expressions { get; set; }
            public List<ParserError> Errors { get; set; }
        }
        
        private ConcurrentDictionary<string, Buffer> _buffers =
            new ConcurrentDictionary<string, Buffer>();

        public void UpdateBuffer(string documentPath, string buffer)
        {
            var inPort = new Parser(buffer);

            // TODO: Read() fails, when comments are present in the file...
            var expressions = new List<DotExpression>();
            DotExpression expression;
            try
            {
                do
                {
                    expression = inPort.Read().AST;
                    expressions.Add(expression);
                } while (expression != null);
            }
            catch (Exception e)
            {
                // _logger.LogError("SemanticTokensHandler error:\n" + e.Message);
                return;
            }

            var b = new Buffer
            {
                Expressions = expressions,
                Errors = inPort.ParserErrors,
            };
            
            _buffers.AddOrUpdate(documentPath, b, (k, v) => b);
        }

        public Buffer GetAstFor(string documentPath)
        {
            return _buffers.TryGetValue(documentPath, out var buffer)
                ? buffer
                : null;
        }
    }
}