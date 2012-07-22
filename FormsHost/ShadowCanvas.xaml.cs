using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Threading;
//-----------------------------------------------------------------------------
namespace FormsHost {
	public partial class ShadowCanvas : UserControl {
		ISystemWindow _systemWindow;
		static List<FormHostSet> _formHostSets = new List<FormHostSet>();
		FormHostSet _formHostSet;
		public ShadowCanvas () {
			InitializeComponent();
		}
		//---------------------------------------------------------------------
		public void Init (IntPtr dependentWindowHandle, Window window, Type preferrableType = null) {
			IntPtr handle = (new WindowInteropHelper(window)).Handle;
			if (_formHostSets.All(fhs => fhs.Handle != handle)) {
				_formHostSet = new FormHostSet(handle, window);
				HwndSource source = PresentationSource.FromVisual(window) as HwndSource;
				source.AddHook(_formHostSet.WndProc);
			}
			else {
				_formHostSet = _formHostSets.First(fhs => fhs.Handle == handle);
			}
			if (_systemWindow != null && _grabbed) {
				Release();
				_systemWindow = SystemWindow.GetSystemWindow(dependentWindowHandle, preferrableType);
				Grab();
			}
			else {
				_systemWindow = SystemWindow.GetSystemWindow(dependentWindowHandle, preferrableType);
			}
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
		bool _grabbed = false;
		//---------------------------------------------------------------------
		public void Grab () {
			_systemWindow.Grab(_formHostSet.Handle);
			if (_systemWindow.IsPositionGlobal) {
				_formHostSet.FormHostMove += OnFormHostMove;
				try {
					WinAPI.Point point = new WinAPI.Point(0, 0);
					WinAPI.ClientToScreen(_formHostSet.Handle, ref point);
					OnFormHostMove(point.X, point.Y, true);
				}
				catch { }
			}
			_formHostSet.AddChildHandle(_systemWindow.Handle);
			_grabbed = true;
		}
		//---------------------------------------------------------------------
		public void Release () {
			if (_systemWindow.IsPositionGlobal) {
				_formHostSet.FormHostMove -= OnFormHostMove;
			}
			_systemWindow.Release();
			_formHostSet.RemoveChildHandle(_systemWindow.Handle);
			_grabbed = false;
		}
		//---------------------------------------------------------------------
		public bool ClipByHost { get; set; }
		//---------------------------------------------------------------------
		void OnFormHostMove (int x, int y, bool global) {
			Point point = TransformToAncestor(_formHostSet.Window).Transform(new Point(0, 0));
			if (ClipByHost) {
				int width = (int) ((Panel) _formHostSet.Window.Content).ActualWidth;
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
				return _systemWindow.Handle;
			}
		}
		//---------------------------------------------------------------------
		class FormHostSet {
			public FormHostSet (IntPtr handle, Window window) {
				Handle = handle;
				Window = window;
				_childHandles = new List<IntPtr>();
				_handleRef = new HandleRef(this, handle);
				Thread t = new Thread(FocusTracker);
				t.IsBackground = true;
				t.Start();
			}
			public readonly Window Window;
			public readonly IntPtr Handle;
			List<IntPtr> _childHandles;
			public void AddChildHandle (IntPtr handle) {
				_childHandles.Add(handle);
				_lastFocusHandle = (IntPtr) (-1);
			}
			public void RemoveChildHandle (IntPtr handle) {
				_childHandles.Remove(handle);
				_lastFocusHandle = (IntPtr) (-1);
			}
			bool _deactivationMode = false;
			public IntPtr WndProc (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
				if (msg == (int) WinAPI.WM.NCACTIVATE && wParam == IntPtr.Zero) {
					if (_deactivationMode) {
						_deactivationMode = false;
					}
					else {
						handled = true;
						return (IntPtr) 1;
					}
				}
				else if (msg == (int) WinAPI.WM.MOVE) {
					if (FormHostMove != null) {
						int x = unchecked((short) lParam);
						int y = unchecked((short) ((uint) lParam >> 16));
						FormHostMove(x, y, true);
					}
				}
				return IntPtr.Zero;
			}
			public event Action<int, int, bool> FormHostMove;
			//---------------------------------------------------------------------
			HandleRef _handleRef;
			IntPtr _lastFocusHandle = (IntPtr) (-1);
			void FocusTracker () {
				while (true) {
					Thread.Sleep(100);
					IntPtr handle = WinAPI.GetForegroundWindow();
					if (handle == _lastFocusHandle) {
						continue;
					}
					if (handle != Handle && !_childHandles.Contains(handle)) {
						_deactivationMode = true;
						WinAPI.SendMessage(_handleRef, (int) WinAPI.WM.NCACTIVATE, IntPtr.Zero, IntPtr.Zero);
					}
					else {
						WinAPI.SendMessage(_handleRef, (int) WinAPI.WM.NCACTIVATE, (IntPtr) 1, IntPtr.Zero);
						CorrectOrder(handle);
					}
				}
			}
			//---------------------------------------------------------------------
			void CorrectOrder (IntPtr handle) {
				/*
				Action meth = delegate {
					SetWindowPos(Handle, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
					foreach (ShadowCanvas canvas1 in _canvases) {
						SetWindowPos(canvas1.Handle, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
					}
				};
				if (handle == Handle) {
					meth();
					return;
				}
				foreach (ShadowCanvas canvas in _canvases) {
					if (canvas.AllHandles.Contains(handle)) {
						meth();
						return;
					}
				}
				*/
			}
		}
	}
}