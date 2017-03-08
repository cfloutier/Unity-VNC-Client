// Unity 3D Vnc Client - Unity 3D VNC Client Library
// Copyright (C) 2017 Christophe Floutier
//
// Based on VncSharp - .NET VNC Client Library
// Copyright (C) 2008 David Humphrey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA


using UnityEngine;

namespace VNCScreen
{
    /// <summary>
    /// Translate from Unity Keycode to XK Code (from x11) 
    /// </summary>
    public class KeyTranslator
    {
        public static uint convertToXKCode(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.None: return 0;
                case KeyCode.Backspace: return 0xff08;
                case KeyCode.Delete: return 0xffff;
                case KeyCode.Tab: return 0xff09;
                case KeyCode.Clear: return 0xff0b;
                case KeyCode.Return: return 0xff0D;
                case KeyCode.Pause: return 0xff13;
                case KeyCode.Escape: return 0xff1B;
                case KeyCode.Space: return 0x20;

                case KeyCode.Keypad0: return 0xffb0;
                case KeyCode.Keypad1: return 0xffb1;
                case KeyCode.Keypad2: return 0xffb2;
                case KeyCode.Keypad3: return 0xffb3;
                case KeyCode.Keypad4: return 0xffb4;
                case KeyCode.Keypad5: return 0xffb5;
                case KeyCode.Keypad6: return 0xffb6;
                case KeyCode.Keypad7: return 0xffb7;
                case KeyCode.Keypad8: return 0xffb8;
                case KeyCode.Keypad9: return 0xffb9;

                case KeyCode.KeypadPeriod: return 0xffac;
                case KeyCode.KeypadDivide: return 0xffaf;
                case KeyCode.KeypadMultiply: return 0xffaa;
                case KeyCode.KeypadMinus: return 0xffad;
                case KeyCode.KeypadPlus: return 0xffab;
                case KeyCode.KeypadEnter: return 0xff8d; // same as return
                                                         //     case KeyCode.KeypadEquals: return -1; // unkown
                case KeyCode.UpArrow: return 0xff52;
                case KeyCode.DownArrow: return 0xff54;
                case KeyCode.RightArrow: return 0xff53;
                case KeyCode.LeftArrow: return 0xff51;
                case KeyCode.Insert: return 0xff63;
                case KeyCode.Home: return 0xff50;
                case KeyCode.End: return 0xff57;
                case KeyCode.PageUp: return 0xff55;
                case KeyCode.PageDown: return 0xff56;


                case KeyCode.F1: return 0xffbe;
                case KeyCode.F2: return 0xffbf;
                case KeyCode.F3: return 0xffc0;
                case KeyCode.F4: return 0xffc1;
                case KeyCode.F5: return 0xffc2;
                case KeyCode.F6: return 0xffc3;
                case KeyCode.F7: return 0xffc4;
                case KeyCode.F8: return 0xffc5;
                case KeyCode.F9: return 0xffc6;
                case KeyCode.F10: return 0xffc7;
                case KeyCode.F11: return 0xffc8;
                case KeyCode.F12: return 0xffc9;
                case KeyCode.F13: return 0xffca;
                case KeyCode.F14: return 0xffcb;
                case KeyCode.F15: return 0xffcc;
                case KeyCode.Alpha0: return 0x0030;
                case KeyCode.Alpha1: return 0x0031;
                case KeyCode.Alpha2: return 0x0032;
                case KeyCode.Alpha3: return 0x0033;
                case KeyCode.Alpha4: return 0x0034;
                case KeyCode.Alpha5: return 0x0035;
                case KeyCode.Alpha6: return 0x0036;
                case KeyCode.Alpha7: return 0x0037;
                case KeyCode.Alpha8: return 0x0038;
                case KeyCode.Alpha9: return 0x0039;
                case KeyCode.Exclaim: return 0x0021;
                case KeyCode.DoubleQuote: return 0x0022;
                case KeyCode.Hash: return 0x0023;
                case KeyCode.Dollar: return 0x0024;
                case KeyCode.Ampersand: return 0x0026;
                case KeyCode.Quote: return 0x0027;
                case KeyCode.LeftParen: return 0x0028;
                case KeyCode.RightParen: return 0x0029;
                case KeyCode.Asterisk: return 0x002a;
                case KeyCode.Plus: return 0x002b;
                case KeyCode.Comma: return 0x002c;
                case KeyCode.Minus: return 0x002d;
                case KeyCode.Period: return 0x002e;
                case KeyCode.Slash: return 0x002f;

                case KeyCode.Colon: return 0x003a;
                case KeyCode.Semicolon: return 0x003b;
                case KeyCode.Less: return 0x003c;
                case KeyCode.Equals: return 0x003d;
                case KeyCode.Greater: return 0x003e;
                case KeyCode.Question: return 0x003f;
                case KeyCode.At: return 0x0040;
                case KeyCode.LeftBracket: return 0x005b;
                case KeyCode.Backslash: return 0x005c;
                case KeyCode.RightBracket: return 0x005d;
                case KeyCode.Caret: return 0x005e;
                case KeyCode.Underscore: return 0x005f;


                case KeyCode.BackQuote: return 0x0060;

                case KeyCode.A: return 0x0061;
                case KeyCode.B: return 0x0062;
                case KeyCode.C: return 0x0063;
                case KeyCode.D: return 0x0064;
                case KeyCode.E: return 0x0065;
                case KeyCode.F: return 0x0066;
                case KeyCode.G: return 0x0067;
                case KeyCode.H: return 0x0068;
                case KeyCode.I: return 0x0069;
                case KeyCode.J: return 0x006a;
                case KeyCode.K: return 0x006b;
                case KeyCode.L: return 0x006c;
                case KeyCode.M: return 0x006d;
                case KeyCode.N: return 0x006e;
                case KeyCode.O: return 0x006f;
                case KeyCode.P: return 0x0070;
                case KeyCode.Q: return 0x0071;
                case KeyCode.R: return 0x0072;
                case KeyCode.S: return 0x0073;
                case KeyCode.T: return 0x0074;
                case KeyCode.U: return 0x0075;
                case KeyCode.V: return 0x0076;
                case KeyCode.W: return 0x0077;
                case KeyCode.X: return 0x0078;
                case KeyCode.Y: return 0x0079;
                case KeyCode.Z: return 0x007a;

                case KeyCode.Numlock: return 0xff7f;
                case KeyCode.CapsLock: return 0xffe5;
                case KeyCode.ScrollLock: return 0xff14;
                case KeyCode.RightShift: return 0xffe2;   //Not good because not taken at all by the OnGUI process
                case KeyCode.LeftShift: return 0xffe1;
                case KeyCode.RightControl: return 0xffe4;
                case KeyCode.LeftControl: return 0xffe3;
                case KeyCode.RightAlt: return 0xffea;
                case KeyCode.LeftAlt: return 0xffe9;
                case KeyCode.LeftCommand: return 0xffe3; // same as LeftApple, LeftControl
                                                         //*    case KeyCode.LeftApple: return;
                case KeyCode.LeftWindows: return 0xffe7;
                case KeyCode.RightCommand: return 0xffe4; // right control, right apple
                                                          //case KeyCode.RightApple:  return;
                case KeyCode.RightWindows: return 0xffe8;
                case KeyCode.AltGr: return 0xffe9;
                case KeyCode.Help: return 0xff6a;
                case KeyCode.Print: return 0xff61;
                case KeyCode.SysReq: return 0xff15;
                case KeyCode.Break: return 0xff6b;
                case KeyCode.Menu: return 0xff67; // alt ?
                default:
                    Debug.LogError("Invalid Key");
                    return 0xffff;
            }
        }
    }
}