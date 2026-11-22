# Give the keyboard back to the player's window without leaving the game minimized: minimizing hands the foreground
# to the window that was in front before, and a minimized game breaks its UI layout (screen size 0), so the window is
# then shown again without being activated, at 1280 x 720, at the bottom of the window stack (no focus stealing,
# 2026-11-22). A window minimized during start-up came back 144 x 1, hence the explicit size.
Add-Type -Name W -Namespace U -MemberDefinition @'
[DllImport("user32.dll")] public static extern bool ShowWindow(System.IntPtr h, int c);
[DllImport("user32.dll")] public static extern bool SetWindowPos(System.IntPtr h, System.IntPtr after, int x, int y, int cx, int cy, uint flags);
[DllImport("user32.dll")] public static extern System.IntPtr GetForegroundWindow();
'@
$p = Get-Process Balls -ErrorAction SilentlyContinue
if (-not $p -or $p.MainWindowHandle -eq 0) { "no window"; return }
$h = $p.MainWindowHandle
if ([U.W]::GetForegroundWindow() -eq $h) { [U.W]::ShowWindow($h, 6) | Out-Null; Start-Sleep -Milliseconds 300 }   # SW_MINIMIZE
[U.W]::ShowWindow($h, 4) | Out-Null                                                                                   # SW_SHOWNOACTIVATE
[U.W]::SetWindowPos($h, [System.IntPtr]1, 0, 0, 1280, 720, 0x0010) | Out-Null                                         # HWND_BOTTOM, NOACTIVATE
$fg = [U.W]::GetForegroundWindow()
if ($fg -eq $h) { "the game is still in front" } else { "behind; foreground is another window" }
