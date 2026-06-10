using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GothicVampire.Player.Inputs
{
    /// <summary>
    /// Handles player camera movement, rotation, focus transitions, and zoom via Cinemachine.
    /// </summary>
    public class CameraMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineCamera _camera;
        [SerializeField] private CinemachineOrbitalFollow _orbitalFollow;
        [SerializeField] private CinemachineFollowZoom _followZoom;

        [Header("Camera Movement")]
        [SerializeField] private Vector2 _moveSpeedRange = new(5f, 25f);
        [Tooltip("1.0 = Linear, 2.0+ = Faster speed at high zoom")]
        [SerializeField] private float _zoomMoveSpeedCurve = 1.5f;
        private Vector2 _keyMoveInput = Vector2.zero;
        private Vector2 _edgeMoveInput = Vector2.zero;
        private Vector2 _currentMousePosition;

        [Header("Edge Scroll")]
        [Tooltip("1.0 = Linear, 2.0+ = Faster speed at further screen edge")]
        [SerializeField][Range(0.0f, 1.0f)] private float _screenBoundaryPercent = 0.05f; // Distance from the edge in percentage
        [SerializeField] private float _speedExponent = 2.0f; // Higher = steeper curve

        [Header("Rotation")]
        [SerializeField] private float _defaultRotation = 45f;
        [SerializeField] private float _rotAngleIncrement = 90f;

        [Header("Focus")]
        [SerializeField] private bool _focusOnEntity;
        [SerializeField] private float _focusSpeed = 25f;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed = 2.5f;
        [SerializeField] private float _zoomTransitionSpeed = 2.5f;
        // Limits based off CinemachineFollowZoom.Width
        //[SerializeField] private float _zoomWidthDefault = 15f;
        [SerializeField] private float _zoomMinWidthThreshold = 5f;
        [SerializeField] private float _zoomMaxWidthThreshold = 20f;
        [SerializeField] private Vector2 _zoomFOVRange = new(20f, 60f);

        private Vector3 _targetPosition;
        private bool _movingToTargetPos;
        private float _startZoom;
        private float _targetZoom;
        private float _targetZoomNormalized;
        private float _startTilt;
        private float _targetTilt;

        private bool _zoomTransitionOngoing = false;
        private float _zoomTime = 1.0f;

        public bool FocusOnEntityEnabled => _focusOnEntity;

        private void Start()
        {
            Assert.IsNotNull(_camera, "CinemachineCamera _camera is null!");
            Assert.IsNotNull(_orbitalFollow, "CinemachineOrbitalFollow _orbitalFollow is null!");
            Assert.IsNotNull(_followZoom, "CinemachineFollowZoom _followZoom is null!");

            _followZoom.FovRange = _zoomFOVRange;
            _targetZoom = Mathf.Lerp(_zoomMinWidthThreshold, _zoomMaxWidthThreshold, 0.5f);
            _targetZoomNormalized = _targetZoom / _zoomMaxWidthThreshold;
            _targetTilt = Mathf.Lerp(_orbitalFollow.VerticalAxis.Range.x, _orbitalFollow.VerticalAxis.Range.y, 0.5f);

            _followZoom.Width = _targetZoom;
            _orbitalFollow.VerticalAxis.Value = _targetTilt;

            transform.rotation = Quaternion.Euler(0, _defaultRotation, 0);
        }

        private void LateUpdate()
        {
            if (Application.isFocused == false) return;

            HandleSnapToPositionUpdate();
            HandleMovementUpdate();
            HandleZoomUpdate();
            HandleZoomTransitionUpdate();
        }

        #region Updates
        private void HandleSnapToPositionUpdate()
        {
            if (_movingToTargetPos)
            {
                // Move toward the target
                transform.position = Vector3.Lerp(
                    transform.position,
                    _targetPosition,
                    _focusSpeed * Time.deltaTime
                );

                // Check if we've reached the target
                if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
                {
                    transform.position = _targetPosition; // snap to exact
                    _movingToTargetPos = false;
                }
            }
        }

        private void HandleMovementUpdate()
        {
            if (_keyMoveInput != Vector2.zero)
            {
                _movingToTargetPos = false;
                Vector3 move = new(_keyMoveInput.x, 0f, _keyMoveInput.y);
                MoveCameraTarget(move);
            }
            else if (_movingToTargetPos == false && EventSystem.current.IsPointerOverGameObject() == false)
            {
                HandleEdgeInput();

                if (_edgeMoveInput != Vector2.zero)
                {
                    Vector3 move = new(_edgeMoveInput.x, 0f, _edgeMoveInput.y);
                    MoveCameraTarget(move);
                }
            }

        }

        private void HandleZoomUpdate()
        {
            if (Mouse.current == null) return;

            Vector2 scroll = Mouse.current.scroll.ReadValue();
            if (scroll.y == 0f) return;

            _targetZoom -= scroll.y * _zoomSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom, _zoomMinWidthThreshold, _zoomMaxWidthThreshold);
            _targetZoomNormalized = _targetZoom / _zoomMaxWidthThreshold;

            _targetTilt = Mathf.Lerp(_orbitalFollow.VerticalAxis.Range.x, _orbitalFollow.VerticalAxis.Range.y, _targetZoomNormalized);

            if (_zoomTransitionOngoing == false)
            {
                _startZoom = _followZoom.Width;
                _startTilt = _orbitalFollow.VerticalAxis.Value;
                _zoomTime = 0.0f;
            }

            _zoomTransitionOngoing = true;
        }

        private void HandleZoomTransitionUpdate()
        {

            if (_zoomTransitionOngoing == true)
            {
                _zoomTime += Time.deltaTime * _zoomTransitionSpeed;
                _followZoom.Width = Mathf.Lerp(_startZoom, _targetZoom, _zoomTime);

                float easedT = _zoomTime * _zoomTime;
                _orbitalFollow.VerticalAxis.Value = Mathf.Lerp(_startTilt, _targetTilt, easedT);

                if (_zoomTime >= 1.0f)
                {
                    _zoomTransitionOngoing = false;
                    _orbitalFollow.VerticalAxis.Value = _targetTilt;
                    _followZoom.Width = _targetZoom;
                }
            }
        }

        /// <summary>
        /// This can be improved further once we have the grid system and proper terrain to test smooth movement
        /// within the terrain/ground
        /// </summary>
        private void MoveCameraTarget(Vector3 move)
        {
            // Get the forward direction of the camera
            Vector3 cameraForward = _camera.transform.forward;

            // Flatten the forward direction to only consider the Y rotation
            Vector3 flattenedForward = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;

            // Get the right direction of the camera (also flattened)
            Vector3 cameraRight = _camera.transform.right;
            Vector3 flattenedRight = new Vector3(cameraRight.x, 0f, cameraRight.z).normalized;

            // Calculate the movement direction based on input
            Vector3 movement = flattenedForward * move.z + flattenedRight * move.x;

            if (movement.sqrMagnitude > 1f)
            {
                movement.Normalize();
            }

            // Scale Move Speed based on zoom normals
            float scaledMoveSpeed = GetZoomSpeedMultiplier();
            transform.position += scaledMoveSpeed * Time.deltaTime * movement;
        }
        #endregion

        #region Input System Context
        public void Move(InputAction.CallbackContext context)
        {
            _keyMoveInput = context.ReadValue<Vector2>();
        }

        public void MousePosition(InputAction.CallbackContext context)
        {
            _currentMousePosition = context.ReadValue<Vector2>();
        }

        // Note: Removed decided not to implement, retained in code in case.
        public void Rotate(InputAction.CallbackContext context)
        {
            //if (context.phase == InputActionPhase.Performed)
            //{
            //    Vector2 rot = context.ReadValue<Vector2>();
            //    bool clockwise = rot.y > 0;
            //    Rotate(clockwise);
            //}
        }
        #endregion

        public void HandleEdgeInput()
        {
            float normX = _currentMousePosition.x / Screen.width;
            float normY = _currentMousePosition.y / Screen.height;

            Vector2 moveDirection = Vector2.zero;
            bool isInsideWindow = normX >= 0 && normX <= 1 && normY >= 0 && normY <= 1;

            if (isInsideWindow)
            {
                // Horizontal Scaling
                if (normX < _screenBoundaryPercent)
                {
                    float rawWeight = 1f - (normX / _screenBoundaryPercent);
                    moveDirection.x -= Mathf.Pow(rawWeight, _speedExponent);
                }
                else if (normX > 1f - _screenBoundaryPercent)
                {
                    float rawWeight = (normX - (1f - _screenBoundaryPercent)) / _screenBoundaryPercent;
                    moveDirection.x += Mathf.Pow(rawWeight, _speedExponent);
                }

                // Vertical Scaling
                if (normY < _screenBoundaryPercent)
                {
                    float rawWeight = 1f - (normY / _screenBoundaryPercent);
                    moveDirection.y -= Mathf.Pow(rawWeight, _speedExponent);
                }
                else if (normY > 1f - _screenBoundaryPercent)
                {
                    float rawWeight = (normY - (1f - _screenBoundaryPercent)) / _screenBoundaryPercent;
                    moveDirection.y += Mathf.Pow(rawWeight, _speedExponent);
                }

                // Clamp the magnitude to 1.0f
                if (moveDirection.sqrMagnitude > 1f)
                {
                    moveDirection.Normalize();
                }
            }

            _edgeMoveInput = moveDirection;
        }

        private float GetZoomSpeedMultiplier()
        {
            float curvedT = Mathf.Pow(_targetZoomNormalized, _zoomMoveSpeedCurve);
            return Mathf.Lerp(_moveSpeedRange.x, _moveSpeedRange.y, curvedT);
        }

        #region Runtime Commands
        public void MoveToPosition(Vector3 newPosition)
        {
            // ToDo: Modify later when integrating terrain/ground layer
            _targetPosition = new(newPosition.x, transform.position.y, newPosition.z);
            _movingToTargetPos = true;
        }

        public void Rotate(bool rotateClockwise)
        {
            float curAngleRot = transform.rotation.eulerAngles.y;

            if (rotateClockwise)
            {
                curAngleRot += _rotAngleIncrement;
                _orbitalFollow.HorizontalAxis.Value += _rotAngleIncrement;
            }
            else
            {
                curAngleRot -= _rotAngleIncrement;
                _orbitalFollow.HorizontalAxis.Value -= _rotAngleIncrement;
            }

            transform.rotation = Quaternion.Euler(0, curAngleRot, 0);
            _orbitalFollow.HorizontalAxis.Value = _orbitalFollow.HorizontalAxis.Value % 360.0f;
        }

        public void CenterToMap()
        {
            MoveToPosition(new Vector3(12f, 0f, 12f));
        }
        #endregion
    }
}
