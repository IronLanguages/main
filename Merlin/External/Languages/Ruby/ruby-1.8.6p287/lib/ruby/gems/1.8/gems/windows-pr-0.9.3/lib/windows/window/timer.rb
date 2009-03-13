require 'windows/api'

module Windows
   module Window
      module Timer
         API.auto_namespace = 'Windows::Window::Timer'
         API.auto_constant  = true
         API.auto_method    = true
         API.auto_unicode   = false

         API.new('KillTimer', 'LP', 'B', 'user32')
         API.new('QueryPerformanceCounter', 'P', 'B', 'user32')
         API.new('QueryPerformanceFrequency', 'P', 'B', 'user32')
         API.new('SetTimer', 'LIIK', 'P', 'user32')
      end
   end
end
