[
:|, :^, :&, :==, :===, :=~, :>, :<, :>=, :<=, :<<, :>>, :+, :-, :*, :/, :%, :**, :~, :+@, :-@, :[], :[]=, :`,
:_, :idf, :idf_0, :nil, :true, :false, :while,
:C, :@x, :@@x, :"@#{'@'}y", :$x, 
:$`, :$0, :$1123, :$+, :$~, :$+, :$`, :$:, :$", :$$, :$=, :$-D, :$-0, 

:'$0X',
:'$1X',
:'$--', 
:'$-&^%*@#@-', :"=>", 
].each { |s| p s }


begin
  eval(':""') 
rescue Exception
  p $!
end

begin
  eval(":''") 
rescue Exception
  p $!
end

begin
  p eval(':"#{}"') 
rescue Exception
  p $!
end