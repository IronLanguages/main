#!D:/Users/Luis/projects/oss/oci/installer2-trunk/ruby/bin/ruby
#!D:/Users/Luis/projects/oss/oci/installer2-trunk/ruby/bin/ruby
#!/usr/bin/env ruby
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
# $Id: proxyserver.rb,v 1.1 2006/01/04 02:03:08 francis Exp $
#

require "drb"
require "dbi"

module DBI
module ProxyServer

USED_DBD_API = "0.2"

module HelperMixin
  def catch_exception
    begin
      yield
    rescue Exception => err
      if err.kind_of? DBI::Error or err.kind_of? DBI::Warning
	err
      else
	DBI::InterfaceError.new("Unexpected Exception was raised in " +
	  "ProxyServer (#{err.inspect})")
      end
    end
  end

  def define_methods(meths)
    meths.each {|d|
      eval %{
	def #{d}(*a) 
	  catch_exception do
	    @handle.#{d}(*a) 
	  end
	end
      }
    } 
  end

end # module HelperMixin


class ProxyServer
  include HelperMixin

  attr_reader :databases

  def initialize
    @databases = []
  end

  def get_used_DBD_version
    USED_DBD_API
  end
  
  def DBD_connect(driver_url, user, auth, attr)
    catch_exception do
      ret = DBI._get_full_driver(driver_url)
      drh = ret[0][1]
      db_args = ret[1]

      dbh = drh.connect(db_args, user, auth, attr)
      DatabaseProxy.new(self, dbh)
    end
  end
end # class ProxyServer

class DatabaseProxy
  include HelperMixin
  
  METHODS = %w(ping commit rollback tables execute do quote [] []= columns)

  attr_reader :statements

  def initialize(parent, dbh)
    @parent = parent
    @handle = dbh
    define_methods METHODS
    @statements = []

    # store it otherwise, it'll get recycled
    @parent.databases << self
  end

  def disconnect
    begin
      catch_exception do
        @handle.disconnect
        @handle = nil
      end
    ensure
      @parent.databases.delete(self) 
    end
  end

  def prepare(stmt)
    catch_exception do
      sth = @handle.prepare(stmt)
      StatementProxy.new(self, sth)
    end
  end
end # class DatabaseProxy

class StatementProxy
  include HelperMixin

  METHODS = %w(bind_param execute fetch column_info bind_params
               cancel fetch_scroll fetch_many fetch_all rows)

  def initialize(parent, sth)
    @parent = parent
    @handle = sth
    define_methods(METHODS)

    # store it otherwise, it'll get recycled
    @parent.statements << self
  end

  def finish
    begin
      catch_exception do
        @handle.finish
        @handle = nil
      end
    ensure
      @parent.statements.delete(self) 
    end
  end


end # class StatementProxy

end # module ProxyServer

end # module DBI


if __FILE__ == $0
  if DBI::DBD::API_VERSION.split(".")[0].to_i != DBI::DBD::DBI::ProxyServer::USED_DBD_API.split(".")[0].to_i 
    raise "Wrong DBD Version"
  end
  
  HOST = ARGV.shift || 'localhost'
  PORT = (ARGV.shift || 9001).to_i
  URL  = "druby://#{HOST}:#{PORT}"

  proxyServer = DBI::ProxyServer::ProxyServer.new
  DRb.start_service(URL, proxyServer)
  puts "DBI::ProxyServer started as #{URL} at #{Time.now.to_s}"
  DRb.thread.join
end

