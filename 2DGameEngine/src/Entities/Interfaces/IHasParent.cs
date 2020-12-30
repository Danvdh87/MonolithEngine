﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace _2DGameEngine.Entities.Interfaces
{
    interface IHasParent
    {
        public Entity GetParent();

        public void AddParent(Entity newParent);
    }
}