#####################################################################
# bench_pathname.rb
#
# Benchmark suite for all methods of the Pathname class, excluding
# the facade methods.
#
# Use the Rake tasks to run this benchmark:
#
# => rake benchmark to run the pure Ruby benchmark.
#####################################################################
require 'benchmark'
require 'pathname2'
require 'rbconfig'

if Config::CONFIG['host_os'].match("mswin")
   path1 = Pathname.new("C:\\Program Files\\Windows NT")
   path2 = Pathname.new("Accessories")
   path3 = Pathname.new("C:\\Program Files\\..\\.\\Windows NT")
else
   path1 = Pathname.new("/usr/local")
   path2 = Pathname.new("bin")
   path3 = Pathname.new("/usr/../local/./bin")
   path4 = Pathname.new("/dev/stdin")
end

MAX = 10000

Benchmark.bm(25) do |bench|
   bench.report("Pathname.new(path)"){
      MAX.times{ Pathname.new("/usr/local/bin") }
   }

   bench.report("Pathname#+(Pathname)"){
      MAX.times{ path1 + path2 }
   }

   bench.report("Pathname#+(String)"){
      MAX.times{ path1 + path2 }
   }

   bench.report("Pathname#children"){
      MAX.times{ path1.children }
   }

   bench.report("Pathname#pstrip"){
      MAX.times{ path1.pstrip }
   }

   bench.report("Pathname#pstrip!"){
      MAX.times{ path1.pstrip! }
   }

   bench.report("Pathname#to_a"){
      MAX.times{ path1.to_a }
   }

   bench.report("Pathname#descend"){
      MAX.times{ path1.descend{} }
   }

   bench.report("Pathname#ascend"){
      MAX.times{ path1.ascend{} }
   }

   bench.report("Pathname#root"){
      MAX.times{ path1.root }
   }

   bench.report("Pathname#root?"){
      MAX.times{ path1.root? }
   }

   bench.report("Pathname#<=>"){
      MAX.times{ path1 <=> path2 }
   }

   bench.report("Pathname#absolute?"){
      MAX.times{ path1.absolute? }
   }

   bench.report("Pathname#relative?"){
      MAX.times{ path1.relative? }
   }

   bench.report("Pathname#clean"){
      MAX.times{ path3.clean }
   }

   bench.report("Pathname#clean!"){
      MAX.times{ path3.clean! }
   }

   # Platform specific tests
   if Config::CONFIG['host_os'].match("mswin")
      bench.report("Pathname.new(file_url)"){
         MAX.times{ Pathname.new("file:///C:/usr/local/bin") }
      }

      bench.report("Pathname#drive_number"){
         MAX.times{ path1.drive_number }
      }

      bench.report("Pathname#unc?"){
         MAX.times{ path1.unc? }
      }

      bench.report("Pathname#undecorate"){
         MAX.times{ path1.undecorate }
      }

      bench.report("Pathname#undecorate!"){
         MAX.times{ path1.undecorate! }
      }

      bench.report("Pathname#short_path"){
         MAX.times{ path1.short_path }
      }

      bench.report("Pathname#long_path"){
         MAX.times{ path1.long_path }
      }
   else      
      bench.report("Pathname#realpath"){
         MAX.times{ path4.realpath }
      }
   end
end
