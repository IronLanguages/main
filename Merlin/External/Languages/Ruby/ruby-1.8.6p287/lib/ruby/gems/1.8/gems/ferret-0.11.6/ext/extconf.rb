# extconf.rb for Ferret extensions
if (/mswin/ =~ RUBY_PLATFORM) and ENV['make'].nil?
  require 'mkmf'
  $LIBS += " msvcprt.lib"
  create_makefile("ferret_ext")
elsif ENV['FERRET_DEV']
  require 'mkmf'
  $CFLAGS += " -fno-stack-protector -fno-common -D_FILE_OFFSET_BITS=64"
  create_makefile("ferret_ext")
else
  require 'mkmf'
  $CFLAGS += " -D_FILE_OFFSET_BITS=64"
  create_makefile("ferret_ext")
end
