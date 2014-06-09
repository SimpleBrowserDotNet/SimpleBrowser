using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleBrowser
{
	[Flags]
	public enum KeyStateOption
	{
		None = 0,
		Shift = 1,
		Ctrl = 2,
		Alt = 4
	}
}
