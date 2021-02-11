﻿using GameEngine2D.Engine.Source.Entities;
using GameEngine2D.Engine.Source.Entities.Controller;
using GameEngine2D.Engine.Source.Entities.Interfaces;
using GameEngine2D.Engine.Source.Physics;
using GameEngine2D.Engine.Source.Physics.Collision;
using GameEngine2D.Engine.Source.Physics.Interface;
using GameEngine2D.Engine.Source.Util;
using GameEngine2D.Entities;
using GameEngine2D.Entities.Interfaces;
using GameEngine2D.Global;
using GameEngine2D.Source;
using GameEngine2D.Source.GridCollision;
using GameEngine2D.Source.Util;
using GameEngine2D.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace GameEngine2D
{
    public class PhysicalEntity : Entity, IColliderEntity
    {

        private Vector2 bump;

        private HashSet<string> CollidesAgainst = new HashSet<string>();

        protected UserInputController UserInput;

        protected float elapsedTime;
        private float steps;
        private float step;
        private float steps2;
        private float step2;
        private float t;

        public Vector2 Velocity = Vector2.Zero;

        protected float Friction = Config.FRICTION;
        protected float BumpFriction = Config.BUMP_FRICTION;

        protected float MovementSpeed = Config.CHARACTER_SPEED;

        public float GravityValue = Config.GRAVITY_FORCE;

        protected float FallSpeed { get; set; }

        public bool HasGravity = Config.GRAVITY_ON;

        protected GameTime GameTime;

        public bool CheckGridCollisions = true;

        private ICollisionComponent collisionComponent;
        public ICollisionComponent CollisionComponent
        {
            get => collisionComponent;

            set
            {

                collisionComponent = value;
                CollisionEngine.Instance.OnCollisionProfileChanged(this);
            }
        }

        private Texture2D circleColliderMarker;

        public bool CollisionsEnabled { get; set; } = true;

        public PhysicalEntity(Layer layer, Entity parent, Vector2 startPosition, Texture2D texture = null, SpriteFont font = null) : base(layer, parent, startPosition, texture, font)
        {
            Active = true;
            ResetPosition(startPosition);
            CollisionEngine.Instance.OnCollisionProfileChanged(this);
        }

        override public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (DEBUG_SHOW_PIVOT)
            {
                //spriteBatch.DrawString(font, "Y: " + Velocity.Y, DrawPosition, Color.White);
            }

            if (DEBUG_SHOW_CIRCLE_COLLIDER)
            {
                if (circleColliderMarker == null)
                {
                    circleColliderMarker = SpriteUtil.CreateCircle((int)((CircleCollisionComponent)CollisionComponent).Radius * 2, Color.Black);
                }
                if (CollisionComponent != null)
                {
                    spriteBatch.Draw(circleColliderMarker, ((CircleCollisionComponent)CollisionComponent).Position - new Vector2(((CircleCollisionComponent)CollisionComponent).Radius, ((CircleCollisionComponent)CollisionComponent).Radius), Color.White);
                }
                else
                {
                    Logger.Debug("Tried to print circle collider, but it's null!");
                }
            }

            base.Draw(spriteBatch, gameTime);
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (UserInput != null)
            {
                UserInput.Update();
            }

            base.PreUpdate(gameTime);
        }

        public override void Update(GameTime gameTime)
        {

            elapsedTime = TimeUtil.GetElapsedTime(gameTime);

            this.GameTime = gameTime;

            steps = (float)Math.Ceiling(Math.Abs((Velocity.X + bump.X) * elapsedTime));
            step = (float)(Velocity.X + bump.X) * elapsedTime / steps;
            while (steps > 0)
            {
                InCellLocation.X += step;

                if (CheckGridCollisions && InCellLocation.X > CollisionOffsetLeft && GridCollisionChecker.HasBlockingColliderAt(GridCoordinates, Direction.RIGHT))
                {
                    InCellLocation.X = CollisionOffsetLeft;
                }

                if (CheckGridCollisions && InCellLocation.X < CollisionOffsetRight && GridCollisionChecker.HasBlockingColliderAt(GridCoordinates, Direction.LEFT))
                {
                    InCellLocation.X = CollisionOffsetRight;
                }

                while (InCellLocation.X > 1) {
                    InCellLocation.X--; 
                    GridCoordinates.X++; 
                }
                while (InCellLocation.X < 0) {
                    InCellLocation.X++; 
                    GridCoordinates.X--; 
                }
                steps--;
            }
            Velocity.X *= (float)Math.Pow(Friction, elapsedTime);
            bump.X *= (float)Math.Pow(BumpFriction, elapsedTime);

            //rounding stuff
            if (Math.Abs(Velocity.X) <= 0.0005 * elapsedTime) Velocity.X = 0;
            if (Math.Abs(bump.X) <= 0.0005 * elapsedTime) bump.X = 0;

            // Y
            if (HasGravity && !OnGround())
            {
                if (FallSpeed == 0)
                {
                    FallSpeed = (float)gameTime.TotalGameTime.TotalSeconds;
                }
                ApplyGravity(gameTime);
            }

            if (OnGround())
            {
                FallSpeed = 0;
            }

            steps2 = (float)Math.Ceiling(Math.Abs((Velocity.Y + bump.Y) * elapsedTime));
            step2 = (float)(Velocity.Y + bump.Y) * elapsedTime / steps2;
            while (steps2 > 0)
            {
                InCellLocation.Y += step2;

                if (CheckGridCollisions && InCellLocation.Y > CollisionOffsetBottom && GridCollisionChecker.HasBlockingColliderAt(GridCoordinates, Direction.DOWN)/* && Velocity.Y > 0*/)
                {
                    if (HasGravity)
                    {
                        Velocity.Y = 0;
                        bump.Y = 0;
                        InCellLocation.Y = CollisionOffsetBottom;
                    }
                    OnLand();

                }

                if (CheckGridCollisions && InCellLocation.Y < CollisionOffsetTop && GridCollisionChecker.HasBlockingColliderAt(GridCoordinates, Direction.UP))
                {
                    Velocity.Y = 0;
                    InCellLocation.Y = CollisionOffsetTop;
                }
                   
                while (InCellLocation.Y > 1) {
                    InCellLocation.Y--; 
                    GridCoordinates.Y++; 
                }
                while (InCellLocation.Y < 0) {
                    InCellLocation.Y++; 
                    GridCoordinates.Y--; 
                }
                steps2--;
            }

            Velocity.Y *= (float)Math.Pow(Friction, elapsedTime);
            bump.Y *= (float)Math.Pow(BumpFriction, elapsedTime);
            //rounding stuff
            if (Math.Abs(Velocity.Y) <= 0.0005 * elapsedTime) Velocity.Y = 0;
            if (Math.Abs(bump.Y) <= 0.0005 * elapsedTime) bump.Y = 0;
            base.Update(gameTime);
        }

        protected virtual void OnLand()
        {
            //bump = Vector2.Zero;
        }

        public void Bump(Vector2 direction)
        {
            bump = direction;
        }

        private void ApplyGravity(GameTime gameTime)
        {
            if (Config.INCREASING_GRAVITY)
            {
                t = (float)(gameTime.TotalGameTime.TotalSeconds - FallSpeed) * Config.GRAVITY_T_MULTIPLIER;
                Velocity.Y += GravityValue * t * elapsedTime;
            }
            else
            {
                Velocity.Y += GravityValue * elapsedTime;
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            //Position = (GridCoordinates + InCellLocation) * Config.GRID;
            if (parent == null)
            {
                X = (int)((GridCoordinates.X + InCellLocation.X) * Config.GRID);
                Y = (int)((GridCoordinates.Y + InCellLocation.Y) * Config.GRID);
            }
            base.PostUpdate(gameTime);
        }

        protected bool OnGround()
        {
            return GridCollisionChecker.HasBlockingColliderAt(GridCoordinates, Direction.DOWN) && InCellLocation.Y == CollisionOffsetBottom && Velocity.Y >= 0;
        }

        public void ResetPosition(Vector2 position)
        {
            InCellLocation = new Vector2(0.5f, 1f);
            //InCellLocation = Vector2.Zero;
            //UpdateInCellCoord();
            Position = position;
            FallSpeed = 0;
        }

        public override void Destroy()
        {
            CheckGridCollisions = false;
            Velocity = Vector2.Zero;
            bump = Vector2.Zero;
            base.Destroy();
            CollisionEngine.Instance.OnCollisionProfileChanged(this);
        }

        protected override void RemoveCollisions()
        {
            base.RemoveCollisions();
            CollisionComponent = null;
            CollidesAgainst.Clear();
        }

        public bool IsMovingAtLeast(float speed)
        {
            return Math.Abs(Velocity.X) >= speed || Math.Abs(Velocity.Y) >= speed;
        }

        public Vector2 GetVelocity()
        {
            return Velocity;
        }

        public void AddForce(Vector2 force)
        {
            Velocity += force;
        }
        

        public void SetPosition(Vector2 position)
        {
            this.Position = position;
        }

        public void SetVelocity(Vector2 velocity)
        {
            Velocity = velocity;
        }

        public void AddVelocity(Vector2 velocity)
        {
            Velocity += velocity;
        }

        public ICollisionComponent GetCollisionComponent()
        {
            return CollisionComponent;
        }

        public virtual void OnCollisionStart(IColliderEntity otherCollider)
        {
            
        }

        public virtual void OnCollisionEnd(IColliderEntity otherCollider)
        {
            
        }

        public HashSet<string> GetCollidesAgainst()
        {
            return CollidesAgainst;
        }

        public void AddCollisionAgainst(string tag)
        {
            CollidesAgainst.Add(tag);
            CollisionEngine.Instance.OnCollisionProfileChanged(this);
        }

        public void RemoveCollisionAgainst(string tag)
        {
            CollidesAgainst.Remove(tag);
            CollisionEngine.Instance.OnCollisionProfileChanged(this);
        }

        public override void AddTag(string tag)
        {
            base.AddTag(tag);
            CollisionEngine.Instance.OnCollisionProfileChanged(this);
        }

        public override void RemoveTag(string tag)
        {
            base.RemoveTag(tag);
            CollisionEngine.Instance.OnCollisionProfileChanged(this);
        }
    }
}
