require 'benchmark'

x10000 = "x" * 10000

Benchmark.bm do |x| 
  x.report { 10.times { "x" * 10_000_000 } } 
  x.report { 20.times { x10000 * 10000 } } 
end