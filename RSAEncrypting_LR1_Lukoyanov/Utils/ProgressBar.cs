﻿using System;
using System.Text;
using System.Threading;

namespace RSAEncrypting_LR1_Lukoyanov
{
    public class ProgressBar : IDisposable, IProgress<double>
    {
        private const int BlockCount = 100;
        private readonly TimeSpan _animationInterval = TimeSpan.FromSeconds(1.0 / 8);
        private const string Animation = @"|/-\";

        private readonly Timer _timer;

        private double _currentProgress;
        private string _currentText = string.Empty;
        private bool _disposed;
        private int _animationIndex;

        public ProgressBar()
        {
            _timer = new Timer(TimerHandler);

            if (!Console.IsOutputRedirected)
                ResetTimer();
        }

        public void Report(double value)
        {
            value = Math.Max(0, Math.Min(100, value));
            Interlocked.Exchange(ref _currentProgress, value);
        }

        private void TimerHandler(object state)
        {
            lock (_timer)
            {
                if (_disposed) return;

                var progressBlockCount = (int) (_currentProgress * BlockCount / 100);
                var percent = (int) _currentProgress;
                var text =
                    $"[{new string('#', progressBlockCount)}{new string('-', BlockCount - progressBlockCount)}] {percent,3}% {Animation[_animationIndex++ % Animation.Length]}";
                UpdateText(text);

                ResetTimer();
            }
        }

        private void UpdateText(string text)
        {
            var commonPrefixLength = 0;
            var commonLength = Math.Min(_currentText.Length, text.Length);
            while (commonPrefixLength < commonLength && text[commonPrefixLength] == _currentText[commonPrefixLength])
                commonPrefixLength++;

            var outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', _currentText.Length - commonPrefixLength);
            outputBuilder.Append(text.Substring(commonPrefixLength));

            Console.Write(outputBuilder);
            _currentText = text;
        }

        private void ResetTimer() => _timer.Change(_animationInterval, TimeSpan.FromMilliseconds(-1));

        public void Dispose()
        {
            TimerHandler(null);
            lock (_timer)
                _disposed = true;
        }
    }
}