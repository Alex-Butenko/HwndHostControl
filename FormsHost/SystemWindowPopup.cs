using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FormsHost {
	public class SystemWindowPopup : SystemWindow, ISystemWindow {
		public SystemWindowPopup (IntPtr handle, IntPtr formsHostHandle, bool setEmbeddable) :
			base(handle, formsHostHandle, setEmbeddable) { }
		protected override void ModStyle (ref uint style, ref uint exStyle) {
			style = (style | (uint) WinAPI.WS.POPUP) ^
			(uint) (WinAPI.WS.BORDER | WinAPI.WS.SYSMENU | WinAPI.WS.CAPTION |
			WinAPI.WS.THICKFRAME | WinAPI.WS.VISIBLE);
			exStyle = exStyle | (uint) (WinAPI.WS_EX.TOOLWINDOW | WinAPI.WS_EX.CONTROLPARENT);
		}
	}
}
