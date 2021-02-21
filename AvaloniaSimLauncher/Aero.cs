using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AvaloniaSimLauncher
{
    public static class Aero
    {

		[Flags]
		enum DwmBlurBehindFlags : uint
		{
			// Analysis disable InconsistentNaming

			/// <summary>
			/// Indicates a value for fEnable has been specified.
			/// </summary>
			DWM_BB_ENABLE = 0x00000001,

			/// <summary>
			/// Indicates a value for hRgnBlur has been specified.
			/// </summary>
			DWM_BB_BLURREGION = 0x00000002,

			/// <summary>
			/// Indicates a value for fTransitionOnMaximized has been specified.
			/// </summary>
			DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004
		}

		[StructLayout(LayoutKind.Sequential)]
		struct DWM_BLURBEHIND
		{
			public DwmBlurBehindFlags dwFlags;
			public bool fEnable;
			public IntPtr hRgnBlur;
			public bool fTransitionOnMaximized;
		}

		[DllImport("dwmapi.dll")]
        static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND blurBehind);
    }
}
