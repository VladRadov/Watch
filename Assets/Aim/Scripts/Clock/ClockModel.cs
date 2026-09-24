using System;
using UniRx;

namespace Aim.Clock
{
    public sealed class ClockModel : IDisposable
    {
        private readonly ReactiveProperty<DateTime> _currentTime = new ReactiveProperty<DateTime>(DateTime.Now);
        private readonly ReactiveProperty<bool> _isRunning = new ReactiveProperty<bool>(false);
        private readonly ReactiveProperty<bool> _isEditMode = new ReactiveProperty<bool>(false);

        private DateTime _baseTime;
        private float _elapsedSeconds;
        private IDisposable _tickSubscription;

        public IReadOnlyReactiveProperty<DateTime> CurrentTime => _currentTime;
        public IReadOnlyReactiveProperty<bool> IsRunning => _isRunning;
        public IReadOnlyReactiveProperty<bool> IsEditMode => _isEditMode;

        public void SetTime(DateTime time)
        {
            _baseTime = time;
            _elapsedSeconds = 0f;
            _currentTime.Value = time;
        }

        public void StartTicking(float intervalSeconds)
        {
            StopTicking();

            _baseTime = _currentTime.Value;
            _elapsedSeconds = 0f;
            _isRunning.Value = true;

            _tickSubscription = Observable
                .Interval(TimeSpan.FromSeconds(intervalSeconds))
                .Where(_ => _isRunning.Value && !_isEditMode.Value)
                .Subscribe(_ =>
                {
                    _elapsedSeconds += intervalSeconds;
                    _currentTime.Value = _baseTime.AddSeconds(_elapsedSeconds);
                });
        }

        public void StopTicking()
        {
            _tickSubscription?.Dispose();
            _tickSubscription = null;
            _isRunning.Value = false;
        }

        public void SetEditMode(bool isEditMode)
        {
            if (isEditMode)
            {
                _baseTime = _currentTime.Value;
                _elapsedSeconds = 0f;
            }

            _isEditMode.Value = isEditMode;
        }

        public void Dispose()
        {
            StopTicking();
            _currentTime.Dispose();
            _isRunning.Dispose();
            _isEditMode.Dispose();
        }
    }
}
