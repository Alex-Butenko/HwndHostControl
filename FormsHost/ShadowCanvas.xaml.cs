using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Runtime.InteropServices;
//-----------------------------------------------------------------------------
namespace FormsHost {
	public partial class ShadowCanvas : UserControl {
		ISystemWindow _systemWindow;
		static List<FormHostSet> _formHostSets = new List<FormHostSet>();
		FormHostSet _formHostSet;
		public ShadowCanvas (IntPtr dependentWindowHandle, Window mainWindow, Type preferrableType = null) {
			InitializeComponent();
			IntPtr handle = (new WindowInteropHelper(mainWindow)).Handle;
			if (_formHostSets.All(fhs => fhs.Handle != handle)) {
				_formHostSet = new FormHostSet(handle, mainWindow);
				HwndSource source = PresentationSource.FromVisual(mainWindow) as HwndSource;
				source.AddHook(_formHostSet.WndProc);
			}
			else {
				_formHostSet = _formHostSets.First(fhs => fhs.Handle == handle);
			}
			_systemWindow = SystemWindow.GetSystemWindow(dependentWindowHandle, preferrableType);
		}
		//---------------------------------------------------------------------
		void UserControl_LayoutUpdated (object sender, EventArgs e) {
			OnFormHostMove(0, 0, false);
		}
		//---------------------------------------------------------------------
		public bool Embeddable {
			get {
				return _systemWindow.Embeddable;
			}
			set {
				_systemWindow.Embeddable = value;
			}
		}
		//---------------------------------------------------------------------
		public void Grab() {
			_systemWindow.Grab(_formHostSet.Handle);
			_formHostSet.FormHostMove += OnFormHostMove;
			try {
				WinAPI.Point point = new WinAPI.Point(0, 0);
				WinAPI.ClientToScreen(_formHostSet.Handle, ref point);
				OnFormHostMove(point.X, point.Y, true);
			}
			catch { }
		}
		//---------------------------------------------------------------------
		public void Release () {
			_formHostSet.FormHostMove -= OnFormHostMove;
			_systemWindow.Release();
			_systemWindow.Embeddable = false;
		}
		//---------------------------------------------------------------------
		public bool ClipByHost { get; set; }
		//---------------------------------------------------------------------
		void OnFormHostMove (int x, int y, bool global) {
			Point point = TransformToAncestor(_formHostSet.Window).Transform(new Point(0, 0));
			if (ClipByHost) {
				int width = (int) ((Panel)_formHostSet.Window.Content).ActualWidth;
				int height = (int) ((Panel) _formHostSet.Window.Content).ActualHeight;
				_systemWindow.OnReposition(new WinAPI.Position(
					(int) point.X,
					(int) point.Y,
					(int) ((point.X + RenderSize.Width < width) ? RenderSize.Width : Width - point.X),
					(int) ((point.Y + RenderSize.Height < height) ? RenderSize.Height : height - point.Y),
					x, y, global));
			}
			else {
				_systemWindow.OnReposition(new WinAPI.Position(
					(int) point.X,
					(int) point.Y,
					(int) RenderSize.Width,
					(int) RenderSize.Height,
					x, y, global));
			}
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
		class FormHostSet {
			public FormHostSet (IntPtr handle, Window window) {
				Handle = handle;
				Window = window;
			}
			public readonly Window Window;
			public readonly IntPtr Handle;
			public IntPtr WndProc (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
				if (msg == (int) WinAPI.WM.NCACTIVATE && wParam == IntPtr.Zero) {
					handled = true;
					return (IntPtr) 1;
				}
				if (msg == (int) WinAPI.WM.MOVE) {
					int x = unchecked((short) lParam);
					int y = unchecked((short) ((uint) lParam >> 16));
					if (FormHostMove != null) {
						FormHostMove(x, y, true);
					}
				}
				return IntPtr.Zero;
			}
			public event Action<int, int, bool> FormHostMove;
		}
	}
}