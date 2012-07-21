using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Interop;
//-----------------------------------------------------------------------------
namespace FormsHost {
	public partial class FormsHostWindow : Window {
		List<ShadowCanvas> _canvases = new List<ShadowCanvas>();
		public FormsHostWindow () {
			InitializeComponent();
			Show();
		}
		//---------------------------------------------------------------------
		public void AddControl (IntPtr handle) {
			ShadowCanvas canvas = new ShadowCanvas(handle, this, true);
			GridMain1.Children.Add(canvas);
			_canvases.Add(canvas);
		}
		//---------------------------------------------------------------------
		void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e) {
			foreach (ShadowCanvas sc in _canvases) {
				sc.Release();
			}
			Environment.Exit(0);
		}
	}
}