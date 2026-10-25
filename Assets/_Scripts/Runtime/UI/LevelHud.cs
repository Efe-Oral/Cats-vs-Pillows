using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CatsVsPillows.UI
{
    /// <summary>In-game overlay: level title in the corner, a menu button, and a title banner that fades out at the start.</summary>
    public sealed class LevelHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text cornerTitle;
        [SerializeField] private Button menuButton;

        [Header("Intro Banner")]
        [SerializeField] private CanvasGroup banner;
        [SerializeField] private TMP_Text bannerTitle;
        [SerializeField] private TMP_Text bannerSubtitle;
        [SerializeField, Min(0f)] private float bannerHoldSeconds = 1.6f;
        [SerializeField, Min(0.01f)] private float bannerFadeSeconds = 0.5f;

        public event Action MenuRequested;

        private void Awake() => menuButton.onClick.AddListener(HandleMenuClicked);

        private void OnDestroy() => menuButton.onClick.RemoveListener(HandleMenuClicked);

        public void ShowLevel(int levelNumber, int levelCount, string levelName)
        {
            cornerTitle.text = $"Level {levelNumber}/{levelCount}  {levelName}";
            bannerTitle.text = $"Level {levelNumber}";
            bannerSubtitle.text = levelName;
            StopAllCoroutines();
            StartCoroutine(BannerRoutine());
        }

        private IEnumerator BannerRoutine()
        {
            banner.gameObject.SetActive(true);
            banner.alpha = 1f;
            yield return new WaitForSecondsRealtime(bannerHoldSeconds);

            for (float time = 0f; time < bannerFadeSeconds; time += Time.unscaledDeltaTime)
            {
                banner.alpha = 1f - time / bannerFadeSeconds;
                yield return null;
            }
            banner.gameObject.SetActive(false);
        }

        private void HandleMenuClicked() => MenuRequested?.Invoke();
    }
}
