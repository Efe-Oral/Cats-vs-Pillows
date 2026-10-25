using System.Collections;
using System.Collections.Generic;
using CatsVsPillows.Core;
using CatsVsPillows.Data;
using CatsVsPillows.Flow;
using CatsVsPillows.Level;
using CatsVsPillows.UI;
using CatsVsPillows.Visuals;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatsVsPillows.Gameplay
{
    /// <summary>
    /// Composition root and game flow: picks the level from the <see cref="LevelCatalog"/>, builds it, wires the cat,
    /// pillow queue, input and UI together, decides when the level is won or lost, and moves on to the next level.
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        private enum GameState
        {
            Intro,
            Playing,
            Failed,
            Won
        }

        [Header("Data")]
        [SerializeField] private LevelCatalog catalog;
        [Tooltip("Level played when pressing Play in this scene directly (the main menu overrides it). " +
                 "Also the level shown in the Scene view preview.")]
        [SerializeField, Min(0)] private int editorLevelIndex;
        [SerializeField] private GameplaySettings settings;
        [SerializeField] private PillowPalette palette;

        [Header("Scene")]
        [SerializeField] private Transform levelRoot;
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private PillowDragController dragController;
        [SerializeField] private QueueView queueView;
        [SerializeField] private ResultPanel resultPanel;
        [SerializeField] private LevelHud hud;
        [SerializeField] private AtmosphereController atmosphere;

        [Header("Messages")]
        [SerializeField] private string failTitle = "Oops!";
        [SerializeField] private string failMessage = "Kitty needs a full stack of pillows.";
        [SerializeField] private string winTitle = "Purr-fect!";
        [SerializeField] private string finalWinTitle = "You did it!";
        [SerializeField] private string finalWinMessage = "Every room is cozy and kitty is ready for a nap.";
        [SerializeField] private string nextLevelLabel = "Next Level";
        [SerializeField] private string replayLabel = "Play Again";
        [SerializeField] private string menuLabel = "Menu";

        private readonly List<Gap> _placeableGaps = new List<Gap>();
        private readonly List<GapModel> _placeableModels = new List<GapModel>();

        private int _levelIndex;
        private LevelData _levelData;
        private LevelInstance _level;
        private CatController _cat;
        private PillowQueueModel _queue;
        private GameState _state;

        /// <summary>The level the edit-mode preview shows.</summary>
        public LevelData PreviewLevel => catalog != null ? catalog.GetLevel(editorLevelIndex) : null;
        public GameplaySettings Settings => settings;
        public PillowPalette Palette => palette;
        public AtmosphereController Atmosphere => atmosphere;

        private bool AcceptsPillows => _state == GameState.Intro || _state == GameState.Playing;

        private void Start()
        {
            _levelIndex = catalog.ClampIndex(LevelSession.CurrentLevelIndex ?? editorLevelIndex);
            LevelSession.Select(_levelIndex);
            _levelData = catalog.GetLevel(_levelIndex);

            foreach (string problem in _levelData.Validate(settings.SlotsPerGap))
                Debug.LogWarning($"[{_levelData.name}] {problem}", _levelData);

            atmosphere.Apply(_levelData.Atmosphere);
            _level = new LevelBuilder(settings, palette).Build(_levelData, levelRoot);

            _cat = Instantiate(settings.CatPrefab, levelRoot);
            _cat.Initialize(_level, _levelData.CatWalkSpeed);
            _cat.GapCrossed += HandleGapCrossed;
            _cat.Fell += HandleCatFell;
            _cat.ReachedGoal += HandleGoalReached;
            cameraFollow.SetTarget(_cat.transform, snap: true);

            RefreshPlaceableGaps();

            _queue = new PillowQueueModel(_levelData.Colors, _levelData.VisibleQueueCount, _levelData.CreateRandom());
            dragController.Initialize(settings, palette, () => _placeableGaps);
            dragController.DropRequested += HandleDropRequested;
            queueView.Bind(_queue, palette, dragController);
            _queue.Fill(_placeableModels);

            resultPanel.Hide();
            hud.MenuRequested += GoToMenu;
            hud.ShowLevel(_levelIndex + 1, catalog.Count, _levelData.DisplayName);
            StartCoroutine(IntroRoutine());
        }

        private void OnValidate() => AuthoringDataChanged.Notify();

        private void OnDestroy()
        {
            if (_cat != null)
            {
                _cat.GapCrossed -= HandleGapCrossed;
                _cat.Fell -= HandleCatFell;
                _cat.ReachedGoal -= HandleGoalReached;
            }
            if (dragController != null)
                dragController.DropRequested -= HandleDropRequested;
            if (hud != null)
                hud.MenuRequested -= GoToMenu;
        }

        private IEnumerator IntroRoutine()
        {
            _state = GameState.Intro;
            yield return new WaitForSeconds(settings.StartDelay);

            if (_state != GameState.Intro)
                yield break;

            _state = GameState.Playing;
            _cat.StartWalking();
        }

        private void HandleDropRequested(Gap gap, int slot)
        {
            if (!AcceptsPillows || !_placeableGaps.Contains(gap) || !gap.Model.CanPlace(slot))
                return;

            gap.Place(slot, _queue.Current);
            _queue.Advance(_placeableModels);
        }

        private void HandleGapCrossed(Gap gap)
        {
            RefreshPlaceableGaps();
            _queue.EnsureCurrentIsPlaceable(_placeableModels);
        }

        private void HandleCatFell(Gap gap)
        {
            _state = GameState.Failed;
            gap.Collapse();
            EndInput();
            resultPanel.Show(failTitle, failMessage);
            StartCoroutine(RestartRoutine());
        }

        private void HandleGoalReached()
        {
            _state = GameState.Won;
            EndInput();
            LevelProgress.MarkCompleted(_levelData);

            if (catalog.HasLevelAfter(_levelIndex))
            {
                resultPanel.Show(winTitle, $"{_levelData.DisplayName} cleared!",
                    new ButtonAction(nextLevelLabel, PlayNextLevel),
                    new ButtonAction(menuLabel, GoToMenu));
            }
            else
            {
                resultPanel.Show(finalWinTitle, finalWinMessage,
                    new ButtonAction(menuLabel, GoToMenu),
                    new ButtonAction(replayLabel, RestartLevel));
            }
        }

        private void PlayNextLevel()
        {
            LevelSession.Select(_levelIndex + 1);
            RestartLevel();
        }

        private void GoToMenu() => SceneManager.LoadScene(catalog.MainMenuSceneName);

        private IEnumerator RestartRoutine()
        {
            yield return new WaitForSeconds(settings.RestartDelay);
            RestartLevel();
        }

        private void EndInput()
        {
            queueView.SetInteractable(false);
            foreach (Gap gap in _level.Gaps)
                gap.SetPlaceable(false);
        }

        private static void RestartLevel() =>
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        /// <summary>The first N gaps the cat has not reached yet accept pillows.</summary>
        private void RefreshPlaceableGaps()
        {
            _placeableGaps.Clear();
            _placeableModels.Clear();

            foreach (Gap gap in _level.Gaps)
            {
                bool isPlaceable = !gap.Model.IsLocked && _placeableGaps.Count < settings.PlaceableGapCount;
                if (isPlaceable)
                {
                    _placeableGaps.Add(gap);
                    _placeableModels.Add(gap.Model);
                }
                gap.SetPlaceable(isPlaceable);
            }
        }
    }
}
