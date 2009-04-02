require './hpricot_scan.so'

doc = "<doc><person><test>YESSS</test></person><train>SET</train></doc>"
Hpricot.scan(doc) { |x| p x }
p Hpricot.lemon(doc)
