
begin require 'rubygems' rescue LoadError end
require 'inline'

class FastMath
  def factorial(n)
    f = 1
    n.downto(2) { |x| f *= x }
    return f
  end
  inline do |builder|
    builder.c "
    long factorial_c(int max) {
      int i=max, result=1;
      while (i >= 2) { result *= i--; }
      return result;
    }"
  end
end

math = FastMath.new

if ARGV.empty? then
  30000.times do math.factorial(20); end
else
  30000.times do math.factorial_c(20); end
end
