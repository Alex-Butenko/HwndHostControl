using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
//-----------------------------------------------------------------------------
namespace FormsHost {
	public partial class ShadowCanvas : UserControl, IShadowCanvasForDispatcher {
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
				_childDispatchers.Add(_childDispatcher);
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
				_childDispatcher.FormHostMinimize += OnFormHostMinimize;
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
				_childDispatcher.FormHostMinimize -= OnFormHostMinimize;
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
		void OnFormHostMinimize (bool minimized) {
			_windowVisibleState = minimized;
			_systemWindow.Visible = _windowVisibleState && _windowVisibleUserDef;
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
		public EventHandler FocusEnter;
		//---------------------------------------------------------------------
		public EventHandler FocusLeave;
		//---------------------------------------------------------------------
		bool _windowVisibleUserDef = true;
		bool _windowVisibleState = true;
		public bool WindowVisible {
			get {
				return _windowVisibleUserDef;
			}
			set {
				_windowVisibleUserDef = value;
				_systemWindow.Visible = _windowVisibleUserDef && _windowVisibleState;
			}
		}
		//---------------------------------------------------------------------
		void IShadowCanvasForDispatcher.RaiseFocusEnter () {
			if (FocusEnter != null) {
				Dispatcher.BeginInvoke(FocusEnter, this, new EventArgs());
			}
		}
		//---------------------------------------------------------------------
		void IShadowCanvasForDispatcher.RaiseFocusLeave () {
			if (FocusLeave != null) {
				Dispatcher.BeginInvoke(FocusLeave, this, new EventArgs());
			}
		}
	}
}