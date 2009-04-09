require 'test/unit'
require 'testcase'
require 'fox16'

include Fox

class TC_FXMenuCommand < TestCase
  def setup
    super(self.class.name)
    @menuCommand = FXMenuCommand.new(mainWindow, "menuCommand")
  end
end
