using System;
using CatsVsPillows.Core;
using CatsVsPillows.Data;
using CatsVsPillows.Level;
using UnityEngine;

namespace CatsVsPillows.Gameplay
{
    /// <summary>Scene representation of a <see cref="GapModel"/>: pillow visuals and empty-slot markers.</summary>
    public sealed class Gap : MonoBehaviour
    {
        private const float ClashPopDelay = 0.22f;
        private const float CollapsePopInterval = 0.08f;

        private GapModel _model;
        private LevelLayout _layout;
        private GameplaySettings _settings;
        private PillowPalette _palette;
        private Pillow[] _pillows;
        private GameObject[] _markers;
        private bool _isPlaceable;

        public event Action<Gap, PlacementResult> PillowPlaced;

        public int Index { get; private set; }
        public GapModel Model => _model;
        public float LeftEdgeX { get; private set; }
        public float RightEdgeX { get; private set; }

        public void Initialize(int index, LevelLayout.Span span, LevelLayout layout, GameplaySettings settings, PillowPalette palette)
        {
            Index = index;
            LeftEdgeX = span.Start;
            RightEdgeX = span.End;
            _layout = layout;
            _settings = settings;
            _palette = palette;
            _model = new GapModel(layout.SlotsPerGap);
            _pillows = new Pillow[layout.SlotsPerGap];
            _markers = new GameObject[layout.SlotsPerGap];

            for (int slot = 0; slot < _markers.Length; slot++)
                _markers[slot] = CreateMarker(slot);

            RefreshMarkers();
        }

        public Vector3 GetSlotPosition(int slot) => transform.position + _layout.GetSlotLocalPosition(slot);

        /// <summary>Places a level's starting pillow without animation. Returns false if it does not fit.</summary>
        public bool TryPlaceInitial(int slot, PillowColor color)
        {
            if (slot < 0 || slot >= _model.SlotCount || !_model.CanPlace(slot) || _model.WouldClash(slot, color))
                return false;

            _model.Place(slot, color);
            SpawnPillow(slot, color);
            RefreshMarkers();
            return true;
        }

        public PlacementResult Place(int slot, PillowColor color)
        {
            PlacementResult result = _model.Place(slot, color);

            Pillow placed = SpawnPillow(slot, color);
            placed.PlayDropIn();

            foreach (int explodedSlot in result.ExplodedSlots)
                PopPillow(explodedSlot, ClashPopDelay);

            RefreshMarkers();
            PillowPlaced?.Invoke(this, result);
            return result;
        }

        /// <summary>Pops every pillow in the gap, bottom to top (used when the cat falls through).</summary>
        public void Collapse()
        {
            ClearClashWarning();
            float delay = 0f;
            foreach (int slot in _model.ClearAll())
            {
                PopPillow(slot, delay);
                delay += CollapsePopInterval;
            }
            RefreshMarkers();
        }

        public void Lock()
        {
            _model.Lock();
            SetPlaceable(false);
        }

        public void SetPlaceable(bool isPlaceable)
        {
            _isPlaceable = isPlaceable;
            RefreshMarkers();
        }

        /// <summary>Wobbles the pillows that would explode if <paramref name="color"/> went into <paramref name="slot"/>.</summary>
        public void ShowClashWarning(int slot, PillowColor color)
        {
            for (int i = 0; i < _pillows.Length; i++)
            {
                bool isNeighbor = Mathf.Abs(i - slot) == 1;
                if (_pillows[i] != null)
                    _pillows[i].SetWarning(isNeighbor && _pillows[i].Color == color);
            }
        }

        public void ClearClashWarning()
        {
            foreach (Pillow pillow in _pillows)
            {
                if (pillow != null)
                    pillow.SetWarning(false);
            }
        }

        private Pillow SpawnPillow(int slot, PillowColor color)
        {
            Pillow pillow = Instantiate(_settings.PillowPrefab, GetSlotPosition(slot), Quaternion.identity, transform);
            pillow.name = $"Pillow {slot} ({color})";
            pillow.SetColor(color, _palette.GetMaterial(color));
            _pillows[slot] = pillow;
            return pillow;
        }

        private void PopPillow(int slot, float delay)
        {
            Pillow pillow = _pillows[slot];
            if (pillow == null)
                return;

            pillow.Pop(_settings.PillowPopVfx, _palette.GetUiColor(pillow.Color), delay);
            _pillows[slot] = null;
        }

        private GameObject CreateMarker(int slot)
        {
            if (_settings.SlotMarkerPrefab == null)
                return null;

            GameObject marker = Instantiate(_settings.SlotMarkerPrefab, GetSlotPosition(slot), Quaternion.identity, transform);
            marker.name = $"Slot Marker {slot}";
            return marker;
        }

        private void RefreshMarkers()
        {
            for (int slot = 0; slot < _markers.Length; slot++)
            {
                if (_markers[slot] != null)
                    _markers[slot].SetActive(_isPlaceable && _model.CanPlace(slot));
            }
        }
    }
}
