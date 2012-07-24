using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//-----------------------------------------------------------------------------
namespace FormsHost {
	class SystemWindowPopup : SystemWindow, ISystemWindow {
		public SystemWindowPopup (IntPtr handle, EmbeddingOptions options) : base(handle, options) {
			TopMost = true;
		}
		//---------------------------------------------------------------------
		protected override void ModStyle (ref uint style, ref uint exStyle) {
			style = (style | WinAPI.WS.POPUP) ^
			(WinAPI.WS.BORDER | WinAPI.WS.SYSMENU | WinAPI.WS.CAPTION |
			WinAPI.WS.THICKFRAME | WinAPI.WS.VISIBLE);
			exStyle = exStyle | WinAPI.WS_EX.TOOLWINDOW | WinAPI.WS_EX.CONTROLPARENT;
		}
		//---------------------------------------------------------------------
		protected int _globalX = 0;
		protected int _globalY = 0;
		public override void OnReposition (WinAPI.Position position) {
			if (position.Global) {
				_globalX = position.GlobalX;
				_globalY = position.GlobalY;
			}
			bool asdf = WinAPI.SetWindowPos(Handle, WinAPI.HWND.TOPMOST,
				position.X + _globalX,
				position.Y + _globalY,
				position.Width, position.Height, (WinAPI.SWP.NOZORDER | WinAPI.SWP.NOACTIVATE));
		}
		//---------------------------------------------------------------------
		public override bool IsPositionGlobal {
			get {
				return true;
			}
		}
	}
}