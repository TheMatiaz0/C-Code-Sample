using System;
using Telegraphist.Helpers;
using Telegraphist.Helpers.Provider;
using Telegraphist.Input;
using Telegraphist.Lifecycle;
using Telegraphist.Utils;
using UniRx;
using UnityEngine;

namespace Telegraphist.Gameplay.QTE.Frequency
{
    public sealed class KnobInputController : SongBehaviour, IInjectable
    {
        private const float RotationsSnapThreshold = 0.1f;

        [SerializeField] private Transform center;
        [SerializeField] private bool restrictBounds = true;
        [SerializeField] private Collider2D bounds;
        [SerializeField] private bool initialAllowRotation = true;
        [SerializeField] private bool debug;
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip rotatingClip;
       
        private float angle;
        private Vector2 previousMouseOrJoystickPosition = Vector2.up;
        private readonly Subject<float> angleSubject = new();
        
        private bool IsInitialRotation => previousMouseOrJoystickPosition == Vector2.up;
        private int MinRotations => Mathf.FloorToInt(FrequencyController.Current.MaxFrequency / 2f) * -1;
        private int MaxRotations => MinRotations + FrequencyController.Current.MaxFrequency;
        public int MinAngle => MinRotations * 360;
        public int MaxAngle => MaxRotations * 360;

        public IObservable<float> OnAngleChange => angleSubject.AsObservable();
        public IObservable<float> OnRotationsChange => OnAngleChange.Select(x => x / 360f);
        public IObservable<int> OnRotationsChangeSnapped => OnRotationsChange
            .Where(x => Mathf.Abs(x - Mathf.RoundToInt(x)) <= RotationsSnapThreshold)
            .Select(x => Mathf.RoundToInt(x))
            .DistinctUntilChanged();
        public IObservable<int> OnRotationsChangeFromZeroSnapped => OnRotationsChangeSnapped
            .Select(x => x - MinRotations);
        public bool AllowRotation { get; set; } = true;

        public float Angle
        {
            get => angle;
            set
            {
                angle = Mathf.Clamp(value, MinAngle - 359.999f, MaxAngle + 359.999f);
                angleSubject.OnNext(angle);

                Log($"angle = {angle}, min = {MinAngle}, max = {MaxAngle}");
            }
        }
        /// <summary>
        /// Rotations == 0 corresponds to middle Frequency
        /// </summary>
        public float Rotations
        {
            get => Angle / 360f;
            set => Angle = value * 360f;
        }
        /// <summary>
        /// RotationsFromZero == 0 corresponds to Frequency == 0
        /// </summary>
        public float RotationsFromZero
        {
            get => Rotations - MinRotations;
            set
            {
                Log($"rotations from zero = {value}, rotations = {value + MinRotations}");
                Rotations = value + MinRotations;
            }
        }

        private void Start()
        {
            GameInputHandler.Current.OnKnobRotated
                .Where(x => x != Vector2.zero && AllowRotation)
                .Subscribe(OnKnobRotate).AddTo(this);
            GameInputHandler.Current.OnKnobRotated
                .Where(x => x == Vector2.zero)
                .Subscribe(_ => OnKnobEndRotate()).AddTo(this);
            
            audioSource.clip = rotatingClip;
            audioSource.loop = true;
        }
        
        protected override void OnSongStart()
        {
            AllowRotation = initialAllowRotation;
            previousMouseOrJoystickPosition = Vector2.up;
            RotationsFromZero = FrequencyController.Current.Frequency;
        }

        private void OnKnobRotate(Vector2 position)
        {
            var isGamepad = position.magnitude <= 1;
            
            float angleDelta;
            var canRotate = isGamepad
                ? TryOnKnobRotateGamepad(position, out angleDelta) : TryOnKnobRotateMouse(position, out angleDelta);
            
            if (!canRotate) return;
            
            Angle += angleDelta;

            if (Mathf.Abs(angleDelta) > 3f)
            {
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }

        private bool TryOnKnobRotateMouse(Vector2 mousePosition, out float angleDelta)
        {
            angleDelta = 0;
            
            var worldSpaceMousePosition = WorldPositionUtils.ScreenToWorldPosition2D(mousePosition, CameraManager.Current.MainCamera);

            if (restrictBounds && !bounds.OverlapPoint(worldSpaceMousePosition))
            {
                return false;
            }

            var relativePosition = worldSpaceMousePosition - (Vector2)center.position;

            if (IsInitialRotation)
            {
                previousMouseOrJoystickPosition = relativePosition;
                return false;
            }

            angleDelta = Vector2.SignedAngle(previousMouseOrJoystickPosition, relativePosition);
            
            previousMouseOrJoystickPosition = relativePosition;

            return true;
        }

        private bool TryOnKnobRotateGamepad(Vector2 position, out float angleDelta)
        {
            angleDelta = Vector2.SignedAngle(previousMouseOrJoystickPosition, position);
            previousMouseOrJoystickPosition = position;
            
            return !IsInitialRotation;
        }

        private void OnKnobEndRotate()
        {
            previousMouseOrJoystickPosition = Vector2.up;
            audioSource.Stop();
        }

        private void Log(object message)
        {
            if (!debug) return;
            Debug.Log(message);
        }
    }
}
