using System;
using System.Collections;
using System.Collections.Generic;
using CatsVsPillows.Level;
using CatsVsPillows.Utilities;
using UnityEngine;

namespace CatsVsPillows.Gameplay
{
    /// <summary>
    /// Walks the cat left to right at a constant speed. Each gap is checked when the cat's paws reach it:
    /// a complete gap is crossed, an incomplete one makes the cat fall.
    /// </summary>
    public sealed class CatController : MonoBehaviour
    {
        private enum State
        {
            Waiting,
            Walking,
            Falling,
            Finished
        }

        [Tooltip("Animator on the model. Its transform may be animated, so keep facing/scale on a parent pivot.")]
        [SerializeField] private Animator animator;
        [SerializeField] private string walkStateName = "Walk";
        [SerializeField] private string idleStateName = "Idle";
        [Tooltip("Ground distance (world units per second) the walk cycle covers at animator speed 1. " +
                 "The animator speed is derived from the level's walk speed so the paws don't slide.")]
        [SerializeField, Min(0.05f)] private float walkCycleGroundSpeed = 0.8f;
        [SerializeField] private Vector2 walkAnimationSpeedRange = new Vector2(0.3f, 2f);
        [SerializeField, Min(0f)] private float animationCrossFade = 0.15f;

        [Tooltip("How far in front of the pivot the front paws are. Gaps are checked when the paws reach them.")]
        [SerializeField, Min(0f)] private float pawOffset = 0.35f;

        [Header("Fall")]
        [SerializeField, Min(0.05f)] private float fallDuration = 0.75f;
        [SerializeField] private float fallSpinDegrees = -120f;

        private int _walkStateHash;
        private int _idleStateHash;
        private IReadOnlyList<Gap> _gaps;
        private float _walkSpeed;
        private float _goalX;
        private int _nextGapIndex;
        private State _state = State.Waiting;

        public event Action<Gap> GapCrossed;
        public event Action<Gap> Fell;
        public event Action ReachedGoal;

        private void Awake()
        {
            animator.applyRootMotion = false;
            _walkStateHash = Animator.StringToHash(walkStateName);
            _idleStateHash = Animator.StringToHash(idleStateName);
        }

        public void Initialize(LevelInstance level, float walkSpeed)
        {
            _gaps = level.Gaps;
            _walkSpeed = walkSpeed;
            _goalX = level.Layout.GoalX;
            _nextGapIndex = 0;
            transform.position = new Vector3(level.Layout.CatStartX, level.Layout.PlatformTopY, 0f);
            PlayIdle();
        }

        public void StartWalking()
        {
            if (_state != State.Waiting)
                return;

            _state = State.Walking;
            animator.speed = Mathf.Clamp(_walkSpeed / walkCycleGroundSpeed, walkAnimationSpeedRange.x, walkAnimationSpeedRange.y);
            animator.CrossFade(_walkStateHash, animationCrossFade);
        }

        private void Update()
        {
            if (_state != State.Walking)
                return;

            Vector3 position = transform.position;
            position.x += _walkSpeed * Time.deltaTime;
            transform.position = position;

            if (_nextGapIndex < _gaps.Count && position.x + pawOffset >= _gaps[_nextGapIndex].LeftEdgeX)
            {
                Gap gap = _gaps[_nextGapIndex];
                if (!gap.Model.IsComplete)
                {
                    StartCoroutine(FallRoutine(gap));
                    return;
                }

                gap.Lock();
                _nextGapIndex++;
                GapCrossed?.Invoke(gap);
            }

            if (position.x >= _goalX)
            {
                _state = State.Finished;
                PlayIdle();
                ReachedGoal?.Invoke();
            }
        }

        private void PlayIdle()
        {
            animator.speed = 1f;
            animator.CrossFade(_idleStateHash, animationCrossFade);
        }

        private IEnumerator FallRoutine(Gap gap)
        {
            _state = State.Falling;
            Fell?.Invoke(gap);

            Vector3 start = transform.position;
            Vector3 end = new Vector3((gap.LeftEdgeX + gap.RightEdgeX) * 0.5f, 0f, start.z);
            Quaternion startRotation = transform.rotation;

            for (float time = 0f; time < fallDuration; time += Time.deltaTime)
            {
                float t = time / fallDuration;
                transform.position = new Vector3(
                    Mathf.Lerp(start.x, end.x, Easing.OutQuad(t)),
                    Mathf.Lerp(start.y, end.y, Easing.InQuad(t)),
                    start.z);
                transform.rotation = startRotation * Quaternion.Euler(0f, 0f, fallSpinDegrees * Easing.InQuad(t));
                yield return null;
            }
            transform.position = end;
            animator.speed = 0f;
        }
    }
}
