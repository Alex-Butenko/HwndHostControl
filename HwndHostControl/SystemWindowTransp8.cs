using System;
using System.Runtime.InteropServices;
//-----------------------------------------------------------------------------
namespace HwndHostControl {
	class SystemWindowTransp8 : SystemWindowEmbedded, ISystemWindow {
		public SystemWindowTransp8 (IntPtr handle, EmbeddingOptions options) : base(handle, options) { }
		//---------------------------------------------------------------------
		bool _transparency = true;
		byte _originalOpacity = 255;
		uint _originalCrKey = 0;
		uint _originalDwFlags = 2;
		//---------------------------------------------------------------------
		public bool Transparency {
			get {
				return _transparency;
			}
			set {
				if (value) {
					if (!_transparency) {
						uint exStyle = WinAPI.GetWindowLongPtr(Handle, WinAPI.GWL.EXSTYLE);
						WinAPI.SetWindowLongPtr(new HandleRef(this, Handle),
							WinAPI.GWL.EXSTYLE, new UIntPtr(exStyle | WinAPI.WS_EX.LAYERED));
						bool asdf = WinAPI.SetLayeredWindowAttributes(Handle, _originalCrKey, _originalOpacity, _originalDwFlags);
					}
				}
				else if (_transparency) {
					WinAPI.GetLayeredWindowAttributes(Handle, out _originalCrKey, out _originalOpacity, out _originalDwFlags);
					uint exStyle = WinAPI.GetWindowLongPtr(Handle, WinAPI.GWL.EXSTYLE);
					IntPtr asdf = WinAPI.SetWindowLongPtr(new HandleRef(this, Handle),
						WinAPI.GWL.EXSTYLE, new UIntPtr(exStyle & ~WinAPI.WS_EX.LAYERED));
				}
				_transparency = value;
			}
		}
		//---------------------------------------------------------------------
		public override bool SetParent (IntPtr handle) {
			Transparency = false;
			bool result = base.SetParent(handle);
			Transparency = true;
			return result;
		}
		//---------------------------------------------------------------------
		protected override void ModStyle (ref uint style, ref uint exStyle) {
			base.ModStyle(ref style, ref exStyle);
			exStyle |= WinAPI.WS_EX.LAYERED;
		}
	}
}