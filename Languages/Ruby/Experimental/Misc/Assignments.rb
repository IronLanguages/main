a = b = 1

(0..255).each do |x|
  begin
    z = eval("a #{x.chr}= b")
    puts "0x#{sprintf('%X', x)}: a #{x.chr}= b: OK";
  rescue SyntaxError:
  rescue NoMethodError:
    puts "0x#{sprintf('%X', x)}: a #{x.chr}= b: OK";
  end
end

a ||= b
a &&= b

# TODO: what is this?
a ,= b

