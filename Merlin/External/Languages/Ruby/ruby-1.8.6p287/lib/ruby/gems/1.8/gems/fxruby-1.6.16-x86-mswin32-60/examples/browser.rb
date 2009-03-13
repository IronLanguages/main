#!/usr/bin/env ruby

require 'fox16'

include Fox

# Node in the class hierarchy tree
class ClassTreeNode
  def initialize(className, parentClassName)
    @className = className
    @parentClassName = parentClassName
    @children = []
  end

  def addChild(child)
    @children.push(child)
  end

  def className
    @className
  end

  def parentClassName
    @parentClassName
  end

  def <=>(other)
    className <=> other.className
  end

  def children
    @children.sort
  end
end

# The class hierarchy tree
class ClassTree
  def initialize
    # Find the Fox module
    @mFox = Fox

    # Get an hash containing class names for Fox
    classes = {}
    @mFox.constants.each do |name|
      c = @mFox.const_get(name)
      if c.kind_of? Class
	parentclass = c.superclass.name.sub("Fox::", "")
	classNode  = ClassTreeNode.new(name, parentclass)
	classes[name] = classNode
      end
    end

    # Go back and do this
    roots = []
    classes.each_value do |aValue|
      parentNode = classes[aValue.parentClassName]
      if parentNode
        parentNode.addChild(aValue)
      else
        parentNode = ClassTreeNode.new(aValue.parentClassName, nil)
      end
    end

    # FXObject is the root
    @root = classes["FXObject"]
  end

  def root
    return @root
  end
end

class BrowserWindow < FXMainWindow

  def initialize(app)
    # Call base class initializer first
    super(app, "Browse", nil, nil, DECOR_ALL, 0, 0, 600, 400)

    # Contents
    contents = FXHorizontalFrame.new(self, LAYOUT_FILL_X|LAYOUT_FILL_Y)

    # Horizontal splitter
    splitter = FXSplitter.new(contents, (LAYOUT_SIDE_TOP|LAYOUT_FILL_X|
      LAYOUT_FILL_Y|SPLITTER_TRACKING|SPLITTER_HORIZONTAL))

    # Create a sunken frame to hold the tree list
    groupbox = FXGroupBox.new(splitter, "Classes",
      LAYOUT_FILL_X|LAYOUT_FILL_Y|FRAME_GROOVE)
    frame = FXHorizontalFrame.new(groupbox,
      LAYOUT_FILL_X|LAYOUT_FILL_Y|FRAME_SUNKEN|FRAME_THICK)

    # Create the empty tree list
    @treeList = FXTreeList.new(frame, nil, 0,
      (TREELIST_BROWSESELECT|TREELIST_SHOWS_LINES|TREELIST_SHOWS_BOXES|
       TREELIST_ROOT_BOXES|LAYOUT_FILL_X|LAYOUT_FILL_Y))
    @treeList.connect(SEL_COMMAND) do |sender, sel, item|
      getApp().beginWaitCursor do
        s = getInstanceMethods(item.to_s).join("\n")
        @methodsText.text = s
        s = getConstants(item.to_s).join("\n")
        @constantsText.text = s
      end
    end

    # Fill it up based on the tree contents
    populateTree(@treeList, nil, ClassTree.new.root)

    # Tabbed notebook on the right
    tabBook = FXTabBook.new(splitter, nil, 0,
      LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_RIGHT)

    tab1 = FXTabItem.new(tabBook, "Methods")
    page1 = FXHorizontalFrame.new(tabBook, FRAME_THICK|FRAME_RAISED)
    frame1 = FXHorizontalFrame.new(page1,
      FRAME_THICK|FRAME_SUNKEN|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    @methodsText = FXText.new(frame1, nil, 0, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    @methodsText.text = "List of methods goes here"
    @methodsText.editable = false

    tab2 = FXTabItem.new(tabBook, "Constants")
    page2 = FXHorizontalFrame.new(tabBook, FRAME_THICK|FRAME_RAISED)
    frame2 = FXHorizontalFrame.new(page2,
      FRAME_THICK|FRAME_SUNKEN|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    @constantsText = FXText.new(frame2, nil, 0, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    @constantsText.text = "List of constants goes here"
    @constantsText.editable = false

    # Cache of classname -> method and classname -> constants lists
    @instanceMethods = {}
    @classConstants = {}
  end

  # Recursively fill up the tree list
  def populateTree(treeList, rootItem, rootNode)
    rootNode.children.each do |childNode|
      childItem = treeList.appendItem(rootItem, childNode.className)
      populateTree(treeList, childItem, childNode)
    end
  end

  # Create and show the main window
  def create
    super
    @treeList.parent.parent.setWidth(@treeList.font.getTextWidth('MMMMMMMMMMMMMMMM'))
    show(PLACEMENT_SCREEN)
  end

  # Returns an array of instance methods for the named class
  def getInstanceMethods(className)
    methods = @instanceMethods[className]
    if methods.nil?
      theClass = Fox.const_get(className)
      methods = theClass.instance_methods.sort
      @instanceMethods[className] = methods
    end
    methods
  end

  # Returns an array of constants for the named class
  def getConstants(className)
    constants = @classConstants[className]
    if constants.nil?
      klass = Fox.const_get(className)
      constants = klass.constants
      superklass = Fox.const_get(className)
      if superklass
        constants -= superklass.superclass.constants
      else
        constants -= klass.superclass.constants
      end
      constants.sort!
      @classConstants[className] = constants
    end
    constants
  end
end

if __FILE__ == $0
  # Create a new application
  application = FXApp.new("Browser", "FoxTest")

  # Construct the main window
  BrowserWindow.new(application)

  # Create the windows
  application.create

  # Start event loop
  application.run
end

