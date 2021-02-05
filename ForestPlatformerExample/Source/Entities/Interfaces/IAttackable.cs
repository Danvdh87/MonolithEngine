using GameEngine2D.Engine.Source.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ForestPlatformerExample.Source.Entities.Interfaces
{
    interface IAttackable
    {
        void Hit(Direction impactDireciton);
    }
}
