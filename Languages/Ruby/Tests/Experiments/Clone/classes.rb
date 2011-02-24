class S < String
 
end

class A < Array
end

class H < Hash
end

class R < Range
end

class RE < Regexp
end

class M < Module
end

class St < Struct
end

class P < Proc
end


[S,A,H,R,RE,M,St,P,
String, Array, Hash, Range, Regexp, Module, Proc, Class, Struct, Object].each { |c|
  c.dup
}

p S.dup.new('foo')
p A.dup.new([1,2])
p H.dup.new('default')[1]
p R.dup.new(1,2)
p RE.dup.new("foo")
p M.dup.new.name
p St.dup.new(:a,:b).dup[1,2].dup.members
p P.dup.new { 'foo' }[]

puts '---'

p String.dup.new('foo')
p Array.dup.new([1,2])
p Hash.dup.new('default')[1]
p Range.dup.new(1,2)
p Regexp.dup.new("foo")
p Module.dup.new.name
p Struct.dup.new(:a,:b).dup[1,2].dup.members
p Proc.dup.new { 'foo' }[]


class Class
  def initialize_copy *a
    puts 'init_copy'
  end
end

String.dup.new('foo') rescue p $!