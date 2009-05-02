require 'rubygems'
version = "> 0"
if ARGV.size > 0 && ARGV[0][0]==95 && ARGV[0][-1]==95
  if Gem::Version.correct?(ARGV[0][1..-2])
    version = ARGV[0][1..-2] 
    ARGV.shift
  end
end
gem 'fxruby'; gem 'fxri', version
load 'fxri'  
