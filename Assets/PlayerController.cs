using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    private readonly float _aimOffset = 90f;
    private Rigidbody2D _rb;
    [SerializeField]
    private float _impulseForce = 5f, _rotationSpeed = 10f;
    private List<Vector2> _spaceshipScales = new();
    private int _currentShipScale = 1;
    private float _currentSpeed = 0;

    [SerializeField]
    private ParticleSystem _changeScaleFX, _shipExplosionFX;
    [SerializeField]
    private List<SpriteRenderer> _healthBars = new();

    private SpriteRenderer spriteRenderer;
    private float flashDuration = 1f; // Duration of the flash in seconds
    private int flashCount = 3; // Number of times to flash
    private float flashAlpha = 0f;

    private bool _isDead;

    [SerializeField]
    private Cinemachine.CinemachineVirtualCamera _mainCam;
    private float _defaultCamZoom;

    void Start()
    {
        _defaultCamZoom = _mainCam.m_Lens.OrthographicSize;
        _rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameManager.Instance.OnEnergyCollected += OnEnergyCollected;
        FillSpaceshipSizes();
    }

    private void OnEnergyCollected(EnergyScales energyScale)
    {
        switch (energyScale)
        {
            case EnergyScales.Small:
                UpdateShipSize(_spaceshipScales[0], 0);
                break;
            case EnergyScales.Regular:
                UpdateShipSize(_spaceshipScales[1], 1);
                break;
            case EnergyScales.Big:
                UpdateShipSize(_spaceshipScales[2], 2);
                break;
        }
    }

    void Update()
    {
        if (_isDead) return;
        _currentSpeed = _rb.velocity.magnitude;
        ScaleResizeController();

        if (Input.GetMouseButton(0))
        {
            _rb.bodyType = RigidbodyType2D.Static;
            RotateTowardMouse();
        }
        if (Input.GetMouseButtonUp(0))
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;
            SoundManager.Instance.PlayImpulse();
            ApplyImpulseTowardsMouse();
        }
    }

    private void FillSpaceshipSizes()
    {
        _spaceshipScales.Add(SmallSize());
        _spaceshipScales.Add(RegularSize());
        _spaceshipScales.Add(BigSize());
    }

    private void RotateTowardMouse()
    {
        // Get the world position of the mouse cursor
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Calculate the direction from the GameObject to the mouse cursor
        Vector2 direction = (mousePosition - transform.position).normalized;

        // Calculate the angle required to rotate the GameObject to face the mouse cursor
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Calculate the target rotation with the aim offset
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle - _aimOffset));

        // Smoothly interpolate the current rotation to the target rotation using Lerp (or Slerp for spherical interpolation)
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
    }


    void ApplyImpulseTowardsMouse()
    {
        // Get the world position of the mouse cursor
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Calculate the direction from the GameObject to the mouse cursor
        Vector2 direction = (mousePosition - transform.position).normalized;

        // Apply an impulse force in the direction of the mouse position
        _rb.AddForce(direction * _impulseForce, ForceMode2D.Impulse);
        //transform.DOShakeScale(.2f, .5f);
    }

    private void ScaleResizeController()
    {
        //if (Input.GetKeyDown(KeyCode.LeftShift))
        //{
        //    var spaceCount = _spaceshipScales.Count - 1;
        //    _currentShipScale++;
        //    _currentShipScale = _currentShipScale > spaceCount ? spaceCount : _currentShipScale;
        //    UpdateShipSize(_spaceshipScales[_currentShipScale], _currentShipScale);
        //}

        //if (Input.GetKeyDown(KeyCode.LeftControl))
        //{
        //    _currentShipScale--;
        //    _currentShipScale = _currentShipScale <= 0 ? 0 : _currentShipScale;
        //    UpdateShipSize(_spaceshipScales[_currentShipScale], _currentShipScale);
        //}
    }

    private void UpdateShipSize(Vector2 size, int index)
    {
        SoundManager.Instance.PlayChangeSize();
        _changeScaleFX.Play();
        transform.localScale = size;
        if (index == 0)
        {
            _rb.mass = 1;
            _rb.drag = .3f;
            _mainCam.m_Lens.OrthographicSize = 3f;
        }
        if (index == 1)
        {
            _rb.mass = 1.5f;
            _rb.drag = .5f;
            _mainCam.m_Lens.OrthographicSize = _defaultCamZoom;
        }
        if (index == 2)
        {
            _rb.mass = 2f;
            _rb.drag = 1.5f;
            _mainCam.m_Lens.OrthographicSize = 5f;
        }
    }

    private Vector2 RegularSize()
    {
        return new Vector2(0.4f, 0.4f);
    }

    private Vector2 SmallSize()
    {
        return new Vector2(0.2f, 0.2f);
    }

    private Vector2 BigSize()
    {
        return new Vector2(.6f, .6f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        print($"Current Speed: {_currentSpeed}");
        if (_currentSpeed > 1.5f)
        {
            var idx = _healthBars.Count - 1;
            var health = _healthBars[_healthBars.Count - 1];
            _healthBars.RemoveAt(idx);
            Destroy(health.gameObject);
            if (_healthBars.Count == 0)
            {
                GameOver();
            }
            TakeDamage();
        }
    }

    public void TakeDamage()
    {
        GameManager.Instance.ScreenShake(1f);
        SoundManager.Instance.PlayHitShip();
        _healthBars.ForEach((sprite) =>
        {
            StartCoroutine(FlashRoutine(sprite));
        });
        StartCoroutine(FlashRoutine(spriteRenderer));
    }

    private IEnumerator FlashRoutine(SpriteRenderer sprite)
    {
        Color originalColor = sprite.color; // Store the original color

        for (int i = 0; i < flashCount; i++)
        {
            // Change the sprite transparency to the flash alpha
            sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, flashAlpha);

            // Wait for half the flash duration
            yield return new WaitForSeconds(flashDuration / (flashCount * 2));

            // Restore the sprite transparency to the original alpha
            sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a);

            // Wait for the other half of the flash duration
            yield return new WaitForSeconds(flashDuration / (flashCount * 2));
        }
    }

    private void GameOver()
    {
        _isDead = true;
        _shipExplosionFX.Play();
        GameManager.Instance.GameOver();
        SoundManager.Instance.PlayDeath();
    }

    public void KillPlayer()
    {
        _isDead = true;
        GetComponent<CircleCollider2D>().enabled = false;
    }
}
