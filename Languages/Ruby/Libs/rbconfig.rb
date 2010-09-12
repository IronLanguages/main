# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

module RbConfig
  CONFIG = {}
  version_components = RUBY_VERSION.split('.')
  abort("Could not parse RUBY_VERSION") unless version_components.size == 3
  CONFIG["MAJOR"], CONFIG["MINOR"], CONFIG["TEENY"] = version_components
  CONFIG["PATCHLEVEL"] = "0"
  CONFIG["EXEEXT"] = ".exe"
  # This value is used by libraries to spawn new processes to run Ruby scripts. Hence it needs to match the ir.exe name
  CONFIG["ruby_install_name"] = "ir"
  CONFIG["RUBY_INSTALL_NAME"] = "ir"
  CONFIG["RUBY_SO_NAME"] = "msvcrt-ruby191"
  CONFIG["BUILD_FILE_SEPARATOR"] = "\\"
  CONFIG["ruby_version"] = RUBY_VERSION.dup
  CONFIG["PATH_SEPARATOR"] = ";"
  
  # Set up paths
  if ENV["DLR_ROOT"] then
    # This is a dev environment. See http://wiki.github.com/ironruby/ironruby
    TOPDIR = File.expand_path(ENV["DLR_BIN"] || System::IO::Path.get_directory_name(
      System::Reflection::Assembly.get_executing_assembly.code_base
    ).gsub('file:\\', ''))
    CONFIG["bindir"] = TOPDIR
    CONFIG["libdir"] = File.expand_path("External.LCA_RESTRICTED/Languages/Ruby/redist-libs", ENV["DLR_ROOT"])
  else
    TOPDIR = File.expand_path("../..", File.dirname(__FILE__))
    CONFIG["bindir"] = TOPDIR + "/bin"
    CONFIG["libdir"] = TOPDIR + "/lib"
  end
  
  DESTDIR = TOPDIR && TOPDIR[/\A[a-z]:/i] || '' unless defined? DESTDIR
  CONFIG["DESTDIR"] = DESTDIR
  CONFIG["prefix"] = (TOPDIR || DESTDIR + "")
  CONFIG["exec_prefix"] = "$(prefix)"
  CONFIG["sbindir"] = "$(exec_prefix)/sbin"
  CONFIG["libexecdir"] = "$(exec_prefix)/libexec"
  CONFIG["datadir"] = "$(prefix)/share"
  CONFIG["sysconfdir"] = "$(prefix)/etc"
  CONFIG["sharedstatedir"] = "$(DESTDIR)/etc"
  CONFIG["localstatedir"] = "$(DESTDIR)/var"
  CONFIG["rubylibdir"] = "$(libdir)/ruby/$(ruby_version)"
  CONFIG["sitedir"] = "$(libdir)/ruby/site_ruby"

  cpu_and_os = RUBY_PLATFORM.split('-')
  abort("Could not parse RUBY_PLATFORM") if cpu_and_os.size != 2
  CONFIG["host_cpu"] = (cpu_and_os[0] == "i386") ? "i686" : cpu_and_os[0]
  CONFIG["host_os"] = cpu_and_os[1]
  clr_version = "#{System::Environment.Version.Major}.#{System::Environment.Version.Minor}"
  CONFIG["target"] = "dotnet#{clr_version}"
  CONFIG["arch"] = "universal-#{CONFIG["target"]}"
  CONFIG["build"] = CONFIG["arch"] # Not strictly true. For example, while running a .NET 2.0 version of IronRuby on .NET 4
  CONFIG["target_alias"] = CONFIG["target"]
  CONFIG["target_cpu"] = cpu_and_os[0]
  CONFIG["target_vendor"] = "pc"
  CONFIG["target_os"] = CONFIG["host_os"]
  CONFIG["CP"] = "copy > nul"
  CONFIG["SHELL"] = "$(COMSPEC)"
  CONFIG["rubylibdir"] = "$(rubylibprefix)/$(ruby_version)"
  CONFIG["DLEXT"] = "so"
  CONFIG["DLEXT2"] = "dll"
  CONFIG["archdir"] = "$(rubylibdir)/$(arch)"
  CONFIG["sitelibdir"] = "$(sitedir)/$(ruby_version)"
  CONFIG["sitearchdir"] = "$(sitelibdir)/$(sitearch)"
  CONFIG["vendorlibdir"] = "$(vendordir)/$(ruby_version)"
  CONFIG["vendorarchdir"] = "$(vendorlibdir)/$(sitearch)"
  CONFIG["topdir"] = File.dirname(__FILE__)
  def RbConfig::expand(val, config = CONFIG)
    newval = val.gsub(/\$\$|\$\(([^()]+)\)|\$\{([^{}]+)\}/) {
      var = $&
      if !(v = $1 || $2)
	'$'
      elsif key = config[v = v[/\A[^:]+(?=(?::(.*?)=(.*))?\z)/]]
	pat, sub = $1, $2
	config[v] = false
	config[v] = RbConfig::expand(key, config)
	key = key.gsub(/#{Regexp.quote(pat)}(?=\s|\z)/n) {sub} if pat
	key
      else
	var
      end
    }
    val.replace(newval) unless newval == val
    val
  end
  CONFIG.each_value do |val|
    RbConfig::expand(val)
  end

  # returns the absolute pathname of the ruby command.
  def RbConfig.ruby
    File.join(
      RbConfig::CONFIG["bindir"],
      RbConfig::CONFIG["ruby_install_name"] + RbConfig::CONFIG["EXEEXT"]
    )
  end
end
Config = RbConfig # compatibility for ruby-1.8.4 and older.
