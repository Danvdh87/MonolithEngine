﻿using GameEngine2D.Entities;
using GameEngine2D.src.Entities.Animation.Interface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine2D.src.Entities.Animation
{
    abstract class AbstractAnimation : IAnimation
    {

        protected int currentFrame;
        private int totalFrames;
        private double delay = 0;
        private double currentDelay = 0;
        protected SpriteBatch spriteBatch;
        protected Entity parent;
        protected float scale = 0f;
        protected Vector2 offset = Vector2.Zero;
        protected SpriteEffects spriteEffect;
        protected Rectangle sourceRectangle;

        public AbstractAnimation(SpriteBatch spriteBatch, Rectangle sourceRectangle, Entity parent, int totalFrames, int framerate = 0, SpriteEffects spriteEffect = SpriteEffects.None)
        {
            this.spriteBatch = spriteBatch;
            this.parent = parent;
            currentFrame = 0;
            this.totalFrames = totalFrames;
            this.spriteEffect = spriteEffect;
            this.sourceRectangle = sourceRectangle;
            if (framerate != 0)
            {
                delay = TimeSpan.FromSeconds(1).TotalMilliseconds / framerate;
            }
        }

        public abstract void Draw(Vector2 position);
        public void Update(GameTime gameTime)
        {
            if (delay == 0)
            {
                currentFrame++;
            }
            else
            {
                if (currentDelay >= delay)
                {
                    currentFrame++;
                    currentDelay = 0;
                }
                else
                {
                    currentDelay += gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }

            if (currentFrame == totalFrames) {
                currentFrame = 0;
            }
        }

        public void SetScale(float scale)
        {
            this.scale = scale;
        }

        public void SetOffset(Vector2 offset)
        {
            this.offset = offset;
        }
    }
}