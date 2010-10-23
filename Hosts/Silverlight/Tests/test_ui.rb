def send_cmd(cmd)
  $repl.input.set_property 'value', ''
  cmd.each_byte do |i|
    send_key i
    $repl.input.set_property 'value', $repl.input.value + i.chr
    $repl.input.set_property 'value', '' if i.chr == "\r"
  end
end

def send_key(type_or_char)
  $__arrows ||= {:up => 38, :down => 40, :return => 13}
  char = if type_or_char.kind_of?(Symbol) || type_or_char.kind_of?(String)
           $__arrows[type_or_char.to_sym]
         else
           type_or_char
         end
  $repl.send_character_code char, false, false
end

def reset
  $repl.reset_history
  $repl.input.set_property 'value', ''
end

describe "REPL History" do
  before { reset }
  after  { reset }

  it 'does not remember current buffer' do
    cmd = "2 + 2"
    send_cmd cmd
    send_key :up
    $repl.history.items.size.should == 0
  end

  it 'has "live" history' do
    send_cmd "2 + 2\r"
    lines = ['def foo', "  puts 'hi'", 'end']
    cmd = lines.join("\r") + "\r"
    send_cmd cmd
    lines.size.times{ send_key :up }
    lines.each do |line|
      $repl.input.value.should == line
      send_key :return
      send_key :down
    end
  end

  it 'uparrow shows previous command' do
    cmd = "2 + 2\r"
    send_cmd cmd
    send_key :up
    $repl.history.items[0].text.should == cmd.chomp
    $repl.history.items.size.should == 1
    send_key :down
    send_key :down
  end

end
