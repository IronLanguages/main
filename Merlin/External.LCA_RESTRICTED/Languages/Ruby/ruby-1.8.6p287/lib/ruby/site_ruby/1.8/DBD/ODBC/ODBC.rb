#
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
#

$:.delete(".")
require "odbc"
$: << "."

module DBI
module DBD
module ODBC

VERSION          = "0.2.2"
USED_DBD_VERSION = "0.2"

ODBCErr = ::ODBC::Error

module Converter
  def convert(val)
    if val.is_a? DBI::Date
      ::ODBC::Date.new(val.year, val.month, val.day)
    elsif val.is_a? DBI::Time
      ::ODBC::Time.new(val.hour, val.minute, val.second)
    elsif val.is_a? DBI::Timestamp
      ::ODBC::TimeStamp.new(val.year, val.month, val.day,
          val.hour, val.minute, val.second, val.fraction)
    elsif val.is_a? DBI::Binary
      val.to_s
    else
      val
    end
  end
end

class Driver < DBI::BaseDriver
  def initialize
    super(USED_DBD_VERSION)
  end

  def data_sources
    ::ODBC.datasources.collect {|dsn| "dbi:ODBC:" + dsn.name }
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def connect(dbname, user, auth, attr)
    driver_attrs = dbname.split(';')

    if driver_attrs.size > 1
      # DNS-less connection
      drv = ::ODBC::Driver.new
      drv.name = 'Driver1'
      driver_attrs.each do |param|
        pv = param.split('=')
        next if pv.size < 2
        drv.attrs[pv[0]] = pv[1]
      end
      db = ::ODBC::Database.new
      handle = db.drvconnect(drv)
    else
      # DNS given
      handle = ::ODBC.connect(dbname, user, auth)
    end

    return Database.new(handle, attr)
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end
end

class Database < DBI::BaseDatabase
  include Converter

  def disconnect
    @handle.rollback
    @handle.disconnect 
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def ping
    @handle.connected?
  end

  def columns(table)
    cols = []

    stmt = @handle.columns(table)
    stmt.ignorecase = true

    stmt.each_hash do |row|
      info = Hash.new
      cols << info

      info['name']      = row['COLUMN_NAME']
      info['type_name'] = row['TYPE_NAME']
      info['sql_type']  = row['DATA_TYPE']
      info['nullable']  = row['NULLABLE']  
      info['precision'] = row['COLUMN_SIZE'] - (row['DECIMAL_DIGITS'] || 0)
      info['scale']     = row['DECIMAL_DIGITS']
    end

    stmt.drop
    cols
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def tables
    stmt = @handle.tables
    stmt.ignorecase = true
    tabs = [] 
    stmt.each_hash {|row|
      tabs << row["TABLE_NAME"]
    }
    stmt.drop
    tabs
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def prepare(statement)
    Statement.new(@handle.prepare(statement))
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def do(statement, *bindvars)
    bindvars = bindvars.collect{|v| convert(v)}
    @handle.do(statement, *bindvars) 
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def execute(statement, *bindvars)
    bindvars = bindvars.collect{|v| convert(v)}
    stmt = @handle.run(statement, *bindvars) 
    Statement.new(stmt)
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def []=(attr, value)
    case attr
    when 'AutoCommit'
      @handle.autocommit(value)
    when 'odbc_ignorecase'
      @handle.ignorecase(value)
    else
      if attr =~ /^odbc_/ or attr != /_/
        raise DBI::NotSupportedError, "Option '#{attr}' not supported"
      else # option for some other driver - quitly ignore
        return
      end
    end
    @attr[attr] = value
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def commit
    @handle.commit
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def rollback
    @handle.rollback
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

end # class Database

class Statement < DBI::BaseStatement
  include Converter

  def initialize(handle)
    @handle = handle
    @params = []
    @arr = []
  end

  def bind_param(param, value, attribs)
    raise InterfaceError, "only ? parameters supported" unless param.is_a? Fixnum
    @params[param-1] = value
  end

  def execute
    bindvars = @params.collect{|v| convert(v)}
    @handle.execute(*bindvars)
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def finish
    @handle.drop
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def cancel
    @handle.cancel
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def fetch
    convert_row(@handle.fetch)
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def fetch_scroll(direction, offset)
    direction = case direction
    when DBI::SQL_FETCH_FIRST    then ::ODBC::SQL_FETCH_FIRST
    when DBI::SQL_FETCH_LAST     then ::ODBC::SQL_FETCH_LAST
    when DBI::SQL_FETCH_NEXT     then ::ODBC::SQL_FETCH_NEXT
    when DBI::SQL_FETCH_PRIOR    then ::ODBC::SQL_FETCH_PRIOR
    when DBI::SQL_FETCH_ABSOLUTE then ::ODBC::SQL_FETCH_ABSOLUTE
    when DBI::SQL_FETCH_RELATIVE then ::ODBC::SQL_FETCH_RELATIVE
    end
    
    convert_row(@handle.fetch_scroll(direction, offset))
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def column_info
    info = []
    @handle.columns(true).each do |col|
      info << {
        'name'       => col.name, 
        'table'      => col.table,
        'nullable'   => col.nullable,
        'searchable' => col.searchable,
        'precision'  => col.precision,
        'scale'      => col.scale,
        'sql_type'   => col.type,
        'type_name'  => DBI::SQL_TYPE_NAMES[col.type],
        'length'     => col.length,
        'unsigned'   => col.unsigned
      }
    end
    info
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  def rows
    @handle.nrows
  rescue ODBCErr => err
    raise DBI::DatabaseError.new(err.message)
  end

  private # -----------------------------------

  # convert the ODBC datatypes to DBI datatypes
  def convert_row(row)
    return nil if row.nil?
    row.collect do |col|
      if col.is_a? ::ODBC::Date
        DBI::Date.new(col.year, col.month, col.day)
      elsif col.is_a? ::ODBC::Time
        DBI::Time.new(col.hour, col.minute, col.second)
      elsif col.is_a? ::ODBC::TimeStamp
        DBI::Timestamp.new(col.year, col.month, col.day,
          col.hour, col.minute, col.second, col.fraction)
      else
        col
      end
    end
  end 
end

end # module ODBC
end # module DBD
end # module DBI




