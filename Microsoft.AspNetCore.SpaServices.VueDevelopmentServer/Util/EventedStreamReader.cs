using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SpaServices.VueDevelopmentServer.Util
{
    internal class EventedStreamReader
    {
        public delegate void OnReceivedChunkHandler(ArraySegment<char> chunk);

        public delegate void OnReceivedLineHandler(string line);

        public delegate void OnStreamClosedHandler();

        private readonly StringBuilder _linesBuffer;
        private readonly StreamReader _streamReader;

        public EventedStreamReader(StreamReader streamReader)
        {
            var streamReader1 = streamReader;
            _streamReader = streamReader1 ?? throw new ArgumentNullException(nameof(streamReader));
            _linesBuffer = new StringBuilder();
            Task.Factory.StartNew(Run);
        }

        public event OnReceivedChunkHandler OnReceivedChunk;

        public event OnReceivedLineHandler OnReceivedLine;

        public event OnStreamClosedHandler OnStreamClosed;

        public Task<Match> WaitForMatch(Regex regex)
        {
            var tcs = new TaskCompletionSource<Match>();
            var completionLock = new object();
            var onReceivedLineHandler = (OnReceivedLineHandler) null;
            var onStreamClosedHandler = (OnStreamClosedHandler) null;
            onReceivedLineHandler = line =>
            {
                var match = regex.Match(line);
                if (!match.Success)
                    return;
                ResolveIfStillPending(() => tcs.SetResult(match));
            };
            onStreamClosedHandler = () => ResolveIfStillPending(() => tcs.SetException(new EndOfStreamException()));
            OnReceivedLine += onReceivedLineHandler;
            OnStreamClosed += onStreamClosedHandler;
            return tcs.Task;

            void ResolveIfStillPending(Action applyResolution)
            {
                lock (completionLock)
                {
                    if (tcs.Task.IsCompleted)
                        return;
                    OnReceivedLine -= onReceivedLineHandler;
                    OnStreamClosed -= onStreamClosedHandler;
                    applyResolution();
                }
            }
        }

        private async Task Run()
        {
            var buf = new char[8192];
            while (true)
            {
                var num1 = await _streamReader.ReadAsync(buf, 0, buf.Length);
                if (num1 != 0)
                {
                    OnChunk(new ArraySegment<char>(buf, 0, num1));
                    var num2 = Array.IndexOf(buf, '\n', 0, num1);
                    if (num2 < 0)
                    {
                        _linesBuffer.Append(buf, 0, num1);
                    }
                    else
                    {
                        _linesBuffer.Append(buf, 0, num2 + 1);
                        OnCompleteLine(_linesBuffer.ToString());
                        _linesBuffer.Clear();
                        _linesBuffer.Append(buf, num2 + 1, num1 - (num2 + 1));
                    }
                }
                else
                {
                    break;
                }
            }

            OnClosed();
        }

        private void OnChunk(ArraySegment<char> chunk)
        {
            var onReceivedChunk = OnReceivedChunk;
            onReceivedChunk?.Invoke(chunk);
        }

        private void OnCompleteLine(string line)
        {
            var onReceivedLine = OnReceivedLine;
            onReceivedLine?.Invoke(line);
        }

        private void OnClosed()
        {
            var onStreamClosed = OnStreamClosed;
            onStreamClosed?.Invoke();
        }
    }
}