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

  # Version
  version_components = RUBY_VERSION.split('.')
  abort("Could not parse RUBY_VERSION") unless version_components.size == 3
  CONFIG["MAJOR"], CONFIG["MINOR"], CONFIG["TEENY"] = version_components
  CONFIG["PATCHLEVEL"] = "0"

  # This value is used by libraries to spawn new processes to run Ruby scripts. Hence it needs to match the iruby.exe name
  CONFIG["ruby_install_name"] = "iruby"
  CONFIG["ruby_install_name"] += "64" if System::Environment.is64_bit_process
  CONFIG["RUBY_INSTALL_NAME"] = CONFIG["ruby_install_name"]
  CONFIG["RUBY_SO_NAME"] = ".NET-ruby191"
  CONFIG["PATH_SEPARATOR"] = ";"

  # Set up paths
  if ENV["DLR_ROOT"] then
    # This is a dev environment. See http://wiki.github.com/ironruby/ironruby
    TOPDIR = File.expand_path(ENV["DLR_BIN"] || System::IO::Path.get_directory_name(System::Reflection::Assembly.get_entry_assembly.location))
    CONFIG["bindir"] = TOPDIR
    CONFIG["libdir"] = File.expand_path("Languages/Ruby/StdLib", ENV["DLR_ROOT"])
  else
    TOPDIR = File.expand_path("../..", File.dirname(__FILE__))
    CONFIG["bindir"] = "$(exec_prefix)/bin"
    CONFIG["libdir"] = "$(exec_prefix)/lib"
  end

  DESTDIR = TOPDIR && TOPDIR[/\A[a-z]:/i] || '' unless defined? DESTDIR
  CONFIG["DESTDIR"] = DESTDIR

  CONFIG["prefix"] = (TOPDIR || DESTDIR + "")
  CONFIG["exec_prefix"] = "$(prefix)"

  # cpu, os
  cpu_and_os = RUBY_PLATFORM.split('-')
  abort("Could not parse RUBY_PLATFORM") if cpu_and_os.size != 2
  CONFIG["host_cpu"] = CONFIG["target_cpu"] = (cpu_and_os[0] == "i386") ? "i686" : cpu_and_os[0]
  CONFIG["host_os"] = CONFIG["target_os"] =  cpu_and_os[1]

  # architecture
  clr_version = "#{System::Environment.Version.Major}.#{System::Environment.Version.Minor}"
  CONFIG["arch"] = ".NET #{clr_version}" # MEMO: Not strictly true. For example, while running a .NET 2.0 version of IronRuby on .NET 4
  CONFIG["sitearch"] = "$(arch)"

  # std lib
  CONFIG["ruby_version"] = "1.9.1"  # std library version
  CONFIG["RUBY_BASE_NAME"] = "ruby" # directory name

  # share lib
  CONFIG["datarootdir"] = "$(prefix)/share"
  CONFIG["datadir"] = "$(datarootdir)"
  CONFIG["rubylibdir"] = "$(rubylibprefix)/$(ruby_version)"
  CONFIG["rubylibprefix"] = "$(libdir)/$(RUBY_BASE_NAME)"

  # ri
  CONFIG["RI_BASE_NAME"] = "ri"
  CONFIG["ridir"] = "$(datarootdir)/$(RI_BASE_NAME)"

  # site and vendor dirs
  CONFIG["sitedir"] = "$(rubylibprefix)/site_ruby"
  CONFIG["sitelibdir"] = "$(sitedir)/$(ruby_version)"
  CONFIG["vendordir"] = "$(rubylibprefix)/vendor_ruby"
  CONFIG["vendorlibdir"] = "$(vendordir)/$(ruby_version)"

  # shell
  CONFIG["SHELL"] = ENV["COMSPEC"]
  CONFIG["DLEXT"] = "so"
  CONFIG["DLEXT2"] = "dll"
  CONFIG["EXEEXT"] = ".exe"

  # Other settings
  CONFIG["archdir"] = "$(rubylibdir)/$(arch)"
  CONFIG["sitearchdir"] = "$(sitelibdir)/$(sitearch)"
  CONFIG["vendorarchdir"] = "$(vendorlibdir)/$(sitearch)"
  CONFIG["topdir"] = File.dirname(__FILE__)

  MAKEFILE_CONFIG = {}
  CONFIG.each{|k,v| MAKEFILE_CONFIG[k] = v.dup}

  def RbConfig::expand(val, config = CONFIG)
    newval = val.gsub(/\$\$|\$\(([^()]+)\)|\$\{([^{}]+)\}/) do
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
    end
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

CROSS_COMPILING = nil unless defined? CROSS_COMPILING
