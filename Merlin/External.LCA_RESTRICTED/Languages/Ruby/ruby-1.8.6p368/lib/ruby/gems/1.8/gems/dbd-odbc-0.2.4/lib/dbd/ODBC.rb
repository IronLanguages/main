#--
# DBD::ODBC
#
# Copyright (c) 2001-2004 Michael Neumann <mneumann@ntecs.de>
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
#++

$:.delete(".") # FIXME oh for the love of pete..
require "odbc"
$: << "."

begin
    require 'rubygems'
    gem 'dbi'
rescue LoadError => e
end

require 'dbi'

module DBI
    module DBD
        #
        # DBD::ODBC - ODBC Database Driver
        #
        # Requires DBI and the 'ruby-odbc' package to work, and unixodbc (iodbc
        # WILL NOT WORK).
        #
        # Only things that extend DBI's results are documented.
        #
        module ODBC

            VERSION          = "0.2.4"
            DESCRIPTION      = "ODBC DBI DBD"

            def self.driver_name
                "odbc"
            end

            DBI::TypeUtil.register_conversion(driver_name) do |obj|
                newobj = if obj.nil?
                             nil
                         else
                             case obj
                             when ::Date
                                 ::ODBC::Date.new(obj)
                             when ::Time
                                 ::ODBC::Time.new(obj)
                             when ::DateTime
                                 ::ODBC::Timestamp.new(obj)
                             else
                                 obj.to_s
                             end
                         end
                [newobj, false]
            end

            ODBCErr = ::ODBC::Error
        end # module ODBC
    end # module DBD
end # module DBI

require 'dbd/odbc/driver'
require 'dbd/odbc/database'
require 'dbd/odbc/statement'
