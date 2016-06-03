﻿#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

using Drivers.Compiler.Attributes;
using Kernel.Framework;

namespace Kernel.VGA.Fonts
{
    public sealed class Jupitor : Object, IFont
    {
#pragma warning disable 649
        /// <summary>
        ///     The underlying font data. Assigned by the InitFont function that is implemented in ASM code.
        /// </summary>
        private static byte[] _FontData;
#pragma warning restore 649

        private static Jupitor _Instance;
        public static Jupitor Instance => _Instance ?? (_Instance = new Jupitor());

        private Jupitor()
        {
            InitFont();
        }

        [PluggedMethod(ASMFilePath = @"ASM\Fonts\Jupitor")]
        private static void InitFont()
        {
        }

        public byte[] FontData
        {
            get
            {
                return _FontData;
            }
        }

        public int FontHeight => 16;
    }
}
