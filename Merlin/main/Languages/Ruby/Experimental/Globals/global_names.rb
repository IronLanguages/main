(32..255).each { |c|
  x = "x"
  x[0] = c
  
  begin
    eval("a = $-#{x}")
  rescue SyntaxError
    val = false
  rescue TypeError
    val = true
  else
	val = true
  end
  
  puts "#{c}: #{x}" if val
}



