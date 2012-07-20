using System;
using System.Runtime.InteropServices;

namespace FormsHost {
	class SystemWindowTranspXPVst7 : SystemWindowPopup, ISystemWindow {
		public SystemWindowTranspXPVst7 (IntPtr handle, IntPtr formsHostHandle, bool setEmbeddable) : base(handle, formsHostHandle, setEmbeddable) { }
		bool _transparency = true;
		byte _originalOpacity = 255;
		public bool Transparency {
			get {
				return _transparency;
			}
			set {
				if (value) {
					if (!_transparency) {
						uint exStyle = WinAPI.GetWindowLongPtr(_Handle, (int) WinAPI.GWL.EXSTYLE);
						WinAPI.SetWindowLongPtr(new HandleRef(this, _Handle),
							(int) WinAPI.GWL.EXSTYLE, new UIntPtr(exStyle | (uint) WinAPI.WS_EX.LAYERED));
						WinAPI.SetLayeredWindowAttributes(_Handle, 0, _originalOpacity, 2);
					}
				}
				else if (_transparency) {
					uint crKey;
					uint dwFlags;
					WinAPI.GetLayeredWindowAttributes(_Handle, out crKey, out _originalOpacity, out dwFlags);
					uint exStyle = WinAPI.GetWindowLongPtr(_Handle, (int) WinAPI.GWL.EXSTYLE);
					WinAPI.SetWindowLongPtr(new HandleRef(this, _Handle),
						(int) WinAPI.GWL.EXSTYLE, new UIntPtr(exStyle ^ (uint) WinAPI.WS_EX.LAYERED));
				}
				_transparency = value;
			}
		}
		public bool SetParent (IntPtr handle) {
			if (handle != IntPtr.Zero) {
				Transparency = false;
			}

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

			if (handle == IntPtr.Zero && result) {
				Transparency = true;
			}
			WinAPI.SetWindowPos(_Handle, new IntPtr(0), 0, 0, 500, 300, (uint) (
				WinAPI.SWP.SHOWWINDOW | WinAPI.SWP.FRAMECHANGED | WinAPI.SWP.DRAWFRAME
				));
			Embeddable = false;
			return result;
		}
	}
}