using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FormsHost {
	[Flags]
	public enum EmbeddingOptions {
		DontClip = 0x1,
		BestPerformance = 0x2,
		BestCrossPlatformness = 0x4,
		DontSaveTransparency = 0x8,
		DontSaveMenu = 0x10,
		ForcedEmbedded = 0x20,
		ForcedPopup = 0x40,
		KeyboardEvents = 0x80,
		MouseEvents = 0x100,
		FocusEvents = 0x200,
	}
}