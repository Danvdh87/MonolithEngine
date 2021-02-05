using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine2D.Engine.Source.Entities.Interfaces
{
    public interface IHasVelocity
    {
        Vector2 GetVelocity();

        void AddForce(Vector2 force);
    }
}
