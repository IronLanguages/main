include System::Windows
include System::Windows::Controls

class App
  def initialize
    @root = Application.current.load_root_visual(UserControl.new, "app.xaml")
    @root.find_name('message').text = "Welcome to Ruby and Silverlight!"
  end
end

$app = App.new
