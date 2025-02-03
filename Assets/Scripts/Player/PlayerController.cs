using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector3 _playerMovementInput;
    private Vector2 _playerMouseInput;
    private float _xRotation;
    private Vector3 _headStartPosition;
    private float _bobTimer = 0f;
    private float _currentTilt = 0f;
    private float _tiltVelocity = 0f;
    private bool _footstepPlayedCycle = false;

    [Header("Player Components")]
    [SerializeField] private Transform _playerHead;
    [SerializeField] private Transform _playerCamera;
    [SerializeField] private Rigidbody _playerBody;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundMask;

    [Header("Movement Settings")]
    [SerializeField, Range(0.5f, 5f)] private float _speed;
    [SerializeField, Range(0.5f, 4f)] private float _jumpForce;

    [Header("Camera Settings")]
    [Range(0f, 10f)] public float sensitivity;
    [SerializeField, Range(0.015f, 0.1f)] private float _bobAmplitude;
    [SerializeField, Range(1.5f, 10f)] private float _bobFrequency;
    [SerializeField, Range(1.5f, 10f)] private float _tiltMultiplier;
    [SerializeField, Range(0.01f, 0.05f)] private float _idleBreathAmplitude;
    [SerializeField, Range(0.5f, 2f)] private float _idleBreathFrequency;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip _footstepAudioClip;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _headStartPosition = _playerHead.localPosition;
    }

    private void Update()
    {
        _playerMovementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        _playerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        HandleMove();
        HandleLook();
        HandleCameraBob();
    }

    private void HandleMove()
    {
        Vector3 moveVector = transform.TransformDirection(_playerMovementInput) * _speed;
        _playerBody.linearVelocity = new Vector3(moveVector.x, _playerBody.linearVelocity.y, moveVector.z);
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
            _playerBody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }

    private void HandleLook()
    {
        _xRotation -= _playerMouseInput.y * sensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        transform.Rotate(0f, _playerMouseInput.x * sensitivity, 0f);
        float targetTilt = -_playerMouseInput.x * _tiltMultiplier;
        _currentTilt = Mathf.SmoothDamp(_currentTilt, targetTilt, ref _tiltVelocity, 0.1f);
        _playerHead.localRotation = Quaternion.Euler(_xRotation, 0f, _currentTilt);
    }

    private void HandleCameraBob()
    {
        if (!IsGrounded())
        {
            ResetCameraPosition();
            return;
        }

        if (IsMoving())
        {
            _bobTimer += Time.deltaTime;
            float bobOffsetY = Mathf.Sin(_bobTimer * _bobFrequency) * _bobAmplitude;
            Vector3 targetPosition = _headStartPosition + new Vector3(0f, bobOffsetY, 0f);
            _playerHead.localPosition = Vector3.Lerp(_playerHead.localPosition, targetPosition, Time.deltaTime * 10f);

            if (bobOffsetY < -_bobAmplitude * 0.9f && !_footstepPlayedCycle)
            {
                SoundFXManager.Instance.PlaySoundFXClip(_footstepAudioClip, transform, 1f);
                _footstepPlayedCycle = true;
            }

            if (bobOffsetY > 0f)  _footstepPlayedCycle = false;
        }
        else
        {
            _bobTimer += Time.deltaTime;
            float idleOffsetY = Mathf.Sin(_bobTimer * _idleBreathFrequency) * _idleBreathAmplitude;
            Vector3 targetPosition = _headStartPosition + new Vector3(0f, idleOffsetY, 0f);
            _playerHead.localPosition = Vector3.Lerp(_playerHead.localPosition, targetPosition, Time.deltaTime * 2f);
        }
    }

    private void ResetCameraPosition()
    {
        _bobTimer = 0f;
        _playerHead.localPosition = Vector3.Lerp(_playerHead.localPosition, _headStartPosition, Time.deltaTime * 2f);
    }

    // Utility Functions
    private bool IsMoving()
    {
        return _playerMovementInput.magnitude > 0.1f;
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(_groundCheck.position, 0.1f, _groundMask);
    }
}
