using GameEngine2D.Entities.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine2D.Source.Entities.Animation.Interface
{
    public interface IAnimation
    {
        void Update(GameTime gameTime);

        void Play(SpriteBatch spriteBatch);
    }
}
