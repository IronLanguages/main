module M1; end 
module M2; end
module M3; end
module N1; end
module N2; end

class Module
  p (private_instance_methods(false) & ["extended", "included", "append_features", "extend_object"]).sort
  
  alias old_e extended
  alias old_eo extend_object
  alias old_i included
  alias old_af append_features
  
  def extended *a
    puts "#{self}.extended: #{a.inspect} -> #{old_e(*a).inspect}"
  end  
  
  def extend_object *a
    puts "#{self}.extend_object: #{a.inspect} -> #{$disable ? '<disabled>': old_eo(*a).inspect}"
  end
  
  def included *a
    puts "#{self}.included: #{a.inspect} -> #{old_i(*a).inspect}"
  end 

  def append_features *a
    puts "#{self}.append_features: #{a.inspect} -> #{old_af(*a).inspect}"
  end   
end  

puts '---'

class C
  include N1, N2  
end

x = C.new 

class << x
  def to_s
    "obj_x"
  end
end 

puts '---'

$disable = false
p x.extend(M1, M2)

puts '---'

$disable = true
p x.extend(M3)

puts '---'

class C
  puts "C.ancestors -> #{ancestors.inspect}"
end

class << x
  puts "S(x).ancestors -> #{ancestors.inspect}"
end

