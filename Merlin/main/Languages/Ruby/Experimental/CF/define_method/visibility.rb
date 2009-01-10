class C
  
  # self is not captured in closure:
  define_method :foo do
    p self
    p self.class
  end

  private
  define_method :priv do
    puts 'private'
  end
end

C.new.foo
C.new.priv rescue puts $!  # private method called