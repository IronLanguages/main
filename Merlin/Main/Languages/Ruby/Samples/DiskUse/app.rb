require 'WindowsBase'
require 'PresentationCore'
require 'PresentationFramework'
require 'System.Windows.Forms'
require 'ui_logic'

def start
  app = Windows::Application.new
  idu = IronDiskUsage.new(app)
  app.run
end

def app_start
  app = System::Windows::Application.new
  idu = IronDiskUsage.new(app)
  $dispatcher = System::Threading::Dispatcher.from_thread(System::Threading::Thread.current_thread)
  $are.set
  app.run
end

def start_interactive
  raise "start_interactive doesn't work yet"
  $are = System::Threading::AutoResetEvent.new(false)
  
  t = Thread.new do
    app_start
  end
  $are.wait_one
end

if __FILE__ == $0
  start
else
  start_interactive
end
