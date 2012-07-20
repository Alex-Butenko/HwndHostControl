using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication4 {
	public partial class Form2 : Form {
		public Form2 () {
			InitializeComponent();
		}
		protected override void WndProc (ref Message m) {
			if (m.Msg == 134 && m.WParam == IntPtr.Zero) {
//				SendMessage(new HandleRef(this, Handle), 134, (IntPtr) 1, (IntPtr) (-1));
				return;

			}
			base.WndProc(ref m);
		}
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage (HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);
	}
}