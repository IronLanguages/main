#encoding: SJIS

p "1234".encoding
p Regexp.new("1234").encoding

y = /12#{"34"}/
p y, y.encoding

y = /12/s
p y, y.encoding

y = /12/e
p y, y.encoding

y = /12/u
p y, y.encoding

y = /abc\u1234/
p y, y.encoding

y = /\x82\x45/
p y, y.encoding

begin
  eval('/\x82/')
rescue Exception
  p $!
end

begin
  eval('/\u1234/')
rescue Exception
  p $!
end

begin
  eval('/a\xaabc\u1234/')
rescue Exception
  p $!
end


