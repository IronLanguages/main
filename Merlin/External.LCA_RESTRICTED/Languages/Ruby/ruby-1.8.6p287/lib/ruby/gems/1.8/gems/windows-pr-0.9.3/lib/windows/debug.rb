require 'windows/api'

module Windows
   module Debug
      API.auto_namespace = 'Windows::Debug'
      API.auto_constant  = true
      API.auto_method    = true
      API.auto_unicode   = false

      API.new('ContinueDebugEvent', 'LLL', 'B')
      API.new('DebugActiveProcess', 'L', 'B')
      API.new('DebugBreak', 'V', 'V')
      API.new('FatalExit', 'I', 'V')
      API.new('FlushInstructionCache', 'LLL', 'B')
      API.new('GetThreadContext', 'LP', 'B')
      API.new('GetThreadSelectorEntry', 'LLP', 'B')
      API.new('IsDebuggerPresent', 'V', 'B')
      API.new('OutputDebugString', 'P', 'V')
      API.new('ReadProcessMemory', 'LLPLP', 'B')
      API.new('SetThreadContext', 'LP', 'B')
      API.new('WaitForDebugEvent', 'PL', 'B')
      API.new('WriteProcessMemory', 'LLPLP', 'B')

      # Windows XP or later
      begin
         API.new('CheckRemoteDebuggerPresent', 'LP', 'B')
         API.new('DebugActiveProcessStop', 'L', 'B')
         API.new('DebugBreakProcess', 'L', 'B')
         API.new('DebugSetProcessKillOnExit', 'I', 'B')
      rescue Windows::API::LoadError
         # Do nothing - not supported on current platform.  It's up to you to
         # check for the existence of the constant in your code.
      end
   end
end
