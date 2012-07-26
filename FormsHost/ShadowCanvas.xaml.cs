using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
//-----------------------------------------------------------------------------
namespace FormsHost {
	public partial class ShadowCanvas : UserControl {
		ISystemWindow _systemWindow;
		static List<ChildWindowsDispatcher> _childDispatchers = new List<ChildWindowsDispatcher>();
		ChildWindowsDispatcher _childDispatcher;
		public ShadowCanvas () {
			InitializeComponent();
		}
		//---------------------------------------------------------------------
		public void Init (IntPtr dependentWindowHandle, Window window,
				EmbeddingOptions options = EmbeddingOptions.BestPerformance) {
			IntPtr handle = (new WindowInteropHelper(window)).Handle;
			if (_childDispatchers.All(fhs => fhs.Handle != handle)) {
				_childDispatcher = new ChildWindowsDispatcher(handle, window);
				HwndSource source = PresentationSource.FromVisual(window) as HwndSource;
				source.AddHook(_childDispatcher.WndProc);
			}
			else {
				_childDispatcher = _childDispatchers.First(fhs => fhs.Handle == handle);
			}
			if (_systemWindow != null && _grabbed) {
				Release();
				_systemWindow = SystemWindow.GetSystemWindow(dependentWindowHandle, options);
				Grab();
			}
			else {
				_systemWindow = SystemWindow.GetSystemWindow(dependentWindowHandle, options);
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
			_systemWindow.Grab(_childDispatcher.Handle);
			if (_systemWindow.IsPositionGlobal) {
				_childDispatcher.FormHostMove += OnFormHostMove;
				try {
					WinAPI.Point point = new WinAPI.Point(0, 0);
					WinAPI.ClientToScreen(_childDispatcher.Handle, ref point);
					OnFormHostMove(point.X, point.Y, true);
				}
				catch { }
			}
			_childDispatcher.AddChild(_systemWindow, this);
			_grabbed = true;
		}
		//---------------------------------------------------------------------
		public void Release () {
			if (_systemWindow.IsPositionGlobal) {
				_childDispatcher.FormHostMove -= OnFormHostMove;
			}
			_systemWindow.Release();
			_childDispatcher.RemoveChild(_systemWindow);
			_grabbed = false;
		}
		//---------------------------------------------------------------------
		void OnFormHostMove (int x, int y, bool global) {
			Point point = TransformToAncestor(_childDispatcher.Window).Transform(new Point(0, 0));
			if ((_systemWindow.EmbeddingOptions & EmbeddingOptions.DontClip) == EmbeddingOptions.DontClip) {
				_systemWindow.OnReposition(new WinAPI.Position(
					(int) point.X,
					(int) point.Y,
					(int) RenderSize.Width,
					(int) RenderSize.Height,
					x, y, global));
			}
			else {
				int width = (int) ((Panel) _childDispatcher.Window.Content).ActualWidth;
				int height = (int) ((Panel) _childDispatcher.Window.Content).ActualHeight;
				_systemWindow.OnReposition(new WinAPI.Position(
					(int) point.X,
					(int) point.Y,
					(int) ((point.X + RenderSize.Width < width) ? RenderSize.Width : width - point.X),
					(int) ((point.Y + RenderSize.Height < height) ? RenderSize.Height : height - point.Y),
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
		class ChildWindowsDispatcher {
			public ChildWindowsDispatcher (IntPtr handle, Window window) {
				Handle = handle;
				Window = window;
				_childEntries = new List<ChildEntry>();
				_handleRef = new HandleRef(this, handle);
			}
			public readonly Window Window;
			public readonly IntPtr Handle;
			List<ChildEntry> _childEntries;
			//-----------------------------------------------------------------
			public void AddChild (ISystemWindow sysWindow, ShadowCanvas canvas) {
				_childEntries.Add(new ChildEntry() { ChildWindow = sysWindow, Canvas = canvas });
				if (!_focusTrackerEnabled && sysWindow.NeedFocusTracking) {
					Thread t = new Thread(FocusTracker);
					t.IsBackground = true;
					t.Start();
				}
				_lastFocusHandle = (IntPtr) (-1);
			}
			//-----------------------------------------------------------------
			public void RemoveChild (ISystemWindow sysWindow) {
				_childEntries.RemoveAll(ce => ce.ChildWindow.Handle == sysWindow.Handle);
				if (_focusTrackerEnabled && sysWindow.NeedFocusTracking &&
						_childEntries.All(ce => !ce.ChildWindow.NeedFocusTracking)) {
					_focusTrackerEnabled = false;
				}
				_lastFocusHandle = (IntPtr) (-1);
			}
			//-----------------------------------------------------------------
			bool _deactivationMode = false;
			public IntPtr WndProc (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
				switch ((uint)msg) {
					case WinAPI.WM.NCACTIVATE:
						if (wParam == IntPtr.Zero && _focusTrackerEnabled) {
							if (_deactivationMode) {
								_deactivationMode = false;
							}
							else {
								handled = true;
								return (IntPtr) 1;
							}
						}
						break;
					case WinAPI.WM.MOVE:
						if (FormHostMove != null) {
							int x = unchecked((short) lParam);
							int y = unchecked((short) ((uint) lParam >> 16));
							FormHostMove(x, y, true);
						}
						break;
				}
				return IntPtr.Zero;
			}
			//-----------------------------------------------------------------
			public event Action<int, int, bool> FormHostMove;
			//-----------------------------------------------------------------
			HandleRef _handleRef;
			IntPtr _lastFocusHandle = (IntPtr) (-1);
			bool _focusTrackerEnabled = false;
			void FocusTracker () {
				_focusTrackerEnabled = true;
				while (_focusTrackerEnabled) {
					Thread.Sleep(100);
					IntPtr handle = WinAPI.GetForegroundWindow();
					if (handle == _lastFocusHandle) {
						continue;
					}
					if (handle != Handle && _childEntries.All(ce => ce.ChildWindow.Handle != handle)) {
						_deactivationMode = true;
						WinAPI.SendMessage(_handleRef, WinAPI.WM.NCACTIVATE, IntPtr.Zero, IntPtr.Zero);
					}
					else {
						WinAPI.SendMessage(_handleRef, WinAPI.WM.NCACTIVATE, (IntPtr) 1, IntPtr.Zero);
						CorrectOrderPopup();
					}
				}
			}
			//-----------------------------------------------------------------
			public void ZindexRecount () {
				List<DependencyObject> visualTree = new List<DependencyObject>();
				Action<DependencyObject> meth = null;
				meth = delegate (DependencyObject obj) {
					if (obj is ShadowCanvas) {
						visualTree.Add(obj);
					}
					for (int i = 0 ; i < VisualTreeHelper.GetChildrenCount(obj) ; i++) {
						meth(VisualTreeHelper.GetChild(obj, i));
					}
				};
				meth(Window);
				for (int i = 0 ; i < visualTree.Count ; i++) {
					_childEntries.Single(ce => visualTree[i] == ce.Canvas).Zindex = i;
				}
			}
			//-----------------------------------------------------------------
			void CorrectOrderChild () {
			}
			//-----------------------------------------------------------------
			void CorrectOrderPopup () {
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
			//-----------------------------------------------------------------
			class ChildEntry {
				public ISystemWindow ChildWindow;
				public ShadowCanvas Canvas;
				public int Zindex;
			}
		}
	}
}