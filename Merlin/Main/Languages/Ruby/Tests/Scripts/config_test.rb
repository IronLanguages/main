require 'fileutils'

unless ENV['ROWAN_BIN']
  ENV['ROWAN_BIN'] = File.join(ENV['MERLIN_ROOT'], "bin", "debug")
end

bin = ENV['ROWAN_BIN']
config_old = File.join(bin, "ir.exe.config")
config_temp = File.join(bin, "not_ir.exe.config")
#TODO: The below should work, see codeplex 1558
# eval_str = "begin; puts 'Hello'; rescue Exception => e; exit 1; end; exit 0" 
eval_str = "puts 'Hello'"
cmd = "#{File.join(bin, "ir.exe")} #{ENV['TEST_OPTIONS']} -e \"#{eval_str}\""

begin
  FileUtils.mv(config_old, config_temp)
  status = `#{cmd}`
rescue Exception => e
  puts e.message
ensure
  FileUtils.mv(config_temp, config_old)
end

if status.chomp == "Hello"
  exit 0
else 
  exit 1
end
