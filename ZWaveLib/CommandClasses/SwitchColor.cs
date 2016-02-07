/*
    This file is part of ZWaveLib Project source code.

    ZWaveLib is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ZWaveLib is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ZWaveLib.  If not, see <http://www.gnu.org/licenses/>.  
*/

/*
 *     Author: Nathan Hartzell <nyasara@hotmail.com>
 *     Project Homepage: https://github.com/genielabs/zwave-lib-dotnet
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZWaveLib.Values;

namespace ZWaveLib.CommandClasses
{
    public class SwitchColor : ICommandClass
    {
        public class ColorValue
        {
            public ZWaveSwitchColorNumber ColorNumber { get; set; }
            public byte Value { get; set; }
        }

        public CommandClass GetClassId()
        {
            return CommandClass.SwitchColor;
        }

        public NodeEvent GetEvent(ZWaveNode node, byte[] message)
        {
            //Set up the color values array
            var colordata = node.GetData("ColorValues");
            if (colordata == null) { colordata = new NodeData("ColorValues", new List<ColorValue>()); }
            var colorvals = colordata.Value as List<ColorValue>;
            NodeEvent nodeEvent = null;
            byte cmdType = message[1];
            if (cmdType == (byte)Command.SwitchColorCapabilityReport)
            {
                for (int i = 2; i < 4; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if ((message[i] & 0x1 << j) > 0)
                        {
                            var colnum = (ZWaveSwitchColorNumber)(8*i+j);
                            var exist = (from val in colorvals
                                         where val.ColorNumber == colnum
                                         select val).FirstOrDefault();
                            if (exist == null) { colorvals.Add(new ColorValue { ColorNumber = colnum, Value = 0 }); }
                            Get(node, 8 * i + j);
                        }
                    }
                }
            }
            else if (cmdType == (byte)Command.SwitchColorReport)
            {

            }
            node.UpdateData("ColorValues", colorvals);
            return nodeEvent;
        }

        public static ZWaveMessage GetCapabilityReport(ZWaveNode node)
        {
            return node.SendDataRequest(new byte[] {
                (byte)CommandClass.SwitchColor,
                (byte)Command.SwitchColorCapabilityGet
            });
        }

        public static ZWaveMessage GetCapability(ZWaveNode node, int colorindex)
        {
            return node.SendDataRequest(new byte[] {
                (byte)CommandClass.SwitchColor,
                (byte)Command.SwitchColorGet,
                (byte)colorindex
            });
        }

        public static ZWaveMessage Set(ZWaveNode node, ColorValue[] colors)
        {
            byte[] data = new byte[3 + 2 * colors.Length];
            data[0] = (byte)CommandClass.SwitchColor;
            data[1] = (byte)Command.SwitchColorSet;
            data[2] = (byte)colors.Length;
            for (int i = 0; i < colors.Length; i++)
            {
                data[3 + 2 * i] = (byte)colors[i].ColorNumber;
                data[4 + 2 * i] = (byte)colors[i].Value;
            }
            return node.SendDataRequest(data);
        }

        public static ZWaveMessage Get(ZWaveNode node, int capability)
        {
            return node.SendDataRequest(new byte[] { 
                (byte)CommandClass.SwitchColor, 
                (byte)Command.SwitchColorGet,
                (byte)capability
            });
        }
    }
}
