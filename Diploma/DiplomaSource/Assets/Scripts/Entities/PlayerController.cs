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

    //Инициализация при запуске сцены
    private void Start()
    {
        StartCoroutine(GlobalInfo.TimeCount());

        _characterController = GetComponent<CharacterController>();
        _camera = GameObject.Find("Main Camera").transform;

        _currentSpeed = Speed;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //На каждом кадре
    private void Update()
    {
        //Если нажата клавиша Escape, то открыть или закрыть меню паузы
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (Menu.gameObject.activeSelf) {
                Menu.Close();
            } else {
                Menu.gameObject.SetActive(true);
                Menu.Open();
            }
        }

        //Если открыто меню, ничего не делать
        if (Menu.gameObject.activeSelf) return;

        //Вращение камеры
        _rotationX += -Input.GetAxis("Mouse Y") * RotationSpeed;
        _rotationX = Mathf.Clamp(_rotationX, -XRotationLimit, XRotationLimit);
        _camera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * RotationSpeed, 0);

        //Прыжок
        if (Input.GetKeyDown(KeyCode.Space) && _characterController.isGrounded) Jump();
        //Включение и отключение спринта
        if (Input.GetKeyDown(KeyCode.LeftShift)) StartSprint();
        else if (Input.GetKeyUp(KeyCode.LeftShift)) StopSprint();
    }

    //Каждые 20 миллисекунд
    private void FixedUpdate()
    {
        //Применить гравитацию
        Gravity();

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        //Изменение направления движения с помощью клавиатуры
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || !_characterController.isGrounded)
            _direction = (forward * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal")).normalized * (_currentSpeed * Time.fixedDeltaTime);
        else _direction = Vector3.zero;
        //Изменение позиции исходя из направления и притяжения
        _characterController.Move(_direction + Vector3.up * _verticalSpeed);
    }

    //Функция притяжения
    private void Gravity()
    {
        if (!_characterController.isGrounded)
            _verticalSpeed += Physics.gravity.y * Time.fixedDeltaTime / 20 * GravityScale;
    }

    //Начало спринта
    private void StartSprint()
    {
        _currentSpeed *= SprintSpeedModifier;
    }

    //Конец спринта
    private void StopSprint()
    {
        _currentSpeed /= SprintSpeedModifier;
    }

    //Прыжок
    private void Jump()
    {
        _verticalSpeed = (JumpForce + Mathf.Abs(Physics.gravity.y) * Time.fixedDeltaTime / 100) * Time.fixedDeltaTime;
    }

    private void OnValidate()
    {
        _currentSpeed = Speed;
    }
}
