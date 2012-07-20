using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Windows.Forms {
	public partial class Popup : ToolStripDropDown {
		IntPtr _handle;
		public Popup (Control content, IntPtr handle) {
			InitializeComponent();
			_handle = handle;
			content.MinimumSize = content.Size;
			MinimumSize = content.MinimumSize;
			Size = content.Size;
			content.Location = Point.Empty;
			ToolStripControlHost _host = new ToolStripControlHost(content);
			Items.Add(_host);
		}
		public void Show (Control control) {
			Show(control, control.ClientRectangle);
			uint style = GetWindowLongPtr(Handle, -16);
			uint exStyle = GetWindowLongPtr(Handle, -20);
		}
		public void Show (Control control, Rectangle area) {
			Point location = control.PointToScreen(new Point(area.Left, area.Top + area.Height));
			Rectangle screen = Screen.FromControl(control).WorkingArea;
			if (location.X + Size.Width > (screen.Left + screen.Width)) {
				location.X = (screen.Left + screen.Width) - Size.Width;
			}
			if (location.Y + Size.Height > (screen.Top + screen.Height)) {
				location.Y -= Size.Height + area.Height;
			}
			location = control.PointToClient(location);
			Show(control, location, ToolStripDropDownDirection.BelowRight);
		}
		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowLongPtr (IntPtr hWnd, int nIndex);
		protected override void WndProc (ref Message m) {
			if (m.Msg != 134) {
				base.WndProc(ref m);
			}
			else {
				SendMessage(new HandleRef(this, _handle), 134, (IntPtr) 1, (IntPtr)(-1));
				//WmNCActivate(ref m);
			}
		}
		void WmNCActivate (ref Message m) {
			if (m.WParam != IntPtr.Zero) {
				Type bt = GetType().BaseType;
				FieldInfo sam = bt.GetField("sendingActivateMessage", BindingFlags.Instance | BindingFlags.NonPublic);
				if (!(bool)sam.GetValue(this)) {
					sam.SetValue(this, true);
					try {
						/*
						Type TSManager = typeof(ToolStripManager);
						Type MMF = TSManager.GetNestedType("ModalMenuFilter", BindingFlags.NonPublic | BindingFlags.Static);
						HandleRef activeHwnd = (HandleRef)MMF.GetProperty("ActiveHwnd", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null);
						*/
						//SendMessage(activeHwnd, 134, (IntPtr) 1, (IntPtr)(-1));
						SendMessage(new HandleRef(this, _handle), 134, (IntPtr) 1, (IntPtr)(-1));
						//RedrawWindow(activeHwnd, null, new HandleRef(null, IntPtr.Zero), 1025);
						//m.WParam = (IntPtr) 1;
					}
					finally {
						sam.SetValue(this, false);
					}
				}
				//this.DefWndProc(ref m);
				return;
			}
			//base.WndProc(ref m);
		}


		/*
		void WmNCActivate (ref Message m) {
			if (m.WParam != IntPtr.Zero) {
				Type bt = GetType().BaseType;
				FieldInfo sam = bt.GetField("sendingActivateMessage", BindingFlags.Instance | BindingFlags.NonPublic);
				if (!(bool)sam.GetValue(this)) {
					sam.SetValue(this, true);
					try {
						Type TSManager = typeof(ToolStripManager);
						Type MMF = TSManager.GetNestedType("ModalMenuFilter", BindingFlags.NonPublic | BindingFlags.Static);
						HandleRef activeHwnd = (HandleRef)MMF.GetProperty("ActiveHwnd", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null);

						//HandleRef activeHwnd = ToolStripManager.ModalMenuFilter.ActiveHwnd;
						SendMessage(activeHwnd, 134, (IntPtr) 1, (IntPtr)(-1));
						RedrawWindow(activeHwnd, null, new HandleRef(null, IntPtr.Zero), 1025);
						m.WParam = (IntPtr) 1;
					}
					finally {
						sam.SetValue(this, false);
					}
				}
				//this.DefWndProc(ref m);
				return;
			}
			//base.WndProc(ref m);
		}
		*/


		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern bool RedrawWindow (HandleRef hwnd, COMRECT rcUpdate, HandleRef hrgnUpdate, int flags);
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage (HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);
		[StructLayout(LayoutKind.Sequential)]
		public class COMRECT {
			public int left;
			public int top;
			public int right;
			public int bottom;
			public COMRECT () {
			}
			public COMRECT (Rectangle r) {
				this.left = r.X;
				this.top = r.Y;
				this.right = r.Right;
				this.bottom = r.Bottom;
			}
			public COMRECT (int left, int top, int right, int bottom) {
				this.left = left;
				this.top = top;
				this.right = right;
				this.bottom = bottom;
			}
			public static COMRECT FromXYWH (int x, int y, int width, int height) {
				return new COMRECT(x, y, x + width, y + height);
			}
			public override string ToString () {
				return string.Concat(new object[]
		{
			"Left = ",
			this.left,
			" Top ",
			this.top,
			" Right = ",
			this.right,
			" Bottom = ",
			this.bottom
		});
			}
		}

	}
}