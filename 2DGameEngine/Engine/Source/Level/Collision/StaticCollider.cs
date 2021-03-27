﻿using MonolithEngine.Engine.Source.Entities;
using MonolithEngine.Engine.Source.Physics.Collision;
using MonolithEngine.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonolithEngine.Engine.Source.Level.Collision
{
    public class StaticCollider : GameObject
    {

        public StaticCollider(Vector2 gridPosition) : base(null)
        {
            Transform = new StaticTransform(this)
            {
                GridCoordinates = gridPosition
            };
            GridCollisionChecker.Instance.Add(this);
        }

        private HashSet<Direction> blockedFrom = new HashSet<Direction>();

        public bool BlocksMovement = true;

        public override void Destroy()
        {
            GridCollisionChecker.Instance.Remove(this);
        }

        public void AddBlockedDirection(Direction direction)
        {
            blockedFrom.Add(direction);
        }

        public void RemoveBlockedDirection(Direction direction)
        {
            blockedFrom.Remove(direction);
        }

        public bool IsBlockedFrom(Direction direction)
        {
            return blockedFrom.Count == 0 || blockedFrom.Contains(direction);
        }

        public bool BlocksMovementFrom(Direction direction)
        {
            return BlocksMovement && IsBlockedFrom(direction);
        }
    }
}
