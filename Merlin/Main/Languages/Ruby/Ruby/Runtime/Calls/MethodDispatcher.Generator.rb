require '../../../Scripts/CodeGenerator.rb'

class Generator
  def add_generic_types(n)
    (n - 1).times { |i| @generated << @template.sub('<>', '<,' + (',' * i) + '>') } 
  end
end

expand_templates(__FILE__)