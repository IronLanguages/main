##############################################################################
# Compare File.join vs. Pathname#+
#
# This benchmark was inspired by a post by Thomas Sawyer.  Note that
# Pathname#+ will never be as fast as File.join, but this provides a
# good base for further optimizations.
#
# Also keep in mind that File.join does no path normalization whatsoever,
# e.g. File.join("foo", "/bar") behaves differently than Pathname.new("foo")
# + Pathname.new("/bar").  This is true of both the pathname and pathname2
# packages.
#
# You can run this via the 'rake benchmark_plus' task.
##############################################################################
require 'benchmark'
require 'pathname2'

MAX = 10000

s1 = "a/b/c"
s2 = "d/e/f"

path1 = Pathname.new(s1)
path2 = Pathname.new(s2)

Benchmark.bm(10) do |bench|
   bench.report("File.join"){
      MAX.times{ File.join(s1, s2) }
   }

   bench.report("Pathname#+"){
      MAX.times{ path1 + path2 }
   }
end
