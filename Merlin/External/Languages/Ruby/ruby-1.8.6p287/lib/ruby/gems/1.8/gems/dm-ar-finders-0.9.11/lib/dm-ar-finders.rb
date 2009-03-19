require 'rubygems'
gem 'dm-core', '~>0.9.11'
require 'dm-core'

module DataMapper
  module Model
    def find_or_create(search_attributes, create_attributes = {})
      first(search_attributes) || create(search_attributes.merge(create_attributes))
    end

    private

    def method_missing_with_find_by(method, *args, &block)
      if match = matches_dynamic_finder?(method)
        finder = determine_finder(match)
        attribute_names = extract_attribute_names_from_match(match)

        conditions = {}
        attribute_names.each {|key| conditions[key] = args.shift}

        send(finder, conditions)
      else
        method_missing_without_find_by(method, *args, &block)
      end
    end

    alias_method :method_missing_without_find_by, :method_missing
    alias_method :method_missing, :method_missing_with_find_by

    def matches_dynamic_finder?(method_id)
      /^find_(all_by|by)_([_a-zA-Z]\w*)$/.match(method_id.to_s)
    end

    def determine_finder(match)
      match.captures.first == 'all_by' ? :all : :first
    end

    def extract_attribute_names_from_match(match)
      match.captures.last.split('_and_')
    end
  end
end
