# Restart the game (kills any running instance by name first). Prints the new PID.
Get-Process Balls -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep 2
$p = Start-Process -FilePath "S:\steam\steamapps\common\BALLxPIT\Balls.exe" -WorkingDirectory "S:\steam\steamapps\common\BALLxPIT" -PassThru
$p.Id
