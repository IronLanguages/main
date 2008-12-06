def foo
  begin
    $! = IOError.new
    begin
      raise StandardError.new("XXX")
    rescue 
      puts "R1: #{$!}"                   # rescue clears exception
    ensure
      puts "E1: #{$!}"
      return if $return
    end
  rescue
    puts "R2: #{$!}"
  ensure 
    puts "E2: #{$!}"
  end
end

def bar
  $! = IOError.new
  begin
    raise StandardError.new("XXX")
  ensure
    puts "E: #{$!}"
    return if $return
  end
rescue
  puts "R2: #{$!}"
ensure 
  puts "E2: #{$!}"
end

$return = false

puts '---'
foo
puts '---'
bar

$return = true

puts '---'
foo
puts '---'
bar
