using UnityEngine;

namespace CatsVsPillows.Gameplay
{
    /// <summary>Side-scrolling camera that tracks the target's X position. Height and angle stay fixed.</summary>
    public sealed class CameraFollow : MonoBehaviour
    {
        [Tooltip("Camera position relative to (target.x, 0, 0). A positive X looks ahead of the cat.")]
        [SerializeField] private Vector3 offset = new Vector3(5.2f, 3.9f, -13.5f);
        [SerializeField, Min(0f)] private float smoothTime = 0.35f;

        private Transform _target;
        private Vector3 _velocity;

        public void SetTarget(Transform target, bool snap)
        {
            _target = target;
            if (snap && target != null)
                transform.position = DesiredPosition();
        }

        private void LateUpdate()
        {
            if (_target == null)
                return;

            transform.position = Vector3.SmoothDamp(transform.position, DesiredPosition(), ref _velocity, smoothTime);
        }

        private Vector3 DesiredPosition() => new Vector3(_target.position.x, 0f, 0f) + offset;
    }
}
