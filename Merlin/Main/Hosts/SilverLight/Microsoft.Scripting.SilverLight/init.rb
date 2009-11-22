module Kernel
  def document
    System::Windows::Browser::HtmlPage.document
  end

  def window
    System::Windows::Browser::HtmlPage.window
  end
end

class NilClass
  def blank?
    true
  end
end

class String
  def blank?
    empty?
  end
end

class System::Windows::FrameworkElement
  # Monkey-patch FrameworkElement to allow element.ChildName instead of window.FindName("ChildName")
  # If FindName doesn't yield an object, it tried the Resources collection (for things like Storyboards)
  def method_missing name, *args
    obj   = find_name(name.to_s.to_clr_string) 
    obj ||= self.resources[name.to_s.to_clr_string]
    obj || super
  end

  def hide!
    self.visibility = System::Windows::Visibility.hidden
  end

  def collapse!
    self.visibility = System::Windows::Visibility.collapsed
  end

  def show!
    self.visibility = System::Windows::Visibility.visible
  end
end

module System::Windows::Browser
  module HtmlInspector
    def inspect
      name = "#<#{self.class}"
      name << " id=#{self.Id}" unless self.Id.blank?
      name << " class=#{self.css_class}" unless self.css_class.blank?
      name << ">"
    end
  end

  class HtmlDocument
    include HtmlInspector

    def method_missing(m, *args)
      get_element_by_id(m.to_s)
    end

    def tags(name)
      get_elements_by_tag_name(name.to_s)
    end
  end

  class System::Windows::Browser::ScriptObjectCollection
    include Enumerable

    def size
      count
    end

    def first
      self[0] if size > 0
    end

    def last
      self[size - 1] if size > 0
    end

    def empty?
      size == 0
    end

    def inspect
      "#<#{self.class} size=#{self.size}>"
    end
  end

  class HtmlElement
    include HtmlInspector

    def [](index)
      get_attribute(index.to_s)
    end

    def []=(index, value)
      set_attribute(index.to_s, value)
    end

    def method_missing(m, *args, &block)
      if block.nil?
        if m.to_s[-1..-1] == '='
          set_property(m.to_s[0..-2], args.first)
        else
          id = get_property(m.to_s) 
          return id unless id.nil?
          raise e
        end
      else
        unless attach_event(m.to_s.to_clr_string, System::EventHandler.of(HtmlEventArgs).new(&block))
          raise e
        end
      end
    end

    def style
      HtmlStyle.new(self)
    end
  end

  class HtmlStyle
    def initialize(element)
      @element = element
    end

    def [](index)
      @element.get_style_attribute(index.to_s)
    end 

    def []=(index, value)
      @element.set_style_attribute(index.to_s, value)
    end

    def method_missing(m, *args)
      super
    rescue => e
      if m.to_s[-1..-1] == "="
        self[m.to_s[0..-2]] = args.first
      else
        style = self[m]
        return style unless style.nil?
        raise e
      end
    end
  end
end
