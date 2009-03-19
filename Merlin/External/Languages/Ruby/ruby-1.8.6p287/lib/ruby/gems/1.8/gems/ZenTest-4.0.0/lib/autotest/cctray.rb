# -*- ruby -*-

require 'fileutils'

module Autotest::CCTray
  MAX = 30
  STATUS = {
    :all_good => "Success",
    :green    => "Success",
    :red      => "Failure",
  }
  DIR = File.expand_path("~/Sites/dashboard")

  def self.project_name= name
    @@project_name = name
  end

  def self.update_status status
    dir = File.join(DIR, @@project_name)
    serial = Time.now.to_i
    file = "status.#{serial}.xml"
    FileUtils.mkdir_p dir
    Dir.chdir dir do
      File.open(file, 'w') do |f|
        f.puts %(<Project name="#{@@project_name}" activity="Sleeping" lastBuildStatus="#{STATUS[status]}" lastBuildLabel="build.#{serial}" lastBuildTime="#{Time.now.xmlschema}" webUrl="http://localhost/~ryan/dashboard/#{@@project_name}/"/>)
      end
      files = Dir["*.xml"].sort_by { |f| File.mtime f }.reverse
      (files - files.first(MAX)).each do |f|
        File.unlink f
      end
    end

    Dir.chdir DIR do
      new_file = "cctray.xml.#{$$}"
      old_file = "cctray.xml"
      File.open(from_file, "w") do |out|
        out.puts "<Projects>"
        Dir["*"].each do |d|
          next unless File.directory? d
          Dir.chdir d do
            latest = Dir["*.xml"].sort_by { |f| File.mtime f }.last
            out.puts File.read(latest)
          end
        end
        out.puts "</Projects>"
      end
      File.rename new_file, old_file
    end
  end

  [:run, :red, :green, :all_good].each do |status|
    Autotest.add_hook status do |at|
      STATUS[Time.now] = at.files_to_test.size
      update_status status
    end
  end
end
