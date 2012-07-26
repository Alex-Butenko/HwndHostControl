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
		FollowZorder,
	}
}