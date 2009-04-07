module Merb
  module Generators
    class MerbVeryFlatGenerator < AppGenerator
      def self.source_root
        File.join(super, 'application', 'merb_very_flat')
      end

      def self.common_templates_dir
        File.expand_path(File.join(File.dirname(__FILE__), '..',
                                   'templates', 'application', 'common'))
      end

      def destination_root
        File.join(@destination_root, base_name)
      end

      def common_templates_dir
        File.expand_path(File.join(File.dirname(__FILE__), '..',
                                   'templates', 'application', 'common'))
      end

      option :testing_framework, :default => :rspec,
      :desc => 'Testing framework to use (one of: rspec, test_unit).'
      option :orm, :default => :none,
      :desc => 'Object-Relation Mapper to use (one of: none, activerecord, datamapper, sequel).'
      option :template_engine, :default => :erb,
      :desc => 'Template engine to prefer for this application (one of: erb, haml).'

      desc <<-DESC
      Generates a new very flat Merb application: the whole application
      in one file, similar to Sinatra or Camping.
    DESC

      first_argument :name, :required => true, :desc => "Application name"

      template :application do |template|
        template.source = 'application.rbt'
        template.destination = "#{base_name}.rb"
      end

      file :gitignore do |file|
        file.source = File.join(common_templates_dir, 'dotgitignore')
        file.destination = ".gitignore"
      end

      directory :test_dir do |directory|
        dir = testing_framework == :rspec ? "spec" : "test"
        directory.source      = dir
        directory.destination = dir
      end

      directory :thor_file do |directory|
        directory.source = File.join(common_templates_dir, "merb_thor")
        directory.destination = File.join("tasks", "merb.thor")
      end

      template :rakefile do |template|
        template.source = File.join(common_templates_dir, "Rakefile")
        template.destination = "Rakefile"
      end

      def class_name
        self.name.gsub("-", "_").camel_case
      end
    end

    add :very_flat, MerbVeryFlatGenerator
  end
end
