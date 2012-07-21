using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
//-----------------------------------------------------------------------------
namespace FormsHost {
	public partial class ShadowCanvas : UserControl {
		ISystemWindow _systemWindow;
		IntPtr _handle ;
		public ShadowCanvas (IntPtr dependentWindowHandle, FormsHostWindow mainWindow) {
			InitializeComponent();
			_handle = (new WindowInteropHelper(mainWindow)).Handle;
			_systemWindow = SystemWindow.GetSystemWindow(dependentWindowHandle, _handle, true);
			_systemWindow.Embeddable = true;
			_systemWindow.GrabWindow();
			HwndSource source = PresentationSource.FromVisual(mainWindow) as HwndSource;
			source.AddHook(WndProc);
		}
		//---------------------------------------------------------------------
		void UserControl_LayoutUpdated (object sender, EventArgs e) {
			Window wnd = Window.GetWindow(this);
			Point point = TransformToAncestor(wnd).Transform(new Point(0, 0));
			_systemWindow.OnReposition(new WinAPI.Position(
				(int) point.X,
				(int) point.Y,
				(int) RenderSize.Width,
				(int) RenderSize.Height, 
				0, 0, false));
		}
		//---------------------------------------------------------------------
		public void Release () {
			_systemWindow.ReleaseWindow();
			_systemWindow.Embeddable = false;
		}
		//---------------------------------------------------------------------
		IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			if (msg == 134 && wParam == IntPtr.Zero) {
				handled = true;
				return (IntPtr) 1;
			}
			if (msg == (int) WinAPI.WM.MOVE) {
				int x = unchecked((short) lParam);
				int y = unchecked((short) ((uint) lParam >> 16));
				Window wnd = Window.GetWindow(this);
				Point point = TransformToAncestor(wnd).Transform(new Point(0, 0));
				_systemWindow.OnReposition(new WinAPI.Position(
					(int) point.X,
					(int) point.Y,
					(int) RenderSize.Width,
					(int) RenderSize.Height,
					x, y, true));
			}
            return IntPtr.Zero;
        }
		//---------------------------------------------------------------------
		public List<IntPtr> AllHandles {
			get {
				/*
				List<IntPtr> handles = _dependentWindow.AllDescendantWindows.Select(w => w.HWnd).ToList();
				handles.Add(_dependentWindow.HWnd);
				return handles;
				*/
				return null;
			}
		}
		//---------------------------------------------------------------------
		public IntPtr Handle {
			get {
				//return _dependentWindow.HWnd;
				return IntPtr.Zero;
			}
		}

		//---------------------------------------------------------------------
	}
}