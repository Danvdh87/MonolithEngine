﻿using GameEngine2D.Engine.Source.Entities;
using GameEngine2D.Engine.Source.Util;
using GameEngine2D.Entities;
using GameEngine2D.Global;
using GameEngine2D.Source;
using GameEngine2D.Source.Util;
using GameEngine2D.Util;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SideScrollerExample.SideScroller.Source.Entities
{
    class Bullet : Entity
    {
        private float speed = 300f;
        private int mul = 1;

        public Bullet(Entity parent, Direction faceDirection) : base(RootContainer.Instance.EntityLayer, null, parent.Position)
        {
            if (faceDirection == GameEngine2D.Engine.Source.Entities.Direction.LEFT)
            {
                mul = -1;
            }

            SinglePointCollisionChecks.Add(faceDirection);

            SetSprite(SpriteUtil.CreateRectangle(Config.GRID / 3, Color.Red));
        }

        public override void Update(GameTime gameTime)
        {
            X += speed * mul * TimeUtil.GetElapsedTime(gameTime);

            base.Update(gameTime);
        }

        protected override void OnCollisionStart(Entity otherCollider)
        {
            otherCollider.Destroy();
            Destroy();
        }

        protected override void OnCollisionEnd(Entity otherCollider)
        {

        }
    }
}
