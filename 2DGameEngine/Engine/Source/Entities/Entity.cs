﻿using GameEngine2D.Engine.Source.Entities;
using GameEngine2D.Engine.Source.Entities.Abstract;
using GameEngine2D.Engine.Source.Entities.Animations;
using GameEngine2D.Engine.Source.Entities.Interfaces;
using GameEngine2D.Engine.Source.Entities.Transform;
using GameEngine2D.Engine.Source.Graphics.Primitives;
using GameEngine2D.Engine.Source.Physics;
using GameEngine2D.Engine.Source.Physics.Collision;
using GameEngine2D.Engine.Source.Physics.Interface;
using GameEngine2D.Engine.Source.Physics.Raycast;
using GameEngine2D.Engine.Source.Physics.Trigger;
using GameEngine2D.Engine.Source.Util;
using GameEngine2D.Global;
using GameEngine2D.Source.Entities.Animation;
using GameEngine2D.Source.GridCollision;
using GameEngine2D.Source.Util;
using GameEngine2D.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine2D.Entities
{
    public class Entity : GameObject, IColliderEntity, IRayBlocker
    {

        protected float CollisionOffsetLeft = 0f;
        protected float CollisionOffsetRight = 0f;
        protected float CollisionOffsetBottom = 0f;
        protected float CollisionOffsetTop = 0f;

        private readonly string DESTROY_AMINATION = "Destroy";

        private HashSet<string> CollidesAgainst = new HashSet<string>();

        private bool checkGridCollisions = false;
        public bool CheckGridCollisions
        {
            get => checkGridCollisions;
            set { 
                if ( value != checkGridCollisions )
                {
                    checkGridCollisions = value;
                    CollisionEngine.Instance.OnCollisionProfileChanged(this);
                }
            }
        }
        

        private Dictionary<string, ITrigger> triggers = new Dictionary<string, ITrigger>();

        private bool canFireTriggers = false;
        public bool CanFireTriggers
        {
            get => canFireTriggers;

            set
            {
                canFireTriggers = value;
                CollisionEngine.Instance.OnCollisionProfileChanged(this);
            }
        }

        public Direction CurrentFaceDirection { get; set; } = Direction.CENTER;

        private bool visible = true;
        private bool active = false;

        public bool Visible {
            get => visible;
            set
            {
                if (value != visible)
                {
                    Layer.OnObjectChanged(this);
                }
                visible = value;
            }
        }
        public bool Active
        {
            get => active;
            set
            {
                if (value != active)
                {
                    Layer.OnObjectChanged(this);
                }
                active = value;
            }
        }

        private float drawPriority = 0;

        public float DrawPriority {
            get => drawPriority;

            set
            {
                if (value != drawPriority)
                {
                    drawPriority = value;
                    Layer.SortByPriority();
                }
            }
        }

        public Vector2 DrawOffset;

        public Vector2 Pivot = Vector2.Zero;

        public Rectangle SourceRectangle;

        protected Texture2D Sprite { get; set; }

        protected Layer Layer { get; set; }
        protected List<(Vector2 start, Vector2 end)> RayBlockerLines;

        public bool BlocksRay { get; set; }

        public SoundEffect DestroySound;

        public HashSet<Direction> GridCollisionCheckDirections = new HashSet<Direction>();

        protected SpriteFont font;

        public AnimationStateMachine Animations { get; set; }

        protected Ray2DEmitter RayEmitter { get; set; }

        private Texture2D pivotMarker;

        public float Depth = 0f;

#if DEBUG
        public bool DEBUG_SHOW_PIVOT = false;

        public bool DEBUG_SHOW_COLLIDER = false;

        public bool DEBUG_SHOW_RAYCAST = false;
#endif

        public Vector2 DrawPosition
        {
            get {
                return Transform.Position + DrawOffset;
            }

            private set {}
        }

        protected bool Destroyed = false;
        protected bool BeingDestroyed = false;

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

        public bool CollisionsEnabled { get; set; } = true;

        public Entity(Layer layer, Entity parent, Vector2 startPosition, Texture2D sprite = null, SpriteFont font = null) : base (parent)
        {
            Transform = new StaticTransform(this, startPosition);
            Layer = layer;
            this.font = font;
            SetSprite(sprite);
            layer.OnObjectChanged(this);
        }

        protected virtual void SetRayBlockers()
        {
            RayBlockerLines.Clear();
            RayBlockerLines.Add((Transform.Position, new Vector2(Transform.X, Transform.Y + Config.GRID))); //0, 1
            RayBlockerLines.Add((Transform.Position, new Vector2(Transform.X + Config.GRID, Transform.Y))); //1, 0
            RayBlockerLines.Add((new Vector2(Transform.X + Config.GRID, Transform.Y), new Vector2(Transform.X + Config.GRID, Transform.Y + Config.GRID))); //1
            RayBlockerLines.Add((new Vector2(Transform.X, Transform.Y + Config.GRID), new Vector2(Transform.X + Config.GRID, Transform.Y + Config.GRID)));
        }

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (!Visible)
            {
                return;
            }

            if (Sprite != null)
            {
                spriteBatch.Draw(Sprite, DrawPosition, SourceRectangle, Color.White, 0f, Pivot, 1f, SpriteEffects.None, Depth);
            }
            else if (Animations != null)
            {
                Animations.Draw(spriteBatch, gameTime);
            }
#if DEBUG
            if (DEBUG_SHOW_PIVOT)
            {
                if (pivotMarker == null)
                {
                    pivotMarker = SpriteUtil.CreateCircle(5, Color.Red, true);
                }
                if (font != null)
                {
                    //spriteBatch.DrawString(font, "Veloctiy.Y : " + Velocity.Y, DrawPosition, Color.Black);
                }
                spriteBatch.Draw(pivotMarker, Transform.Position - new Vector2(2.5f, 2.5f), Color.White);
            }
#endif
            /*if (font != null)
            {
                spriteBatch.DrawString(font, "InCell : " + Transform.InCellLocation, DrawPosition, Color.Red);
            }*/

            if (Children.Count > 0)
            {
                foreach (Entity child in Children)
                {
                    child.Draw(spriteBatch, gameTime);
                }
            }
            
        }

        public virtual void PreUpdate(GameTime gameTime)
        {
            if (!Active)
            {
                return;
            }

            foreach (Entity child in Children)
            {
                child.PreUpdate(gameTime);
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!Active)
            {
                return;
            }

            if (Animations != null)
            {
                Animations.Update(gameTime);
            }

            foreach (Entity child in Children.ToList())
            {
                child.Update(gameTime);
            }
        }

        public virtual void PostUpdate(GameTime gameTime)
        {
            if (!Active)
            {
                return;
            }

            if (RayEmitter != null)
            {
                RayEmitter.UpdateRays();
            }

            foreach (Entity child in Children)
            {
                child.PostUpdate(gameTime);
            }
        }

        public override void Destroy()
        {
            if (BeingDestroyed)
            {
                return;
            }
            BeingDestroyed = true;
            RemoveCollisions();
            if (DestroySound != null)
            {
                DestroySound.Play();
            }
            if (Animations != null && Animations.HasAnimation(DESTROY_AMINATION + CurrentFaceDirection))
            {
                //RemoveCollisions();
                Animations.PlayAnimation(DESTROY_AMINATION + CurrentFaceDirection);
                //Animations.GetAnimation(DESTROY_AMINATION).StoppedCallback = () => Cleanup();
            } 
            else
            {
                Cleanup();
            }
        }

        protected void Cleanup()
        {
            if (Sprite != null)
            {
                Sprite.Dispose();
            }
            if (Animations != null)
            {
                Animations.Destroy();
                Animations = null;
            }
            if (DestroySound != null)
            {
                DestroySound.Dispose();
            }
            if (Parent != null)
            {
                Parent.RemoveChild(this);
            }
            if (Children.Any())
            {
                foreach (Entity o in Children.ToList())
                {
                    if (o != null)
                    {
                        o.Destroy();
                    }
                }
            }
            Active = false;
            Visible = false;
            Destroyed = true;
        }

        protected virtual void RemoveCollisions()
        {
            CollisionComponent = null;
            CollidesAgainst.Clear();
            RayEmitter = null;
            BlocksRay = false;
            GridCollisionCheckDirections = new HashSet<Direction>();
        }

        public void SetDestroyAnimation(AbstractAnimation destroyAnimation, Direction direction = Direction.CENTER)
        {
            if (Animations == null)
            {
                Animations = new AnimationStateMachine();
            }
            //destroyAnimation.StartedCallback += () => RemoveCollisions();
            destroyAnimation.StoppedCallback += () => Cleanup();
            destroyAnimation.Looping = false;
            Animations.RegisterAnimation(DESTROY_AMINATION + direction.ToString(), destroyAnimation, () => false);
        }

        public void SetSprite(Texture2D sprite)
        {
            if (sprite == null)
            {
                return;
            }

            this.Sprite = sprite;
            SourceRectangle = new Rectangle(0, 0, sprite.Width, sprite.Height);
        }

        public virtual List<(Vector2 start, Vector2 end)> GetRayBlockerLines()
        {
            if (RayBlockerLines == null)
            {
                RayBlockerLines = new List<(Vector2 start, Vector2 end)>();
            }
            SetRayBlockers();
            return RayBlockerLines;
        }

        public Vector2 GetGridCoord()
        {
            return Transform.GridCoordinates;
        }

        public float GetCollisionOffset(Direction direction)
        {
            if (direction == Direction.EAST)
            {
                return CollisionOffsetLeft;
            } else if (direction == Direction.WEST)
            {
                return CollisionOffsetRight;
            }
            else if (direction == Direction.NORTH)
            {
                return CollisionOffsetTop;
            }
            else if (direction == Direction.SOUTH)
            {
                return CollisionOffsetBottom;
            }
            throw new Exception("Unsupported direction");
        }

        public ICollisionComponent GetCollisionComponent()
        {
            return CollisionComponent;
        }

        public virtual void OnCollisionStart(IGameObject otherCollider)
        {

        }

        public virtual void OnCollisionEnd(IGameObject otherCollider)
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

        public ICollection<ITrigger> GetTriggers()
        {
            return triggers.Values;
        }

        public void AddTrigger(AbstractTrigger trigger)
        {
            trigger.SetOwner(this);
            triggers.Add(trigger.GetTag(), trigger);
            CollisionEngine.Instance.OnCollisionProfileChanged(this);
        }

        public void RemoveTrigger(AbstractTrigger trigger)
        {
            foreach (ITrigger t in triggers.Values.ToList())
            {
                if (t.Equals(trigger))
                {
                    RemoveTrigger(t.GetTag());
                    return;
                }
            }
        }

        public void RemoveTrigger(string tag)
        {
            triggers.Remove(tag);
            CollisionEngine.Instance.OnCollisionProfileChanged(this);
        }

        public virtual void OnEnterTrigger(string triggerTag, IGameObject otherEntity)
        {
        }

        public virtual void OnLeaveTrigger(string triggerTag, IGameObject otherEntity)
        {
        }
    }
}
