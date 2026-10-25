using System;
using System.Collections.Generic;
using CatsVsPillows.Core;
using CatsVsPillows.Data;
using UnityEngine;

namespace CatsVsPillows.Gameplay
{
    /// <summary>
    /// Turns a screen-space drag into a slot choice. Shows a ghost pillow that snaps to the nearest empty
    /// slot of a placeable gap, and wobbles the pillows it would explode with. It never changes game state;
    /// it only raises <see cref="DropRequested"/>.
    /// </summary>
    public sealed class PillowDragController : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [Tooltip("Lifts the drag point above the finger (fraction of screen height) so the target stays visible.")]
        [SerializeField, Range(0f, 0.2f)] private float fingerLift = 0.06f;
        [Tooltip("Z offset of the ghost while it is not snapped, so it floats in front of the furniture.")]
        [SerializeField] private float freeGhostZ = -1.5f;

        private static readonly Plane DragPlane = new Plane(Vector3.back, Vector3.zero);

        private GameplaySettings _settings;
        private PillowPalette _palette;
        private Func<IReadOnlyList<Gap>> _placeableGaps;
        private Pillow _ghost;
        private PillowColor _color;
        private Gap _targetGap;
        private int _targetSlot = -1;

        public event Action<Gap, int> DropRequested;

        public bool IsDragging { get; private set; }

        public void Initialize(GameplaySettings settings, PillowPalette palette, Func<IReadOnlyList<Gap>> placeableGaps)
        {
            _settings = settings;
            _palette = palette;
            _placeableGaps = placeableGaps;
        }

        public void BeginDrag(PillowColor color, Vector2 screenPosition)
        {
            IsDragging = true;
            EnsureGhost();
            SetColor(color);
            _ghost.gameObject.SetActive(true);
            UpdateDrag(screenPosition);
        }

        /// <summary>Updates the ghost if the current pillow changed mid-drag.</summary>
        public void SetColor(PillowColor color)
        {
            _color = color;
            if (_ghost != null)
                _ghost.SetColor(color, _palette.GetMaterial(color));
        }

        public void UpdateDrag(Vector2 screenPosition)
        {
            if (!IsDragging)
                return;

            screenPosition.y += Screen.height * fingerLift;
            if (!TryGetPointerWorld(screenPosition, out Vector3 pointer))
                return;

            SetTarget(FindNearestSlot(pointer, out int slot), slot);

            _ghost.transform.position = _targetGap != null
                ? _targetGap.GetSlotPosition(_targetSlot)
                : new Vector3(pointer.x, pointer.y, freeGhostZ);
        }

        public void EndDrag(Vector2 screenPosition)
        {
            if (!IsDragging)
                return;

            UpdateDrag(screenPosition);
            Gap gap = _targetGap;
            int slot = _targetSlot;
            CancelDrag();

            if (gap != null)
                DropRequested?.Invoke(gap, slot);
        }

        public void CancelDrag()
        {
            IsDragging = false;
            SetTarget(null, -1);
            if (_ghost != null)
                _ghost.gameObject.SetActive(false);
        }

        private void SetTarget(Gap gap, int slot)
        {
            if (_targetGap != null && _targetGap != gap)
                _targetGap.ClearClashWarning();

            _targetGap = gap;
            _targetSlot = slot;

            if (gap != null)
                gap.ShowClashWarning(slot, _color);
        }

        private Gap FindNearestSlot(Vector3 pointer, out int nearestSlot)
        {
            Gap nearestGap = null;
            nearestSlot = -1;
            float bestDistance = _settings.SlotSnapRadius;

            foreach (Gap gap in _placeableGaps())
            {
                for (int slot = 0; slot < gap.Model.SlotCount; slot++)
                {
                    if (!gap.Model.CanPlace(slot))
                        continue;

                    float distance = Vector2.Distance(pointer, gap.GetSlotPosition(slot));
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        nearestGap = gap;
                        nearestSlot = slot;
                    }
                }
            }
            return nearestGap;
        }

        private bool TryGetPointerWorld(Vector2 screenPosition, out Vector3 world)
        {
            Ray ray = worldCamera.ScreenPointToRay(screenPosition);
            if (DragPlane.Raycast(ray, out float distance))
            {
                world = ray.GetPoint(distance);
                return true;
            }
            world = default;
            return false;
        }

        private void EnsureGhost()
        {
            if (_ghost != null)
                return;

            _ghost = Instantiate(_settings.PillowPrefab, transform);
            _ghost.name = "Drag Ghost";
        }
    }
}
