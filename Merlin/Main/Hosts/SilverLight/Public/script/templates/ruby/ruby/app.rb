require "silverlight"

class App < SilverlightApplication
  use_xaml

  def initialize
    message.text = "Welcome to Ruby and Silverlight!"
  end
end

$app = App.new
