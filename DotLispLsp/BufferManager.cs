using System.Collections.Concurrent;

namespace DotLispLsp
{
    public class BufferManager
    {
        private ConcurrentDictionary<string, string> _buffers =
            new ConcurrentDictionary<string, string>();

        public void UpdateBuffer(string documentPath, string buffer)
        {
            _buffers.AddOrUpdate(documentPath, buffer, (k, v) => buffer);
        }

        public string GetBuffer(string documentPath)
        {
            return _buffers.TryGetValue(documentPath, out var buffer)
                ? buffer
                : null;
        }
    }
}