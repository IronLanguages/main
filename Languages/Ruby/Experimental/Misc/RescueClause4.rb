# $! is stacked

class A < Exception; end
class B < Exception; end
class C < Exception; end

$! = IOError.new
begin
  raise A
rescue A
  begin 
    raise B
  rescue B
    begin 
      raise C
    rescue C
      puts $!              # C
    end 
    puts $!                # B
  end  
  puts $!                  # A
end
puts $!                    # IOError




