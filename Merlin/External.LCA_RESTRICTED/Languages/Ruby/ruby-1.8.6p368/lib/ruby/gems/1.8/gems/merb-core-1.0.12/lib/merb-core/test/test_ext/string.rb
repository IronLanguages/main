class String
  def contain?(value)
    self.include?(value)
  end
  
  alias_method :contains?, :contain?

  def match?(regex)
    self.match(regex)
  end
  
  alias_method :matches?, :match?
  
end
