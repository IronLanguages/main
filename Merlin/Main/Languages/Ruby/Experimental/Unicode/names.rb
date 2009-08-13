#encoding: UTF-8

class Cß
  def initialize
    @ß = 1
    @@ß = 2
  end
  
  def ß
  end
  
  Xß = 5
end


$Xß = 3
ß = 4


p Cß.name
p Cß.name.encoding
p Cß.new

p Cß.class_variables.each { |x| puts x,x.encoding }
p Cß.instance_methods(false).each { |x| puts x,x.encoding }
p Cß.constants.each { |x| puts x,x.encoding }
p Cß.new.instance_variables.each { |x| puts x,x.encoding }
p local_variables.each { |x| puts x,x.encoding }
global_variables.each { |x| puts x,x.encoding if x[0] == 'X' }
