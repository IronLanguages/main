class FoxDisplayer
  attr_accessor :reader

  def initialize(text_field)
    @text_field = text_field
    @formatter = FoxTextFormatter.new(70, "") do |arg, style|
      startpos = @str.size
      @str << arg
      @formats.push [startpos, arg.size, style]
    end
    @reader = nil
  end

  def width=(newWidth)
    @formatter.width = newWidth
  end

  def no_info_available
    @text_field.text="nothing here, move on!"
  end

  def init_text
    @str = ""
    @formats = Array.new
  end

  # Sets a new text, and all styles
  def set_text
    @text_field.text = @str
    @formats.each do |start, n, style|
      case style
      when FoxTextFormatter::STYLE_BOLD
        @text_field.changeStyle(start, n, 2)
      when FoxTextFormatter::STYLE_H1
        @text_field.changeStyle(start, n, 3)
      when FoxTextFormatter::STYLE_H2
        @text_field.changeStyle(start, n, 4)
      when FoxTextFormatter::STYLE_H3
        @text_field.changeStyle(start, n, 5)
      when FoxTextFormatter::STYLE_TELETYPE
        @text_field.changeStyle(start, n, 6)
      when FoxTextFormatter::STYLE_CODE
        @text_field.changeStyle(start, n, 7)
      when FoxTextFormatter::STYLE_EMPHASIS
        @text_field.changeStyle(start, n, 8)
      when FoxTextFormatter::STYLE_CLASS
        @text_field.changeStyle(start, n, 9)
      else
        @text_field.changeStyle(start, n, 1)
      end

    end
  end

  # Display method information
  def display_method_info(method)
    init_text
    @formatter.draw_line(method.full_name)
    @formatter.display_params(method)
    @formatter.draw_line
    display_flow(method.comment)

    if method.aliases && !method.aliases.empty?
      @formatter.blankline
      aka = "(also known as "
      aka << method.aliases.map {|a| a.name }.join(", ")
      aka << ")"
      @formatter.wrap(aka)
    end
    set_text
  end

  def display_information(message)
    init_text
    display_flow(message)
    set_text
  end

  def display_class_info(klass)
    init_text
    superclass = klass.superclass_string
    if superclass
      superclass = " < " + superclass
    else
      superclass = ""
    end
    @formatter.draw_line(klass.display_name + ": " + klass.full_name + superclass)
    display_flow(klass.comment)
    @formatter.draw_line

    unless klass.includes.empty?
      @formatter.blankline
      @formatter.display_heading("Includes:", 2, "")
      incs = []
      klass.includes.each do |inc|
        inc_desc = @reader.find_class_by_name(inc.name)
        if inc_desc
          str = inc.name + "("
          str << inc_desc.instance_methods.map{|m| m.name}.join(", ")
          str << ")"
          incs << str
        else
          incs << inc.name
        end
      end
      @formatter.wrap(incs.sort.join(', '))
    end

    unless klass.constants.empty?
      @formatter.blankline
      @formatter.display_heading("Constants:", 2, "")
      len = 0
      klass.constants.each { |c| len = c.name.length if c.name.length > len }
      len += 2
      klass.constants.each do |c|
        @formatter.wrap(c.value, @formatter.indent+((c.name+":").ljust(len)))
      end
    end

    unless klass.class_methods.empty?
      @formatter.blankline
      @formatter.display_heading("Class methods:", 2, "")
      @formatter.wrap(klass.class_methods.map{|m| m.name}.sort.join(', '))
    end

    unless klass.instance_methods.empty?
      @formatter.blankline
      @formatter.display_heading("Instance methods:", 2, "")
      @formatter.wrap(klass.instance_methods.map{|m| m.name}.sort.join(', '))
    end

    unless klass.attributes.empty?
      @formatter.blankline
      @formatter.wrap("Attributes:", "")
      @formatter.wrap(klass.attributes.map{|a| a.name}.sort.join(', '))
    end

    set_text
  end

  def display_flow(flow)
    if !flow || flow.empty?
      @formatter.wrap("(no description...)\n")
    else
      @formatter.display_flow(flow)
    end
  end
end
