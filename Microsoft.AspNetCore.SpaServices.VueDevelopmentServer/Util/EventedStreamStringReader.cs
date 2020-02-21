using System;
using System.Text;

namespace Microsoft.AspNetCore.SpaServices.VueDevelopmentServer.Util
{
    internal class EventedStreamStringReader : IDisposable
    {
        private readonly EventedStreamReader _eventedStreamReader;
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private bool _isDisposed;

        public EventedStreamStringReader(EventedStreamReader eventedStreamReader)
        {
            var eventedStreamReader1 = eventedStreamReader;
            _eventedStreamReader = eventedStreamReader1 ?? throw new ArgumentNullException(nameof(eventedStreamReader));
            _eventedStreamReader.OnReceivedLine += OnReceivedLine;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            _eventedStreamReader.OnReceivedLine -= OnReceivedLine;
            _isDisposed = true;
        }

        public string ReadAsString()
        {
            return _stringBuilder.ToString();
        }

        private void OnReceivedLine(string line)
        {
            _stringBuilder.AppendLine(line);
        }
    }
}