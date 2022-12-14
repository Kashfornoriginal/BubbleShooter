using System;
using Zenject;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using KasherOriginal.Settings;
using Random = UnityEngine.Random;
using KasherOriginal.Factories.BallFactory;

public class BallSpawner : MonoBehaviour
{
    [Inject]
    public void Construct(IBallsFactory ballsFactory, GameSettings gameSettings,
        IShootableBallsContainer shootableBallsContainer)
    {
        _ballsFactory = ballsFactory;
        _gameSettings = gameSettings;
        _shootableBallsContainer = shootableBallsContainer;
    }

    public event UnityAction BallsAmountIsZero;

    [SerializeField] private BallColorDecorator[] _ballColorDecorators;

    [SerializeField] private Transform _spawnPosition;

    private IBallsFactory _ballsFactory;
    private GameSettings _gameSettings;
    private IShootableBallsContainer _shootableBallsContainer;
    
    private void Start()
    {
        CreateBallsForGame();
    }

    public void Construct(Transform ballSpawnPosition)
    {
        _spawnPosition = ballSpawnPosition;
    }

    public async Task<GameObject> CreateMovingBall()
    {
        if (!_shootableBallsContainer.CanTakeCurrentBall())
        {
            BallsAmountIsZero?.Invoke();
            return null;
        }
        
        if (!isActiveAndEnabled)
        {
            return null;
        }

        Ball ball = _shootableBallsContainer.GetCurrentBall();

        GameObject ballInstance = await _ballsFactory.CreateMovableInstance(_spawnPosition.position, ball);
        
        _shootableBallsContainer.DeleteBall(ball);

        return ballInstance;
    }

    public async Task<GameObject> CreateStaticBall(Vector2 position, BallTypeBehavior ballType)
    {
        if (!isActiveAndEnabled)
        {
            return null;
        }

        int decoratorPosition;

        for (decoratorPosition = 0; decoratorPosition < _ballColorDecorators.Length; decoratorPosition++)
        {
            if (_ballColorDecorators[decoratorPosition].BallType == ballType)
            {
                GameObject ballInstance = await _ballsFactory.CreateStaticInstance(position, _ballColorDecorators[decoratorPosition]);
                return ballInstance;
            }
        }

        return null;
    }

    private async void CreateBallsForGame()
    {
        for (int i = 0; i < _gameSettings.GameBallsAmount; i++)
        {
            var ball = await _ballsFactory.DecorateBall(_ballColorDecorators[Random.Range(0, _ballColorDecorators.Length)]);
            _shootableBallsContainer.RegisterBall(ball);
        }
    }
}
