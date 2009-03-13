class String
  
  def realign_indentation
    basis = self.index(/\S/) # find the first non-whitespace character
    return self.to_a.map { |s| s[basis..-1] }.join
  end
  
end