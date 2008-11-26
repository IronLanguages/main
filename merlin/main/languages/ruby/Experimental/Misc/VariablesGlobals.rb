puts "$#"

puts
(0..255).each do |x|
  begin
    z = eval("$#{x.chr}")
    puts "case '#{x.chr}': break;";
  rescue SyntaxError:
  end
end

puts "$-#"

puts
(0..255).each do |x|
  begin
    z = eval("$-#{x.chr}")
    puts "case '#{x.chr}': break;";
  rescue SyntaxError:
  end
end

puts "-------"

g = [
"-Z",
"FOO65456",
"_",
"0124364654654",
"_0",
"-0", 
"0", 
"00", 
"100", 
"0100", 
"000Z", 
"104A", 
"104545_",
"104545FOO",
"_",
"<",
"&",
]

puts "-------"

for x in g
  begin
    cmd = "$x = $#{x}"
    z = eval(cmd)
    puts "#{cmd}: OK";
  rescue SyntaxError:
    puts "#{cmd}: syntax error";
  rescue NameError:
    puts "#{cmd}: name error";
  end
end

puts "-------"

for x in g
  begin
    cmd = "alias $x $#{x}"
    z = eval(cmd)
    puts "#{cmd}: OK";
  rescue SyntaxError:
    puts "#{cmd}: syntax error";
  rescue NameError:
    puts "#{cmd}: name error";
  end
end

puts "-------"

for x in g
  begin
    cmd = "alias $#{x} $x"
    z = eval(cmd)
    puts "#{cmd}: OK";
  rescue SyntaxError:
    puts "#{cmd}: syntax error";
  rescue NameError:
    puts "#{cmd}: name error";
  end
end
