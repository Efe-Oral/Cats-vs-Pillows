using CatsVsPillows.Core;
using CatsVsPillows.Data;
using CatsVsPillows.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CatsVsPillows.UI
{
    /// <summary>
    /// Vertical pillow feed. Item 0 (bottom) is the current pillow and the only one that can be dragged;
    /// the items above it are previews. When a pillow is used, every preview slides down one step.
    /// </summary>
    public sealed class QueueView : MonoBehaviour
    {
        [Tooltip("Index 0 is the current (draggable) pillow; higher indexes are previews further up the feed.")]
        [SerializeField] private QueueItemView[] items;
        [Tooltip("Where a newly dealt pillow slides in from, relative to the top item's resting position.")]
        [SerializeField] private Vector2 newPillowEntryOffset = new Vector2(0f, 110f);

        private PillowQueueModel _queue;
        private PillowPalette _palette;
        private PillowDragController _dragController;
        private bool _isInteractable = true;

        private QueueItemView CurrentItem => items[0];

        public void Bind(PillowQueueModel queue, PillowPalette palette, PillowDragController dragController)
        {
            Unbind();
            _queue = queue;
            _palette = palette;
            _dragController = dragController;

            _queue.Changed += HandleQueueChanged;
            CurrentItem.DragStarted += HandleDragStarted;
            CurrentItem.Dragged += HandleDragged;
            CurrentItem.DragEnded += HandleDragEnded;
            for (int i = 1; i < items.Length; i++)
                items[i].Pressed += HandlePreviewPressed;

            Refresh();
        }

        public void SetInteractable(bool isInteractable)
        {
            _isInteractable = isInteractable;
            if (!isInteractable && _dragController.IsDragging)
            {
                _dragController.CancelDrag();
                CurrentItem.SetLifted(false);
            }
        }

        private void OnDestroy() => Unbind();

        private void Unbind()
        {
            if (_queue == null)
                return;

            _queue.Changed -= HandleQueueChanged;
            CurrentItem.DragStarted -= HandleDragStarted;
            CurrentItem.Dragged -= HandleDragged;
            CurrentItem.DragEnded -= HandleDragEnded;
            for (int i = 1; i < items.Length; i++)
                items[i].Pressed -= HandlePreviewPressed;
            _queue = null;
        }

        private void HandleQueueChanged(QueueChange change)
        {
            Refresh();

            if (change == QueueChange.Advanced)
                PlayAdvanceAnimation();
            else if (change == QueueChange.Rerolled)
                CurrentItem.PlayPunch();
        }

        private void Refresh()
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (i < _queue.Upcoming.Count)
                    items[i].Show(_palette.GetUiColor(_queue.Upcoming[i]));
                else
                    items[i].Hide();
            }

            if (_dragController.IsDragging)
                _dragController.SetColor(_queue.Current);
        }

        /// <summary>Each visible item slides in from the slot above it; the top item drops in from outside.</summary>
        private void PlayAdvanceAnimation()
        {
            int visible = Mathf.Min(items.Length, _queue.Upcoming.Count);
            for (int i = 0; i < visible; i++)
            {
                QueueItemView item = items[i];
                bool isTop = i == visible - 1;
                QueueItemView source = isTop ? item : items[i + 1];

                Vector2 from = isTop ? item.RestPosition + newPillowEntryOffset : source.RestPosition;
                float fromScale = source.RectTransform.rect.width / item.RectTransform.rect.width;
                item.PlaySlideIn(from, fromScale, isTop ? 0f : 1f);
            }
        }

        private void HandlePreviewPressed(QueueItemView preview)
        {
            if (_isInteractable)
                preview.PlayDenied();
        }

        private void HandleDragStarted(PointerEventData eventData)
        {
            if (!_isInteractable)
                return;

            CurrentItem.SetLifted(true);
            _dragController.BeginDrag(_queue.Current, eventData.position);
        }

        private void HandleDragged(PointerEventData eventData) => _dragController.UpdateDrag(eventData.position);

        private void HandleDragEnded(PointerEventData eventData)
        {
            CurrentItem.SetLifted(false);
            _dragController.EndDrag(eventData.position);
        }
    }
}
