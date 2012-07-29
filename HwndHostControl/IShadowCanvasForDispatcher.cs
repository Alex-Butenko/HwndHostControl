using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HwndHostControl {
	interface IShadowCanvasForDispatcher {
		void RaiseSetFocus ();
		void RaiseKillFocus ();
		//void RaiseKeyDown ();
		//void RaiseKeyUp ();
		bool KeyboardEventsTracking { get; }
		bool MouseEventsTracking { get; }
		bool FocusEventsTracking { get; }
	}
}