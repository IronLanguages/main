include Microsoft::Scripting::Silverlight

class RubyReplFormatter
  include IReplFormatter

  def initialize(repl, formatter)
    @repl = repl
    @formatter = formatter
  end

  def prompt_element(element)
  end

  def prompt_html
  end

  def sub_prompt_html
  end

  def format(obj)
    "=> #{obj.inspect}"
  end
end

def create_repl_formatter(repl, formatter)
  RubyReplFormatter.new(repl, formatter)
end
