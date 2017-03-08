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

using System;

namespace VNCScreen
{
	/// <summary>
	/// Used in connection with the ConnectComplete event. Contains information about the remote desktop useful for setting-up the client's GUI.
	/// </summary>
	public class ErrorEventArg : EventArgs
	{
		string reason;
        Exception e;
		
		public ErrorEventArg(string reason) : base()
		{
            this.reason = reason;
            this.e = null;
		}

        public ErrorEventArg(Exception e) : base()
        {
            this.reason = "Exception caught";
            this.e = e;
        }


        public string Reason
        {
			get { 
				return reason; 
			}
		}

        public Exception Exception
        {
            get
            {
                return e;
            }
        }

    }
}
