using CatsVsPillows.Data;
using CatsVsPillows.Flow;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatsVsPillows.UI
{
    /// <summary>Main menu: shows a card per level in the catalog and starts the chosen one.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private LevelCatalog catalog;
        [Tooltip("Inactive card used as a template; one copy is made per level.")]
        [SerializeField] private LevelCardView cardTemplate;
        [SerializeField] private Transform cardContainer;

        private void Start()
        {
            // The menu always runs at normal speed, even if a test speed was set in the game scene.
            Time.timeScale = 1f;
            cardTemplate.gameObject.SetActive(false);

            for (int i = 0; i < catalog.Count; i++)
            {
                LevelData level = catalog.GetLevel(i);
                int levelIndex = i;

                LevelCardView card = Instantiate(cardTemplate, cardContainer);
                card.name = $"Level Card {i + 1}";
                card.gameObject.SetActive(true);
                card.Bind(i + 1, level, LevelProgress.IsCompleted(level), () => StartLevel(levelIndex));
            }
        }

        public void StartLevel(int levelIndex)
        {
            LevelSession.Select(levelIndex);
            SceneManager.LoadScene(catalog.GameSceneName);
        }
    }
}
