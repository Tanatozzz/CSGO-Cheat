using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GlowESP.ProccesManager;
using static GlowESP.Property;

namespace GlowESP
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        ///
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

        }
        public static void glowesp()
        {
            while (true)
            {
                int localplayer = memory.Read<int>(ProccesManager.client_dll + Offsets.dwLocalPlayer);
                int myteam = memory.Read<int>(localplayer + Offsets.m_iTeamNum);
                for (byte i = 0; i < 64; i++)
                {
                    int entityList = memory.Read<int>(ProccesManager.client_dll + Offsets.dwEntityList + i * 0x10);
                    int enemyteam = memory.Read<int>(entityList + Offsets.m_iTeamNum);
                    float EntityHp = memory.Read<int>(entityList + Offsets.m_iHealth) / 100f;
                    if (GLOWESP)
                    {
                        if (entityList != 0)
                        {
                            if (enemyteam != 0 && enemyteam != myteam)
                            {
                                int glowindex = memory.Read<int>(entityList + Offsets.m_iGlowIndex);
                                if (HpBased)
                                {
                                    DrawEntity(glowindex, EntityHp);
                                }
                                else
                                {
                                    DrawEntity(glowindex, 255, 0, 0);
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void DrawEntity(int GlowIndex, int red, int green, int blue)
        {
            int GlowObject = memory.Read<int>(ProccesManager.client_dll + Offsets.dwGlowObjectManager);
            memory.Write(GlowObject + (GlowIndex * 0x38) + 0x8, red / 255.0f);
            memory.Write(GlowObject + (GlowIndex * 0x38) + 0xC, green / 255.0f);
            memory.Write(GlowObject + (GlowIndex * 0x38) + 0x19, blue / 255.0f);
            memory.Write(GlowObject + (GlowIndex * 0x38) + 0x14, 255 / 255.0f);
            memory.Write(GlowObject + (GlowIndex * 0x38) + 0x28, true);
            memory.Write(GlowObject + (GlowIndex * 0x38) + 0x29, false);
        } // color for enemy
        public static void DrawEntity(int GlowIndex, float hp)
        {
            int GlowObject = memory.Read<int>(ProccesManager.client_dll + Offsets.dwGlowObjectManager);
            memory.Write(GlowObject + (GlowIndex * 0x38) + 0x8, 1f - hp);
            memory.Write(GlowObject + (GlowIndex * 0x38) + 0xC, hp);
            memory.Write(GlowObject + (GlowIndex * 0x38) + 0x19, 0);
            memory.Write(GlowObject + (GlowIndex * 0x38) + 0x14, 255 / 255.0f);
            memory.Write(GlowObject + (GlowIndex * 0x38) + 0x28, true);
            memory.Write(GlowObject + (GlowIndex * 0x38) + 0x29, false);
        } // hp




    }
}
