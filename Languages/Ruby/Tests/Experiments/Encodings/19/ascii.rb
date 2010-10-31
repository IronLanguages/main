p "a".encoding
p "a\x20".encoding
p "a\x81".encoding
p ("a\xff" "b").encoding
p ("b" "a\xff").encoding
p "a\u00ff".encoding
p "a\x20\u00ff".encoding
p ?\x20.encoding
p ?\x82.encoding

# error: utf8_mixed_within_usascii_source.rb
begin
  eval('"a\x81\u00ff"')
rescue SyntaxError
  p $!.message
end
