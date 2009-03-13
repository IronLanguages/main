require 'fox16'

module Fox


  class FX4Splitter
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => FOURSPLITTER_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0 }     
      params = {}
      params = args.pop if args.last.is_a? Hash
      if args.length > 0 && (args.first.nil? || args.first.is_a?(FXObject))
        tgt, sel = args[0], args[1]
        args.each_with_index { |e, i| params[argument_names[i-2].intern] = e if i >= 2 }
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(p, tgt, sel, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
      else
        args.each_with_index { |e, i| params[argument_names[i].intern] = e }
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
      end
    end
  end

  class FXDockBar
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :opts => LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FILL_X, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 3, :padRight => 3, :padTop => 2, :padBottom => 2, :hSpacing => DEFAULT_SPACING, :vSpacing => DEFAULT_SPACING }     
      params = {}
      params = args.pop if args.last.is_a? Hash
      if args.length > 0 && (args.first.nil? || args.first.is_a?(FXComposite))
        q = args[0]
        args.each_with_index { |e, i| params[argument_names[i-1].intern] = e if i >= 1 }
        if params.key? :padding
          value = params.delete(:padding)
          [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
        end
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(p, q, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
      else
        args.each_with_index { |e, i| params[argument_names[i].intern] = e }
        if params.key? :padding
          value = params.delete(:padding)
          [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
        end
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
      end
    end
  end
  
  class FXFont

    alias old_initialize initialize
    
    def initialize(a, arg1, *args, &blk)
      if args.length > 0
        face, size = arg1, args[0]
        argument_names = %w{weight slant encoding setWidth hints}
        default_params = { :weight => FXFont::Normal, :slant => FXFont::Straight, :encoding => FONTENCODING_DEFAULT, :setWidth => FXFont::NonExpanded, :hints => 0 }     
        params = {}
        params = args.pop if args.last.is_a? Hash
        args.each_with_index { |e, i| params[argument_names[i-1].intern] = e if i >= 1 }
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(a, face, size, params[:weight], params[:slant], params[:encoding], params[:setWidth], params[:hints], &blk)
      else
        old_initialize(a, arg1, &blk)
      end
    end

    class << self
      alias old_listFonts listFonts
    end
    
    def FXFont.listFonts(face, *args)
      argument_names = %w{weight slant setWidth encoding hints}
      default_params = { :weight => 0, :slant => 0, :setWidth => 0, :encoding => 0, :hints => 0 }     
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_listFonts(face, params[:weight], params[:slant], params[:setWidth], params[:encoding], params[:hints])
    end

  end
  
  class FXGLCanvas
    alias old_initialize initialize
    def initialize(parent, vis, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      if args.length > 0 && (args[0].is_a?(FXGLCanvas))
        sharegroup = args[0]
        args.each_with_index { |e, i| params[argument_names[i-1].intern] = e if i >= 1 }
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(parent, vis, sharegroup, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
      else
        args.each_with_index { |e, i| params[argument_names[i].intern] = e }
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(parent, vis, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
      end
    end
  end

  class FXGLViewer
    alias old_initialize initialize
    def initialize(parent, vis, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      if args.length > 0 && (args[0].is_a?(FXGLViewer))
        sharegroup = args[0]
        args.each_with_index { |e, i| params[argument_names[i-1].intern] = e if i >= 1 }
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(parent, vis, sharegroup, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
      else
        args.each_with_index { |e, i| params[argument_names[i].intern] = e }
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(parent, vis, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
      end
    end
  end
  
  class FXMenuBar
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :opts => LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FILL_X, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 3, :padRight => 3, :padTop => 2, :padBottom => 2, :hSpacing => DEFAULT_SPACING, :vSpacing => DEFAULT_SPACING }
      params = {}
      params = args.pop if args.last.is_a? Hash
      if args.length > 0 && (args[0].nil? || args[0].is_a?(FXComposite))
        q = args[0]
        args.each_with_index { |e, i| params[argument_names[i-1].intern] = e if i >= 1 }
        if params.key? :padding
          value = params.delete(:padding)
          [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
        end
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(p, q, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
      else
        args.each_with_index { |e, i| params[argument_names[i].intern] = e }
        if params.key? :padding
          value = params.delete(:padding)
          [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
        end
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
      end
    end
  end

  class FXSplitter
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => SPLITTER_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0 }     
      params = {}
      params = args.pop if args.last.is_a? Hash
      if args.length > 0 && (args.first.nil? || args.first.is_a?(FXObject))
        tgt, sel = args[0], args[1]
        args.each_with_index { |e, i| params[argument_names[i-2].intern] = e if i >= 2 }
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(p, tgt, sel, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
      else
        args.each_with_index { |e, i| params[argument_names[i].intern] = e }
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
      end
    end
  end

  class FXToolBar
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :opts => LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FILL_X, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 3, :padRight => 3, :padTop => 2, :padBottom => 2, :hSpacing => DEFAULT_SPACING, :vSpacing => DEFAULT_SPACING }     
      params = {}
      params = args.pop if args.last.is_a? Hash
      if args.length > 0 && (args[0].nil? || args[0].is_a?(FXComposite))
        q = args[0]
        args.each_with_index { |e, i| params[argument_names[i-1].intern] = e if i >= 1 }
        if params.key? :padding
          value = params.delete(:padding)
          [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
        end
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(p, q, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
      else
        args.each_with_index { |e, i| params[argument_names[i].intern] = e }
        if params.key? :padding
          value = params.delete(:padding)
          [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
        end
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
      end
    end
  end
  
  class FXWindow
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      if p.is_a? FXApp
        old_initialize(p, *args, &blk)
      else
        argument_names = %w{opts x y width height}
        default_params = { :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
        params = {}
        params = args.pop if args.last.is_a? Hash
        args.each_with_index { |e, i| params[argument_names[i].intern] = e }
        params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
        params = default_params.merge(params)
        old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
      end
    end
  end

  class FX7Segment
    alias old_initialize initialize
    def initialize(p, text, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom}
      default_params = { :opts => SEVENSEGMENT_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, text, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXApp
    alias old_initialize initialize
    def initialize(*args, &blk)
      argument_names = %w{appName vendorName}
      default_params = { :appName => "Application", :vendorName => "FoxDefault" }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(params[:appName], params[:vendorName], &blk)
    end
  end

  class FXArrowButton
    alias old_initialize initialize
    def initialize(parent, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => ARROW_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXBitmap
    alias old_initialize initialize
    def initialize(app, *args, &blk)
      argument_names = %w{pixels opts width height}
      default_params = { :pixels => nil, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(app, params[:pixels], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXBitmapFrame
    alias old_initialize initialize
    def initialize(p, bmp, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom}
      default_params = { :opts => FRAME_SUNKEN|FRAME_THICK, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, bmp, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXBitmapView
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{bmp target selector opts x y width height}
      default_params = { :bmp => nil, :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:bmp], params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXBMPIcon
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix clr opts width height}
      default_params = { :pix => nil, :clr => FXRGB(192,192,192), :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:clr], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXBMPImage
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix opts width height}
      default_params = { :pix => nil, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXButton
    alias old_initialize initialize
    def initialize(parent, text, *args, &blk)
      argument_names = %w{icon target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :icon => nil, :target => nil, :selector => 0, :opts => BUTTON_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, text, params[:icon], params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXCanvas
    alias old_initialize initialize
    def initialize(parent, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXCheckButton
    alias old_initialize initialize
    def initialize(parent, text, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => CHECKBUTTON_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, text, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXChoiceBox
    alias old_initialize initialize
    def initialize(owner, caption, text, icon, choices, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, caption, text, icon, choices, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXColorBar
    alias old_initialize initialize
    def initialize(parent, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXColorDialog
    alias old_initialize initialize
    def initialize(owner, title, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, title, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXColorItem
    alias old_initialize initialize
    def initialize(text, clr, *args, &blk)
      argument_names = %w{data}
      default_params = { :data => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(text, clr, params[:data], &blk)
    end
  end

  class FXColorList
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => LIST_BROWSESELECT, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXColorRing
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXColorSelector
    alias old_initialize initialize
    def initialize(parent, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXColorWell
    alias old_initialize initialize
    def initialize(parent, *args, &blk)
      argument_names = %w{color target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :color => 0, :target => nil, :selector => 0, :opts => COLORWELL_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, params[:color], params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXColorWheel
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXComboBox
    alias old_initialize initialize
    def initialize(p, cols, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => COMBOBOX_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, cols, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXComposite
    alias old_initialize initialize
    def initialize(parent, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXDataTarget
    alias old_initialize initialize
    def initialize(*args, &blk)
      argument_names = %w{value target selector}
      default_params = { :value => nil, :target => nil, :selector => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(params[:value], params[:target], params[:selector], &blk)
    end
  end

  class FXDelegator
    alias old_initialize initialize
    def initialize(*args, &blk)
      argument_names = %w{delegate}
      default_params = { :delegate => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(params[:delegate], &blk)
    end
  end

  class FXDial
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => DIAL_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXDialogBox
    alias old_initialize initialize
    def initialize(owner, title, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :opts => DECOR_TITLE|DECOR_BORDER, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 10, :padRight => 10, :padTop => 10, :padBottom => 10, :hSpacing => 4, :vSpacing => 4 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, title, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
    end
  end

  class FXDirBox
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_SUNKEN|FRAME_THICK|TREELISTBOX_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXDirDialog
    alias old_initialize initialize
    def initialize(owner, name, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 500, :height => 300 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, name, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXDirItem
    alias old_initialize initialize
    def initialize(text, *args, &blk)
      argument_names = %w{oi ci data}
      default_params = { :oi => nil, :ci => nil, :data => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(text, params[:oi], params[:ci], params[:data], &blk)
    end
  end

  class FXDirList
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXDirSelector
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXDockSite
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0, :hSpacing => 0, :vSpacing => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
    end
  end

  class FXDockTitle
    alias old_initialize initialize
    def initialize(p, text, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_NORMAL|JUSTIFY_CENTER_X|JUSTIFY_CENTER_Y, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, text, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXDriveBox
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_SUNKEN|FRAME_THICK|LISTBOX_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXFileDialog
    alias old_initialize initialize
    def initialize(owner, name, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 500, :height => 300 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, name, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXFileDict
    alias old_initialize initialize
    def initialize(app, *args, &blk)
      argument_names = %w{db}
      default_params = { :db => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(app, params[:db], &blk)
    end
  end

  class FXFileItem
    alias old_initialize initialize
    def initialize(text, *args, &blk)
      argument_names = %w{bi mi ptr}
      default_params = { :bi => nil, :mi => nil, :ptr => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(text, params[:bi], params[:mi], params[:ptr], &blk)
    end
  end

  class FXFileList
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXFileSelector
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXFileStream
    alias old_initialize initialize
    def initialize(*args, &blk)
      argument_names = %w{cont}
      default_params = { :cont => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(params[:cont], &blk)
    end
  end

  class FXFoldingItem
    alias old_initialize initialize
    def initialize(text, *args, &blk)
      argument_names = %w{openIcon closedIcon data}
      default_params = { :openIcon => nil, :closedIcon => nil, :data => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(text, params[:openIcon], params[:closedIcon], params[:data], &blk)
    end
  end

  class FXFoldingList
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => TREELIST_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXFontDialog
    alias old_initialize initialize
    def initialize(owner, name, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 600, :height => 380 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, name, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXFontSelector
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXFrame
    alias old_initialize initialize
    def initialize(parent, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom}
      default_params = { :opts => FRAME_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXGIFCursor
    alias old_initialize initialize
    def initialize(a, pix, *args, &blk)
      argument_names = %w{hx hy}
      default_params = { :hx => -1, :hy => -1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, pix, params[:hx], params[:hy], &blk)
    end
  end

  class FXGIFIcon
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix clr opts width height}
      default_params = { :pix => nil, :clr => 0, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:clr], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXGIFImage
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix opts width height}
      default_params = { :pix => nil, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXGLContext
    alias old_initialize initialize
    def initialize(app, visual, *args, &blk)
      argument_names = %w{other}
      default_params = { :other => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(app, visual, params[:other], &blk)
    end
  end

  class FXGradientBar
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXGroupBox
    alias old_initialize initialize
    def initialize(parent, text, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :opts => GROUPBOX_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_SPACING, :padRight => DEFAULT_SPACING, :padTop => DEFAULT_SPACING, :padBottom => DEFAULT_SPACING, :hSpacing => DEFAULT_SPACING, :vSpacing => DEFAULT_SPACING }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, text, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
    end
  end

  class FXHeaderItem
    alias old_initialize initialize
    def initialize(text, *args, &blk)
      argument_names = %w{ic s ptr}
      default_params = { :ic => nil, :s => 0, :ptr => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(text, params[:ic], params[:s], params[:ptr], &blk)
    end
  end

  class FXHeader
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => HEADER_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXHorizontalFrame
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_SPACING, :padRight => DEFAULT_SPACING, :padTop => DEFAULT_SPACING, :padBottom => DEFAULT_SPACING, :hSpacing => DEFAULT_SPACING, :vSpacing => DEFAULT_SPACING }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
    end
  end

  class FXICOIcon
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix clr opts width height}
      default_params = { :pix => nil, :clr => 0, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:clr], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXICOImage
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix opts width height}
      default_params = { :pix => nil, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXIcon
    alias old_initialize initialize
    def initialize(app, *args, &blk)
      argument_names = %w{pix clr opts width height}
      default_params = { :pix => nil, :clr => 0, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(app, params[:pix], params[:clr], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXIconDict
    alias old_initialize initialize
    def initialize(app, *args, &blk)
      argument_names = %w{path}
      default_params = { :path => FXIconDict.defaultIconPath }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(app, params[:path], &blk)
    end
  end

  class FXIconItem
    alias old_initialize initialize
    def initialize(text, *args, &blk)
      argument_names = %w{bigIcon miniIcon data}
      default_params = { :bigIcon => nil, :miniIcon => nil, :data => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(text, params[:bigIcon], params[:miniIcon], params[:data], &blk)
    end
  end

  class FXIconList
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => ICONLIST_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXImage
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pixels opts width height}
      default_params = { :pixels => nil, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pixels], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXImageFrame
    alias old_initialize initialize
    def initialize(p, img, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom}
      default_params = { :opts => FRAME_SUNKEN|FRAME_THICK, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, img, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXImageView
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{img target selector opts x y width height}
      default_params = { :img => nil, :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:img], params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXInputDialog
    alias old_initialize initialize
    def initialize(owner, caption, label, *args, &blk)
      argument_names = %w{icon opts x y width height}
      default_params = { :icon => nil, :opts => INPUTDIALOG_STRING, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, caption, label, params[:icon], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXJPGIcon
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix clr opts width height}
      default_params = { :pix => nil, :clr => 0, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:clr], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXJPGImage
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix opts width height}
      default_params = { :pix => nil, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXKnob
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => KNOB_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXLabel
    alias old_initialize initialize
    def initialize(parent, text, *args, &blk)
      argument_names = %w{icon opts x y width height padLeft padRight padTop padBottom}
      default_params = { :icon => nil, :opts => LABEL_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, text, params[:icon], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXListItem
    alias old_initialize initialize
    def initialize(text, *args, &blk)
      argument_names = %w{icon data}
      default_params = { :icon => nil, :data => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(text, params[:icon], params[:data], &blk)
    end
  end

  class FXList
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => LIST_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXListBox
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_SUNKEN|FRAME_THICK|LISTBOX_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXMainWindow
    alias old_initialize initialize
    def initialize(app, title, *args, &blk)
      argument_names = %w{icon miniIcon opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :icon => nil, :miniIcon => nil, :opts => DECOR_ALL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0, :hSpacing => 4, :vSpacing => 4 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(app, title, params[:icon], params[:miniIcon], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
    end
  end

  class FXMatrix
    alias old_initialize initialize
    def initialize(parent, *args, &blk)
      argument_names = %w{n opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :n => 1, :opts => MATRIX_BY_ROWS, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_SPACING, :padRight => DEFAULT_SPACING, :padTop => DEFAULT_SPACING, :padBottom => DEFAULT_SPACING, :hSpacing => DEFAULT_SPACING, :vSpacing => DEFAULT_SPACING }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, params[:n], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
    end
  end

  class FXMDIDeleteButton
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_RAISED, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXMDIRestoreButton
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_RAISED, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXMDIMaximizeButton
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_RAISED, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXMDIMinimizeButton
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_RAISED, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXMDIWindowButton
    alias old_initialize initialize
    def initialize(p, pup, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, pup, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXMDIMenu
    alias old_initialize initialize
    def initialize(owner, *args, &blk)
      argument_names = %w{target}
      default_params = { :target => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, params[:target], &blk)
    end
  end

  class FXMDIChild
    alias old_initialize initialize
    def initialize(p, name, *args, &blk)
      argument_names = %w{ic pup opts x y width height}
      default_params = { :ic => nil, :pup => nil, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, name, params[:ic], params[:pup], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXMDIClient
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXMemoryStream
    alias old_initialize initialize
    def initialize(*args, &blk)
      argument_names = %w{cont}
      default_params = { :cont => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(params[:cont], &blk)
    end
  end

  class FXMenuButton
    alias old_initialize initialize
    def initialize(parent, text, *args, &blk)
      argument_names = %w{icon popupMenu opts x y width height padLeft padRight padTop padBottom}
      default_params = { :icon => nil, :popupMenu => nil, :opts => JUSTIFY_NORMAL|ICON_BEFORE_TEXT|MENUBUTTON_DOWN, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_SPACING, :padRight => DEFAULT_SPACING, :padTop => DEFAULT_SPACING, :padBottom => DEFAULT_SPACING }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, text, params[:icon], params[:popupMenu], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXMenuCaption
    alias old_initialize initialize
    def initialize(parent, text, *args, &blk)
      argument_names = %w{icon opts}
      default_params = { :icon => nil, :opts => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, text, params[:icon], params[:opts], &blk)
    end
  end

  class FXMenuCascade
    alias old_initialize initialize
    def initialize(parent, text, *args, &blk)
      argument_names = %w{icon popupMenu opts}
      default_params = { :icon => nil, :popupMenu => nil, :opts => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, text, params[:icon], params[:popupMenu], params[:opts], &blk)
    end
  end

  class FXMenuCheck
    alias old_initialize initialize
    def initialize(p, text, *args, &blk)
      argument_names = %w{target selector opts}
      default_params = { :target => nil, :selector => 0, :opts => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, text, params[:target], params[:selector], params[:opts], &blk)
    end
  end

  class FXMenuCommand
    alias old_initialize initialize
    def initialize(p, text, *args, &blk)
      argument_names = %w{ic target selector opts}
      default_params = { :ic => nil, :target => nil, :selector => 0, :opts => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, text, params[:ic], params[:target], params[:selector], params[:opts], &blk)
    end
  end

  class FXMenuPane
    alias old_initialize initialize
    def initialize(owner, *args, &blk)
      argument_names = %w{opts}
      default_params = { :opts => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, params[:opts], &blk)
    end
  end

  class FXMenuRadio
    alias old_initialize initialize
    def initialize(p, text, *args, &blk)
      argument_names = %w{target selector opts}
      default_params = { :target => nil, :selector => 0, :opts => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, text, params[:target], params[:selector], params[:opts], &blk)
    end
  end

  class FXMenuSeparator
    alias old_initialize initialize
    def initialize(parent, *args, &blk)
      argument_names = %w{opts}
      default_params = { :opts => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, params[:opts], &blk)
    end
  end

  class FXMenuTitle
    alias old_initialize initialize
    def initialize(parent, text, *args, &blk)
      argument_names = %w{icon popupMenu opts}
      default_params = { :icon => nil, :popupMenu => nil, :opts => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, text, params[:icon], params[:popupMenu], params[:opts], &blk)
    end
  end

  class FXMessageBox
    alias old_initialize initialize
    def initialize(owner, caption, text, *args, &blk)
      argument_names = %w{ic opts x y}
      default_params = { :ic => nil, :opts => 0, :x => 0, :y => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, caption, text, params[:ic], params[:opts], params[:x], params[:y], &blk)
    end
  end

  class FXOption
    alias old_initialize initialize
    def initialize(p, text, *args, &blk)
      argument_names = %w{ic target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :ic => nil, :target => nil, :selector => 0, :opts => JUSTIFY_NORMAL|ICON_BEFORE_TEXT, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, text, params[:ic], params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXOptionMenu
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{pup opts x y width height padLeft padRight padTop padBottom}
      default_params = { :pup => nil, :opts => JUSTIFY_NORMAL|ICON_BEFORE_TEXT, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:pup], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXPacker
    alias old_initialize initialize
    def initialize(parent, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_SPACING, :padRight => DEFAULT_SPACING, :padTop => DEFAULT_SPACING, :padBottom => DEFAULT_SPACING, :hSpacing => DEFAULT_SPACING, :vSpacing => DEFAULT_SPACING }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
    end
  end

  class FXPCXIcon
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix clr opts width height}
      default_params = { :pix => nil, :clr => 0, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:clr], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXPCXImage
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix opts width height}
      default_params = { :pix => nil, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXPicker
    alias old_initialize initialize
    def initialize(p, text, *args, &blk)
      argument_names = %w{ic target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :ic => nil, :target => nil, :selector => 0, :opts => BUTTON_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, text, params[:ic], params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXPNGIcon
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix clr opts width height}
      default_params = { :pix => nil, :clr => 0, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:clr], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXPNGImage
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix opts width height}
      default_params = { :pix => nil, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXPopup
    alias old_initialize initialize
    def initialize(owner, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => POPUP_VERTICAL|FRAME_RAISED|FRAME_THICK, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXPPMIcon
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix clr opts width height}
      default_params = { :pix => nil, :clr => 0, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:clr], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXPPMImage
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix opts width height}
      default_params = { :pix => nil, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXPrintDialog
    alias old_initialize initialize
    def initialize(owner, name, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, name, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXProgressBar
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => PROGRESSBAR_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXProgressDialog
    alias old_initialize initialize
    def initialize(owner, caption, label, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => PROGRESSDIALOG_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, caption, label, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXRadioButton
    alias old_initialize initialize
    def initialize(parent, text, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => RADIOBUTTON_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, text, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXRealSlider
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => REALSLIDER_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXRealSpinner
    alias old_initialize initialize
    def initialize(p, cols, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => REALSPIN_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, cols, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXRegistry
    alias old_initialize initialize
    def initialize(*args, &blk)
      argument_names = %w{appKey vendorKey}
      default_params = { :appKey => "", :vendorKey => "" }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(params[:appKey], params[:vendorKey], &blk)
    end
  end

  class FXReplaceDialog
    alias old_initialize initialize
    def initialize(owner, caption, *args, &blk)
      argument_names = %w{ic opts x y width height}
      default_params = { :ic => nil, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, caption, params[:ic], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXRGBIcon
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix clr opts width height}
      default_params = { :pix => nil, :clr => 0, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:clr], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXRGBImage
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix opts width height}
      default_params = { :pix => nil, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXRuler
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => RULER_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXRulerView
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXScintilla
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXScrollArea
    alias old_initialize initialize
    def initialize(parent, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(parent, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXScrollBar
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => SCROLLBAR_VERTICAL, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXScrollPane
    alias old_initialize initialize
    def initialize(owner, nvis, *args, &blk)
      argument_names = %w{opts}
      default_params = { :opts => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, nvis, params[:opts], &blk)
    end
  end

  class FXScrollWindow
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXSearchDialog
    alias old_initialize initialize
    def initialize(owner, caption, *args, &blk)
      argument_names = %w{ic opts x y width height}
      default_params = { :ic => nil, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, caption, params[:ic], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXSeparator
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom}
      default_params = { :opts => SEPARATOR_GROOVE|LAYOUT_FILL_X, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXHorizontalSeparator
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom}
      default_params = { :opts => SEPARATOR_GROOVE|LAYOUT_FILL_X, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 1, :padRight => 1, :padTop => 0, :padBottom => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXVerticalSeparator
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom}
      default_params = { :opts => SEPARATOR_GROOVE|LAYOUT_FILL_Y, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 0, :padRight => 0, :padTop => 1, :padBottom => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXShutterItem
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{text icon opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :text => "", :icon => nil, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_SPACING, :padRight => DEFAULT_SPACING, :padTop => DEFAULT_SPACING, :padBottom => DEFAULT_SPACING, :hSpacing => DEFAULT_SPACING, :vSpacing => DEFAULT_SPACING }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:text], params[:icon], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
    end
  end

  class FXShutter
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_SPACING, :padRight => DEFAULT_SPACING, :padTop => DEFAULT_SPACING, :padBottom => DEFAULT_SPACING, :hSpacing => DEFAULT_SPACING, :vSpacing => DEFAULT_SPACING }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
    end
  end

  class FXSlider
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => SLIDER_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXSpinner
    alias old_initialize initialize
    def initialize(p, cols, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => SPIN_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, cols, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXSplashWindow
    alias old_initialize initialize
    def initialize(owner, icon, *args, &blk)
      argument_names = %w{opts ms}
      default_params = { :opts => SPLASH_SIMPLE, :ms => 5000 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, icon, params[:opts], params[:ms], &blk)
    end
  end

  class FXSpring
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts relw relh x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :opts => 0, :relw => 0, :relh => 0, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_SPACING, :padRight => DEFAULT_SPACING, :padTop => DEFAULT_SPACING, :padBottom => DEFAULT_SPACING, :hSpacing => DEFAULT_SPACING, :vSpacing => DEFAULT_SPACING }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:opts], params[:relw], params[:relh], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
    end
  end

  class FXStatusBar
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 3, :padRight => 3, :padTop => 2, :padBottom => 2, :hSpacing => 4, :vSpacing => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
    end
  end

  class FXStatusLine
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector}
      default_params = { :target => nil, :selector => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], &blk)
    end
  end

  class FXStream
    alias old_initialize initialize
    def initialize(*args, &blk)
      argument_names = %w{cont}
      default_params = { :cont => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(params[:cont], &blk)
    end
  end

  class FXSwitcher
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_SPACING, :padRight => DEFAULT_SPACING, :padTop => DEFAULT_SPACING, :padBottom => DEFAULT_SPACING }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXTabBar
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => TABBOOK_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_SPACING, :padRight => DEFAULT_SPACING, :padTop => DEFAULT_SPACING, :padBottom => DEFAULT_SPACING }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXTabBook
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => TABBOOK_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_SPACING, :padRight => DEFAULT_SPACING, :padTop => DEFAULT_SPACING, :padBottom => DEFAULT_SPACING }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXTabItem
    alias old_initialize initialize
    def initialize(p, text, *args, &blk)
      argument_names = %w{ic opts x y width height padLeft padRight padTop padBottom}
      default_params = { :ic => nil, :opts => TAB_TOP_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, text, params[:ic], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXTableItem
    alias old_initialize initialize
    def initialize(text, *args, &blk)
      argument_names = %w{icon data}
      default_params = { :icon => nil, :data => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(text, params[:icon], params[:data], &blk)
    end
  end

  class FXTable
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_MARGIN, :padRight => DEFAULT_MARGIN, :padTop => DEFAULT_MARGIN, :padBottom => DEFAULT_MARGIN }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXText
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 3, :padRight => 3, :padTop => 2, :padBottom => 2 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
    alias old_findText findText
    def findText(string, *args)
      argument_names = %w{start flags}
      default_params = { :start => 0, :flags => SEARCH_FORWARD|SEARCH_WRAP|SEARCH_EXACT }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_findText(string, params[:start], params[:flags])
    end
  end

  class FXTextField
    alias old_initialize initialize
    def initialize(p, ncols, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => TEXTFIELD_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, ncols, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXTGAIcon
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix clr opts width height}
      default_params = { :pix => nil, :clr => 0, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:clr], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXTGAImage
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix opts width height}
      default_params = { :pix => nil, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXTIFIcon
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix clr opts width height}
      default_params = { :pix => nil, :clr => 0, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:clr], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXTIFImage
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix opts width height}
      default_params = { :pix => nil, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXToggleButton
    alias old_initialize initialize
    def initialize(p, text1, text2, *args, &blk)
      argument_names = %w{icon1 icon2 target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :icon1 => nil, :icon2 => nil, :target => nil, :selector => 0, :opts => TOGGLEBUTTON_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, text1, text2, params[:icon1], params[:icon2], params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXToolBarGrip
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => TOOLBARGRIP_SINGLE, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXToolBarShell
    alias old_initialize initialize
    def initialize(owner, *args, &blk)
      argument_names = %w{opts x y width height hSpacing vSpacing}
      default_params = { :opts => FRAME_RAISED|FRAME_THICK, :x => 0, :y => 0, :width => 0, :height => 0, :hSpacing => 4, :vSpacing => 4 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:hSpacing], params[:vSpacing], &blk)
    end
  end

  class FXToolBarTab
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_RAISED, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXToolTip
    alias old_initialize initialize
    def initialize(app, *args, &blk)
      argument_names = %w{opts x y width height}
      default_params = { :opts => TOOLTIP_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(app, params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXTreeItem
    alias old_initialize initialize
    def initialize(text, *args, &blk)
      argument_names = %w{openIcon closedIcon data}
      default_params = { :openIcon => nil, :closedIcon => nil, :data => nil }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(text, params[:openIcon], params[:closedIcon], params[:data], &blk)
    end
  end

  class FXTreeList
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height}
      default_params = { :target => nil, :selector => 0, :opts => TREELIST_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], &blk)
    end
  end

  class FXTreeListBox
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :target => nil, :selector => 0, :opts => FRAME_SUNKEN|FRAME_THICK|TREELISTBOX_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXTriStateButton
    alias old_initialize initialize
    def initialize(p, text1, text2, text3, *args, &blk)
      argument_names = %w{icon1 icon2 icon3 target selector opts x y width height padLeft padRight padTop padBottom}
      default_params = { :icon1 => nil, :icon2 => nil, :icon3 => nil, :target => nil, :selector => 0, :opts => TOGGLEBUTTON_NORMAL, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_PAD, :padRight => DEFAULT_PAD, :padTop => DEFAULT_PAD, :padBottom => DEFAULT_PAD }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, text1, text2, text3, params[:icon1], params[:icon2], params[:icon3], params[:target], params[:selector], params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], &blk)
    end
  end

  class FXVerticalFrame
    alias old_initialize initialize
    def initialize(p, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :opts => 0, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => DEFAULT_SPACING, :padRight => DEFAULT_SPACING, :padTop => DEFAULT_SPACING, :padBottom => DEFAULT_SPACING, :hSpacing => DEFAULT_SPACING, :vSpacing => DEFAULT_SPACING }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(p, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
    end
  end

  class FXVisual
    alias old_initialize initialize
    def initialize(a, flgs, *args, &blk)
      argument_names = %w{d}
      default_params = { :d => 32 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, flgs, params[:d], &blk)
    end
  end

  class FXWizard
    alias old_initialize initialize
    def initialize(owner, name, image, *args, &blk)
      argument_names = %w{opts x y width height padLeft padRight padTop padBottom hSpacing vSpacing}
      default_params = { :opts => DECOR_TITLE|DECOR_BORDER|DECOR_RESIZE, :x => 0, :y => 0, :width => 0, :height => 0, :padLeft => 10, :padRight => 10, :padTop => 10, :padBottom => 10, :hSpacing => 10, :vSpacing => 10 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      if params.key? :padding
        value = params.delete(:padding)
        [:padLeft, :padRight, :padTop, :padBottom].each { |s| params[s] ||= value }
      end
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(owner, name, image, params[:opts], params[:x], params[:y], params[:width], params[:height], params[:padLeft], params[:padRight], params[:padTop], params[:padBottom], params[:hSpacing], params[:vSpacing], &blk)
    end
  end

  class FXXBMIcon
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pixels mask clr opts width height}
      default_params = { :pixels => nil, :mask => nil, :clr => 0, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pixels], params[:mask], params[:clr], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXXBMImage
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pixels mask opts width height}
      default_params = { :pixels => nil, :mask => nil, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pixels], params[:mask], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXXPMIcon
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix clr opts width height}
      default_params = { :pix => nil, :clr => 0, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:clr], params[:opts], params[:width], params[:height], &blk)
    end
  end

  class FXXPMImage
    alias old_initialize initialize
    def initialize(a, *args, &blk)
      argument_names = %w{pix opts width height}
      default_params = { :pix => nil, :opts => 0, :width => 1, :height => 1 }
      params = {}
      params = args.pop if args.last.is_a? Hash
      args.each_with_index { |e, i| params[argument_names[i].intern] = e }
      params.keys.each { |key| raise ArgumentError, "Unrecognized parameter #{key}" unless default_params.keys.include?(key) }
      params = default_params.merge(params)
      old_initialize(a, params[:pix], params[:opts], params[:width], params[:height], &blk)
    end
  end

end
