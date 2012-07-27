using System;
using System.Runtime.InteropServices;
//-----------------------------------------------------------------------------
namespace FormsHost {
	abstract class SystemWindow {
		public static ISystemWindow GetSystemWindow (IntPtr handle, EmbeddingOptions options) {
			if ((options & EmbeddingOptions.DontClip) == EmbeddingOptions.DontClip) {
				return new SystemWindowPopup(handle, options);
			}
			if ((options & EmbeddingOptions.DontSaveTransparency) == EmbeddingOptions.DontSaveTransparency) {
				return new SystemWindowEmbedded(handle, options);
			}
			uint exStyle = WinAPI.GetWindowLongPtr(handle, WinAPI.GWL.EXSTYLE);
			if ((exStyle & WinAPI.WS_EX.LAYERED) != WinAPI.WS_EX.LAYERED) {
				return new SystemWindowEmbedded(handle, options);
			}
			if ((options & EmbeddingOptions.BestCrossPlatformness) == EmbeddingOptions.BestCrossPlatformness) {
				return new SystemWindowPopup(handle, options);
			}
			if ((options & EmbeddingOptions.BestPerformance) == EmbeddingOptions.BestPerformance) {
				if (Environment.OSVersion.Version.Major == 6 &&
					Environment.OSVersion.Version.Minor == 2) {
					return new SystemWindowTransp8(handle, options & ~EmbeddingOptions.DontSaveMenu);
				}
				else {
					return new SystemWindowPopup(handle, options);
				}
			}
			throw new Exception();
		}
		//---------------------------------------------------------------------
		EmbeddingOptions _embeddingOptions;
		public EmbeddingOptions EmbeddingOptions {
			get {
				return _embeddingOptions;
			}
		}
		//---------------------------------------------------------------------
		uint _originalStyle = 0;
		uint _originalExStyle = 0;
		bool _embeddable = false;
		WinAPI.Position _originalPosition;
		HandleRef _handleRef;
		protected SystemWindow (IntPtr handle, EmbeddingOptions options) {
			Handle = handle;
			_handleRef = new HandleRef(this, Handle);
			_originalStyle = WinAPI.GetWindowLongPtr(Handle, WinAPI.GWL.STYLE);
			_originalExStyle = WinAPI.GetWindowLongPtr(Handle, WinAPI.GWL.EXSTYLE);
			WinAPI.RECT rect;
			WinAPI.GetWindowRect(_handleRef, out rect);
			_originalPosition = new WinAPI.Position(rect);
			_embeddingOptions = options;
		}
		//---------------------------------------------------------------------
		protected bool Visible {
			get {
				uint style = WinAPI.GetWindowLongPtr(Handle, WinAPI.GWL.EXSTYLE);
				return (style & WinAPI.WS.VISIBLE) == WinAPI.WS.VISIBLE;
			}
			set {
				int sw = value ? WinAPI.SW.ShowNA : WinAPI.SW.Hide;
				WinAPI.ShowWindow(Handle, sw);
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
				uint style = WinAPI.GetWindowLongPtr(Handle, WinAPI.GWL.EXSTYLE);
				return (style & WinAPI.WS_EX.TOPMOST) == WinAPI.WS_EX.TOPMOST;
			}
			set {
				IntPtr tp = value ? WinAPI.HWND.TOPMOST : WinAPI.HWND.NOTOPMOST;
				WinAPI.SetWindowPos(Handle, tp, 0, 0, 0, 0, (WinAPI.SWP.NOMOVE | WinAPI.SWP.NOSIZE));
			}
		}
		//---------------------------------------------------------------------
		public IntPtr Handle { get; private set; }
		//---------------------------------------------------------------------
		public virtual void OnReposition (WinAPI.Position position) { }
		//---------------------------------------------------------------------
		public IntPtr[] AllHandles { get; set; }
		//---------------------------------------------------------------------
		protected virtual void ModStyle (ref uint style, ref uint exStyle) { }
		//---------------------------------------------------------------------
		void SetEmbeddable () {
			Visible = false;
			uint modStyle = _originalStyle;
			uint modExStyle = _originalExStyle;
			ModStyle(ref modStyle, ref modExStyle);
			WinAPI.SetWindowLongPtr(new HandleRef(this, Handle),
				WinAPI.GWL.STYLE, new UIntPtr(modStyle));
			WinAPI.SetWindowLongPtr(new HandleRef(this, Handle),
				WinAPI.GWL.EXSTYLE, new UIntPtr(modExStyle));
			_embeddable = true;
			WinAPI.SetWindowPos(Handle, WinAPI.HWND.TOP, 0, 0,
				_originalPosition.Width+1, _originalPosition.Height+1, WinAPI.SWP.NOZORDER);
			Visible = true;
		}
		void SetOriginalStyle () {
			WinAPI.RECT rect;
			WinAPI.GetWindowRect(_handleRef, out rect);
			WinAPI.Position pos = new WinAPI.Position(rect);
			Visible = false;
			WinAPI.SetWindowLongPtr(new HandleRef(this, Handle),
				WinAPI.GWL.STYLE, new UIntPtr(_originalStyle));
			WinAPI.SetWindowLongPtr(new HandleRef(this, Handle),
				WinAPI.GWL.EXSTYLE, new UIntPtr(_originalExStyle));
			_embeddable = false;
			Visible = true;
			WinAPI.SetWindowPos(Handle, WinAPI.HWND.TOP, pos.X, pos.Y,
				pos.Width, pos.Height, WinAPI.SWP.NOZORDER);
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
		public virtual void Grab (IntPtr hostHandle) { }
		//---------------------------------------------------------------------
		public virtual void Release () { }
		//---------------------------------------------------------------------
		public virtual bool IsPositionGlobal {
			get {
				return false;
			}
		}
		//---------------------------------------------------------------------
		public virtual bool NeedFocusTracking {
			get {
				return true;
			}
		}
	}
}