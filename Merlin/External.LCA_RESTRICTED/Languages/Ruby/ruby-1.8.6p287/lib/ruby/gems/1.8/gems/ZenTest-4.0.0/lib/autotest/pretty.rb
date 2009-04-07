# -*- ruby -*-

require 'osx/cocoa'
include Math
include OSX

OSX::NSBundle.bundleWithPath(File.expand_path("~/Library/Frameworks/Aquaterm.framework")).load
OSX.ns_import :AQTAdapter

class Autotest::Pretty
  BLACK = 0
  WHITE = 1
  RED = 2
  GREEN = 3
  GRAY = 4

  def initialize
    @past = []

    @adapter = AQTAdapter.alloc.init
    @adapter.openPlotWithIndex 1
    @adapter.setPlotSize([122,122])
    @adapter.setPlotTitle("Autotest Status")

    @adapter.setColormapEntry_red_green_blue(0, 0.0, 0.0, 0.0) # black
    @adapter.setColormapEntry_red_green_blue(1, 1.0, 1.0, 1.0) # white
    @adapter.setColormapEntry_red_green_blue(2, 1.0, 0.0, 0.0) # red
    @adapter.setColormapEntry_red_green_blue(3, 0.0, 1.0, 0.0) # green
    @adapter.setColormapEntry_red_green_blue(4, 0.7, 0.7, 0.7) # gray

    draw
  end

  def draw
    @past.shift if @past.size > 100

    @adapter.takeColorFromColormapEntry(@past.last ? GREEN : RED)
    @adapter.addFilledRect([0, 0, 122, 122])

    @adapter.takeColorFromColormapEntry(BLACK)
    @adapter.addFilledRect([10, 10, 102, 102])

    @adapter.takeColorFromColormapEntry(GRAY)
    @adapter.addFilledRect([11, 11, 100, 100])

    @adapter.takeColorFromColormapEntry(0)

    @past.each_with_index do |passed,i|
      x = i % 10
      y = i / 10
      
      @adapter.takeColorFromColormapEntry(passed ? GREEN : RED)
      @adapter.addFilledRect([x*10+11, y*10+11, 10, 10])
    end
    @adapter.renderPlot
  end

  def pass
    @past.push true
    draw
  end

  def fail
    @past.push false
    draw
  end

  def close
    @adapter.closePlot
  end
end

unless $TESTING then
  board = Autotest::Pretty.new

  Autotest.add_hook :red do |at|
    board.fail unless $TESTING
  end

  Autotest.add_hook :green do |at|
    board.pass unless $TESTING
  end
end
