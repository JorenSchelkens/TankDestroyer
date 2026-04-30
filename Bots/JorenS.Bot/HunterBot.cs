using TankDestroyer.API;

namespace JorenS.Bot;

[Bot("Hunter", "Joren", "CC0404")]
public class HunterBot : IPlayerBot
{
    private Random _random = new();
    private ITank? _tankToChase = null;

    public void DoTurn(ITurnContext context)
    {
        SetTankToChase(context);
        MoveTowardsTankToChase(context);
        RotateToDirectionOfTankToChase(context);
        context.Fire();
    }

    private void SetTankToChase(ITurnContext context)
    {
        if (_tankToChase is not null
           && !_tankToChase.Destroyed)
        {
            return;
        }

        var otherTanks = context.GetTanks().Where(v => v.OwnerId != context.Tank.OwnerId && !v.Destroyed).ToArray();
        _tankToChase = otherTanks[_random.Next(0, otherTanks.Length)];
    }

    private void MoveTowardsTankToChase(ITurnContext context)
    {
        if (_tankToChase is null)
        {
            return;
        }

        var xDifference = _tankToChase.X - context.Tank.X;
        var yDifference = _tankToChase.Y - context.Tank.Y;

        var newXPosition = context.Tank.X;
        var newYPosition = context.Tank.Y;
        Direction direction;

        switch (xDifference, yDifference)
        {
            case (_, < 0):
                direction = Direction.North;
                newYPosition -= 1;
                break;
            case (_, > 0):
                direction = Direction.South;
                newYPosition += 1;
                break;
            case ( < 0, _):
                direction = Direction.West;
                newXPosition += 1;
                break;
            case ( > 0, _):
                direction = Direction.East;
                newXPosition -= 1;
                break;
            default:
                direction = Direction.South;
                break;
        }

        while (context.GetTile(newYPosition, newXPosition).TileType == TileType.Water)
        {
            if (newYPosition != context.Tank.Y)
            {
                newYPosition = context.Tank.Y;
                switch (_tankToChase.X - newXPosition)
                {
                    case < 0:
                        newXPosition += 1;
                        direction = Direction.West;
                        break;
                    case > 0:
                        newXPosition -= 1;
                        direction = Direction.East;
                        break;
                }

                continue;
            }

            if (newXPosition != context.Tank.X)
            {
                newXPosition = context.Tank.X;
                switch (_tankToChase.Y - newYPosition)
                {
                    case < 0:
                        newYPosition -= 1;
                        direction = Direction.North;
                        break;
                    case > 0:
                        newYPosition += 1;
                        direction = Direction.South;
                        break;
                }

                continue;
            }

            break;
        }

        if (direction == Direction.North)
        {
            direction = Direction.South;
        }
        else if (direction == Direction.South)
        {
            direction = Direction.North;
        }
        else if (direction == Direction.East)
        {
            direction = Direction.West;
        }
        else if (direction == Direction.West)
        {
            direction = Direction.East;
        }

        context.MoveTank(direction);
        return;
    }

    private void RotateToDirectionOfTankToChase(ITurnContext context)
    {
        if (_tankToChase is null)
        {
            return;
        }

        var xDifference = _tankToChase.X - context.Tank.X;
        var yDifference = _tankToChase.Y - context.Tank.Y;

        var direction = (xDifference, yDifference) switch
        {
            (0, < 0) => TurretDirection.South,
            (> 0, < 0) => TurretDirection.SouthWest,
            (< 0, < 0) => TurretDirection.SouthEast,

            (0, > 0) => TurretDirection.North,
            (> 0, > 0) => TurretDirection.NorthWest,
            (< 0, > 0) => TurretDirection.NorthEast,

            (> 0, 0) => TurretDirection.West,
            (< 0, 0) => TurretDirection.East,

            _ => TurretDirection.South,
        };

        context.RotateTurret(direction);
    }
}