require 'windows/api'

# See WinUser.h

module Windows
   module Window
      API.auto_namespace = 'Windows::Window'
      API.auto_constant  = true
      API.auto_method    = true
      API.auto_unicode   = false

      # ShowWindow() constants
      SW_HIDE             = 0
      SW_SHOWNORMAL       = 1
      SW_NORMAL           = 1
      SW_SHOWMINIMIZED    = 2
      SW_SHOWMAXIMIZED    = 3
      SW_MAXIMIZE         = 3
      SW_SHOWNOACTIVATE   = 4
      SW_SHOW             = 5
      SW_MINIMIZE         = 6
      SW_SHOWMINNOACTIVE  = 7
      SW_SHOWNA           = 8
      SW_RESTORE          = 9
      SW_SHOWDEFAULT      = 10
      SW_FORCEMINIMIZE    = 11
      SW_MAX              = 11
      
      API.new('EnumWindows', 'KP', 'L', 'user32')
      API.new('FindWindow', 'PP', 'L', 'user32')
      API.new('FindWindowEx', 'LLPP', 'L', 'user32')
      API.new('GetAltTabInfo', 'LIPPI', 'B', 'user32')
      API.new('GetAncestor', 'LI', 'L', 'user32')
      API.new('GetClientRect', 'LP', 'B', 'user32')
      API.new('GetDesktopWindow', 'V', 'L', 'user32')
      API.new('GetForegroundWindow', 'V', 'L', 'user32')
      API.new('GetGUIThreadInfo', 'LP', 'B', 'user32')
      API.new('GetLastActivePopup', 'L', 'L', 'user32')
      API.new('GetParent', 'L', 'L', 'user32')
      API.new('GetProcessDefaultLayout', 'P', 'B', 'user32')
      API.new('GetShellWindow', 'V', 'L', 'user32')
      API.new('GetTitleBarInfo', 'LP', 'B', 'user32')
      API.new('GetTopWindow', 'L', 'L', 'user32')
      API.new('GetWindow', 'LI', 'L', 'user32')
      API.new('GetWindowInfo', 'LP', 'B', 'user32')
      API.new('GetWindowModuleFileName', 'LLI', 'I', 'user32')
      API.new('GetWindowPlacement', 'LP', 'B', 'user32')
      API.new('GetWindowRect', 'LP', 'B', 'user32')
      API.new('GetWindowText', 'LPI', 'I', 'user32')
      API.new('GetWindowTextLength', 'L', 'I', 'user32')
      API.new('GetWindowThreadProcessId', 'LP', 'L', 'user32')

      # Windows XP or later
      begin
         API.new('GetLayeredWindowAttributes', 'LPBL', 'B', 'user32')
      rescue Windows::API::Error
         # Do nothing - not supported on current platform. It's up to you
         # to check for the existence of the constant in your code.
      end
   end
end
