using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
//-----------------------------------------------------------------------------
namespace FormsHost {
	public partial class ShadowCanvas : UserControl {
		//ManagedWinapi.Windows.SystemWindow _dependentWindow;
		System.Drawing.Point _lastCanvasLocation = System.Drawing.Point.Empty;
		System.Drawing.Point _lastMainWindowLocation = System.Drawing.Point.Empty;
		ISystemWindow _systemWindow;
		IntPtr _handle ;
		public ShadowCanvas (IntPtr dependentWindowHandle, FormsHostWindow mainWindow) {
			InitializeComponent();
			_handle = (new WindowInteropHelper(mainWindow)).Handle;
			_systemWindow = SystemWindow.GetSystemWindow(dependentWindowHandle, _handle, true);
			_systemWindow.Embeddable = true;
			_systemWindow.GrabWindow();
			//meth.BeginInvoke(null, null);
			//HwndSource source = PresentationSource.FromVisual(mainWindow) as HwndSource;
			//source.AddHook(WndProc);
			//_dependentWindow = new ManagedWinapi.Windows.SystemWindow(dependentWindowHandle);
			//mainWindow.OnMove += MainWindow_OnMove;
			//_lastMainWindowLocation = new System.Drawing.Point((int) mainWindow.Left, (int) mainWindow.Top);
			//CorrectLocation();
		}
		//---------------------------------------------------------------------
		void MainWindow_OnMove (object sender, FormsHostWindow.CoordinatesChangedEvevtArg e) {
			/*
			_lastMainWindowLocation = e.NewLocation;
			CorrectLocation();
			*/
		}
		//---------------------------------------------------------------------
		void UserControl_LayoutUpdated (object sender, EventArgs e) {
			Window wnd = Window.GetWindow(this);
			Point point = TransformToAncestor(wnd).Transform(new Point(0, 0));
			//_lastCanvasLocation = new System.Drawing.Point((int) point.X + 8, (int) point.Y + 30);
			_systemWindow.OnReposition(new WinAPI.Position(
				(int) point.X,
				(int) point.Y,
				(int) RenderSize.Width,
				(int) RenderSize.Height));
			//CorrectLocation();
		}
		//---------------------------------------------------------------------
		public void Close () {
			_systemWindow.Close();
		}
		//---------------------------------------------------------------------
		void CorrectLocation () {
			/*
			_dependentWindow.Location = new System.Drawing.Point(
				_lastMainWindowLocation.X + _lastCanvasLocation.X,
				_lastMainWindowLocation.Y + _lastCanvasLocation.Y);
			_dependentWindow.Refresh();
			*/
		}
		/*
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			if (msg == (int) WinAPI.WM.ENTERSIZEMOVE) {
				if (!_grabbed) {
					GrabForm();
				}
			}
			else if (msg == (int) WinAPI.WM.EXITSIZEMOVE) {
				if (_grabbed) {
					ReleaseForm();
				}
			}
            // Handle messages...
            return IntPtr.Zero;
        }
		*/





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