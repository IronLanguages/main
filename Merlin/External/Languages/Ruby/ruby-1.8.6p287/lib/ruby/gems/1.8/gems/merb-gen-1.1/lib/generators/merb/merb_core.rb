module Merb
  module Generators
    class MerbCoreGenerator < AppGenerator
      #
      # ==== Paths
      #
      
      def self.source_root
        File.join(super, 'application', 'merb_core')
      end

      def self.common_templates_dir
        File.expand_path(File.join(File.dirname(__FILE__), '..',
                                   'templates', 'application', 'common'))
      end

      def destination_root
        File.join(@destination_root, base_name)
      end

      def common_templates_dir
        self.class.common_templates_dir
      end

      #
      # ==== Generator options
      #

      option :testing_framework, :default => :rspec,
      :desc => 'Testing framework to use (one of: rspec, test_unit).'
      option :orm, :default => :none,
      :desc => 'Object-Relation Mapper to use (one of: none, activerecord, datamapper, sequel).'
      option :template_engine, :default => :erb,
      :desc => 'Template engine to prefer for this application (one of: erb, haml).'

      desc <<-DESC
      Generates a new Merb application with Ruby on Rails like structure.
      You can specify the ORM and testing framework.
    DESC

      first_argument :name, :required => true, :desc => "Application name"

      #
      # ==== Common directories & files
      #

      empty_directory :gems, 'gems'
      template :rakefile do |template|
        template.source = File.join(common_templates_dir, "Rakefile")
        template.destination = "Rakefile"
      end

      file :gitignore do |file|
        file.source = File.join(common_templates_dir, 'dotgitignore')
        file.destination = ".gitignore"
      end

      file :htaccess do |file|
        file.source = File.join(common_templates_dir, 'dothtaccess')
        file.destination = 'public/.htaccess'
      end

      directory :test_dir do |directory|
        dir    = testing_framework == :rspec ? "spec" : "test"

        directory.source      = File.join(source_root, dir)
        directory.destination = dir
      end
      
      directory :thor_file do |directory|
        directory.source = File.join(common_templates_dir, "merb_thor")
        directory.destination = File.join("tasks", "merb.thor")
      end

      #
      # ==== Layout specific things
      #

      # empty array means all files are considered to be just
      # files, not templates
      glob! "app"
      glob! "autotest"
      glob! "config"
      glob! "doc",      []
      glob! "public"      

      invoke :layout do |generator|
        generator.new(destination_root, options, 'application')
      end
    end

    add :core, MerbCoreGenerator

  end
end
