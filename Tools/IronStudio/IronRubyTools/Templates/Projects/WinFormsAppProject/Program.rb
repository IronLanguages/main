require 'System.Windows.Forms'

include System::Windows::Forms

class MyForm < Form
  def initialize
    button = Button.new
    button.text = 'Click Me'
    controls.add button 
  end
end

form = MyForm.new
Application.run form
