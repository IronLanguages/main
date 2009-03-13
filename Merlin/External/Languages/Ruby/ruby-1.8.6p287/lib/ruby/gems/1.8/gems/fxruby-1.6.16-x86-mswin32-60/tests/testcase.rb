require 'test/unit'
require 'fox16'

module Fox
  class TestCase < Test::Unit::TestCase
    #
    # Construct an application and main window for this test case's
    # use, based on the supplied application name.
    #
    def setup(*args)
      unless args.empty?
        appName = args[0]
      	if FXApp.instance.nil?
      	  @theApp = FXApp.new(appName, 'FXRuby')
      	  @theApp.init([])
      	else
      	  @theApp = FXApp.instance
      	end
      	@theMainWindow = FXMainWindow.new(@theApp, appName)
      end      
    end
    
    # Return a reference to the application
    def app
      @theApp
    end
    
    # Return a reference to the main window
    def mainWindow
      @theMainWindow
    end
    
    # Override the base class version of default_test() so that
    # a test case with no tests doesn't trigger an error.
    def default_test; end
  end
end
