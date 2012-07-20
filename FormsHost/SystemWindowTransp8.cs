using System;
using System.Runtime.InteropServices;

namespace FormsHost {
	public class SystemWindowTransp8 : SystemWindowChild, ISystemWindow {
		public SystemWindowTransp8 (IntPtr handle, IntPtr formsHostHandle, bool setEmbeddable) :
			base(handle, formsHostHandle, setEmbeddable) { }
		bool _transparency = true;
		byte _originalOpacity = 255;
		uint _originalCrKey = 0;
		uint _originalDwFlags = 2;
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
						bool asdf = WinAPI.SetLayeredWindowAttributes(_Handle, _originalCrKey, _originalOpacity, _originalDwFlags);
					}
				}
				else if (_transparency) {
					WinAPI.GetLayeredWindowAttributes(_Handle, out _originalCrKey, out _originalOpacity, out _originalDwFlags);
					uint exStyle = WinAPI.GetWindowLongPtr(_Handle, (int) WinAPI.GWL.EXSTYLE);
					IntPtr asdf = WinAPI.SetWindowLongPtr(new HandleRef(this, _Handle),
						(int) WinAPI.GWL.EXSTYLE, new UIntPtr(exStyle ^ (uint) WinAPI.WS_EX.LAYERED));
				}
				_transparency = value;
			}
		}
		public override bool SetParent (IntPtr handle) {
			Transparency = false;
			bool result = base.SetParent(handle);
			Transparency = true;
			return result;
		}
		protected override void ModStyle (ref uint style, ref uint exStyle) {
			base.ModStyle(ref style, ref exStyle);
			exStyle = exStyle | (uint) WinAPI.WS_EX.LAYERED;
		}
	}
}