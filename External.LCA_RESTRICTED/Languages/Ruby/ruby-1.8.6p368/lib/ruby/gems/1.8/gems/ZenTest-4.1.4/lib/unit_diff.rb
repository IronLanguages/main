require 'tempfile'

##
# UnitDiff makes reading Test::Unit output easy and fun.  Instead of a
# confusing jumble of text with nearly unnoticable changes like this:
#
#   1) Failure:
#   test_to_gpoints(RouteTest) [test/unit/route_test.rb:29]:
#   <"new GPolyline([\n  new GPoint(  47.00000, -122.00000),\n  new GPoint(  46.5000
#   0, -122.50000),\n  new GPoint(  46.75000, -122.75000),\n  new GPoint(  46.00000,
#    -123.00000)])"> expected but was
#   <"new Gpolyline([\n  new GPoint(  47.00000, -122.00000),\n  new GPoint(  46.5000
#   0, -122.50000),\n  new GPoint(  46.75000, -122.75000),\n  new GPoint(  46.00000,
#    -123.00000)])">.
#
#
# You get an easy-to-read diff output like this:
#
#   1) Failure:
#   test_to_gpoints(RouteTest) [test/unit/route_test.rb:29]:
#   1c1
#   < new GPolyline([
#   ---
#   > new Gpolyline([
#
# == Usage
#
#   test.rb | unit_diff [options]
#     options:
#     -b ignore whitespace differences
#     -c contextual diff
#     -h show usage
#     -k keep temp diff files around
#     -l prefix line numbers on the diffs
#     -u unified diff
#     -v display version

class UnitDiff

  WINDOZE = /win32/ =~ RUBY_PLATFORM unless defined? WINDOZE
  DIFF = if WINDOZE
           'diff.exe'
         else
           if system("gdiff", __FILE__, __FILE__)
             'gdiff' # solaris and kin suck
           else
             'diff'
           end
         end unless defined? DIFF

  ##
  # Handy wrapper for UnitDiff#unit_diff.

  def self.unit_diff
    trap 'INT' do exit 1 end
    puts UnitDiff.new.unit_diff
  end

  def parse_input(input, output)
    current = []
    data = []
    data << current
    print_lines = true

    term = "\nFinished".split(//).map { |c| c[0] }
    term_length = term.size

    old_sync = output.sync
    output.sync = true
    while line = input.gets
      case line
      when /^(Loaded suite|Started)/ then
        print_lines = true
        output.puts line
        chars = []
        while c = input.getc do
          output.putc c
          chars << c
          tail = chars[-term_length..-1]
          break if chars.size >= term_length and tail == term
        end
        output.puts input.gets # the rest of "Finished in..."
        output.puts
       next
      when /^\s*$/, /^\(?\s*\d+\) (Failure|Error):/, /^\d+\)/ then
        print_lines = false
        current = []
        data << current
      when /^Finished in \d/ then
        print_lines = false
      end
      output.puts line if print_lines
      current << line
    end
    output.sync = old_sync
    data = data.reject { |o| o == ["\n"] or o.empty? }
    footer = data.pop

    return data, footer
  end

  # Parses a single diff recording the header and what
  # was expected, and what was actually obtained.
  def parse_diff(result)
    header = []
    expect = []
    butwas = []
    footer = []
    found = false
    state = :header

    until result.empty? do
      case state
      when :header then
        header << result.shift
        state = :expect if result.first =~ /^<|^Expected/
      when :expect then
        case result.first
        when /^Expected (.*?) to equal (.*?):$/ then
          expect << $1
          butwas << $2
          state = :footer
          result.shift
        when /^Expected (.*?), not (.*)$/m then
          expect << $1
          butwas << $2
          state = :footer
          result.shift
        when /^Expected (.*?)$/ then
          expect << "#{$1}\n"
          result.shift
        when /^to equal / then
          state = :spec_butwas
          bw = result.shift.sub(/^to equal (.*):?$/, '\1')
          butwas << bw
        else
          state = :butwas if result.first.sub!(/ expected( but was|, not)/, '')
          expect << result.shift
        end
      when :butwas then
        butwas = result[0..-1]
        result.clear
      when :spec_butwas then
        if result.first =~ /^\s+\S+ at |^:\s*$/
          state = :footer
        else
          butwas << result.shift
        end
      when :footer then
        butwas.last.sub!(/:$/, '')
        footer = result.map {|l| l.chomp }
        result.clear
      else
        raise "unknown state #{state}"
      end
    end

    return header, expect, nil, footer if butwas.empty?

    expect.last.chomp!
    expect.first.sub!(/^<\"/, '')
    expect.last.sub!(/\">$/, '')

    butwas.last.chomp!
    butwas.last.chop! if butwas.last =~ /\.$/
    butwas.first.sub!( /^<\"/, '')
    butwas.last.sub!(/\">$/, '')

    return header, expect, butwas, footer
  end

  ##
  # Scans Test::Unit output +input+ looking for comparison failures and makes
  # them easily readable by passing them through diff.

  def unit_diff(input=ARGF, output=$stdout)
    $b = false unless defined? $b
    $c = false unless defined? $c
    $k = false unless defined? $k
    $u = false unless defined? $u

    data, footer = self.parse_input(input, output)

    output = []

    # Output
    data.each do |result|
      first = []
      second = []

      if result.first =~ /Error/ then
        output.push result.join('')
        next
      end

      prefix, expect, butwas, result_footer = parse_diff(result)

      output.push prefix.compact.map {|line| line.strip}.join("\n")

      if butwas then
        output.push self.diff(expect, butwas)

        output.push result_footer
        output.push ''
      else
        output.push expect.join('')
      end
    end

    if footer then
      footer.shift if footer.first.strip.empty?# unless footer.first.nil?
      output.push footer.compact.map {|line| line.strip}.join("\n")
    end

    return output.flatten.join("\n")
  end

  def diff expect, butwas
    output = nil

    Tempfile.open("expect") do |a|
      a.write(massage(expect))
      a.rewind
      Tempfile.open("butwas") do |b|
        b.write(massage(butwas))
        b.rewind

        diff_flags = $u ? "-u" : $c ? "-c" : ""
        diff_flags += " -b" if $b

        result = `#{DIFF} #{diff_flags} #{a.path} #{b.path}`
        output = if result.empty? then
                   "[no difference--suspect ==]"
                 else
                   result.split(/\n/)
                 end

        if $k then
          warn "moving #{a.path} to #{a.path}.keep"
          File.rename a.path, a.path + ".keep"
          warn "moving #{b.path} to #{b.path}.keep"
          File.rename b.path, b.path + ".keep"
        end
      end
    end

    output
  end

  def massage(data)
    count = 0
    # unescape newlines, strip <> from entire string
    data = data.join
    data = data.gsub(/\\n/, "\n").gsub(/0x[a-f0-9]+/m, '0xXXXXXX') + "\n"
    data += "\n" unless data[-1] == ?\n
    data
  end
end
