using System;
using UniRx;
using UnityEngine;

namespace Aim.Clock
{
    public sealed class ClockController : IDisposable
    {
        private readonly ClockModel _model;
        private readonly ClockView _view;
        private readonly ClockConfig _config;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ClockController(ClockModel model, ClockView view, ClockConfig config)
        {
            _model = model;
            _view = view;
            _config = config;
        }

        public void Initialize()
        {
            _model.CurrentTime
                .Subscribe(time =>
                {
                    if (!_model.IsEditMode.Value)
                    {
                        _view.RenderTime(time, _config.DigitalTimeFormat);
                    }
                })
                .AddTo(_disposables);

            _model.IsEditMode
                .Subscribe(_view.SetEditModeVisual)
                .AddTo(_disposables);

            _view.EditClicked
                .Subscribe(_ => EnterEditMode())
                .AddTo(_disposables);

            _view.SaveClicked
                .Subscribe(_ => SaveEditedTime())
                .AddTo(_disposables);

            _view.CancelClicked
                .Subscribe(_ => CancelEdit())
                .AddTo(_disposables);

            _view.HourHandDragged
                .Subscribe(ApplyHourAngle)
                .AddTo(_disposables);

            _view.MinuteHandDragged
                .Subscribe(ApplyMinuteAngle)
                .AddTo(_disposables);

            _view.KeyboardTimeSubmitted
                .Subscribe(TryApplyKeyboardTime)
                .AddTo(_disposables);
        }

        private void EnterEditMode()
        {
            _model.SetEditMode(true);
            _view.SetKeyboardTime(_model.CurrentTime.Value);
            _view.RenderTime(_model.CurrentTime.Value, _config.DigitalTimeFormat);
        }

        private void CancelEdit()
        {
            _model.SetEditMode(false);
            _view.RenderTime(_model.CurrentTime.Value, _config.DigitalTimeFormat);
        }

        private void SaveEditedTime()
        {
            var edited = _view.BuildEditedTime(_model.CurrentTime.Value);
            _model.SetTime(edited);
            _model.SetEditMode(false);

            if (!_model.IsRunning.Value)
            {
                _model.StartTicking(_config.TickIntervalSeconds);
            }
        }

        private void ApplyHourAngle(float angleDegrees)
        {
            if (!_model.IsEditMode.Value)
            {
                return;
            }

            var time = _model.CurrentTime.Value;
            var hours12 = AngleToHour(angleDegrees);
            var hours24 = CombineHourWithPeriod(hours12, time.Hour);
            var updated = new DateTime(time.Year, time.Month, time.Day, hours24, time.Minute, time.Second);
            _model.SetTime(updated);
            _view.RenderTime(updated, _config.DigitalTimeFormat);
            _view.SetKeyboardTime(updated);
        }

        private void ApplyMinuteAngle(float angleDegrees)
        {
            if (!_model.IsEditMode.Value)
            {
                return;
            }

            var time = _model.CurrentTime.Value;
            var minutes = AngleToMinute(angleDegrees);
            var updated = new DateTime(time.Year, time.Month, time.Day, time.Hour, minutes, time.Second);
            _model.SetTime(updated);
            _view.RenderTime(updated, _config.DigitalTimeFormat);
            _view.SetKeyboardTime(updated);
        }

        private void TryApplyKeyboardTime(string input)
        {
            if (!_model.IsEditMode.Value)
            {
                return;
            }

            if (!TryParseTime(input, _model.CurrentTime.Value, out var parsed))
            {
                Debug.LogWarning($"[Clock] Invalid time input: '{input}'");
                return;
            }

            _model.SetTime(parsed);
            _view.RenderTime(parsed, _config.DigitalTimeFormat);
        }

        private static int AngleToHour(float angleDegrees)
        {
            var normalized = (angleDegrees + 360f) % 360f;
            return Mathf.FloorToInt(normalized / 30f) % 12;
        }

        private static int AngleToMinute(float angleDegrees)
        {
            var normalized = (angleDegrees + 360f) % 360f;
            return Mathf.FloorToInt(normalized / 6f) % 60;
        }

        private static int CombineHourWithPeriod(int hour12, int previousHour24)
        {
            var isPm = previousHour24 >= 12;
            if (hour12 == 0)
            {
                return isPm ? 12 : 0;
            }

            return isPm ? hour12 + 12 : hour12;
        }

        private static bool TryParseTime(string input, DateTime datePart, out DateTime result)
        {
            result = datePart;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var parts = input.Trim().Split(':');
            if (parts.Length < 2 || parts.Length > 3)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out var hours) ||
                !int.TryParse(parts[1], out var minutes))
            {
                return false;
            }

            var seconds = 0;
            if (parts.Length == 3 && !int.TryParse(parts[2], out seconds))
            {
                return false;
            }

            if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59 || seconds < 0 || seconds > 59)
            {
                return false;
            }

            result = new DateTime(datePart.Year, datePart.Month, datePart.Day, hours, minutes, seconds);
            return true;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
