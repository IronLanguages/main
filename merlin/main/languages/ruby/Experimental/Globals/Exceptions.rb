def foo
  raise 'Foo'
end

puts 1
p $!
p $@

begin
  foo
rescue
  puts 2
  p $!
  p $@
ensure
  puts 3
  p $!
  p $@
end  

puts 4
p $!
p $@

puts
puts '### assignment: '
puts

def try_set(value)
  begin
    $@ = value 
  rescue 
    puts "#{value.inspect}: #{$!.class}('#{$!}')"
  else
    puts "#{value.inspect}: OK"
  end  
end

try_set ["A"]
try_set 1

begin
  foo
rescue
  try_set nil 
  try_set []
  try_set ["foo"]
  try_set 1
  try_set [1]
  try_set ["foo", 1]

  class BobArray
    def to_ary
      ['BobArray']
    end
  end
  
  class BobString
    def to_s
      'BobString'
    end
  end

  try_set BobArray.new
  try_set [BobString.new]
  
  class SubArray < Array    
  end

  class SubString < String
  end
  
  try_set SubArray.new
  try_set [SubString.new]
  
  $! = nil
  try_set ["B"]
end

puts
puts '### identity: '
puts

$! = Exception.new
x = ["hello"]
p ($@ = x) rescue puts "Error: #{$!}"
p $@.object_id
p $!.backtrace
p $!.backtrace.object_id
p $!.backtrace.object_id

