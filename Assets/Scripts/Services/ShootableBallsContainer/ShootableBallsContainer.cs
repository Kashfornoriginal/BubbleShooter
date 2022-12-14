using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class ShootableBallsContainer : IShootableBallsContainer
{
    public event UnityAction<Color, int> NextBallColorChanged;

    private List<Ball> _balls = new List<Ball>();

    public IReadOnlyList<Ball> Balls
    {
        get => _balls;
    }

    public void RegisterBall(Ball instance)
    {
        _balls.Add(instance);
    }

    public bool CanTakeCurrentBall()
    {
        if (_balls.Count <= 0)
        {
            return false;
        }

        return true;
    }

    public Ball GetCurrentBall()
    {
        if (_balls.Count <= 1)
        {
            NextBallColorChanged?.Invoke(new Color(0,0,0,0), _balls.Count - 1);
            return _balls[0];
        }
        
        NextBallColorChanged?.Invoke(_balls[1].Color, _balls.Count - 1);
        return _balls[0];
    }

    public void DeleteBall(Ball instance)
    {
        if (!_balls.Contains(instance))
        {
            Debug.LogError("There is no instance in list");
        }
        
        _balls.Remove(instance);
    }

    public void DeleteAllBalls()
    {
        _balls.Clear();
    }
}