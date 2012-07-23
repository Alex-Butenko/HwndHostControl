using System;
using System.Collections.Generic;
using System.Windows;
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
			ShadowCanvas canvas = new ShadowCanvas();
			canvas.Init(handle, this, 
				EmbeddingOptions.DontSaveMenu | EmbeddingOptions.BestCrossPlatformness);
			GridMain1.Children.Add(canvas);
			_canvases.Add(canvas);
			canvas.Embeddable = true;
			canvas.Grab();
		}
		//---------------------------------------------------------------------
		void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e) {
			foreach (ShadowCanvas sc in _canvases) {
				sc.Release();
				sc.Embeddable = false;
			}
			Environment.Exit(0);
		}
	}
}