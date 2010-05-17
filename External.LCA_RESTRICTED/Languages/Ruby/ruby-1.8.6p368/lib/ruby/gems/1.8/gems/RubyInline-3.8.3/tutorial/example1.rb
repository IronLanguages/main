#!/usr/bin/ruby -w

# We started out with some code that averaged a ton of numbers in a
# bunch of arrays. Once we finally lost our patience with the average
# running time of the code, we decided to profile and optimize it
# using RubyInline. 

class Array

  def average
    result = 0
    self.each { |x| result += x }
    result / self.size.to_f
  end

end

max_loop = (ARGV.shift || 5).to_i
max_size = (ARGV.shift || 100_000).to_i
a = (1..max_size).to_a

1.upto(max_loop) do
  avg = a.average
  $stderr.print "."
end
$stderr.puts ""

# The first step to profiling is to get a simple run of the code using
# 'time' and a large dataset. This is because a profile run should
# only be used for figuring out where your bottlenecks are, but the
# runtime of a profile run should be considered invalid. This is
# because set_trace_func, the main mechanism used by profile is a very
# costly function.

#   & time ruby ./example1.rb 5 100000
#   .....
#  
#   real 0m4.580s
#   user 0m3.310s
#   sys  0m0.090s
# (user+sys = 3.400s)

# This gives us a tangible goal, to reduce the runtime of 4.58 seconds
# as much as possible. The next step is to run with a smaller dataset
# (because profiling is VERY SLOW) while including the profile module.

#   & ruby -rprofile ./example1.rb 3 10000
#   ...
#     %   cumulative   self              self     total
#    time   seconds   seconds    calls  ms/call  ms/call  name
#    69.78     4.78      4.78        3  1593.33  2273.33  Array#each
#    29.78     6.82      2.04    30000     0.07     0.07  Fixnum#+
#     0.15     6.83      0.01        3     3.33  2276.67  Array#average
#     0.15     6.84      0.01        1    10.00    10.00  Range#each
#     0.00     6.84      0.00        1     0.00    10.00  Enumerable.to_a
# -- CUT ALL FOLLOWING LINES WHERE %time == 0.00

# This says that Array#each and Fixnum#+ are the only two things we
# should focus on at all. The rest of the time is statistically
# insignificant. So, since average itself is a rather uncomplicated
# method, we decided to convert the entire method rather than just try
# to speed up the math or the loop separately. See example2.rb for the
# continuation of this example.
