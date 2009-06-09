#!/usr/bin/ruby -w -I..

require 'inline'

class Array

  inline do |builder|
    builder.c_raw "
      static VALUE average(int argc, VALUE *argv, VALUE self) {
        double result;
        long  i, len;
        VALUE *arr = RARRAY(self)->ptr;
        len = RARRAY(self)->len;
        
        for(i=0; i<len; i++) {
          result += NUM2DBL(arr[i]);
        }
  
        return rb_float_new(result/(double)len);
      }
    "
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

#   ruby -rprofile ./example2.rb 3 10000
#   ...
#     %   cumulative   self              self     total
#    time   seconds   seconds    calls  ms/call  ms/call  name
#    23.53     0.08      0.08        4    20.00   122.50  Kernel.require
#    14.71     0.13      0.05      123     0.41     0.98  Config.expand
#    11.76     0.17      0.04      135     0.30     0.59  String#gsub!
#    11.76     0.21      0.04        1    40.00    40.00  Hash#each
#     8.82     0.24      0.03        1    30.00   110.00  Hash#each_value
#     5.88     0.26      0.02        3     6.67     6.67  Mod_Array_average.average
#     2.94     0.27      0.01       10     1.00     1.00  Kernel.singleton_method_added
#     2.94     0.28      0.01        1    10.00    30.00  Fixnum#upto
#     2.94     0.29      0.01        2     5.00    10.00  Module#parse_signature
#     2.94     0.30      0.01        2     5.00     5.00  File#stat
#     2.94     0.31      0.01        1    10.00    10.00  Range#each
#     2.94     0.32      0.01        1    10.00    50.00  Module#inline_c_real
#     2.94     0.33      0.01      182     0.05     0.05  Hash#[]=
#     2.94     0.34      0.01        1    10.00    20.00  Module#inline_c_gen
#     0.00     0.34      0.00        1     0.00     0.00  Module#include
# -- CUT ALL FOLLOWING LINES WHERE %time == 0.00

# The first example's cumulative time for Array#average was 6.83
# seconds (wallclock) and the second example's average
# (Mod_Array_average#average) was .26 seconds (a 26x speedup). The
# rest of the time was spent dealing with RubyInline's compile of the
# code. Subsequent runs of the code skip most of RubyInline's work
# because the code has already been compiled and it hasn't changed.
# Looking at the profile, there was really nothing more that we wanted
# to speed up. If there was, then we would have done a few more
# iterations of using RubyInline to extract slower ruby code into
# faster C code and profiling again.
#
# At this point, we were satisfied with the time of the code and
# decided to stop profiling. All that was left to do was to run with
# 'time' again and our larger dataset:

#   & time ruby ./example2.rb 5 100000
#   .....
#  
#   real 0m1.403s
#   user 0m1.120s
#   sys  0m0.070s
# (user+sys = 1.190s)

# We've reduced the running time of the program from 3.40s of CPU time
# to 1.19s of CPU. This is a speed-up of 2.85. Not too shabby...

# You don't want to compare the runtime of the profiled code because
# the cost of running with set_trace_func is so great that it skews
# the results heavily. Looking at the ratio between the normal run
# versus the profiled runs between the pure ruby and the inlined
# versions shows this skew quite clearly:

#        norm   prof
#   ruby 3.40   6.83  (1:2 roughly)
#   C    1.19   0.26  (5:1 roughly)

# This happens simply because our call to Mod_Array_average.average
# causes 30000-1 less method calls than the pure ruby version. This
# translates directly into a multiplier per method call when using
# set_trace_func (which we estimate to be about 200us per call (6.83 /
# 30000) on my machine.
