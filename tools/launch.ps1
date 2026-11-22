# Restart the game (kills any running instance by name first). Prints the new PID.
# For its first 45 seconds, whenever the game window comes to the front (it does at start and after the splash
# screens), minimize.ps1 hands the keyboard back to the window the player was using and leaves the game open behind
# it (a game window in front takes the player's keys, 2026-11-22). Drive it with dev.txt. -Front leaves it in front.
param([switch]$Front)
Get-Process Balls -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep 2
$p = Start-Process -FilePath "S:\steam\steamapps\common\BALLxPIT\Balls.exe" -WorkingDirectory "S:\steam\steamapps\common\BALLxPIT" -PassThru
$p.Id
if ($Front) { return }
Add-Type -Name F -Namespace U -MemberDefinition '[DllImport("user32.dll")] public static extern System.IntPtr GetForegroundWindow();'
$n = 0
for ($i = 0; $i -lt 180; $i++) {
    Start-Sleep -Milliseconds 250
    $p.Refresh()
    if ($p.HasExited) { break }
    $h = $p.MainWindowHandle
    if ($h -ne 0 -and [U.F]::GetForegroundWindow() -eq $h) { & "$PSScriptRoot\minimize.ps1" | Out-Null; $n++ }
}
"sent behind $n times"
