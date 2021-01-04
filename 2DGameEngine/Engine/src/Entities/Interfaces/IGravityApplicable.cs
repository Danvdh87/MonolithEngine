﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine2D.Entities.Interfaces
{
    interface IGravityApplicable
    {
        public bool HasGravity { get; set; }

        public float GetGravityConstant();
    }
}
