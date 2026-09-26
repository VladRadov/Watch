using System;
using UnityEngine;
using UnityEngine.UI;

using Watch.Common;

using UniRx;

namespace Watch.Clock
{
    public sealed class ClockView : MonoBehaviour
    {
        [Header("Analog")]
        [SerializeField]
        private RectTransform _hourHand;

        [SerializeField]
        private RectTransform _minuteHand;

        [SerializeField]
        private RectTransform _secondHand;

        [Header("Digital")]
        [SerializeField]
        private Text _digitalTimeText;

        [Header("Edit UI (wired in edit stage)")]
        [SerializeField]
        private Button _editButton;

        [SerializeField]
        private Button _saveButton;

        [SerializeField]
        private Button _cancelButton;

        [SerializeField]
        private InputField _timeInputField;

        [SerializeField]
        private Button _addHoursButton;

        [SerializeField]
        private Button _decreaseHoursButton;

        [SerializeField]
        private Button _addMinutesButton;

        [SerializeField]
        private Button _decreaseMinutesButton;

        [SerializeField]
        private Button _addSecondsButton;

        [SerializeField]
        private Button _decreaseSecondsButton;

        [SerializeField]
        private PanelAnimationView _editPanel;

        private const int HOURS_PER_CIRCLE = 12;
        private const float MILLISECONDS_PER_SECOND = 1000f;
        private const float DEGREES_PER_MINUTE = 6f;
        private const float DEGREES_PER_SECOND = 6f;
        private const float HOUR_HAND_DEGREES_PER_MINUTE = 0.5f;
        private const float MINUTE_HAND_DEGREES_PER_SECOND = 0.1f;
        private const float DEGREES_PER_HOUR = 30f;

        private readonly Subject<Unit> _editClicked = new Subject<Unit>();
        private readonly Subject<string> _saveClicked = new Subject<string>();
        private readonly Subject<Unit> _cancelClicked = new Subject<Unit>();
        private readonly Subject<float> _hourHandDragged = new Subject<float>();
        private readonly Subject<float> _minuteHandDragged = new Subject<float>();
        private readonly Subject<string> _keyboardTimeSubmitted = new Subject<string>();

        private bool _isEditInteractionEnabled;

        public bool IsEditInteractionEnabled => _isEditInteractionEnabled;

        public IObservable<Unit> EditClicked => _editClicked;
        public IObservable<string> SaveClicked => _saveClicked;
        public IObservable<Unit> CancelClicked => _cancelClicked;
        public IObservable<float> HourHandDragged => _hourHandDragged;
        public IObservable<float> MinuteHandDragged => _minuteHandDragged;
        public IObservable<string> KeyboardTimeSubmitted => _keyboardTimeSubmitted;

        private void Awake()
        {
            if (_editButton != null)
            {
                _editButton.onClick.AddListener(() => _editClicked.OnNext(Unit.Default));
            }

            if (_saveButton != null)
            {
                _saveButton.onClick.AddListener(() => _saveClicked.OnNext(_timeInputField.text));
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(() => _cancelClicked.OnNext(Unit.Default));
            }

            if (_timeInputField != null)
            {
                _timeInputField.onEndEdit.AddListener(value => _keyboardTimeSubmitted.OnNext(value));
            }

            if (_addHoursButton != null)
            {
                _addHoursButton.onClick.AddListener(() => ChangeTime(hours: 1));
            }

            if (_decreaseHoursButton != null)
            {
                _decreaseHoursButton.onClick.AddListener(() => ChangeTime(hours: -1));
            }

            if (_addMinutesButton != null)
            {
                _addMinutesButton.onClick.AddListener(() => ChangeTime(minutes: 1));
            }

            if (_decreaseMinutesButton != null)
            {
                _decreaseMinutesButton.onClick.AddListener(() => ChangeTime(minutes: -1));
            }

            if (_addSecondsButton != null)
            {
                _addSecondsButton.onClick.AddListener(() => ChangeTime(seconds: 1));
            }

            if (_decreaseSecondsButton != null)
            {
                _decreaseSecondsButton.onClick.AddListener(() => ChangeTime(seconds: -1));
            }

            SetEditModeVisual(false);
        }

        public void RenderTime(DateTime time, string digitalFormat)
        {
            if (_digitalTimeText != null)
            {
                _digitalTimeText.text = time.ToString(digitalFormat);
            }

            var hours = time.Hour % HOURS_PER_CIRCLE;
            var minutes = time.Minute;
            var seconds = time.Second + time.Millisecond / MILLISECONDS_PER_SECOND;

            var hourAngle = -(hours * DEGREES_PER_HOUR + minutes * HOUR_HAND_DEGREES_PER_MINUTE);
            var minuteAngle = -(minutes * DEGREES_PER_MINUTE + seconds * MINUTE_HAND_DEGREES_PER_SECOND);
            var secondAngle = -(seconds * DEGREES_PER_SECOND);

            SetHandRotation(_hourHand, hourAngle);
            SetHandRotation(_minuteHand, minuteAngle);
            SetHandRotation(_secondHand, secondAngle);
        }

        public void SetEditModeVisual(bool isEditMode)
        {
            _isEditInteractionEnabled = isEditMode;

            if (_editPanel != null)
            {
                if (isEditMode)
                    _editPanel.Show();
                else
                    _editPanel.Hide();
            }
        }

        public void SetKeyboardTime(DateTime time)
        {
            if (_timeInputField != null)
            {
                _timeInputField.text = time.ToString("HH:mm:ss");
            }
        }

        public void NotifyHourHandDragged(float angleDegrees)
        {
            _hourHandDragged.OnNext(angleDegrees);
        }

        public void NotifyMinuteHandDragged(float angleDegrees)
        {
            _minuteHandDragged.OnNext(angleDegrees);
        }

        private void SetHandRotation(RectTransform hand, float zAngle)
        {
            if (hand == null)
            {
                return;
            }

            hand.localRotation = Quaternion.Euler(0f, 0f, zAngle);
        }

        private void ChangeTime(double hours = 0, double minutes = 0, double seconds = 0)
        {
            var newDateTime = DateTime.Parse(_timeInputField.text)
                .AddHours(hours)
                .AddMinutes(minutes)
                .AddSeconds(seconds);

            SetKeyboardTime(newDateTime);
            _timeInputField.onEndEdit.Invoke(_timeInputField.text);
        }

        private void OnDestroy()
        {
            _editClicked.Dispose();
            _saveClicked.Dispose();
            _cancelClicked.Dispose();
            _hourHandDragged.Dispose();
            _minuteHandDragged.Dispose();
            _keyboardTimeSubmitted.Dispose();
        }
    }
}
