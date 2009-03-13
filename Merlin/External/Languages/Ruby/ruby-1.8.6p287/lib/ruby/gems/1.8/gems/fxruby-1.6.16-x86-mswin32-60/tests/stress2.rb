require 'test/unit'
require 'thread'
require 'fox16'

include Fox

class TC_stress2 < Test::Unit::TestCase
  def set_up_main_window(theApp)
    theMainWindow = FXMainWindow.new(theApp, "TC_stress2", nil, nil, DECOR_ALL, 0, 0, 200, 100)
    @countLabel = FXLabel.new(theMainWindow, "0", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    theMainWindow
  end
  
  def on_timeout(sender, sel, ptr)
    safeToQuit = false
    $lock.synchronize {
      if $count > 1000000
        # if it were going to crash, it probably would have done
	# so by now, so it's safe to quit.
	safeToQuit = true
      end
      @countLabel.setText($count.to_s)
    }
    if safeToQuit
      @theApp.handle(@theMainWindow, MKUINT(FXApp::ID_QUIT, SEL_COMMAND), nil)
    else
      @theApp.addTimeout(100, method(:on_timeout))
    end
  end
  
  def test_run
    # Set up the counter thread
    $count = 0
    $lock = Mutex.new
    w = Thread.new do
      loop do
        $lock.synchronize { $count += 1 }
        sleep 0
      end
    end

    # Start the app
    @theApp = FXApp.new("TC_stress2", "FXRuby")
    @theMainWindow = set_up_main_window(@theApp)
    @theApp.create
    @theMainWindow.show(PLACEMENT_SCREEN)
    @theApp.addTimeout(100, method(:on_timeout))
    @theApp.run
  end
end
 
