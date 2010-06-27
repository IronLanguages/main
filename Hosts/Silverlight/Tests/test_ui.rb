def send_cmd(cmd)
  cmd.each_byte do |i|
    send_key i
    $repl.input.set_property 'value', $repl.input.value + i.chr
  end
end

def send_key(type_or_char)
  arrows = {:up => 38, :down => 40, :return => 13}
  char = if type_or_char.kind_of?(Symbol) || type_or_char.kind_of?(String)
           arrows[type_or_char.to_sym]
         else
           type_or_char
         end
  $repl.send_character_code char, false, false
end

def reset
  $repl.history.clear
  $repl.input.set_property 'value', ''
end

describe "REPL History" do
  before { reset }
  after  { reset }

  it 'remembered current buffer' do
    cmd = "2 + 2"
    send_cmd cmd
    send_key :up
    $repl.history.commands[0].should == cmd.chomp
    $repl.history.commands.size.should == 1
    send_key :down
    send_key :down
  end

  it 'has Windows-style history' do
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
    $repl.history.commands[0].should == cmd.chomp
    $repl.history.commands.size.should == 2
    send_key :down
    send_key :down
  end

  it 'handles deletes in current buffer' do
    # NOTE: the REPL doesn't track any keystrokes other than return, up, and 
    # down, so actually simulating a delete is not possible. Instead, just make
    # sure when the up key is pressed, the actual value of the repl input is
    # remembered, rather than what this history might know.
    send_cmd "2 + 2"
    $repl.input.set_property('value', "2 - 2")
    send_key :up
    $repl.input.value.should == "2 - 2"
  end
end
