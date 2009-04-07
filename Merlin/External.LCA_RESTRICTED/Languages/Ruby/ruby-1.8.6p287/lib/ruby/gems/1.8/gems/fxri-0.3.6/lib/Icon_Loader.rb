# Copyright (c) 2004, 2005 Martin Ankerl

# Converts a Recursive_Open_Struct that contains filenames of PNG-icons into real icons.
class Icon_Loader
  # Create a new Icon_Loader. You need to specify the Fox-application.
  def initialize(app)
    @app = app
  end
  
  # Takes each attribute of the given Recursive_Open_Struct,
  # converts it into a real icon, and sets it.
  def cfg_to_icons(cfg)
    cfg.attrs.each do |attr|
      value = cfg.send(attr.to_sym)
      if (value.class == Recursive_Open_Struct)
        cfg_to_icons(value)
      else
        # value is a filename
        icon = make_icon(value)
        cfg.send((attr + "=").to_sym, icon)
      end
    end
  end
  
  # Constructs an icon from the given filename (from the icons directory).
  def make_icon(filename)
    filename = File.join($cfg.icons_path, filename)
    icon = nil
    File.open(filename, "rb") do |f|
      icon = FXPNGIcon.new(@app, f.read, 0)
    end
    icon.create
    icon
  end
end
