# encoding: UTF-8

$KCODE = 'U'

string = 'xxx€€€xxx'

x = "€"
y = "\342\202\254"

p /(#{x}+)/ =~ string 
puts $1.dump

p /(#{y}+)/ =~ string 
puts $1.dump




s = /(€+)/
r = /(\342\202\254+)/
t = /(\xe2\x82\xac+)/

p s, r, t

p s =~ string 
p $1
p r =~ string 
p $1
p t =~ string 
p $1

a = "(\\xe2\\x82\\xac+)"
u = /#{a}/
p u =~ string

a = "\\xe2\\x82\\xac"
u = /#{a}/
p u =~ "hello\xe2\x82\xacworld"



r = /([\xE0-\xEF][\x80-\xBF]{2})+/
p r =~ "xxx€€€xxx"
p r.source
