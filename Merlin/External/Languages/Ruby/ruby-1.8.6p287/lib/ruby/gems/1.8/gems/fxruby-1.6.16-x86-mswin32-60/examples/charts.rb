require 'fox16'
require 'google_chart'
require 'open-uri'

include Fox

class ChartsWindow < FXMainWindow
  def initialize(app)
    super(app, "Google Charts Demo", :width => 650, :height => 250)
    FXImageFrame.new(self, nil, :opts => LAYOUT_FILL) do |f|
      f.image = FXPNGImage.new(app, open(bar_chart.to_escaped_url, "rb").read)
    end
  end
  
  def bar_chart
    GoogleChart::BarChart.new('600x200', 'My Chart', :vertical) do |bc|
      bc.data 'Trend 1', [5,4,3,1,3,5], '0000ff'
      bc.data 'Trend 2', [1,2,3,4,5,6], 'ff0000'
      bc.data 'Trend 3', [6,5,4,4,5,6], '00ff00'
    end
  end

  def create
    super
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  FXApp.new do |app|
    ChartsWindow.new(app)
    app.create
    app.run
  end
end