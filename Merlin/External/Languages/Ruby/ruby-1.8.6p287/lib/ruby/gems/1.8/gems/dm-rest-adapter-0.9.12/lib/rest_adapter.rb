$:.push File.expand_path(File.dirname(__FILE__))

require 'dm-core'
require 'extlib'
require 'pathname'
require 'rexml/document'
require 'rubygems'
require 'dm-serializer'
require 'rest_adapter/version'
require 'rest_adapter/adapter'
require 'rest_adapter/connection'
require 'rest_adapter/formats'
require 'rest_adapter/exceptions'

DataMapper::Adapters::RestAdapter = DataMapperRest::Adapter
