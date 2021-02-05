using GameEngine2D.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ForestPlatformerExample.Source.Entities.Interfaces
{
    interface IMovableItem
    {
        void Lift(Entity entity, Vector2 newPosition);

        void PutDown(Entity entity, Vector2 newPosition);

        void Throw(Entity entity, Vector2 force);
    }
}
