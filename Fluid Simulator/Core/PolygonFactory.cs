﻿using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using System;

namespace Fluid_Simulator.Core
{
    public static class PolygonFactory
    {
        public static Polygon CreateCircle(double radius, int sides)
        {
            Vector2[] array = new Vector2[sides];
            double num = Math.PI * 2.0 / sides;
            double num2 = 0.0;
            for (int i = 0; i < sides; i++)
            {
                array[i] = new Vector2((float)(radius * Math.Cos(num2)) + (float)radius, (float)(radius * Math.Sin(num2) + (float)radius));
                num2 += num;
            }

            return new(array);
        }

        public static Polygon CreateRectangle(int widthCount, int heightCount)
        {
            var rectangle = new RectangleF(0, 0, widthCount + 1, heightCount + 1);
            var corners = rectangle.GetCorners();
            return new(corners);
        }
    }
}
