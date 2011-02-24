prefixes = ["", "c", "m", "cm", "mc", "C", "C-", "C-\\M-", "cc"]

prefixes.each { |p| print '?\\', p, "$," }

puts

(0..255).each do |x|
  print x, "('", x.chr, "'): "
  prefixes.each do |prefix|
      begin
        print eval("?\\" + prefix + x.chr)
      rescue SyntaxError:
        print "error"
      end
      
      print ","
  end
  
  puts
  
end

puts "---"

['?\\', '?\1', '?\10', '?\100', '?\1000', 
'?\01', '?\001', '?\0001', 
'?\1_23',
'?\x', '?\x0', '?\x10', '?\xFa', '?\x100', 
'?\X10', '?\d10', '?\b101',
'?\C', '?\Ca', '?\C-', '?\C-a', 
'?\C-\M-\C-\M-\C-\C-x',
'?\c\c\c\cg',
'?\c\C-\cg',
'?\c\cM-\cg',
'?\c\n', '?\c\x10',
'?\m', '?\cm#', '?\m#'
].each do |x|
  print "\"", x, "\":"
  begin
    puts eval(x)
  rescue SyntaxError:
    print "error\n"
  end
end