using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace FormsHost {
	partial class MainWindow : Window {
		FormsHostWindow _formsHostWindow;
		public MainWindow () {
			InitializeComponent();
			//	Test1();
			Test2();
		}
		void Test1 () {
			_formsHostWindow = new FormsHostWindow();
			ProcessStartInfo psi = new ProcessStartInfo("notepad");
			Process proc = new Process() { StartInfo = psi };
			proc.Start();
			Thread.Sleep(3000);
			IntPtr handle = proc.MainWindowHandle;
			_formsHostWindow.AddControl(handle);
		}
		void Test2 () {
			_formsHostWindow = new FormsHostWindow();
			ProcessStartInfo psi = new ProcessStartInfo("notepad");
			//ProcessStartInfo psi = new ProcessStartInfo(@"E:\Projects\Articles\Example1\WindowsFormsApplication1\bin\Debug\WindowsFormsApplication1.exe");
			//ProcessStartInfo psi = new ProcessStartInfo("cmd");
			Process proc = new Process() { StartInfo = psi };
			proc.Start();
			Thread.Sleep(1000);
			IntPtr handle = proc.MainWindowHandle;
			uint exStyle = WinAPI.GetWindowLongPtr(handle, -20);
			WinAPI.SetWindowLongPtr(new HandleRef(this, handle), -20, new UIntPtr(exStyle | WinAPI.WS_EX.LAYERED));
			WinAPI.SetLayeredWindowAttributes(handle, 0, 255 * 80 / 100, 2);
			_formsHostWindow.AddControl(handle);
			Action meth = () => {
				Thread.Sleep(5000);
				Action meth2 = () => _formsHostWindow.AddControl(handle);
				Dispatcher.Invoke(meth2);
			};
			//meth.BeginInvoke(null, null);
		}
	}
}