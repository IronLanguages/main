class Array
  def initialize_copy x
    $g = self
    raise
  end
end

a = [1,2,3]
a.freeze

a.clone rescue 0

p $g.frozen?