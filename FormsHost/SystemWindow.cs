using System;
using System.Runtime.InteropServices;
//-----------------------------------------------------------------------------
namespace FormsHost {
	public abstract class SystemWindow : System.Windows.Forms.NativeWindow {
		public static ISystemWindow GetSystemWindow (IntPtr handle, IntPtr formsHostHandle, bool setEmbeddable) {
			uint exStyle = WinAPI.GetWindowLongPtr(handle, (int) WinAPI.GWL.EXSTYLE);
			//
			return new SystemWindowTransp8(handle, formsHostHandle, setEmbeddable);
			//
			if ((exStyle & (uint) WinAPI.WS_EX.LAYERED) != 0) {
				if (Environment.OSVersion.Version.Major == 6 &&
					Environment.OSVersion.Version.Minor == 2) {
					return new SystemWindowTransp8(handle, formsHostHandle, setEmbeddable);
				}
				else {
					return new SystemWindowTranspXPVst7(handle, formsHostHandle, setEmbeddable);
				}
			}
			else {
				return new SystemWindowNotTransparent(handle, formsHostHandle, setEmbeddable);
			}
		}
		//---------------------------------------------------------------------
		uint _originalStyle = 0;
		uint _originalExStyle = 0;
		bool _embeddable = false;
		WinAPI.Position _originalPosition;
		HandleRef _handleRef;
		protected SystemWindow (IntPtr handle, IntPtr formsHostHandle, bool setEmbeddable) {
			_Handle = handle;
			_formsHostHandle = formsHostHandle;
			_handleRef = new HandleRef(this, _Handle);
			_originalStyle = WinAPI.GetWindowLongPtr(_Handle, (int) WinAPI.GWL.STYLE);
			_originalExStyle = WinAPI.GetWindowLongPtr(_Handle, (int) WinAPI.GWL.EXSTYLE);
			WinAPI.RECT rect;
			WinAPI.GetWindowRect(_handleRef, out rect);
			_originalPosition = new WinAPI.Position(rect);
		}
		//---------------------------------------------------------------------
		protected bool Visible {
			get {
				uint style = WinAPI.GetWindowLongPtr(_Handle, (int) WinAPI.GWL.EXSTYLE);
				return (style & (uint) WinAPI.WS.VISIBLE) != 0;
			}
			set {
				WinAPI.SW sw = value ? WinAPI.SW.ShowNA : WinAPI.SW.Hide;
				WinAPI.ShowWindow(_Handle, sw);
				/*
				SWP swp = value ? SWP.SHOWWINDOW : SWP.HIDEWINDOW;
				SetWindowPos(Handle, HWND.TOP, 0, 0, 0, 0,
					(uint) (SWP.NOMOVE | SWP.NOSIZE | SWP.NOZORDER | swp));
				*/
			}
		}
		//---------------------------------------------------------------------
		protected bool TopMost {
			get {
				uint style = WinAPI.GetWindowLongPtr(_Handle, (int) WinAPI.GWL.EXSTYLE);
				return (style & (uint) WinAPI.WS_EX.TOPMOST) != 0;
			}
			set {
				IntPtr tp = value ? WinAPI.HWND.TOPMOST : WinAPI.HWND.NOTOPMOST;
				WinAPI.SetWindowPos(_Handle, tp, 0, 0, 0, 0, (uint) (WinAPI.SWP.NOMOVE | WinAPI.SWP.NOSIZE));
			}
		}
		//---------------------------------------------------------------------
		public IntPtr _Handle { get; private set; }
		//---------------------------------------------------------------------
		protected IntPtr _formsHostHandle;
		//---------------------------------------------------------------------
		public virtual void OnReposition (WinAPI.Position position) { }
		//---------------------------------------------------------------------
		public IntPtr[] AllHandles { get; set; }
		//---------------------------------------------------------------------
		/*
		WinAPI.WindowState State {
			get {
				uint style = WinAPI.GetWindowLongPtr(Handle, (int) WinAPI.GWL.STYLE);
				if ((style & (uint) WinAPI.WS.MAXIMIZE) != 0) {
					return WinAPI.WindowState.Maximized;
				}
				else if ((style & (int) WinAPI.WS.MINIMIZE) != 0) {
					return WinAPI.WindowState.Minimized;
				}
				else {
					return WinAPI.WindowState.Normal;
				}
			}
		}
		*/
		//---------------------------------------------------------------------
		protected virtual void ModStyle (ref uint style, ref uint exStyle) { }
		void SetEmbeddable () {
			WinAPI.RECT rect;
			WinAPI.GetWindowRect(_handleRef, out rect);
			WinAPI.Position pos = new WinAPI.Position(rect);
			Visible = false;
			uint modStyle = _originalStyle;
			uint modExStyle = _originalExStyle;
			ModStyle(ref modStyle, ref modExStyle);
			WinAPI.SetWindowLongPtr(new HandleRef(this, Handle),
				(int) WinAPI.GWL.STYLE, new UIntPtr(modStyle));
			WinAPI.SetWindowLongPtr(new HandleRef(this, Handle),
				(int) WinAPI.GWL.EXSTYLE, new UIntPtr(modExStyle));
			/*
			WinAPI.SetWindowLongPtr(new HandleRef(this, _Handle),
				(int) WinAPI.GWL.STYLE, new UIntPtr(
				(uint) (WinAPI.WS.POPUP | WinAPI.WS.VISIBLE
				| WinAPI.WS.CLIPCHILDREN | WinAPI.WS.CLIPSIBLINGS |
				WinAPI.WS.TABSTOP)));
			WinAPI.SetWindowLongPtr(new HandleRef(this, _Handle),
				(int) WinAPI.GWL.EXSTYLE, new UIntPtr((uint) (WinAPI.WS_EX.CONTROLPARENT |
					WinAPI.WS_EX.LAYERED | WinAPI.WS_EX.TOOLWINDOW)));
			*/
			_embeddable = true;
			Visible = true;
			WinAPI.SetWindowPos(_Handle, WinAPI.HWND.TOP, pos.X, pos.Y,
				pos.Width, pos.Height, (uint) WinAPI.SWP.NOZORDER);
		}
		void SetOriginalStyle () {
			WinAPI.RECT rect;
			WinAPI.GetWindowRect(_handleRef, out rect);
			WinAPI.Position pos = new WinAPI.Position(rect);
			Visible = false;
			WinAPI.SetWindowLongPtr(new HandleRef(this, _Handle),
				(int)WinAPI.GWL.STYLE, new UIntPtr(_originalStyle));
			WinAPI.SetWindowLongPtr(new HandleRef(this, _Handle),
				(int)WinAPI.GWL.EXSTYLE, new UIntPtr(_originalExStyle));
			_embeddable = false;
			Visible = true;
			WinAPI.SetWindowPos(_Handle, WinAPI.HWND.TOP, pos.X, pos.Y,
				pos.Width, pos.Height, (uint) WinAPI.SWP.NOZORDER);
		}
		public bool Embeddable {
			get {
				return _embeddable;
			}
			set {
				if (value) {
					if (!_embeddable) {
						SetEmbeddable();
					}
				}
				else if(_embeddable) {
					SetOriginalStyle();
				}
			}
		}
		//---------------------------------------------------------------------
		public bool Close () {
			return WinAPI.CloseWindow(_Handle);
		}
		//---------------------------------------------------------------------
		public virtual void GrabWindow () { }
		//---------------------------------------------------------------------
		public virtual void ReleaseWindow () { }
		//---------------------------------------------------------------------
	}
}