if RUBY_PLATFORM =~ /darwin/
  ENV["RC_ARCHS"] = `uname -m`.chomp if `uname -sr` =~ /^Darwin/

  # On PowerPC the defaults are fine
  ENV["RC_ARCHS"] = '' if `uname -m` =~ /^Power Macintosh/
end

require 'mkmf'

# All instances of mysql_config on PATH ...
def mysql_config_paths
  ENV['PATH'].split(File::PATH_SEPARATOR).collect do |path|
    [ "#{path}/mysql_config", "#{path}/mysql_config5" ].
      detect { |bin| File.exist?(bin) }
  end
end

# The first mysql_config binary on PATH ...
def default_mysql_config_path
  mysql_config_paths.compact.first
end

def default_prefix
  if mc = default_mysql_config_path
    File.dirname(File.dirname(mc))
  else
    "/usr/local"
  end
end

# Allow overriding path to mysql_config on command line using:
# ruby extconf.rb --with-mysql-config=/path/to/mysql_config
if RUBY_PLATFORM =~ /mswin|mingw/
  dir_config('mysql')
  have_header 'mysql.h' || exit(1)
  have_library 'libmysql' || exit(1)
  have_func('mysql_query', 'mysql.h') || exit(1)
  have_func('mysql_ssl_set', 'mysql.h')
elsif mc = with_config('mysql-config', default_mysql_config_path)
  mc = default_mysql_config_path if mc == true
  cflags = `#{mc} --cflags`.chomp
  exit 1 if $? != 0
  libs = `#{mc} --libs`.chomp
  exit 1 if $? != 0
  $CPPFLAGS += ' ' + cflags
  $libs = libs + " " + $libs
else
  inc, lib = dir_config('mysql', default_prefix)
  libs = ['m', 'z', 'socket', 'nsl']
  lib_dirs =
    [ lib, "/usr/lib", "/usr/local/lib", "/opt/local/lib" ].collect do |path|
      [ path, "#{path}/mysql", "#{path}/mysql5/mysql" ]
    end
  find_library('mysqlclient', 'mysql_query', *lib_dirs.flatten) || exit(1)
  find_header('mysql.h', *lib_dirs.flatten.map { |p| p.gsub('/lib', '/include') })
end

unless RUBY_PLATFORM =~ /mswin|mingw/
  have_header 'mysql.h'
  have_library 'mysqlclient' || exit(1)
  have_func 'mysql_query' || exit(1)
  have_func 'mysql_ssl_set'
end

$CFLAGS << ' -Wall ' unless RUBY_PLATFORM =~ /mswin/

if RUBY_VERSION < '1.8.6'
  $CFLAGS << ' -DRUBY_LESS_THAN_186'
end

create_makefile('do_mysql_ext')
