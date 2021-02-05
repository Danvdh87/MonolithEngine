using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine2D.Engine.Source.Entities.Interfaces
{
    public interface IRayBlocker
    {
        bool BlocksRay { get; set; }
        List<(Vector2 start, Vector2 end)> GetRayBlockerLines();
    }
}
