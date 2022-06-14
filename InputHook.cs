using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ComboSkills {
    public delegate int KeyHookProc(int code, int wParam, ref KeyHookData lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyHookData {
        public uint vkCode;
        public uint scanCode;
        public KeyHookDataFlags flags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [Flags]
    public enum KeyHookDataFlags : uint {
        LLKHF_EXTENDED = 0x01,
        LLKHF_INJECTED = 0x10,
        LLKHF_ALTDOWN = 0x20,
        LLKHF_UP = 0x80,
    }

    public enum KeyState : int {
        VKeyDown = 0x0000,
        VKeyExtended = 0x0001,
        VKeyUp = 0x0002,
        KeyDown = 0x100,
        KeyUp = 0x101,
        SysKeyDown = 0x104,
        SysKeyUp = 0x105
    }

    public class InputHook {
        protected IntPtr KeyHook = IntPtr.Zero;
        protected KeyHookProc KeyHookDelegate;

        private bool CtrlDown = false;
        private bool ShiftDown = false;
        private bool AltDown = false;

        private enum HookID : int {
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        public InputHook() {
            Hook();
        }

        ~InputHook() { Unhook(); }

        private void Hook() {
            IntPtr hInstance = WinApi.LoadLibrary("User32");

            KeyHookDelegate = new KeyHookProc(KeyHookProc);
            KeyHook = WinApi.SetWindowsHookEx((int)HookID.WH_KEYBOARD_LL, KeyHookDelegate, hInstance, 0);
        }

        public virtual void Unhook() {
            WinApi.UnhookWindowsHookEx(KeyHook);
        }

        private void KeyPress(Keys key, KeyState keyState) {
            WinApi.keybd_event((byte)key, 0, keyState == KeyState.KeyDown ? (int)KeyState.VKeyDown : (int)KeyState.VKeyUp, 0);
        }

        private int KeyHookProc(int code, int wParam, ref KeyHookData lParam) {
            if(code >= 0) {
                Keys key = (Keys)lParam.vkCode;
                KeyState state = (KeyState)wParam;

                if(key == Keys.LControlKey) {
                    CtrlDown = state == KeyState.KeyDown;
                } else if(key == Keys.LShiftKey) {
                    ShiftDown = state == KeyState.KeyDown;
                } else if(key == Keys.LMenu) {
                    AltDown = state == KeyState.KeyDown;
                }

                List<Skill> skills = Config.SkillCombo.Skills.FindAll(x => x.Key == key && x.CtrlMod == CtrlDown && x.ShiftMod == ShiftDown && x.AltMod == AltDown);
                if(skills != null) {
                    Combo combo = Config.SkillCombo.Combos.Find(x => x.Class == Config.Localization.PlayerClass && x.Skills.Count > 0 && skills.Find(y => y.Name == x.Skills[0]) != null);

                    if(combo != null) {
                        if(state == KeyState.KeyDown) {
                            foreach(Combo c in Config.SkillCombo.Combos) {
                                if(c != combo) { c.Index = 0; c.Triggered = false; c.ExpirationTimestamp = 0; }
                            }
                            combo.Triggered = true;
                            combo.State = KeyState.KeyUp;

                            if(combo.Index != 0) {
                                if(combo.Expiration != 0 && combo.ExpirationTimestamp != 0 && DateTime.Now.Ticks - combo.ExpirationTimestamp > 10000000 * combo.Expiration) {
                                    combo.Index = 0;
                                    combo.ExpirationTimestamp = 0;
                                } else {
                                    Skill skill = Config.SkillCombo.Skills.Find(x => x.Name == combo.Skills[combo.Index]);

                                    if(skill.CtrlMod) { KeyPress(Keys.LControlKey, KeyState.KeyDown); }
                                    if(skill.ShiftMod) { KeyPress(Keys.LShiftKey, KeyState.KeyDown); }
                                    if(skill.AltMod) { KeyPress(Keys.LMenu, KeyState.KeyDown); }
                                    KeyPress(skill.Key, KeyState.KeyDown);

                                    new System.Threading.Thread(() => {
                                        System.Threading.Thread.Sleep(50);
                                        KeyPress(skill.Key, KeyState.KeyUp);
                                        if(skill.CtrlMod) { KeyPress(Keys.LControlKey, KeyState.KeyUp); }
                                        if(skill.ShiftMod) { KeyPress(Keys.LShiftKey, KeyState.KeyUp); }
                                        if(skill.AltMod) { KeyPress(Keys.LMenu, KeyState.KeyUp); }
                                    }).Start();

                                    combo.State = KeyState.KeyDown;
                                    return -1;
                                }
                            }
                        } else if(combo.State == KeyState.KeyDown) {
                            combo.State = KeyState.KeyUp;
                            return -1;
                        }
                    }
                }
            }

            return WinApi.CallNextHookEx(KeyHook, code, wParam, ref lParam);
        }
    }
}
