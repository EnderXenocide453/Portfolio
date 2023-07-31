using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController _characterController;
    private Transform _camera;
    private GroundController _groundController;
    
    private Vector3 _direction = Vector3.zero;
    
    private float _rotationX;
    private float _verticalSpeed = 0;
    private float _currentSpeed = 1;

    private Transform _activeParticles;

    public float Speed = 5;
    public float SprintSpeedModifier = 1.6f;
    public float JumpForce = 4;
    public float RotationSpeed = 3;
    public float XRotationLimit = 60;
    public float GravityScale = 1;

    public GameObject magicParticles;

    public PauseMenu Menu;

    //������������� ��� ������� �����
    private void Start()
    {
        StartCoroutine(GlobalInfo.TimeCount());

        _characterController = GetComponent<CharacterController>();
        _camera = GameObject.Find("Main Camera").transform;

        _currentSpeed = Speed;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //�� ������ �����
    private void Update()
    {
        //���� ������ ������� Escape, �� ������� ��� ������� ���� �����
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (Menu.gameObject.activeSelf) {
                Menu.Close();
            } else {
                Menu.gameObject.SetActive(true);
                Menu.Open();
            }
        }

        //���� ������� ����, ������ �� ������
        if (Menu.gameObject.activeSelf) return;

        //�������� ������
        _rotationX += -Input.GetAxis("Mouse Y") * RotationSpeed;
        _rotationX = Mathf.Clamp(_rotationX, -XRotationLimit, XRotationLimit);
        _camera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * RotationSpeed, 0);

        //������
        if (Input.GetKeyDown(KeyCode.Space) && _characterController.isGrounded) Jump();
        //��������� � ���������� �������
        if (Input.GetKeyDown(KeyCode.LeftShift)) StartSprint();
        else if (Input.GetKeyUp(KeyCode.LeftShift)) StopSprint();
    }

    //������ 20 �����������
    private void FixedUpdate()
    {
        //��������� ����������
        Gravity();

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        //��������� ����������� �������� � ������� ����������
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || !_characterController.isGrounded)
            _direction = (forward * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal")).normalized * (_currentSpeed * Time.fixedDeltaTime);
        else _direction = Vector3.zero;
        //��������� ������� ������ �� ����������� � ����������
        _characterController.Move(_direction + Vector3.up * _verticalSpeed);
    }

    //������� ����������
    private void Gravity()
    {
        if (!_characterController.isGrounded)
            _verticalSpeed += Physics.gravity.y * Time.fixedDeltaTime / 20 * GravityScale;
    }

    //������ �������
    private void StartSprint()
    {
        _currentSpeed *= SprintSpeedModifier;
    }

    //����� �������
    private void StopSprint()
    {
        _currentSpeed /= SprintSpeedModifier;
    }

    //������
    private void Jump()
    {
        _verticalSpeed = (JumpForce + Mathf.Abs(Physics.gravity.y) * Time.fixedDeltaTime / 100) * Time.fixedDeltaTime;
    }

    private void OnValidate()
    {
        _currentSpeed = Speed;
    }
}
