using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Events;
using KasherOriginal.Settings;
using Random = UnityEngine.Random;

public class CellsMatrixWatcher : ICellsMatrixWatcher
{
    public CellsMatrixWatcher(IBallTypesRelation ballTypesRelation, GameSettings gameSettings)
    {
        _ballTypesRelation = ballTypesRelation;
        _gameSettings = gameSettings;
    }
    public event Action BallOutOfBorder;
    
    public const int ROWS_COUNT = 14;
    public const int COLUMNS_COUNT = 18;
    
    private IBallTypesRelation _ballTypesRelation;
    private GameSettings _gameSettings;

    private CellTypeBehavior[] _cellTypes = new[]
    {
        CellTypeBehavior.R,
        CellTypeBehavior.G,
        CellTypeBehavior.B,
        CellTypeBehavior.Y,
        CellTypeBehavior.E
    };

    public Cell[,] Cells { get; private set; } = new Cell[ROWS_COUNT,COLUMNS_COUNT];

    public void CreateRandomField()
    {
        CreateEmptyFieldOfCells();

        int randomBallsAmount = Random.Range(_gameSettings.MinBallsAmount, _gameSettings.MaxBallsAmount);

        for (int i = 0; i < randomBallsAmount;)
        {
            int randomRow = Random.Range(1, ROWS_COUNT - 1);
            int randomColumn = Random.Range(0, COLUMNS_COUNT - 8);
            
            CellTypeBehavior randomCellType = _cellTypes[Random.Range(0, _cellTypes.Length)];

            if (randomCellType != CellTypeBehavior.E)
            {
                Cell cell = new Cell(randomCellType, null, randomRow, randomColumn);
                Cells[randomRow, randomColumn] = cell;
                i++;
            }
        }
    }

    public void SetLevelField(Cell[,] levelField)
    {
        Cells = levelField;
    }

    public void ChangeCellInfo(BallTypeBehavior ballType, BallSpriteBehavior ballSpriteBehavior, int positionX, int positionY)
    {
        Cells[positionX, positionY] = new Cell(_ballTypesRelation.GetCellTypeFromBallType(ballType), ballSpriteBehavior, positionX, positionY);
    }

    public void ProcessBallConnection(BallConnectionType connectionType, BallSpriteBehavior originalBallSpriteBehavior, BallSpriteBehavior shootedBallSpriteBehavior)
    {
        var originalCell = FindCellByBall(originalBallSpriteBehavior);
        
        if (originalCell != null)
        {
            var originalXPos = originalCell.XPosition;
            var originalYPos = originalCell.YPosition;

            var newCellType = _ballTypesRelation.GetCellTypeFromBallType(shootedBallSpriteBehavior.BallType);

            switch (connectionType)
            {
                case BallConnectionType.Up:
                    Cells[originalXPos, originalYPos - 1] = new Cell(newCellType, originalBallSpriteBehavior, originalXPos, originalYPos - 1);
                    FindAllNeighbors(shootedBallSpriteBehavior.BallType, originalXPos, originalYPos - 1);
                    break;
                case BallConnectionType.Down:
                    if (originalYPos + 1 >= COLUMNS_COUNT)
                    {
                        BallOutOfBorder?.Invoke();
                        return;
                    }

                    Cells[originalXPos, originalYPos + 1] = new Cell(newCellType, originalBallSpriteBehavior, originalXPos, originalYPos + 1);
                    FindAllNeighbors(shootedBallSpriteBehavior.BallType, originalXPos, originalYPos + 1);
                    break;
                case BallConnectionType.Left:
                    Cells[originalXPos - 1, originalYPos] = new Cell(newCellType, originalBallSpriteBehavior, originalXPos - 1, originalYPos);
                    FindAllNeighbors(shootedBallSpriteBehavior.BallType, originalXPos - 1, originalYPos);
                    break;
                case BallConnectionType.Right:
                    Cells[originalXPos + 1, originalYPos] = new Cell(newCellType, originalBallSpriteBehavior, originalXPos + 1, originalYPos);
                    FindAllNeighbors(shootedBallSpriteBehavior.BallType, originalXPos + 1, originalYPos);
                    break;
            }
            
            
        }
    }

    private void CreateEmptyFieldOfCells()
    {
        for (int x = 0; x < ROWS_COUNT; x++)
        {
            for (int y = 0; y < COLUMNS_COUNT; y++)
            {
                Cell cell = new Cell(CellTypeBehavior.E, null, x, y);
                Cells[x, y] = cell;
            }
        }
    }

    private Cell FindCellByBall(BallSpriteBehavior ballSpriteBehavior)
    {
        foreach (var cell in Cells)
        {
            if (cell.BallSpriteBehavior == ballSpriteBehavior)
            {
                return cell;
            }
        }

        return null;
    }

    private void FindAllNeighbors(BallTypeBehavior ballType, int xBallPosition, int yBallPosition)
    {
        List<Neighbor> uncheckedNeighbors = new List<Neighbor>();
        List<Neighbor> checkedNeighbors = new List<Neighbor>();

        var seltNeighbor = new Neighbor(Cells[xBallPosition, yBallPosition]);
        
        checkedNeighbors.Add(seltNeighbor);
        
        CheckForNearbyNeighbors(ballType, xBallPosition, yBallPosition, ref uncheckedNeighbors);

        while (uncheckedNeighbors.Count > 0)
        {
            CheckForNearbyNeighbors(ballType, uncheckedNeighbors[0].Cell.XPosition, uncheckedNeighbors[0].Cell.YPosition, ref uncheckedNeighbors);
            checkedNeighbors.Add(uncheckedNeighbors[0]);
            uncheckedNeighbors.Remove(uncheckedNeighbors[0]);
        }

        foreach (var neighbor in uncheckedNeighbors)
        {
            Debug.Log(neighbor.Cell.CellType);
        }
    }

    private void CheckForNearbyNeighbors(BallTypeBehavior ballType, int xBallPosition, int yBallPosition, ref List<Neighbor> uncheckedNeighbors)
    {
        for (int x = math.max(0, xBallPosition - 1); x <= math.min(xBallPosition + 1, ROWS_COUNT - 1); x++)
        {
            for (int y = math.max(0, yBallPosition - 1); y <= math.min(yBallPosition + 1, COLUMNS_COUNT - 1); y++)
            {
                if (x != xBallPosition && y != yBallPosition)
                {
                    if (Cells[x, y].CellType == _ballTypesRelation.GetCellTypeFromBallType(ballType))
                    {
                        var neighbor = new Neighbor(Cells[x, y]);

                        uncheckedNeighbors.Add(neighbor);
                    }
                }
            }
        }
    }
}

public struct Neighbor
{
    private Cell _cell;

    public Cell Cell => _cell;

    public Neighbor(Cell cell)
    {
        _cell = cell;
    }
}