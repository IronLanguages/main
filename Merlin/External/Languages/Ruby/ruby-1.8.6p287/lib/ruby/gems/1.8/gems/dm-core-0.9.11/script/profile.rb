#!/usr/bin/env ruby

require File.join(File.dirname(__FILE__), '..', 'lib', 'dm-core')

require 'rubygems'

gem 'ruby-prof', '~>0.7.1'
require 'ruby-prof'

gem 'faker', '~>0.3.1'
require 'faker'

OUTPUT = DataMapper.root / 'profile_results.txt'
#OUTPUT = DataMapper.root / 'profile_results.html'

SOCKET_FILE = Pathname.glob(%w[
  /opt/local/var/run/mysql5/mysqld.sock
  /tmp/mysqld.sock
  /tmp/mysql.sock
  /var/mysql/mysql.sock
  /var/run/mysqld/mysqld.sock
]).find { |path| path.socket? }

DataMapper::Logger.new(DataMapper.root / 'log' / 'dm.log', :debug)
DataMapper.setup(:default, "mysql://root@localhost/data_mapper_1?socket=#{SOCKET_FILE}")

class Exhibit
  include DataMapper::Resource

  property :id,         Serial
  property :name,       String
  property :zoo_id,     Integer
  property :notes,      Text, :lazy => true
  property :created_on, Date
#  property :updated_at, DateTime

  auto_migrate!
  create  # create one row for testing
end

touch_attributes = lambda do |exhibits|
  [*exhibits].each do |exhibit|
    exhibit.id
    exhibit.name
    exhibit.created_on
    exhibit.updated_at
  end
end

# RubyProf, making profiling Ruby pretty since 1899!
def profile(&b)
  result  = RubyProf.profile &b
  printer = RubyProf::FlatPrinter.new(result)
  #printer = RubyProf::GraphHtmlPrinter.new(result)
  printer.print(OUTPUT.open('w+'))
end

profile do
#  10_000.times { touch_attributes[Exhibit.get(1)] }
#
#  repository(:default) do
#    10_000.times { touch_attributes[Exhibit.get(1)] }
#  end
#
#  1000.times { touch_attributes[Exhibit.all(:limit => 100)] }
#
#  repository(:default) do
#    1000.times { touch_attributes[Exhibit.all(:limit => 100)] }
#  end
#
#  10.times { touch_attributes[Exhibit.all(:limit => 10_000)] }
#
#  repository(:default) do
#    10.times { touch_attributes[Exhibit.all(:limit => 10_000)] }
#  end

  create_exhibit = {
    :name       => Faker::Company.name,
    :zoo_id     => rand(10).ceil,
    :notes      => Faker::Lorem.paragraphs.join($/),
    :created_on => Date.today
  }

  1000.times { Exhibit.create(create_exhibit) }
end

puts "Done!"
