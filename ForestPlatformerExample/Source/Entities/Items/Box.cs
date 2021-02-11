﻿using ForestPlatformerExample.Source.Enemies;
using ForestPlatformerExample.Source.Entities.Interfaces;
using ForestPlatformerExample.Source.Items;
using ForestPlatformerExample.Source.PlayerCharacter;
using GameEngine2D;
using GameEngine2D.Engine.Source.Entities;
using GameEngine2D.Engine.Source.Entities.Animations;
using GameEngine2D.Engine.Source.Physics.Collision;
using GameEngine2D.Engine.Source.Physics.Interface;
using GameEngine2D.Engine.Source.Util;
using GameEngine2D.Entities;
using GameEngine2D.Global;
using GameEngine2D.Source.Entities;
using GameEngine2D.Source.Util;
using GameEngine2D.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace ForestPlatformerExample.Source.Entities.Items
{
    class Box : AbstractInteractive, IAttackable, IMovableItem
    {

        int life = 2;

        private int bumps;
        private int currentBump = 1;

        private List<Coin> coins = new List<Coin>();

        public Box(Vector2 position, int bumps = 1) : base(position)
        {
            //ColliderOnGrid = true;

            BumpFriction = 0.2f;

            DrawPriority = 1;

            AddCollisionAgainst("Enemy");
            AddTag("Box");

            this.bumps = currentBump = bumps;

            CollisionComponent = new CircleCollisionComponent(this, 10, new Vector2(0, -8));

            CollisionOffsetBottom = 1f;
            CollisionOffsetRight = 0.5f;

            GravityValue /= 2;
            Friction = 0.6f;

            Active = true;

            //DEBUG_SHOW_PIVOT = true;
            //DEBUG_SHOW_CIRCLE_COLLIDER = true;

            Animations = new AnimationStateMachine();
            Animations.Offset = new Vector2(0, -16);

            SpriteSheetAnimation boxIdle = new SpriteSheetAnimation(this, "ForestAssets/Items/box-idle", 24);
            Animations.RegisterAnimation("BoxIdle", boxIdle);

            SpriteSheetAnimation boxHit = new SpriteSheetAnimation(this, "ForestAssets/Items/box-hit", 24);
            boxHit.Looping = false;
            Animations.RegisterAnimation("BoxHit", boxHit);

            SpriteSheetAnimation boxDestroy = new SpriteSheetAnimation(this, "ForestAssets/Items/box-destroy", 24);
            boxDestroy.StartedCallback += () => Pop();
            boxDestroy.Looping = false;
            SetDestroyAnimation(boxDestroy);

            int numOfCoins = MyRandom.Between(3, 6);
            for (int i = 0; i < numOfCoins; i++)
            {

                Coin c = new Coin(Position, 3, friction: (float)MyRandom.Between(87, 93) / (float)100);
                c.SetParent(this);
                c.Visible = false;
                c.CollisionsEnabled = false;
                c.Active = false;
                coins.Add(c);
            }

            //SetSprite(SpriteUtil.CreateRectangle(Config.GRID, Color.Brown));
        }

        public void Hit(Direction impactDireciton)
        {
            if (life == 0)
            {
                Destroy();
                return;
            }

            life--;
            Animations.PlayAnimation("BoxHit");
        }

        public void Lift(Entity entity, Vector2 newPosition)
        {
            currentBump = bumps;
            DisablePysics();
            SetParent(entity, newPosition);
        }

        public void PutDown(Entity entity, Vector2 newPosition)
        {
            throw new NotImplementedException();
        }

        public void Throw(Entity entity, Vector2 force)
        {
            Velocity = Vector2.Zero;
            Velocity += force;
            RemoveParent();
            EnablePhysics();
            FallSpeed = 0;
        }

        private void EnablePhysics()
        {
            GridCoordinates = CalculateGridCoord();
            UpdateInCellCoord();
            GridCollisionCheckDirections = new HashSet<Direction>() { Direction.UP, Direction.DOWN, Direction.LEFT, Direction.RIGHT };
            HasGravity = true;
            Active = true;
        }

        private void DisablePysics()
        {
            GridCollisionCheckDirections = new HashSet<Direction>();
            HasGravity = false;
        }

        private void Pop()
        {
            foreach (Coin c in coins)
            {
                c.RemoveParent();
                c.Active = true;
                c.Visible = true;
                c.Velocity += new Vector2(MyRandom.Between(-2, 2), MyRandom.Between(-5, -1));
                Timer.TriggerAfter(500, () => c.CollisionsEnabled = true);
            }
            Layer.Camera.Shake(2f, 0.5f);
            Destroy();
        }

        /*protected override void OnCircleCollisionStart(Entity otherCollider, CollisionResult collisionResult)
        {
            if (otherCollider is Carrot && IsMovingAtLeast(0.5f))
            {
                otherCollider.Destroy();
                Explode();
            }
        }*/

        public override void OnCollisionStart(IColliderEntity otherCollider)
        {
            if (otherCollider is Carrot && IsMovingAtLeast(0.5f))
            {
                (otherCollider as Carrot).Destroy();
                Explode();
            }
            base.OnCollisionStart(otherCollider);
        }

        protected override void OnLand()
        {
            if (currentBump < 1)
            {
                return;
            }
            Bump(new Vector2(0, -currentBump * 2));
            currentBump--;
        }

        private void Explode()
        {
            Velocity = Vector2.Zero;
            GravityValue = 0;
            Destroy();
        }
    }
}
