class String
  alias x dup
  alias y dup
  def dup
    puts 'dup'
    x
  end
  
  def freeze
    puts 'freeze'
    y
  end
end

x = { "sadA", 1}
x["foo"] = 2

puts x.keys[0].frozen?