using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlowESP
{
    class ProccesManager
    {
        public static Memory memory;
        public static int client_dll;
        public static int engine_dll;
        public static void ProcessSearch()
        {
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Process csgo = Process.GetProcessesByName("csgo")[0];
                        memory = new Memory("csgo");

                        foreach (ProcessModule item in csgo.Modules)
                        {
                            if (item.ModuleName == "client.dll")
                            {
                                client_dll = (int)item.BaseAddress;
                            }
                            if (item.ModuleName == "engine.dll")
                            {
                                engine_dll = (int)item.BaseAddress;
                            }
                        }
                        if (client_dll != 0 & engine_dll != 0)
                        {
                            RunProcessCheat();
                            break;
                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    Thread.Sleep(1000);
                }
            }).Start();
        }
        public static void RunProcessCheat()
        {
            RunInNewProcess(Program.glowesp);
        }
        public static void RunInNewProcess(Action action)
        {
            Thread thread = new Thread(()=>
            {
                while (true)
                {
                    if (checkgame())
                    {
                        action();
                    }
                }
            });
            thread.Priority = ThreadPriority.Highest;
            thread.Start();
        }

        public static bool checkgame()
        {
            int clientState = memory.Read<int>(engine_dll + Offsets.dwClientState);
            bool isPlaying = memory.Read<int>(clientState + Offsets.dwClientState_State) >= 5 && !memory.Read<bool>(clientState + Offsets.dwClientState_IsHLTV);
            return isPlaying;
        }

        public class swed
        {

            #region imports

            [DllImport("kernel32.dll")]
            static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, IntPtr lpNumberOfBytesRead);

            [DllImport("kernel32.dll")]
            static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, IntPtr lpNumberOfBytesWritten);

            #endregion

            #region procstuff
            public Process proc;
            public Process GetProcess(string procname)
            {
                proc = Process.GetProcessesByName(procname)[0];
                return proc;
            }
            public IntPtr GetModuleBase(string modulename)
            {
                if (modulename.Contains(".exe"))
                    return proc.MainModule.BaseAddress;

                foreach (ProcessModule module in proc.Modules)
                {
                    if (module.ModuleName == modulename)
                        return module.BaseAddress;
                }
                return IntPtr.Zero;
            }
            #endregion

            #region readpointer
            public IntPtr ReadPointer(IntPtr addy)
            {
                byte[] buffer = new byte[4];
                ReadProcessMemory(proc.Handle, addy, buffer, buffer.Length, IntPtr.Zero);
                return new IntPtr(BitConverter.ToInt32(buffer, 0));
            }

            public IntPtr ReadPointer(IntPtr addy, int offset)
            {
                byte[] buffer = new byte[4];
                ReadProcessMemory(proc.Handle, IntPtr.Add(addy, offset), buffer, buffer.Length, IntPtr.Zero);

                return new IntPtr(BitConverter.ToInt32(buffer, 0));
            }
            #endregion

            #region readbytes
            public byte[] ReadBytes(IntPtr addy, int bytes)
            {
                byte[] buffer = new byte[bytes];
                ReadProcessMemory(proc.Handle, addy, buffer, buffer.Length, IntPtr.Zero);
                return buffer;
            }
            public byte[] ReadBytes(IntPtr addy, int offset, int bytes)
            {
                byte[] buffer = new byte[bytes];
                ReadProcessMemory(proc.Handle, IntPtr.Add(addy, offset), buffer, buffer.Length, IntPtr.Zero);
                return buffer;
            }

            #endregion

            #region write 
            public bool WriteBytes(IntPtr address, byte[] newbytes)
            {
                return WriteProcessMemory(proc.Handle, address, newbytes, newbytes.Length, IntPtr.Zero);
            }
            public bool WriteBytes(IntPtr address, int offset, byte[] newbytes)
            {
                return WriteProcessMemory(proc.Handle, IntPtr.Add(address, offset), newbytes, newbytes.Length, IntPtr.Zero);
            }
            #endregion
        }
    }
}
