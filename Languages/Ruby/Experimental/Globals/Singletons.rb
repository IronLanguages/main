class << Object.new
  p included_modules
  p instance_methods(false)
end

puts '---'

p ENV.class
class << ENV
  p included_modules
  p instance_methods(false) - Enumerable.instance_methods(false)
end

puts '---'

p ENV["TEMP"]

ENV["TEMP"] = 'goo'
p `echo %TEMP%`

puts '---'

p ARGF.class
class << ARGF
  p included_modules
  p instance_methods(false) - Enumerable.instance_methods(false)
end

puts '---'

p ARGV.class
class << ARGV
  p included_modules
  p instance_methods(false) - Array.instance_methods(false)
end
