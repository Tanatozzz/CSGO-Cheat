using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowESP
{
    public class Entity
    {

        public int health, team;
        public float x, y, z, mag;
        public Point top, bot;
        public Rectangle rect() // easier to access :)
        {
            return new Rectangle
            {
                Location = new Point(bot.X - (bot.Y - top.Y) / 4, top.Y),
                Size = new Size((bot.Y - top.Y) / 2, (bot.Y - top.Y))
            };
        }
    }

    public class viewmatrix
    {
        public float

            m11, m12, m13, m14,
            m21, m22, m23, m24,
            m31, m32, m33, m34,
            m41, m42, m43, m44;


    }
}
