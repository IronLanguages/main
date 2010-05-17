class C
end

module M
  class << 'x'
    $Sx = self
    class << C
      $SC = self
      @@a = 1
      class_variable_set :@@ea, 1
    end
    @@b = 2
    class_variable_set :@@eb, 2
  end
  @@c = 3
  class_variable_set :@@em, 3
end

p M.class_variables.sort
p $SC.class_variables.sort
p $Sx.class_variables.sort

class C
  @@x = 1
end

class D < C
  remove_class_variable :@@x rescue puts 'Error'
  @@y = 'foo bar baz'
  p remove_class_variable(:@@y)
end
