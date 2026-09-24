using UnityEngine;
using UnityEngine.UI;

namespace Aim.Loading
{
    public sealed class LoadingView : MonoBehaviour
    {
        [SerializeField]
        private GameObject _root;

        [SerializeField]
        private Text _statusText;

        [SerializeField]
        private Slider _progressSlider;

        public void Show(string status)
        {
            if (_root != null)
            {
                _root.SetActive(true);
            }

            SetStatus(status);
            SetProgress(0f);
        }

        public void SetStatus(string status)
        {
            if (_statusText != null)
            {
                _statusText.text = status;
            }
        }

        public void SetProgress(float normalized)
        {
            if (_progressSlider != null)
            {
                _progressSlider.value = Mathf.Clamp01(normalized);
            }
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }
    }
}
