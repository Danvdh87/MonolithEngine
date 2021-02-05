using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine2D.Source.Level
{
    public interface MapSerializer
    {
        LDTKMap Deserialize(string filePath);
    }
}
