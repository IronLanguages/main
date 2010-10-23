x = "foo"
p x.gsub(/(o)/, '1')
p $1

r = x.gsub!(/a/) { break '2' }
p r.object_id == x.object_id
r = x.gsub(/a/) { break '2' }
p r.object_id == x.object_id
p r

p $1

x = "foo"
x.freeze

x.gsub!(/a/) { '2' } 
x.gsub!(/a/, '2') rescue p ($!).class


puts '---'

begin
  x.gsub!(/o/) { '2' } 
rescue 
  p $!.class
end  

x.gsub!(/o/, '2') rescue p ($!).class
