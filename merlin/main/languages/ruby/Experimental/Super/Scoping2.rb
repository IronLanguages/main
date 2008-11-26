class C
  def foo
    puts 'C.foo'
  end
end

module M
  def foo
    puts 'D.foo'
    
    eval <<-END
      class E
        def foo
          puts 'E.foo'
        end
        super
      end
    END
  end
end

class D < C
  include M
end

d = D.new
d.foo
