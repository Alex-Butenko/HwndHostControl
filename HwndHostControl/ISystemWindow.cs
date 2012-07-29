using System;
//-----------------------------------------------------------------------------
namespace HwndHostControl {
	interface ISystemWindow {
		IntPtr[] AllHandles { get; set; }
		bool Embeddable { get; set; }
		IntPtr Handle { get; }
		void OnReposition (WinAPI.Position position);
		void Grab (IntPtr hostHandle);
		void Release ();
		bool IsPositionGlobal { get; }
		bool NeedFocusTracking { get; }
		EmbeddingOptions EmbeddingOptions { get; }
		bool Visible { get; set; }
	}
}