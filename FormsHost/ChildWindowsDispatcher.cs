using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
//-----------------------------------------------------------------------------
namespace FormsHost {
	class ChildWindowsDispatcher {
		public ChildWindowsDispatcher (IntPtr handle, Window window) {
			Handle = handle;
			Window = window;
			_childEntries = new List<ChildEntry>();
			_handleRef = new HandleRef(this, handle);
		}
		public readonly Window Window;
		public readonly IntPtr Handle;
		bool _hasEmbeddedChild = false;
		bool _hasPopupChild = false;
		StreamWriter _streamWriter = null;
		//-----------------------------------------------------------------
		List<ChildEntry> _childEntries;
		//-----------------------------------------------------------------
		public void AddChild (ISystemWindow sysWindow, ShadowCanvas canvas) {
			lock (_childEntries) {
				//
				if (_childEntries.Count == 0) {
		//			_streamWriter = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\WinAPILog.txt", false);
				}
				//
				_childEntries.Add(new ChildEntry() { ChildWindow = sysWindow, Canvas = canvas });
			}
			if (sysWindow.IsPositionGlobal) {
				_hasPopupChild = true;
			}
			else {
				_hasEmbeddedChild = true;
			}
			ZindexRecount();
			if (!_focusTrackerEnabled && sysWindow.NeedFocusTracking) {
				Thread t = new Thread(FocusTracker);
				t.IsBackground = true;
				t.Start();
			}
			_lastFocusHandle = (IntPtr) (-1);
			CorrectOrderPopup();
			CorrectOrderEmbedded();
		}
		//-----------------------------------------------------------------
		public void RemoveChild (ISystemWindow sysWindow) {
			lock (_childEntries) {
				_childEntries.RemoveAll(ce => ce.ChildWindow.Handle == sysWindow.Handle);
				//
				if (_streamWriter != null && _childEntries.Count == 0) {
					_streamWriter.Close();
					_streamWriter = null;
				}
				//
				if (_childEntries.Count > 0) {
					_hasPopupChild = _childEntries.Any(c => c.ChildWindow.IsPositionGlobal);
					_hasEmbeddedChild = _childEntries.Any(c => !c.ChildWindow.IsPositionGlobal);
				}
				else {
					_hasPopupChild = false;
					_hasEmbeddedChild = false;
				}
			}
			ZindexRecount();
			lock (_childEntries) {
				if (_focusTrackerEnabled && sysWindow.NeedFocusTracking &&
						_childEntries.All(ce => !ce.ChildWindow.NeedFocusTracking)) {
					_focusTrackerEnabled = false;
				}
			}
			_lastFocusHandle = (IntPtr) (-1);
		}
		//-----------------------------------------------------------------
		void AddLog (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam) {
			if (_streamWriter != null) {
				_streamWriter.WriteLine("" + DateTime.Now.Minute + ":" +
					DateTime.Now.Second + ":" + DateTime.Now.Millisecond + "\t|\t" +
					msg.ToString("X") + "\t|\t" + wParam.ToInt64().ToString("X8") +
					(wParam.ToInt64().ToString("X").Length < 8 ? "\t\t|\t" : "\t|\t") +
					lParam.ToInt64().ToString("X"));
			}
		}
		//-----------------------------------------------------------------
		bool _deactivationMode = false;
		public IntPtr WndProc (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			//AddLog(hwnd, msg, wParam, lParam);
			switch ((uint) msg) {
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
				case WinAPI.WM.WINDOWPOSCHANGING:
					WinAPI.WINDOWPOS pos = (WinAPI.WINDOWPOS) Marshal.PtrToStructure(lParam, typeof(WinAPI.WINDOWPOS));
					if ((pos.flags & WinAPI.SWP.NOZORDER) != WinAPI.SWP.NOZORDER &&
						(pos.flags & WinAPI.SWP.NOACTIVATE) != WinAPI.SWP.NOACTIVATE) {
						pos.flags = pos.flags | WinAPI.SWP.NOZORDER;
						Marshal.StructureToPtr(pos, lParam, true);
						CorrectOrderPopup();
					}
					break;
				case WinAPI.WM.SYSCOMMAND:
					if (wParam == (IntPtr) 0x0000F012) {
						_focFlag = 2;
					}
					if (wParam == (IntPtr) WinAPI.SC.MINIMIZE) {
						if (FormHostMinimize != null) {
							FormHostMinimize(false);
						}
					}
					else if (wParam == (IntPtr) WinAPI.SC.RESTORE) {
						if (FormHostMinimize != null) {
							FormHostMinimize(true);
						}
					}
					break;
				case WinAPI.WM.MOVE:
					if (FormHostMove != null) {
						try {
							int x = unchecked((short) lParam);
							int y = unchecked((short) ((uint) lParam >> 16));
							FormHostMove(x, y, true);
						}
						catch (OverflowException e) { }
					}
					break;
				default:
					break;
			}
			return IntPtr.Zero;
		}
		//-----------------------------------------------------------------
		public event Action<int, int, bool> FormHostMove;
		//-----------------------------------------------------------------
		public event Action<bool> FormHostMinimize;
		//-----------------------------------------------------------------
		HandleRef _handleRef;
		IntPtr _lastFocusHandle = (IntPtr) (-1);
		bool _focusTrackerEnabled = false;
		void FocusTracker () {
			_focusTrackerEnabled = true;
			while (_focusTrackerEnabled) {
				Thread.Sleep(50);
				IntPtr handle = WinAPI.GetForegroundWindow();
				if (handle == _lastFocusHandle) {
					continue;
				}
				if (handle == Handle) {
					_lastFocusHandle = handle;
					continue;
				}
				bool flag;
				lock (_childEntries) {
					flag = _childEntries.Any(ce => ce.ChildWindow.Handle == handle);
				}
				if (!flag) {
					_deactivationMode = true;
					WinAPI.SendMessage(_handleRef, WinAPI.WM.NCACTIVATE, IntPtr.Zero, IntPtr.Zero);
				}
				else {
					WinAPI.SendMessage(_handleRef, WinAPI.WM.NCACTIVATE, (IntPtr) 1, IntPtr.Zero);
					CorrectOrderPopup();
					CorrectOrderEmbedded();
				}
				_lastFocusHandle = handle;
			}
		}
		//-----------------------------------------------------------------
		public void ZindexRecount () {
			List<DependencyObject> visualTree = new List<DependencyObject>();
			Action<DependencyObject> meth = null;
			meth = delegate(DependencyObject obj) {
				if (obj is ShadowCanvas) {
					visualTree.Add(obj);
				}
				for (int i = 0 ; i < VisualTreeHelper.GetChildrenCount(obj) ; i++) {
					meth(VisualTreeHelper.GetChild(obj, i));
				}
			};
			meth(Window);
			lock (_childEntries) {
				for (int i = 0 ; i < visualTree.Count ; i++) {
					if (_childEntries.Any(ce => visualTree[i] == ce.Canvas)) {
						_childEntries.Single(ce => visualTree[i] == ce.Canvas).Zindex = i;
					}
				}
			}
		}
		//-----------------------------------------------------------------
		void CorrectOrderEmbedded () {
			if (_hasEmbeddedChild) {
				IntPtr[] handles;
				lock (_childEntries) {
					handles = _childEntries.Where(c => !c.ChildWindow.IsPositionGlobal).
						OrderByDescending(c => c.Zindex).Select(c => c.ChildWindow.Handle).ToArray();
				}
				WinAPI.SetWindowPos(handles[0], WinAPI.HWND.TOP, 0, 0, 0, 0,
					WinAPI.SWP.NOMOVE | WinAPI.SWP.NOSIZE | WinAPI.SWP.NOACTIVATE);
				for (int i = 1 ; i < handles.Length ; i++) {
					WinAPI.SetWindowPos(handles[i], handles[i - 1], 0, 0, 0, 0,
						WinAPI.SWP.NOMOVE | WinAPI.SWP.NOSIZE | WinAPI.SWP.NOACTIVATE);
				}
			}
		}
		//-----------------------------------------------------------------
		void CorrectOrderPopup () {
			uint exStyle = WinAPI.GetWindowLongPtr(Handle, WinAPI.GWL.EXSTYLE);
			bool topMost = (exStyle & WinAPI.WS_EX.TOPMOST) == WinAPI.WS_EX.TOPMOST;
			if (_hasPopupChild) {
				IntPtr[] handles;
				lock (_childEntries) {
					handles = _childEntries.Where(c => c.ChildWindow.IsPositionGlobal).
						OrderByDescending(c => c.Zindex).Select(c => c.ChildWindow.Handle).ToArray();
				}
				WinAPI.SetWindowPos(handles[0], WinAPI.HWND.TOPMOST, 0, 0, 0, 0,
					WinAPI.SWP.NOMOVE | WinAPI.SWP.NOSIZE | WinAPI.SWP.NOACTIVATE);
				if (!topMost) {
					WinAPI.SetWindowPos(handles[0], WinAPI.HWND.NOTOPMOST, 0, 0, 0, 0,
						WinAPI.SWP.NOMOVE | WinAPI.SWP.NOSIZE | WinAPI.SWP.NOACTIVATE);
				}
				for (int i = 1 ; i < handles.Length ; i++) {
					WinAPI.SetWindowPos(handles[i], handles[i - 1], 0, 0, 0, 0,
						WinAPI.SWP.NOMOVE | WinAPI.SWP.NOSIZE | WinAPI.SWP.NOACTIVATE);
				}
				WinAPI.SetWindowPos(Handle, handles[handles.Length - 1], 0, 0, 0, 0,
					WinAPI.SWP.NOMOVE | WinAPI.SWP.NOSIZE | WinAPI.SWP.NOACTIVATE);
			}
			else {
				WinAPI.SetWindowPos(Handle, WinAPI.HWND.TOP, 0, 0, 0, 0,
					WinAPI.SWP.NOMOVE | WinAPI.SWP.NOSIZE | WinAPI.SWP.NOACTIVATE);
			}
		}
		//-----------------------------------------------------------------
		class ChildEntry {
			public ISystemWindow ChildWindow;
			public ShadowCanvas Canvas;
			public int Zindex;
		}
	}
}