using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GlowESP.Property;
using static GlowESP.ProccesManager;
using System.Threading;

namespace GlowESP
{
    public partial class Form1 : Form
    {
        const int localplayer = Offsets.dwLocalPlayer;
        const int entityList = Offsets.dwEntityList;
        const int viewmatrix = Offsets.dwViewMatrix;
        const int xyz = Offsets.m_vecOrigin;
        const int Team = Offsets.m_iTeamNum;
        const int dormant = Offsets.m_bDormant;
        const int health = Offsets.m_iHealth;

        Pen teampen = new Pen(Color.Blue, 3);
        Pen enemypen = new Pen(Color.Red, 3);

        swed swed = new swed();
        ez ez = new ez();
        Entity player = new Entity();
        List<Entity> list = new List<Entity>();

        IntPtr Client;

        public Form1()
        {
            InitializeComponent();
            GLOWESP = true;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            swed.GetProcess("csgo");

            Client = swed.GetModuleBase("client.dll");
            ez.SetInvi(this);
            ez.DoStuff("Counter-Strike: Global offensive - Direct3D 9", this);
            Thread thread = new Thread(main) { IsBackground = true };
            thread.Start();
        }

        void main()
        {
            while (true)
            {
                
                updatelocal();
                updateentities();
                panel1.Refresh();
                Thread.Sleep(13);
            }
        }
        void updatelocal()
        {
            var buffer = swed.ReadPointer(Client, localplayer);
            player.team = BitConverter.ToInt32(swed.ReadBytes(buffer, Team), 0);
        }
        void updateentities()
        {
            list.Clear();
            for (int i = 0; i < 32; i++)
            {
                var buffer = swed.ReadPointer(Client, entityList + i + 0x10);
                var tm = BitConverter.ToInt32(swed.ReadBytes(buffer, Team, 4), 0);
                var dorm = BitConverter.ToInt32(swed.ReadBytes(buffer, dormant, 4), 0);
                var hp = BitConverter.ToInt32(swed.ReadBytes(buffer, health, 4), 0);

                if (hp < 2 || dorm != 0)
                {
                    continue;

                }
                var coords = swed.ReadBytes(buffer, xyz, 12);
                var ent = new Entity
                {
                    x = BitConverter.ToSingle(coords, 0),
                    y = BitConverter.ToSingle(coords, 4),
                    z = BitConverter.ToSingle(coords, 8),
                    team = tm,
                    health = hp,
                };
                ent.bot = WorldToScreen(readmatrix(), ent.x, ent.y, ent.z, Width, Height);
                ent.top = WorldToScreen(readmatrix(), ent.x, ent.y, ent.z+58, Width, Height);
                list.Add(ent);

            }
        }
        viewmatrix readmatrix()
        {
            var matrix = new viewmatrix();
            var buffer = new byte[16 * 4];
            buffer = swed.ReadBytes(Client, viewmatrix, buffer.Length);
            matrix.m11 = BitConverter.ToSingle(buffer, 0 * 4);
            matrix.m12 = BitConverter.ToSingle(buffer, 1 * 4);
            matrix.m13 = BitConverter.ToSingle(buffer, 2 * 4);
            matrix.m14 = BitConverter.ToSingle(buffer, 3 * 4);

            matrix.m21 = BitConverter.ToSingle(buffer, 4 * 4);
            matrix.m22 = BitConverter.ToSingle(buffer, 5 * 4);
            matrix.m23 = BitConverter.ToSingle(buffer, 6 * 4);
            matrix.m24 = BitConverter.ToSingle(buffer, 7 * 4);

            matrix.m31 = BitConverter.ToSingle(buffer, 8 * 4);
            matrix.m32 = BitConverter.ToSingle(buffer, 9 * 4);
            matrix.m33 = BitConverter.ToSingle(buffer, 10 * 4);
            matrix.m34 = BitConverter.ToSingle(buffer, 11 * 4);

            matrix.m41 = BitConverter.ToSingle(buffer, 12 * 4);
            matrix.m42 = BitConverter.ToSingle(buffer, 13 * 4);
            matrix.m43 = BitConverter.ToSingle(buffer, 14 * 4);
            matrix.m44 = BitConverter.ToSingle(buffer, 15 * 4);
            return matrix;
        }

        Point WorldToScreen(viewmatrix matrix, float x, float y, float z, int width, int height)
        {
            var twoD = new Point();

            float screeW = (matrix.m41 * x) + (matrix.m42 * y) + (matrix.m43 * z) + matrix.m44;
            if (screeW > 0.001f)
            {
                float screenX = (matrix.m11 * x) + (matrix.m12 * y) + (matrix.m13 * z) + matrix.m14;
                float screenY = (matrix.m21 * x) + (matrix.m22 * y) + (matrix.m23 * z) + matrix.m24;
                float camX = width / 2f;
                float camY = height / 2f;
                float X = camX + (camX * screenX / screeW);
                float Y = camY - (camY * screenY / screeW);
                twoD.X = (int)X;
                twoD.Y = (int)Y;
                return twoD;
            }
            else
            {
                return new Point(-99, -99);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;
            if (list.Count > 0)
            {
                try
                {
                    foreach (var ent in list)
                    {
                        if (ent.team == player.team && ent.bot.X > 0 && ent.bot.X < Width && ent.bot.Y > 0 && ent.bot.Y < Height)
                        {
                            graphics.DrawRectangle(teampen, ent.rect());
                        }
                        else if (ent.team != player.team && ent.bot.X > 0 && ent.bot.X < Width && ent.bot.Y > 0 && ent.bot.Y < Height)
                        {
                            graphics.DrawRectangle(enemypen, ent.rect());
                        }
                    }
                }
                catch
                {

                    
                }
            }
        }


    }
}
