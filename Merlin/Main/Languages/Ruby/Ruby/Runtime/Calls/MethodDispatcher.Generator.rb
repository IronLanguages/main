require '../../../Scripts/CodeGenerator.rb'

class Generator
  attr_reader :n

  def add_generic_types(n)
    (n - 1).times { |i| @generated << @template.sub('<>', '<,' + (',' * i) + '>') } 
  end
  
  def GenericDecl
    "<" + Array.new(@n) { |i| "T#{i}" }.join(", ") + ">"
  end
  
  def GenericDeclWithReturn(*args)
    "<" + Array.new(@n) { |i| "T#{i}" }.join(", ") + ", TReturn>"
  end

  def GenericParams
    Array.new(@n) { |i| ", T#{i}" }.join(" ")
  end
  
  def GenericParamsComma
    Array.new(@n) { |i| "T#{i}, " }.join(" ")
  end
  
  def GenericParamsBrackets
    "<" + (Array.new(@n) { |i| "T#{i}" }.join(", ")) + ">"
  end

  def Objects *args
    ", object" * @n
  end

  def Parameters
    "," + Array.new(@n) { |i| "T#{i} arg#{i}" }.join(", ") 
  end

  def Arguments
    "," + Array.new(@n) { |i| "arg#{i}" }.join(", ") 
  end

  def ParameterExpressions
    Array.new(@n) { |i| %{Expression.Parameter(typeof(T#{i}), "$arg#{i}")} }.join(", ")
  end
end

expand_templates(__FILE__)