# While testing: whenever the game window takes the foreground, give the foreground back to the window the player
# used last and put the game at the bottom of the window stack, without minimizing it (a minimized game breaks its UI
# layout, and restoring it made the game take the foreground again, every second or two: 2026-11-22).
# Runs until the game closes or for -Minutes.
param([int]$Minutes = 120)
Add-Type -Name W -Namespace U -MemberDefinition @'
[DllImport("user32.dll")] public static extern System.IntPtr GetForegroundWindow();
[DllImport("user32.dll")] public static extern bool SetForegroundWindow(System.IntPtr h);
[DllImport("user32.dll")] public static extern bool SetWindowPos(System.IntPtr h, System.IntPtr after, int x, int y, int cx, int cy, uint flags);
[DllImport("user32.dll")] public static extern bool IsIconic(System.IntPtr h);
[DllImport("user32.dll")] public static extern bool ShowWindow(System.IntPtr h, int c);
[DllImport("user32.dll")] public static extern bool IsWindow(System.IntPtr h);
[DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, System.UIntPtr extra);
'@
$end = (Get-Date).AddMinutes($Minutes)
$last = [System.IntPtr]::Zero
while ((Get-Date) -lt $end) {
    $p = Get-Process Balls -ErrorAction SilentlyContinue
    $fg = [U.W]::GetForegroundWindow()
    if ($p -and $p.MainWindowHandle -ne 0) {
        $g = $p.MainWindowHandle
        if ([U.W]::IsIconic($g)) { [U.W]::ShowWindow($g, 4) | Out-Null; [U.W]::SetWindowPos($g, [System.IntPtr]1, 0, 0, 1280, 720, 0x0010) | Out-Null }
        if ($fg -ne $g -and $fg -ne [System.IntPtr]::Zero) { $last = $fg }
        elseif ($fg -eq $g -and $last -ne [System.IntPtr]::Zero -and [U.W]::IsWindow($last)) {
            [U.W]::SetWindowPos($g, [System.IntPtr]1, 0, 0, 0, 0, 0x0013) | Out-Null       # bottom, no move / size / activate
            # A process in the background may only take the foreground right after a key event: an Alt tap (no
            # menu opens for a lone press and release sent this way to a window that is not active yet).
            [U.W]::keybd_event(0x12, 0, 0, [System.UIntPtr]::Zero); [U.W]::keybd_event(0x12, 0, 2, [System.UIntPtr]::Zero)
            [U.W]::SetForegroundWindow($last) | Out-Null
            "$(Get-Date -Format HH:mm:ss) gave the foreground back to $last"
        }
    }
    Start-Sleep -Milliseconds 150
}
