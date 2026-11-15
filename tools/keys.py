"""Send keys to the game window by scan code, with realistic hold times.
Usage: python keys.py <spec> [<spec> ...]
  spec: name[:holdms][+name...]   e.g.  enter  down:80  ctrl+shift+d  wait:500  f1
Brings the game window to the foreground first and refuses to send otherwise."""
import ctypes, sys, time
from ctypes import wintypes
user32 = ctypes.WinDLL('user32', use_last_error=True)
user32.FindWindowW.restype = ctypes.c_void_p
user32.GetForegroundWindow.restype = ctypes.c_void_p
SC = {'esc':0x01,'1':0x02,'2':0x03,'3':0x04,'4':0x05,'5':0x06,'6':0x07,'7':0x08,'8':0x09,'9':0x0A,'0':0x0B,
 'back':0x0E,'minus':0x0C,'equals':0x0D,'del':(0x53,1),'tab':0x0F,'q':0x10,'w':0x11,'e':0x12,'r':0x13,'t':0x14,'y':0x15,'u':0x16,'i':0x17,'o':0x18,'p':0x19,
 'enter':0x1C,'ctrl':0x1D,'a':0x1E,'s':0x1F,'d':0x20,'f':0x21,'g':0x22,'h':0x23,'j':0x24,'k':0x25,'l':0x26,
 'grave':0x29,'lshift':0x2A,'shift':0x2A,'z':0x2C,'x':0x2D,'c':0x2E,'v':0x2F,'b':0x30,'n':0x31,'m':0x32,'rshift':0x36,
 'alt':0x38,'space':0x39,'f1':0x3B,'f2':0x3C,'f3':0x3D,'f4':0x3E,'f5':0x3F,'f6':0x40,'f7':0x41,'f8':0x42,'f9':0x43,'f10':0x44,'f11':0x57,'f12':0x58,
 'home':(0x47,1),'up':(0x48,1),'pgup':(0x49,1),'left':(0x4B,1),'right':(0x4D,1),'end':(0x4F,1),'down':(0x50,1),'pgdn':(0x51,1),
 'num8':0x48,'num4':0x4B,'num5':0x4C,'num6':0x4D,'num2':0x50}
class KI(ctypes.Structure): _fields_=[('wVk',wintypes.WORD),('wScan',wintypes.WORD),('dwFlags',wintypes.DWORD),('time',wintypes.DWORD),('dwExtraInfo',ctypes.c_size_t)]
class MI(ctypes.Structure): _fields_=[('dx',wintypes.LONG),('dy',wintypes.LONG),('mouseData',wintypes.DWORD),('dwFlags',wintypes.DWORD),('time',wintypes.DWORD),('dwExtraInfo',ctypes.c_size_t)]
class U(ctypes.Union): _fields_=[('ki',KI),('mi',MI)]
class INPUT(ctypes.Structure): _fields_=[('type',wintypes.DWORD),('u',U)]
def key(sc, up):
    ext = 0
    if isinstance(sc, tuple): sc, ext = sc
    flags = 0x0008 | (0x0002 if up else 0) | (0x0001 if ext else 0)
    inp = INPUT(1, U(ki=KI(0, sc, flags, 0, 0)))
    user32.SendInput(1, ctypes.byref(inp), ctypes.sizeof(INPUT))
def find_window():
    h = user32.FindWindowW(None, 'BALL x PIT')
    if h: return h
    import subprocess
    pid = subprocess.run(['powershell','-NoProfile','-Command','(Get-Process Balls -ErrorAction SilentlyContinue | ? MainWindowHandle -ne 0 | select -First 1).MainWindowHandle'],capture_output=True,text=True).stdout.strip()
    return int(pid) if pid else 0
def focus():
    h = find_window()
    if not h: sys.exit('game window not found')
    if user32.GetForegroundWindow() != h:
        user32.keybd_event(0x12, 0, 0, 0); user32.keybd_event(0x12, 0, 2, 0)  # Alt tap allows SetForegroundWindow
        user32.ShowWindow(h, 9); user32.SetForegroundWindow(h); time.sleep(0.3)
    if user32.GetForegroundWindow() != h: sys.exit('could not focus game window')
focus()
for spec in sys.argv[1:]:
    if spec.startswith('wait:'): time.sleep(int(spec[5:])/1000); continue
    hold = 80
    if ':' in spec: spec, h = spec.rsplit(':',1); hold = int(h)
    names = spec.split('+')
    for n in names: key(SC[n.lower()], False); time.sleep(0.02)
    time.sleep(hold/1000)
    for n in reversed(names): key(SC[n.lower()], True); time.sleep(0.02)
    time.sleep(0.05)
print('sent', ' '.join(sys.argv[1:]))
