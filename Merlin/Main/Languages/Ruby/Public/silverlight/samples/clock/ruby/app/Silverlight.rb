include System::Windows
include System::Windows::Controls
include System::Windows::Media

class SilverlightApplication
  def application
    Application.current
  end

  def self.use_xaml(options = {})
    options = {:type => UserControl, :name => "app"}.merge(options)
    Application.current.load_root_visual(options[:type].new, "#{options[:name]}.xaml")
  end
  
  def root
    application.root_visual
  end
  
  def method_missing(m)
    root.send(m) rescue super
  end
end

class FrameworkElement
  def method_missing(m)
    find_name(m.to_s.to_clr_string) || super
  end
end

