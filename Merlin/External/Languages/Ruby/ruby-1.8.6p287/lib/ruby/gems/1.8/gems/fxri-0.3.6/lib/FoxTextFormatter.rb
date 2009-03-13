# This class is mostly copy & paste from ri_formatter.rb.
# except that it always returns a string and does not print anything.
class FoxTextFormatter
  # define all possible styles
  STYLE_NORMAL = :STYLE_NORMAL
  STYLE_BOLD = :STYLE_BOLD
  STYLE_CLASS = :STYLE_CLASS
  STYLE_H1 = :STYLE_H1
  STYLE_H2 = :STYLE_H2
  STYLE_H3 = :STYLE_H3
  STYLE_TELETYPE = :STYLE_TELETYPE
  STYLE_CODE = :STYLE_CODE
  STYLE_EMPHASIS = :STYLE_EMPHASIS

  attr_reader :indent
  attr_accessor :width

  # whenever text should be printed/added/shown, proc is called with the text as the argument.
  def initialize(width, indent, &proc)
    @width = width
    @indent = indent
    @proc = proc
  end

  ######################################################################

  def draw_line(label=nil)
    len = @width
    len -= (label.size+1) if label
    len = [0, len].max
    @proc.call("-"*len)
    if label
      @proc.call(" ")
      @proc.call(label, STYLE_CLASS)
    end
    @proc.call("\n")
  end

  def display_params(method)
    params = method.params

    if params[0] == ?(
      if method.is_singleton
        params = method.full_name + params
      else
        params = method.name + params
      end
    end
    params.split(/\n/).each do |param|
      @proc.call(param+"\n", STYLE_BOLD)
    end
  end

  def wrap(txt,  prefix=@indent, linelen=@width)
    return if !txt || txt.empty?
    @proc.call(prefix, STYLE_EMPHASIS)
    conv_markup(txt, prefix, linelen)
=begin
    textLen = linelen - prefix.length
    patt = Regexp.new("^(.{0,#{textLen}})[ \n]")
    next_prefix = prefix.tr("^ ", " ")

    res = []

    while work.length > textLen
      if work =~ patt
        res << $1
        work.slice!(0, $&.length)
      else
        res << work.slice!(0, textLen)
      end
    end
    res << work if work.length.nonzero?
    @proc.call(prefix +  res.join("\n" + next_prefix) + "\n")
=end
  end

  ######################################################################

  def blankline
    @proc.call("\n")
  end

  ######################################################################

  # called when we want to ensure a nbew 'wrap' starts on a newline
  # Only needed for HtmlFormatter, because the rest do their
  # own line breaking

  def break_to_newline
  end

  ######################################################################

  def bold_print(txt)
    @proc.call(txt, STYLE_BOLD)
  end

  ######################################################################

  def raw_print_line(txt)
    @proc.call(txt)
  end

  ######################################################################

  # convert HTML entities back to ASCII
  def conv_html(txt)
    case txt
    when Array
      txt.join.
      gsub(/&gt;/, '>').
      gsub(/&lt;/, '<').
      gsub(/&quot;/, '"').
      gsub(/&amp;/, '&')
    else # it's a String
      txt.
      gsub(/&gt;/, '>').
      gsub(/&lt;/, '<').
      gsub(/&quot;/, '"').
      gsub(/&amp;/, '&')
    end
  end

  # convert markup into display form
  def conv_markup(txt, prefix, linelen)

    # this code assumes that tags are not used inside tags
    pos = 0
    old_pos = 0
    style = STYLE_NORMAL
    current_indent = prefix.size
    while pos = txt.index(%r{(<tt>|<code>|<b>|<em>|</tt>|</code>|</b>|</em>)}, old_pos)
      new_part = txt[old_pos...pos]
      @proc.call(new_part, style)

      # get tag name
      old_pos = txt.index(">", pos) + 1
      style = case txt[(pos+1)...(old_pos-1)]
        when "tt"
          STYLE_TELETYPE
        when "code"
          STYLE_CODE
        when "b"
          STYLE_BOLD
        when "em"
          STYLE_EMPHASIS
        else
          # closing or unknown tags
          STYLE_NORMAL
        end
    end
    @proc.call(txt[old_pos...txt.size], style)
    @proc.call("\n")
  end

  ######################################################################

  def display_list(list)
    case list.type
    when SM::ListBase::BULLET
      prefixer = proc { |ignored| @indent + "*   " }

    when SM::ListBase::NUMBER,
      SM::ListBase::UPPERALPHA,
      SM::ListBase::LOWERALPHA

      start = case list.type
        when SM::ListBase::NUMBER      then 1
        when SM::ListBase::UPPERALPHA then 'A'
        when SM::ListBase::LOWERALPHA then 'a'
        end
      prefixer = proc do |ignored|
        res = @indent + "#{start}.".ljust(4)
        start = start.succ
        res
      end

    when SM::ListBase::LABELED
      prefixer = proc do |li|
        li.label
      end

    when SM::ListBase::NOTE
      longest = 0
      list.contents.each do |item|
        if item.kind_of?(SM::Flow::LI) && item.label.length > longest
          longest = item.label.length
        end
      end

      prefixer = proc do |li|
        @indent + li.label.ljust(longest+1)
      end

    else
      fail "unknown list type"
    end

    list.contents.each do |item|
      if item.kind_of? SM::Flow::LI
        prefix = prefixer.call(item)
        display_flow_item(item, prefix)
      else
        display_flow_item(item)
      end
    end
  end

  ######################################################################

  def display_flow_item(item, prefix=@indent)
    
    case item
    when SM::Flow::P, SM::Flow::LI
      wrap(conv_html(item.body), prefix)
      blankline

    when SM::Flow::LIST
      display_list(item)

    when SM::Flow::VERB
      display_verbatim_flow_item(item, @indent)

    when SM::Flow::H
      display_heading(conv_html(item.text), item.level, @indent)

    when SM::Flow::RULE
      draw_line

    when String
      wrap(conv_html(item), prefix)

    else
      fail "Unknown flow element: #{item.class}"
    end
  end

  ######################################################################

  def display_verbatim_flow_item(item, prefix=@indent)
    item.body.split(/\n/).each do |line|
      @proc.call(@indent)
      @proc.call(conv_html(line))
      @proc.call("\n")
    end
    blankline
  end

  ######################################################################

  def display_heading(text, level, indent)
    case level
    when 1
      @proc.call(text, STYLE_H1)
      @proc.call("\n")

    when 2
      @proc.call(text, STYLE_H2)
      @proc.call("\n")

    else
      @proc.call(indent)
      @proc.call(text, STYLE_H3)
      @proc.call("\n")
    end
  end


  def display_flow(flow)
    flow.each do |f|
      display_flow_item(f)
    end
  end
end