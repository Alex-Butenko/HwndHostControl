using System;
using System.Runtime.InteropServices;

namespace FormsHost {
	public class SystemWindowNotTransparent : SystemWindowChild, ISystemWindow {
		public SystemWindowNotTransparent (IntPtr handle, IntPtr formsHostHandle, bool setEmbeddable) :
			base(handle, formsHostHandle, setEmbeddable) { }
		public bool SetParent (IntPtr handle) {
			uint style = WinAPI.GetWindowLongPtr(_Handle, (int) WinAPI.GWL.STYLE);
			if (handle != IntPtr.Zero) {
				WinAPI.SetWindowLongPtr(new HandleRef(this, _Handle),
					(int) WinAPI.GWL.STYLE, new UIntPtr(style | (uint) WinAPI.WS.CHILD));
			}

			bool result = WinAPI.SetParent(_Handle, handle) != IntPtr.Zero;

			if (handle != IntPtr.Zero) {
				WinAPI.SetWindowLongPtr(new HandleRef(this, _Handle),
					(int) WinAPI.GWL.STYLE, new UIntPtr(style ^ (uint) WinAPI.WS.CHILD));
			}

			WinAPI.SetWindowPos(_Handle, new IntPtr(0), 0, 0, 500, 300, (uint) (
				WinAPI.SWP.SHOWWINDOW | WinAPI.SWP.FRAMECHANGED | WinAPI.SWP.DRAWFRAME
				));
			Embeddable = false;
			return result;
		}
	}
}