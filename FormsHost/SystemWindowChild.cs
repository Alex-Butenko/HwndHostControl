﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FormsHost {
	public class SystemWindowChild : SystemWindow, ISystemWindow {
		public SystemWindowChild (IntPtr handle) :
			base(handle) { }
		public virtual bool SetParent (IntPtr handle) {
			return WinAPI.SetParent(Handle, handle) != IntPtr.Zero;
		}
		//---------------------------------------------------------------------
		public override void Grab (IntPtr hostHandle) {
			SetParent(hostHandle);
		}
		//---------------------------------------------------------------------
		public override void Release () {
			SetParent(IntPtr.Zero);
		}
		//---------------------------------------------------------------------
		public override void OnReposition (WinAPI.Position position) {
			WinAPI.SetWindowPos(Handle, WinAPI.HWND.TOP, position.X, position.Y,
				position.Width, position.Height + 100, (uint) (WinAPI.SWP.NOZORDER |
				WinAPI.SWP.NOACTIVATE));
		}
		//---------------------------------------------------------------------
		protected override void ModStyle (ref uint style, ref uint exStyle) {
			style = (style) ^
			(uint) (WinAPI.WS.BORDER | WinAPI.WS.SYSMENU | WinAPI.WS.CAPTION |
			WinAPI.WS.THICKFRAME | WinAPI.WS.VISIBLE);
			exStyle = exStyle | (uint) (WinAPI.WS_EX.TOOLWINDOW | WinAPI.WS_EX.CONTROLPARENT);
		}
	}
}