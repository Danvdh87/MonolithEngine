﻿using GameEngine2D.Engine.Source.Physics.Collision;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine2D.Engine.Source.Physics.Interface
{
    public interface ICircleCollider
    {
        CircleCollider CircleCollider { get; set; }

        Vector2 GetPosition();

    }
}
