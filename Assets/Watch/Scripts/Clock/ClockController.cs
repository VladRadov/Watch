using System;
using UnityEngine;

using Watch.Common;

using UniRx;

namespace Watch.Clock
{
    public sealed class ClockController : IDisposable
    {
        private const float FULL_R_CIRCLE_DEGREES = 360f;
        private const int HOURS_ON_CLOCK = 12;
        private const int MINUTES_PER_CIRCLE = 60;
        private const float DEGREES_PER_HOUR = 30f;
        private const float DEGREES_PER_MINUTE = 6f;

        private readonly HandlerDateTime _handlerDateTime;
        private readonly ClockModel _model;
        private readonly ClockView _view;
        private readonly ClockConfig _config;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ClockController(ClockModel model, 
            ClockView view, 
            ClockConfig config,
            HandlerDateTime handlerDateTime)
        {
            _model = model;
            _view = view;
            _config = config;
            _handlerDateTime = handlerDateTime;
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
                .Subscribe(dateTimeEdited => SaveEditedTime(dateTimeEdited))
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

        private void SaveEditedTime(string dateTimeEdited)
        {
            var edited = _handlerDateTime.TryParseTime(dateTimeEdited, _model.CurrentTime.Value, out var fromInput)
                ? fromInput : _model.CurrentTime.Value;

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
            var hours24 = _handlerDateTime.CombineHourWithPeriod(hours12, time.Hour);
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

            if (!_handlerDateTime.TryParseTime(input, _model.CurrentTime.Value, out var parsed))
            {
                Debug.LogWarning($"[Clock] Invalid time input: '{input}'");
                return;
            }

            _view.RenderTime(parsed, _config.DigitalTimeFormat);
        }

        private int AngleToHour(float angleDegrees)
        {
            var normalized = (angleDegrees + FULL_R_CIRCLE_DEGREES) % FULL_R_CIRCLE_DEGREES;
            return Mathf.FloorToInt(normalized / DEGREES_PER_HOUR) % HOURS_ON_CLOCK;
        }

        private int AngleToMinute(float angleDegrees)
        {
            var normalized = (angleDegrees + FULL_R_CIRCLE_DEGREES) % FULL_R_CIRCLE_DEGREES;
            return Mathf.FloorToInt(normalized / DEGREES_PER_MINUTE) % MINUTES_PER_CIRCLE;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
