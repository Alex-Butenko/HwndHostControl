using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Interop;
//-----------------------------------------------------------------------------
namespace FormsHost {
	public partial class FormsHostWindow : Window {
		List<ShadowCanvas> _canvases = new List<ShadowCanvas>();
		public FormsHostWindow () {
			InitializeComponent();
			Show();
			Handle = (new WindowInteropHelper(this)).Handle;
			SubscribeToFocusChange();
		}
		//---------------------------------------------------------------------
		public void AddControl (IntPtr handle) {
			ShadowCanvas canvas = new ShadowCanvas(handle, this);
			GridMain1.Children.Add(canvas);
			_canvases.Add(canvas);
			//canvas.GrabForm();
			//canvas.ReleaseForm();
		}
		//---------------------------------------------------------------------
		void Window_LocationChanged (object sender, EventArgs e) {
			if (OnMove != null) {
				OnMove(this, new CoordinatesChangedEvevtArg(new System.Drawing.Point((int) Left, (int) Top)));
			}
		}
		//---------------------------------------------------------------------
		public event EventHandler<CoordinatesChangedEvevtArg> OnMove;
		public class CoordinatesChangedEvevtArg : EventArgs {
			public readonly System.Drawing.Point NewLocation;
			public CoordinatesChangedEvevtArg (System.Drawing.Point location) {
				NewLocation = location;
			}
		}
		//---------------------------------------------------------------------
		[DllImport("user32.dll")]
		static extern bool SetWindowPos (IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
		static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
		static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
		static readonly IntPtr HWND_TOP = new IntPtr(0);
		const UInt32 SWP_NOSIZE = 0x0001;
		const UInt32 SWP_NOMOVE = 0x0002;
		const UInt32 SWP_SHOWWINDOW = 0x0040;
		const UInt32 SWP_NOACTIVATE = 0x0010;
		//---------------------------------------------------------------------
		void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e) {
			foreach (ShadowCanvas sc in _canvases) {
				sc.Close();
			}
			Environment.Exit(0);
		}












		//---------------------------------------------------------------------
		//---------------------------------------------------------------------
		void OnFocusChange (object src, AutomationFocusChangedEventArgs e) {
			try {
				AutomationElement ae = (AutomationElement) src;
				IntPtr handle = new IntPtr(ae.Current.NativeWindowHandle);
				CorrectOrder(handle);
			}
			catch { }
		}
		//---------------------------------------------------------------------
		void UnsubscribeFocusChange () {
			if (focusHandler != null) {
				Automation.RemoveAutomationFocusChangedEventHandler(focusHandler);
			}
		}
		//---------------------------------------------------------------------
		AutomationFocusChangedEventHandler focusHandler = null;
		//---------------------------------------------------------------------
		void SubscribeToFocusChange () {
			//Automation.AddAutomationFocusChangedEventHandler(OnFocusChange);
		}
		//---------------------------------------------------------------------
		void CorrectOrder (IntPtr handle) {
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
		}
		//---------------------------------------------------------------------
		IntPtr Handle;
		//---------------------------------------------------------------------
	}
}