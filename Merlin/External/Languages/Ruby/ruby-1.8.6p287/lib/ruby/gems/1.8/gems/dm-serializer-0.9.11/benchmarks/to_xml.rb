require "rubygems"
require 'pathname'

gem 'dm-core', '~>0.9.11'
require 'dm-core'

spec_dir_path = Pathname(__FILE__).dirname.expand_path
$LOAD_PATH.unshift(spec_dir_path.parent + 'lib/')
require 'dm-serializer'

def load_driver(name, default_uri)
  begin
    DataMapper.setup(name, ENV["#{name.to_s.upcase}_SPEC_URI"] || default_uri)
    DataMapper::Repository.adapters[:default] =  DataMapper::Repository.adapters[name]
    DataMapper::Repository.adapters[:alternate] = DataMapper::Repository.adapters[name]
    true
  rescue LoadError => e
    warn "Could not load do_#{name}: #{e}"
    false
  end
end

HAS_SQLITE3  = load_driver(:sqlite3,  'sqlite3::memory:')

class Cow
  include DataMapper::Resource

  property :id,        Integer, :key => true
  property :composite, Integer, :key => true
  property :name,      String
  property :breed,     String

  has n, :baby_cows, :class_name => 'Cow'
  belongs_to :mother_cow, :class_name => 'Cow'
end

require "benchwarmer"

TIMES = 2000
DataMapper.auto_migrate!
cow = Cow.create(
  :id        => 89,
  :composite => 34,
  :name      => 'Berta',
  :breed     => 'Guernsey'
)
all_cows = Cow.all

puts "REXML"
Benchmark.warmer(TIMES) do
  group("Serialization:") do
    report "Single Resource" do
      cow.to_xml
    end
    report "Collection" do
      all_cows.to_xml
    end
  end
end

require 'nokogiri'
load 'dm-serializer/xml_serializers.rb'

puts "Nokogiri"
Benchmark.warmer(TIMES) do
  group("Serialization:") do
    report "Single Resource" do
      cow.to_xml
    end
    report "Collection" do
      all_cows.to_xml
    end
  end
end

require 'libxml'
load 'dm-serializer/xml_serializers.rb'

puts "LibXML"
Benchmark.warmer(TIMES) do
  group("Serialization:") do
    report "Single Resource" do
      cow.to_xml
    end
    report "Collection" do
      all_cows.to_xml
    end
  end
end
