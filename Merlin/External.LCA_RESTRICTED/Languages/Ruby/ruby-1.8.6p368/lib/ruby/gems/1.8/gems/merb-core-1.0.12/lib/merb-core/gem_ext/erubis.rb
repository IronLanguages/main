require 'erubis'
module Erubis
  # This adds support for embedding the return value of a block call:
  #   <%= foo do %>...<% end =%>
  #
  # :api: private
  module Basic::Converter
    def convert_input(src, input)
      pat = @pattern
      regexp = pat.nil? || pat == '<% %>' ? DEFAULT_REGEXP : pattern_regexp(pat)
      pos = 0
      is_bol = true     # is beginning of line
      input.scan(regexp) do |indicator, code, tailch, rspace|
        match = Regexp.last_match()
        len  = match.begin(0) - pos
        text = input[pos, len]
        pos  = match.end(0)
        ch   = indicator ? indicator[0] : nil
        lspace = ch == ?= ? nil : detect_spaces_at_bol(text, is_bol)
        is_bol = rspace ? true : false
        add_text(src, text) if text && !text.empty?
        ## * when '<%= %>', do nothing
        ## * when '<% %>' or '<%# %>', delete spaces iff only spaces are around '<% %>'
        if ch == ?=              # <%= %>
          rspace = nil if tailch && !tailch.empty?
          add_text(src, lspace) if lspace
          add_expr(src, code, indicator)
          add_text(src, rspace) if rspace
        elsif ch == ?\#          # <%# %>
          n = code.count("\n") + (rspace ? 1 : 0)
          if @trim && lspace && rspace
            add_stmt(src, "\n" * n)
          else
            add_text(src, lspace) if lspace
            add_stmt(src, "\n" * n)
            add_text(src, rspace) if rspace
          end
        elsif ch == ?%           # <%% %>
          s = "#{lspace}#{@prefix||='<%'}#{code}#{tailch}#{@postfix||='%>'}#{rspace}"
          add_text(src, s)
        else                     # <% %>
          if @trim && lspace && rspace
            if respond_to?(:add_stmt2)
              add_stmt2(src, "#{lspace}#{code}#{rspace}", tailch)
            else
              add_stmt(src, "#{lspace}#{code}#{rspace}")
            end
          else
            add_text(src, lspace) if lspace
            if respond_to?(:add_stmt2)
              add_stmt2(src, code, tailch)
            else
              add_stmt(src, code)
            end
            add_text(src, rspace) if rspace
          end
        end
      end
      #rest = $' || input                        # ruby1.8
      rest = pos == 0 ? input : input[pos..-1]   # ruby1.9
      add_text(src, rest)
    end
    
  end
  
  class MEruby < Erubis::Eruby
    include PercentLineEnhancer
    include StringBufferEnhancer
  end

  # Loads a file, runs it through Erubis and parses it as YAML.
  #
  # ===== Parameters
  # file<String>:: The name of the file to load.
  # binding<Binding>::
  #   The binding to use when evaluating the ERB tags. Defaults to the current
  #   binding.
  #
  # :api: private
  def self.load_yaml_file(file, binding = binding)
    YAML::load(Erubis::MEruby.new(IO.read(File.expand_path(file))).result(binding))
  end
end
