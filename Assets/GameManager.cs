using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using TMPro;
using Cinemachine;
using UnityEngine.UI;
using DG.Tweening;

public enum EnergyScales
{
    Small,
    Regular,
    Big
}

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private TMP_Text _gameOverTXT, _timeTrackingTXT;
    [SerializeField]
    private Image _transitionPanel;
    private CinemachineImpulseSource _impulseSource;
    [SerializeField]
    private Texture2D _cursor;
    private bool _isRunning = false;
    private float _currentTime = 0f;
    [SerializeField]
    private GameObject _endingPanel;
    [SerializeField]
    private PlayerController _playerCtrl;

    public event Action<EnergyScales> OnEnergyCollected;
    public void EnergyCollected(EnergyScales energyScale)
    {
        OnEnergyCollected?.Invoke(energyScale);
    }

    private void Start()
    {
        OnEnergyCollected += OnNewEnergyCollected;
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        Cursor.SetCursor(_cursor, Vector2.zero, CursorMode.Auto);
        StartTimer();
    }

    private void OnNewEnergyCollected(EnergyScales energyScale)
    {
        _impulseSource.GenerateImpulse(.4f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
        if (_isRunning)
        {
            _currentTime += Time.deltaTime;
        }
    }

    public void GameOver()
    {
        _transitionPanel.DOFade(1, .5f).OnComplete(() =>
        {
            _gameOverTXT.gameObject.SetActive(true);
            DOVirtual.DelayedCall(2f, () =>
            {
                ResetGame();
            });
        });
    }

    private void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ScreenShake(float force = .4f)
    {
        _impulseSource.GenerateImpulse(force);
    }

    public void FinishCurrentLevel()
    {
        _endingPanel.SetActive(true);
        PauseTimer();
        _timeTrackingTXT.text = GetFormattedTimeTracking();
        _timeTrackingTXT.rectTransform.DOShakeScale(.1f, .4f);
        _transitionPanel.DOFade(1, .5f);
        _playerCtrl.KillPlayer();
    }

    private string GetFormattedTimeTracking()
    {
        int minutes = Mathf.FloorToInt(_currentTime / 60f);
        float seconds = _currentTime % 60f;
        return string.Format("{0:00}.{1:00}", minutes, Mathf.Floor(seconds));
    }

    public void StartTimer()
    {
        _isRunning = true;
    }

    public void PauseTimer()
    {
        _isRunning = false;
    }

    public void ResetTimer()
    {
        _currentTime = 0f;
        _isRunning = false;
    }
}
