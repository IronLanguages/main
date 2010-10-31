path = "C:\\temp\\ruby.txt"

["rb",
nil, 
"", 
"r", 
"w",
"b",
"t",
"a",

"+",
"r+", 
"w+",
"b+",
"t+",
"a+",

"rb",
"wb",
"ab",
"rt",
"wt",
"at",

"rb+",
"wb+",
"ab+",
"rt+",
"wt+",
"at+",

"br",
"bw",
"ba",
"tr",
"tw",
"bt",

"rrb+",

].each { |mode|
  begin
    open(path, mode) { }
  rescue
    puts "#{mode.inspect} -> #{$!.inspect}"
  else
    puts "#{mode.inspect} -> ok"
  end
}