using System;
namespace FormsHost {
	public interface ISystemWindow {
		IntPtr[] AllHandles { get; set; }
		bool Embeddable { get; set; }
		IntPtr Handle { get; }
		void OnReposition (WinAPI.Position position);
		void GrabWindow ();
		void ReleaseWindow ();
	}
}