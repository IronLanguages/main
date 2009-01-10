def dump_class x
  puts "-- #{x.name}"
  p x.instance_methods(false).sort
  p x.singleton_methods(false).sort
  p x.ancestors
  puts
end

dump_class Struct

StructC = Struct.new(:foo)
dump_class StructC

C = Class.new(StructC)
dump_class C

puts "-- C.new"
p C.new
puts

# remove new() on StructC's singleton:
class StructC
  class << self
    remove_method :new
  end
end

# create a structure that derives from an existing one:
StructD = StructC.new(:xxx, :yyy)

dump_class StructD

D = Class.new(StructD)
dump_class D

puts "-- D"
p D.new
puts
