using System.Windows.Input;
//-----------------------------------------------------------------------------
namespace HwndHostControl {
	interface IShadowCanvasForDispatcher {
		void RaiseSetFocusEvent ();
		void RaiseKillFocusEvent ();
		void RaiseKeyboardEvent (Key key, bool isPressed, int time);
		bool KeyboardEventsTracking { get; }
		bool MouseEventsTracking { get; }
		bool FocusEventsTracking { get; }
	}
}