#
# DBD::Proxy
#
# Copyright (c) 2001, 2002 Michael Neumann <neumann@s-direktnet.de>
# 
# All rights reserved.
#
# Redistribution and use in source and binary forms, with or without 
# modification, are permitted provided that the following conditions 
# are met:
# 1. Redistributions of source code must retain the above copyright 
#    notice, this list of conditions and the following disclaimer.
# 2. Redistributions in binary form must reproduce the above copyright 
#    notice, this list of conditions and the following disclaimer in the 
#    documentation and/or other materials provided with the distribution.
# 3. The name of the author may not be used to endorse or promote products
#    derived from this software without specific prior written permission.
#
# THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES,
# INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
# AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL
# THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
# EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
# PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
# OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
# WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
# OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
# ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#
# $Id$
#

require "drb"

module DBI
module DBD
module Proxy

VERSION          = "0.2"
USED_DBD_VERSION = "0.2"

module HelperMixin
  def check_exception(obj)
    if obj.kind_of? Exception
      raise obj
    else
      obj
    end
  end

  def define_methods(meths)
    meths.each {|d|
      eval %{
	def #{d}(*a) 
	  check_exception(@handle.#{d}(*a))
	end
      }
    } 
  end
end # module HelperMixin


class Driver < DBI::BaseDriver
  include HelperMixin 

  DEFAULT_PORT = 9001
  DEFAULT_HOSTNAME = "localhost"

  def initialize
    super(USED_DBD_VERSION)
    DRb.start_service
  end

  def connect(dsn, user, auth, attr)
    # split dsn in two parts
    i = dsn.index("dsn=")
    raise InterfaceError, "must specify a DSN" if i.nil?

    hash = Utils.parse_params(dsn[0...i])
    dsn  = dsn[(i+4)..-1] # without dsn=

    host = hash['hostname'] || DEFAULT_HOSTNAME
    port = (hash['port'] || DEFAULT_PORT).to_i 

    if dsn.nil? or dsn.empty?
      raise InterfaceError, "must specify a DSN"
    end

    handle = DRbObject.new(nil, "druby://#{host}:#{port}")

    major, minor = USED_DBD_VERSION.split(".")
    r_major, r_minor = handle.get_used_DBD_version.split(".")

    if major.to_i != r_major.to_i 
      raise InterfaceError, "Proxy uses not compatible DBD version"
    elsif minor.to_i > r_minor.to_i
      # DBI may call methods, not available in former "minor" versions (e.g.DatbaseHandle#columns )
      raise InterfaceError, "Proxy uses not compatible DBD version; may result in problems"
    end

    db_handle = handle.DBD_connect(dsn, user, auth, attr)
    check_exception(db_handle)   

    Database.new(db_handle)
  end

end

class Database < DBI::BaseDatabase
  include HelperMixin 
  METHODS = %w(disconnect ping commit rollback tables execute
               do quote [] []= columns)

  def initialize(db_handle)
    @handle = db_handle
    define_methods METHODS
  end

  def prepare(statement)
    sth = @handle.prepare(statement)
    check_exception(sth)
    Statement.new(sth)
  end

end # class Database


class Statement < DBI::BaseStatement
  include HelperMixin

  METHODS = %w(bind_param execute finish fetch column_info bind_params
               cancel fetch_scroll fetch_many fetch_all rows)

  def initialize(handle)
    @handle = handle
    define_methods METHODS
  end

end # class Statement


end # module Proxy
end # module DBD
end # module DBI
