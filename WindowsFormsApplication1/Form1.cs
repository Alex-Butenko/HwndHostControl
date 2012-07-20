using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace System.Windows.Forms {
	public partial class Form1 : Form {
		IntPtr handle;
		Popup complex;
		public Form1 () {
			InitializeComponent();
			handle = contextMenuStrip1.Handle;
			//ISystemWindow sw = SystemWindow.GetSystemWindow(handle, Handle, false);
			complex = new Popup(new TextBox(), Handle);
			complex.AutoClose = false;
			//NativeWindow nw;
			/*
			ProcessStartInfo psi = new ProcessStartInfo("notepad");
			//ProcessStartInfo psi = new ProcessStartInfo("cmd");
			Process proc = new Process() { StartInfo = psi };
			proc.Start();
			Thread.Sleep(3000);
			handle = proc.MainWindowHandle;
			//SystemWindow sw = new SystemWindow(handle);
			//sw.Embeddable = true;
			*/
			//WinAPI.SetWindowLongPtr(new HandleRef(this, handle), -20, new UIntPtr(exStyle | 0x80000));
			//WinAPI.SetLayeredWindowAttributes(handle, 0, 255 * 80 / 100, 2);
			//ToolStripDropDown tsdd = new ToolStripDropDown();
			//tsdd.Show();
		}
		//bool flag = false;
		/*
		protected override void WndProc (ref Message m) {
			if (m.Msg == (int) WinAPI.WM.ENTERSIZEMOVE) {
				if (!flag) {
					IntPtr asdfasdfasdf = WinAPI.SetParent(handle, Handle);
					Refresh();
					flag = true;
				}
			}
			base.WndProc(ref m);
		}
		*/
		void Form1_Shown (object sender, EventArgs e) {
		}

		void button1_Click (object sender, EventArgs e) {
			complex.Show(sender as Button);
			//uint style = WinAPI.GetWindowLongPtr(complex.Handle, -16);
			//uint exStyle = WinAPI.GetWindowLongPtr(complex.Handle, -20);
		}
		protected override void WndProc (ref System.Windows.Forms.Message m) {
			if (m.Msg == 134) {
				IntPtr hndl = GetParent(Handle);
				if (hndl != IntPtr.Zero) {
					WinAPI.SendMessage(new HandleRef(this, hndl), 134, (IntPtr) 1, (IntPtr) (-1));
					return;
				}
			}
			base.WndProc(ref m);
		}
		//---------------------------------------------------------------------
		[DllImport("user32.dll")]
		static extern IntPtr GetParent (IntPtr handle);
	}
}