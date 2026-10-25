using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatsVsPillows.UI
{
    /// <summary>A result-panel button: its label and what it does.</summary>
    public readonly struct ButtonAction
    {
        public ButtonAction(string label, Action onClick)
        {
            Label = label;
            OnClick = onClick;
        }

        public string Label { get; }
        public Action OnClick { get; }
    }

    /// <summary>Full-screen message shown when the level is won or lost, with up to two buttons.</summary>
    public sealed class ResultPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button primaryButton;
        [SerializeField] private TMP_Text primaryButtonLabel;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private TMP_Text secondaryButtonLabel;

        private Action _onPrimary;
        private Action _onSecondary;

        private void Awake()
        {
            primaryButton.onClick.AddListener(HandlePrimaryClicked);
            if (secondaryButton != null)
                secondaryButton.onClick.AddListener(HandleSecondaryClicked);
        }

        private void OnDestroy()
        {
            primaryButton.onClick.RemoveListener(HandlePrimaryClicked);
            if (secondaryButton != null)
                secondaryButton.onClick.RemoveListener(HandleSecondaryClicked);
        }

        public void Hide() => gameObject.SetActive(false);

        /// <summary>Shows the panel. Buttons without an action are hidden.</summary>
        public void Show(string title, string message, ButtonAction? primary = null, ButtonAction? secondary = null)
        {
            titleText.text = title;
            messageText.text = message;
            _onPrimary = Configure(primaryButton, primaryButtonLabel, primary);
            _onSecondary = Configure(secondaryButton, secondaryButtonLabel, secondary);
            gameObject.SetActive(true);
        }

        private static Action Configure(Button button, TMP_Text label, ButtonAction? action)
        {
            if (button == null)
                return null;

            button.gameObject.SetActive(action.HasValue);
            if (!action.HasValue)
                return null;

            label.text = action.Value.Label;
            return action.Value.OnClick;
        }

        private void HandlePrimaryClicked() => _onPrimary?.Invoke();

        private void HandleSecondaryClicked() => _onSecondary?.Invoke();
    }
}
