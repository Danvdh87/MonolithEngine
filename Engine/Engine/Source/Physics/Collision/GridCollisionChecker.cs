﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MonolithEngine
{
    /// <summary>
    /// Class to check simple static grid collisions (environmental collisions).
    /// </summary>
    public class GridCollisionChecker
    {
        private Dictionary<Vector2, StaticCollider> objects = new Dictionary<Vector2, StaticCollider>();
        private Dictionary<StaticCollider, Vector2> objectPositions = new Dictionary<StaticCollider, Vector2>();

        private ICollection<Direction> whereToCheck;

        private List<(StaticCollider, Direction)> allCollisionsResult = new List<(StaticCollider, Direction)>();
        private List<Direction> tagCollisionResult = new List<Direction>();

        private static readonly List<Direction> basicDirections = new List<Direction>() { Direction.CENTER, Direction.WEST, Direction.EAST, Direction.NORTH, Direction.SOUTH };

        public GridCollisionChecker()
        {

        }

        /// <summary>
        /// Adds a new collider to the grid
        /// </summary>
        /// <param name="gameObject"></param>
        public void Add(StaticCollider gameObject)
        {
            objectPositions[gameObject] = gameObject.Transform.GridCoordinates;
            objects.Add(gameObject.Transform.GridCoordinates, gameObject);
        }

        /// <summary>
        /// Returns the collider from the grid on the current position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public StaticCollider GetColliderAt(Vector2 position)
        {
            if (!objects.ContainsKey(position))
            {
                return null;
            }
            return objects[position];
        }

        public List<Direction> CollidesWithTag(IGameObject entity, string tag, ICollection<Direction> directionsToCheck = null)
        {

            tagCollisionResult.Clear();

            whereToCheck = directionsToCheck ?? basicDirections;

            foreach (Direction direction in whereToCheck)
            {
                if (objects.ContainsKey(GetGridCoord(entity.Transform.GridCoordinates, direction)) && objects[GetGridCoord(entity.Transform.GridCoordinates, direction)].HasTag(tag)
                    && IsExactCollision(entity, direction))
                {
                     tagCollisionResult.Add(direction);
                }
            }
            
            return tagCollisionResult;
        }

        public List<(StaticCollider, Direction)> HasGridCollisionAt(IGameObject entity, ICollection<Direction> directionsToCheck = null)
        {

            allCollisionsResult.Clear();

            whereToCheck = directionsToCheck ?? basicDirections;

            foreach (Direction direction in whereToCheck)
            {
                if (objects.ContainsKey(GetGridCoord(entity.Transform.GridCoordinates, direction))
                    && IsExactCollision(entity, direction))
                {
                    allCollisionsResult.Add((objects[GetGridCoord(entity.Transform.GridCoordinates, direction)], direction));
                }
            }
            return allCollisionsResult;
            
        }

        /// <summary>
        /// Exact collision: whether the next grid coordinate has a collider AND the entity's grid coordinates is greater/small than the 
        /// configured collision offset.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private bool IsExactCollision(IGameObject entity, Direction direction)
        {
            if (direction == Direction.WEST)
            {
                return entity.Transform.InCellLocation.X <= (entity as Entity).GetCollisionOffset(direction);
            }
            else if (direction == Direction.EAST)
            {
                return entity.Transform.InCellLocation.X >= (entity as Entity).GetCollisionOffset(direction);
            }
            else if (direction == Direction.NORTH)
            {
                return entity.Transform.InCellLocation.Y <= (entity as Entity).GetCollisionOffset(direction);
            }
            else if (direction == Direction.SOUTH)
            {
                return entity.Transform.InCellLocation.Y >= (entity as Entity).GetCollisionOffset(direction);
            }
            else if (direction == Direction.NORTHWEST)
            {
                return IsExactCollision(entity, Direction.NORTH) && IsExactCollision(entity, Direction.WEST);
            }
            else if (direction == Direction.NORTHEAST)
            {
                return IsExactCollision(entity, Direction.NORTH) && IsExactCollision(entity, Direction.EAST);
            }
            else if (direction == Direction.SOUTHWEST)
            {
                return IsExactCollision(entity, Direction.SOUTH) && IsExactCollision(entity, Direction.WEST);
            }
            else if (direction == Direction.SOUTHEAST)
            {
                return IsExactCollision(entity, Direction.SOUTH) && IsExactCollision(entity, Direction.EAST);
            } else if (direction == Direction.CENTER)
            {
                return true;
            }
            throw new Exception("Uknown direction");
        }

        protected Vector2 GetGridCoord(Vector2 gridCoord, Direction direction)
        {
            if (direction == Direction.CENTER) return gridCoord;
            if (direction == Direction.WEST) return GridUtil.GetLeftGrid(gridCoord);
            if (direction == Direction.EAST) return GridUtil.GetRightGrid(gridCoord);
            if (direction == Direction.NORTH) return GridUtil.GetUpperGrid(gridCoord);
            if (direction == Direction.SOUTH) return GridUtil.GetBelowGrid(gridCoord);
            if (direction == Direction.SOUTHEAST) return GridUtil.GetRightBelowGrid(gridCoord);
            if (direction == Direction.SOUTHWEST) return GridUtil.GetLeftBelowGrid(gridCoord);
            if (direction == Direction.NORTHWEST) return GridUtil.GetUpperLeftGrid(gridCoord);
            if (direction == Direction.NORTHEAST) return GridUtil.GetUpperRightGrid(gridCoord);

            throw new Exception("Unknown direction!");
        }

        /// <summary>
        /// Returns whether this block contains a collider that blocks movement from
        /// the given direction.
        /// </summary>
        /// <param name="gridCoord"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool HasBlockingColliderAt(Vector2 gridCoord, Direction direction)
        {
            Vector2 coord = GetGridCoord(gridCoord, direction);
            return objects.ContainsKey(coord) && objects[coord].BlocksMovementFrom(direction);
        }

        public void Remove(StaticCollider gameObject)
        {
            Vector2 position = objectPositions[gameObject];
            objects.Remove(position);
        }

        public void Destroy()
        {
            objects.Clear();
            objectPositions.Clear();
            allCollisionsResult.Clear();
            tagCollisionResult.Clear();
        }

    }
}
