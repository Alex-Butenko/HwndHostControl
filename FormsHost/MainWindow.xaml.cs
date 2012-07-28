using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
//-----------------------------------------------------------------------------
namespace FormsHost {
	partial class MainWindow : Window {
		List<ShadowCanvas> _canvases = new List<ShadowCanvas>();
		List<Process> _processes = new List<Process>();
		System.Windows.Forms.TextBox _textbox;
		System.Windows.Forms.Form _form;
		//---------------------------------------------------------------------
		public MainWindow () {
			InitializeComponent();
		}
		//---------------------------------------------------------------------
		void Window_Loaded (object sender, RoutedEventArgs e) {
			//Test3();
			Test1();
			//Test2();
			//Test4();
			//Test5();
			//Test6();
			//Test7();
		}
		//---------------------------------------------------------------------
		void Test1 () {
			ProcessStartInfo psi1 = new ProcessStartInfo("notepad");
			Process proc1 = new Process() { StartInfo = psi1 };
			proc1.Start();
			proc1.WaitForInputIdle();
			IntPtr handle1 = proc1.MainWindowHandle;
			uint exStyle1 = WinAPI.GetWindowLongPtr(handle1, -20);
			//WinAPI.SetWindowLongPtr(new HandleRef(this, handle1), -20, new UIntPtr(exStyle1 | WinAPI.WS_EX.LAYERED));
			//WinAPI.SetLayeredWindowAttributes(handle1, 0, 255 * 80 / 100, 2);
			ShadowCanvas scanvas1 = new ShadowCanvas();
			scanvas1.Init(handle1, this, EmbeddingOptions.DontClip);
			scanvas1.Embeddable = true;
			Grid1.Children.Add(scanvas1);
			scanvas1.Grab();
			_canvases.Add(scanvas1);
			_processes.Add(proc1);
		}
		//---------------------------------------------------------------------
		void Test2 () {
			ProcessStartInfo psi2 = new ProcessStartInfo("cmd");
			Process proc2 = new Process() { StartInfo = psi2 };
			proc2.Start();
			Thread.Sleep(500);
			IntPtr handle2 = proc2.MainWindowHandle;
			uint exStyle2 = WinAPI.GetWindowLongPtr(handle2, -20);
			WinAPI.SetWindowLongPtr(new HandleRef(this, handle2), -20, new UIntPtr(exStyle2 | WinAPI.WS_EX.LAYERED));
			WinAPI.SetLayeredWindowAttributes(handle2, 0, 255 * 80 / 100, 2);
			ShadowCanvas scanvas2 = new ShadowCanvas();
			scanvas2.Init(handle2, this, EmbeddingOptions.BestCrossPlatformness);
			scanvas2.Embeddable = true;
			Grid2.Children.Add(scanvas2);
			scanvas2.Grab();
			_canvases.Add(scanvas2);
			_processes.Add(proc2);
		}
		//---------------------------------------------------------------------
		void Test3 () {
			ProcessStartInfo psi3 = new ProcessStartInfo("calc");
			Process proc3 = new Process() { StartInfo = psi3 };
			proc3.Start();
			Thread.Sleep(1000);
			IntPtr handle3 = proc3.MainWindowHandle;
			uint style3 = WinAPI.GetWindowLongPtr(handle3, -16);
			uint exStyle3 = WinAPI.GetWindowLongPtr(handle3, -20);
			WinAPI.SetWindowLongPtr(new HandleRef(this, handle3), -20, new UIntPtr(exStyle3 | WinAPI.WS_EX.LAYERED));
			WinAPI.SetLayeredWindowAttributes(handle3, 0, 255 * 80 / 100, 2);
			ShadowCanvas scanvas3 = new ShadowCanvas();
			scanvas3.Init(handle3, this, EmbeddingOptions.BestCrossPlatformness);
			scanvas3.Embeddable = true;
			uint style33 = WinAPI.GetWindowLongPtr(handle3, -16);
			Grid3.Children.Add(scanvas3);
			scanvas3.Grab();
			_canvases.Add(scanvas3);
			_processes.Add(proc3);
		}
		//---------------------------------------------------------------------
		void Test4 () {
			ProcessStartInfo psi4 = new ProcessStartInfo("write");
			Process proc4 = new Process() { StartInfo = psi4 };
			proc4.Start();
			proc4.WaitForInputIdle();
			IntPtr handle4 = proc4.MainWindowHandle;
			uint style4 = WinAPI.GetWindowLongPtr(handle4, -16);
			uint exStyle4 = WinAPI.GetWindowLongPtr(handle4, -20);
			WinAPI.SetWindowLongPtr(new HandleRef(this, handle4), -20, new UIntPtr(exStyle4 | WinAPI.WS_EX.LAYERED));
			WinAPI.SetLayeredWindowAttributes(handle4, 0, 255 * 80 / 100, 2);
			ShadowCanvas scanvas4 = new ShadowCanvas();
			scanvas4.Init(handle4, this, EmbeddingOptions.ForcedPopup);
			scanvas4.Embeddable = true;
			Grid4.Children.Add(scanvas4);
			scanvas4.Grab();
			_canvases.Add(scanvas4);
			_processes.Add(proc4);
		}
		//---------------------------------------------------------------------
		void Test5 () {
			ProcessStartInfo psi5 = new ProcessStartInfo(@"E:\Projects\Articles\HwndHostControl\WindowsFormsApplication1\bin\Debug\WindowsFormsApplication1.exe");
			Process proc5 = new Process() { StartInfo = psi5 };
			proc5.Start();
			Thread.Sleep(1000);
			IntPtr handle5 = proc5.MainWindowHandle;
			ShadowCanvas scanvas5 = new ShadowCanvas();
			scanvas5.Init(handle5, this, EmbeddingOptions.BestCrossPlatformness);
			scanvas5.Embeddable = true;
			Grid5.Children.Add(scanvas5);
			scanvas5.Grab();
			_canvases.Add(scanvas5);
			_processes.Add(proc5);
		}
		//---------------------------------------------------------------------
		void Test6 () {
			ProcessStartInfo psi6 = new ProcessStartInfo(@"E:\Projects\Articles\HwndHostControl\WpfApplication1\bin\Debug\WpfApplication1.exe");
			Process proc6 = new Process() { StartInfo = psi6 };
			proc6.Start();
			Thread.Sleep(1000);
			IntPtr handle6 = proc6.MainWindowHandle;
			ShadowCanvas scanvas6 = new ShadowCanvas();
			scanvas6.Init(handle6, this, EmbeddingOptions.BestCrossPlatformness);
			scanvas6.Embeddable = true;
			Grid6.Children.Add(scanvas6);
			scanvas6.Grab();
			_canvases.Add(scanvas6);
			_processes.Add(proc6);
		}
		//---------------------------------------------------------------------
		void Test7 () {
			_textbox = new System.Windows.Forms.TextBox();
			_form = new System.Windows.Forms.Form() { ShowInTaskbar = false };
			_form.Controls.Add(_textbox);
			_textbox.Multiline = true;
			_textbox.Dock = System.Windows.Forms.DockStyle.Fill;
			IntPtr handle7 = _form.Handle;
			uint exStyle7 = WinAPI.GetWindowLongPtr(handle7, -20);
			WinAPI.SetWindowLongPtr(new HandleRef(this, handle7), -20, new UIntPtr(exStyle7 | WinAPI.WS_EX.LAYERED));
			WinAPI.SetLayeredWindowAttributes(handle7, 0, 255 * 80 / 100, 2);
			ShadowCanvas scanvas7 = new ShadowCanvas();
			scanvas7.Init(handle7, this, EmbeddingOptions.BestCrossPlatformness);
			scanvas7.Embeddable = true;
			Grid7.Children.Add(scanvas7);
			scanvas7.Grab();
			_canvases.Add(scanvas7);
		}
		//---------------------------------------------------------------------
		void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e) {
			foreach (ShadowCanvas sc in _canvases) {
				sc.Release();
				sc.Embeddable = false;
			}
			Thread.Sleep(2000);
			foreach (Process proc in _processes) {
				try {
					proc.Kill();
				}
				catch { }
			}
			Environment.Exit(0);
		}
	}
}