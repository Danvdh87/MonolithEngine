using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine2D.Engine.Source.Entities.Interfaces
{
    public interface IGridCollider
    {
        Vector2 GetInCellLocation();
        Vector2 GetGridCoord();
        Vector2 GetPosition();
        int GridCollisionPriority { get; set; }

        bool HasTag(string tag);

        float GetCollisionOffset(Direction direction);

        bool BlocksMovementFrom(Direction direction);
    }
}
