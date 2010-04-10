def banner(msg, size=80, char="=")
  border = char * size
  puts border, "#{char*2} #{msg}", border
end

def error?(msg = "At least 1 of the dev unit tests failed")
  unless $?.success?
    puts "",msg
    exit 1
  end
end

flags = ["", "/partial", "/noadaptive", "/partial /noadaptive", "/sync0", "/sync1", "/debug", "/partial /debug"]

# TODO: Check OS and set the debug folder name and cmd prefix appropriately.
if RUBY_PLATFORM =~ /(mswin|mingw|bccwin|wince)/i then
  debug_folder = 'debug'
  cmd_prefix = ''
else
  debug_folder = 'mono_debug'
  cmd_prefix = 'mono'
end

ENV['DLR_BIN'] ||= File.join("#{ENV['DLR_ROOT']}",'Bin',debug_folder)

flags.each do |flag|  
  cmd = cmd_prefix + "#{ENV['ROWAN_RUNTIME']} IronRuby.Tests.exe #{flag}"
  banner cmd
  Dir.chdir(ENV['DLR_BIN']) do
    exit 1 unless system cmd
  end
end

puts 'OK'
