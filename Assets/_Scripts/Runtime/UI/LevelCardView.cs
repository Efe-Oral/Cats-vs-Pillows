using System;
using CatsVsPillows.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatsVsPillows.UI
{
    /// <summary>One level button in the main menu: number, name, description, difficulty pips and a "done" badge.</summary>
    public sealed class LevelCardView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text numberText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text detailsText;
        [Tooltip("One image per difficulty point, left to right.")]
        [SerializeField] private Image[] difficultyPips;
        [SerializeField] private Color pipOnColor = new Color(1f, 0.55f, 0.4f);
        [SerializeField] private Color pipOffColor = new Color(0.3f, 0.2f, 0.2f, 0.2f);
        [SerializeField] private GameObject completedBadge;

        private Action _onClick;

        private void Awake() => button.onClick.AddListener(HandleClicked);

        private void OnDestroy() => button.onClick.RemoveListener(HandleClicked);

        public void Bind(int levelNumber, LevelData level, bool isCompleted, Action onClick)
        {
            _onClick = onClick;
            numberText.text = levelNumber.ToString();
            nameText.text = level.DisplayName;
            descriptionText.text = level.Description;
            detailsText.text = $"{level.GapCount} gaps  -  {level.Colors.Count} colors";

            for (int i = 0; i < difficultyPips.Length; i++)
                difficultyPips[i].color = i < level.Difficulty ? pipOnColor : pipOffColor;

            completedBadge.SetActive(isCompleted);
        }

        private void HandleClicked() => _onClick?.Invoke();
    }
}
