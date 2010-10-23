# See how Ruby calls the < and > operators on the result returned by <=>

h = Array.new(100) {|i| i}

class Foo
    def initialize(a, b)
        @a = a, b
        @c = a <=> b
        puts self.inspect
    end

    def < (x)
        puts "#{self.inspect} < #{x.inspect}"
        @c < x
    end
    
    def > (x)
        puts "#{self.inspect} > #{x.inspect}"
        @c > x
    end
end

h.sort! { |x,y| Foo.new(x,y) }
puts h.inspect
