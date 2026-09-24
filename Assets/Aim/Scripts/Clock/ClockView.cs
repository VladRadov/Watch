using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Aim.Clock
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
        private GameObject _editPanel;

        private readonly Subject<Unit> _editClicked = new Subject<Unit>();
        private readonly Subject<Unit> _saveClicked = new Subject<Unit>();
        private readonly Subject<Unit> _cancelClicked = new Subject<Unit>();
        private readonly Subject<float> _hourHandDragged = new Subject<float>();
        private readonly Subject<float> _minuteHandDragged = new Subject<float>();
        private readonly Subject<string> _keyboardTimeSubmitted = new Subject<string>();

        private bool _isEditInteractionEnabled;

        public bool IsEditInteractionEnabled => _isEditInteractionEnabled;

        public IObservable<Unit> EditClicked => _editClicked;
        public IObservable<Unit> SaveClicked => _saveClicked;
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
                _saveButton.onClick.AddListener(() => _saveClicked.OnNext(Unit.Default));
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(() => _cancelClicked.OnNext(Unit.Default));
            }

            if (_timeInputField != null)
            {
                _timeInputField.onEndEdit.AddListener(value => _keyboardTimeSubmitted.OnNext(value));
            }

            SetEditModeVisual(false);
        }

        public void RenderTime(DateTime time, string digitalFormat)
        {
            if (_digitalTimeText != null)
            {
                _digitalTimeText.text = time.ToString(digitalFormat);
            }

            var hours = time.Hour % 12;
            var minutes = time.Minute;
            var seconds = time.Second + time.Millisecond / 1000f;

            var hourAngle = -(hours * 30f + minutes * 0.5f);
            var minuteAngle = -(minutes * 6f + seconds * 0.1f);
            var secondAngle = -(seconds * 6f);

            SetHandRotation(_hourHand, hourAngle);
            SetHandRotation(_minuteHand, minuteAngle);
            SetHandRotation(_secondHand, secondAngle);
        }

        public void SetEditModeVisual(bool isEditMode)
        {
            _isEditInteractionEnabled = isEditMode;

            if (_editPanel != null)
            {
                _editPanel.SetActive(isEditMode);
            }

            if (_editButton != null)
            {
                _editButton.gameObject.SetActive(!isEditMode);
            }

            if (_saveButton != null)
            {
                _saveButton.gameObject.SetActive(isEditMode);
            }

            if (_cancelButton != null)
            {
                _cancelButton.gameObject.SetActive(isEditMode);
            }

            if (_timeInputField != null)
            {
                _timeInputField.gameObject.SetActive(isEditMode);
            }
        }

        public void SetKeyboardTime(DateTime time)
        {
            if (_timeInputField != null)
            {
                _timeInputField.text = time.ToString("HH:mm:ss");
            }
        }

        public DateTime BuildEditedTime(DateTime datePart)
        {
            if (_timeInputField != null &&
                TryParseTime(_timeInputField.text, datePart, out var fromInput))
            {
                return fromInput;
            }

            return datePart;
        }

        public void NotifyHourHandDragged(float angleDegrees)
        {
            _hourHandDragged.OnNext(angleDegrees);
        }

        public void NotifyMinuteHandDragged(float angleDegrees)
        {
            _minuteHandDragged.OnNext(angleDegrees);
        }

        private static void SetHandRotation(RectTransform hand, float zAngle)
        {
            if (hand == null)
            {
                return;
            }

            hand.localRotation = Quaternion.Euler(0f, 0f, zAngle);
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
