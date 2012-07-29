using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Input;
//-----------------------------------------------------------------------------
namespace HwndHostControl {
	static class Hook {
		delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, [In] IntPtr lParam);
		[DllImport("user32.dll")]
		static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, [In] IntPtr lParam);
		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr SetWindowsHookEx(HookType hookType, LowLevelProc lpfn, IntPtr hMod, int dwThreadId);
		[DllImport("user32.dll", SetLastError = true)]
		static extern bool UnhookWindowsHookEx(IntPtr hhk);
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		static extern int GetWindowThreadProcessId(IntPtr handleWindow, out int lpdwProcessID);
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		static extern IntPtr GetKeyboardLayout(int WindowsThreadProcessID);
		[DllImport("user32.dll", SetLastError = true)]
		static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
		[StructLayout(LayoutKind.Sequential)]
		struct KBDLLHOOKSTRUCT {
			public int vkCode;
			public uint scanCode;
			public KBDLLHOOKSTRUCTFlags flags;
			public int time;
			public IntPtr dwExtraInfo;
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct MSLLHOOKSTRUCT {
			//public Point pt;
			public int mouseData; // be careful, this must be ints, not uints (was wrong before I changed it...). regards, cmew.
			public int flags;
			public int time;
			public UIntPtr dwExtraInfo;
		}
		//[StructLayout(LayoutKind.Sequential)]
		/*
		public struct POINT {
			public int X;
			public int Y;

			public POINT(int x, int y) {
				X = x;
				Y = y;
			}

			public static implicit operator Point(POINT p) {
				return new Point(p.X, p.Y);
			}

			public static implicit operator POINT(Point p) {
				return new POINT(p.X, p.Y);
			}
		}
		*/
		[Flags]
		enum KBDLLHOOKSTRUCTFlags {
			LLKHF_EXTENDED = 0x01,
			LLKHF_INJECTED = 0x10,
			LLKHF_ALTDOWN = 0x20,
			LLKHF_UP = 0x80,
		}
		enum HookType {
			WH_JOURNALRECORD = 0,
			WH_JOURNALPLAYBACK = 1,
			WH_KEYBOARD = 2,
			WH_GETMESSAGE = 3,
			WH_CALLWNDPROC = 4,
			WH_CBT = 5,
			WH_SYSMSGFILTER = 6,
			WH_MOUSE = 7,
			WH_HARDWARE = 8,
			WH_DEBUG = 9,
			WH_SHELL = 10,
			WH_FOREGROUNDIDLE = 11,
			WH_CALLWNDPROCRET = 12,
			WH_KEYBOARD_LL = 13,
			WH_MOUSE_LL = 14
		}
		//---------------------------------------------------------------------
		static Hook() {
			MouseHook = MouseHookProc;
			KbdHook = KbdHookProc;
		}
		//---------------------------------------------------------------------
		static IntPtr hKbdHook = IntPtr.Zero;
		//---------------------------------------------------------------------
		static bool kbdHookInstall = false;
		//---------------------------------------------------------------------
		public delegate void HookKeyPress(Key key, bool isPressed, int time);
		//---------------------------------------------------------------------
		public static event HookKeyPress OnHookKeyPressEventHandler;
		//---------------------------------------------------------------------
		static LowLevelProc KbdHook;
		//---------------------------------------------------------------------
		public static void InstallKbdHook(HookKeyPress hkp) {
			if (hkp == null) {
				return;
			}
			if (IsKbdHookInstalled) {
				return;
			}
			OnHookKeyPressEventHandler = hkp;
			IntPtr hModule = Marshal.GetHINSTANCE(AppDomain.CurrentDomain.GetAssemblies()[0].GetModules()[0]);
			hKbdHook = SetWindowsHookEx(HookType.WH_KEYBOARD_LL, KbdHook, hModule, 0);
			if (hKbdHook != IntPtr.Zero) {
				kbdHookInstall = true;
			}
			else {
				throw new Win32Exception("Can't install low level keyboard hook!");
			}
		}
		//---------------------------------------------------------------------
		public static bool IsKbdHookInstalled {
			get {
				return kbdHookInstall && hKbdHook != IntPtr.Zero;
			}
		}
		//---------------------------------------------------------------------
		public static void UninstallKbdHook() {
			if (IsKbdHookInstalled) {
				if (!UnhookWindowsHookEx(hKbdHook)) {
					throw new Win32Exception("Can't uninstall low level keyboard hook!");
				}
				hKbdHook = IntPtr.Zero;
				kbdHookInstall = false;
			}
		}
		//---------------------------------------------------------------------
		static IntPtr KbdHookProc(int nCode, IntPtr wParam, [In] IntPtr lParam) {
			if (nCode == 0) {
				KBDLLHOOKSTRUCT kbd = (KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
				bool pressed = wParam.ToInt32() == 0x100 || wParam.ToInt32() == 0x104;
				Key key = KeyInterop.KeyFromVirtualKey(kbd.vkCode);
				OnHookKeyPressEventHandler.BeginInvoke(key, pressed, kbd.time, null, null);
			}
			return CallNextHookEx(hKbdHook, nCode, wParam, lParam);
		}
		//---------------------------------------------------------------------
		//---------------------------------------------------------------------
		static IntPtr hMouseHook = IntPtr.Zero;
		//---------------------------------------------------------------------
		static bool mouseHookInstall = false;
		//---------------------------------------------------------------------
		public delegate void HookMouseClick(int time);
		//---------------------------------------------------------------------
		public static event HookMouseClick OnHookMouseClickEventHandler;
		//---------------------------------------------------------------------
		static LowLevelProc MouseHook;
		//---------------------------------------------------------------------
		public static void InstallMouseHook(HookMouseClick hmc) {
			if (hmc == null) {
				return;
			}
			if (IsMouseHookInstalled) {
				return;
			}
			OnHookMouseClickEventHandler = hmc;
			IntPtr hModule = Marshal.GetHINSTANCE(AppDomain.CurrentDomain.GetAssemblies()[0].GetModules()[0]);
			hMouseHook = SetWindowsHookEx(HookType.WH_MOUSE_LL, MouseHook, hModule, 0);
			if (hMouseHook != IntPtr.Zero) {
				mouseHookInstall = true;
			}
			else {
				throw new Win32Exception("Can't install low level keyboard hook!");
			}
		}
		//---------------------------------------------------------------------
		public static bool IsMouseHookInstalled {
			get {
				return mouseHookInstall && hMouseHook != IntPtr.Zero;
			}
		}
		//---------------------------------------------------------------------
		public static void UninstallMouseHook() {
			if (IsMouseHookInstalled) {
				if (!UnhookWindowsHookEx(hMouseHook)) {
					throw new Win32Exception("Can't uninstall low level keyboard hook!");
				}
				hMouseHook = IntPtr.Zero;
				mouseHookInstall = false;
			}
		}
		//---------------------------------------------------------------------
		static IntPtr MouseHookProc(int nCode, IntPtr wParam, [In] IntPtr lParam) {
			if (nCode == 0 && wParam.ToInt32() != 0x0200) {
				MSLLHOOKSTRUCT ms = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
				OnHookMouseClickEventHandler.BeginInvoke(ms.time, null, null);
			}
			return CallNextHookEx(hMouseHook, nCode, wParam, lParam);
		}
		//---------------------------------------------------------------------
		public static void PressKey(Key key, bool up) {
			const int KEYEVENTF_EXTENDEDKEY = 0x1;
			const int KEYEVENTF_KEYUP = 0x2;
			// I had some Compile errors until I Casted the final 0 to UIntPtr like this...
			if (up) {
				keybd_event((byte) KeyInterop.VirtualKeyFromKey(key), 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr) 0);
			}
			else {
				keybd_event((byte) KeyInterop.VirtualKeyFromKey(key), 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr) 0);
			}
		}
	}
}