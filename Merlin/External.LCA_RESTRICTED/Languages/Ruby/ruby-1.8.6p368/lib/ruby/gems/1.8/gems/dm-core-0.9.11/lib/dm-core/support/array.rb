class Array

  ##
  # atm it assumes self is an array of [key,value]-arrays
  # this is just a better way to make hashes than Hash[*array.flatten]
  # since you cannot flatten only one level in ruby 1.8.6
  #
  def to_hash
    h = {}
    self.each{ |k,v| h[k] = v }
    h
  end
end # class Symbol
