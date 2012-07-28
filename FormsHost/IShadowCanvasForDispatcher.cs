using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FormsHost {
	interface IShadowCanvasForDispatcher {
		void RaiseFocusEnter ();
		void RaiseFocusLeave ();
		//void RaiseKeyDown ();
		//void RaiseKeyUp ();
	}
}