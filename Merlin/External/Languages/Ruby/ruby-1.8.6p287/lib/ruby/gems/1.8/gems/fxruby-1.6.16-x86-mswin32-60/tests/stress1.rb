#
# An FXRuby stress test developed by Gilles Filippini
#

require "fox16"

include Fox

# Test case tuning
NUMBER_OF_ITEMS = 1000
RESTART_FREQUENCY = 20

# =======================================================================
# Tree
# =======================================================================
class DirTree < FXTreeList
  def initialize(p)
    super(p, :opts => TREELIST_SHOWS_LINES|TREELIST_SHOWS_BOXES|TREELIST_ROOT_BOXES|LAYOUT_FILL_X|LAYOUT_FILL_Y)
  end

  def create
    super
    item = addItemLast(nil, "root")
    @currentItem = item
    expand
  end

  def expand
    expandTree(@currentItem, true)
    listSubDir(@currentItem)
  end

  # Updating entries of dir
  def listSubDir(parentItem)
    entries = (1..NUMBER_OF_ITEMS).collect { |i| i.to_s }
    entries.each do |entry|
      item = addItemLast(parentItem, entry)
      @currentItem = item if entry == "1"
    end
  end
end

# =======================================================================
# Application
# =======================================================================
class Application < FXApp
  include Responder

  ID_TIMER, ID_LAST = enum(FXApp::ID_LAST, 2)

  def initialize
    super("FXTreeList Bug (the come back)", "Pini")

    FXMAPFUNC(SEL_TIMEOUT, ID_TIMER, "onCount")

    self.threadsEnabled = false
    init(ARGV)

    @mainWindow = FXMainWindow.new(self, appName, nil, nil, DECOR_ALL, 0, 0, 400, 600)
    @dirTree = DirTree.new(@mainWindow) 

    @count = 0
  end 

  def create
    super
    @mainWindow.show(PLACEMENT_SCREEN)
    addTimeout(100, self, ID_TIMER)
  end

  def onCount(sender, sel, ptr)
    @count += 1
    puts "count = #{@count}"
    if @count % RESTART_FREQUENCY == 0
      @dirTree.clearItems
      @dirTree.create
    end
    @dirTree.expand
    addTimeout(100, self, ID_TIMER)
  end
end

if __FILE__ == $0
# Make application
  application = Application.new
  
  # Create app  
  application.create()
  
  # Run
  application.run()
end
