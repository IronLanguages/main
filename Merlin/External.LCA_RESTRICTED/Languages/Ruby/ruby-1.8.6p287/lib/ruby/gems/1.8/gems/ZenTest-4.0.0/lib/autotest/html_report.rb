# -*- mode -*-

module Autotest::HtmlConsole
  MAX = 30
  STATUS = {}
  PATH = File.expand_path("~/Sites/autotest.html")

  def self.update
    STATUS.delete STATUS.keys.sort.last if STATUS.size > MAX
    File.open(PATH, "w") do |f|
      f.puts "<title>Autotest Status</title>"
      STATUS.sort.reverse.each do |t,s|
        if s > 0 then
          f.puts "<p style=\"color:red\">#{t}: #{s}"
        else
          f.puts "<p style=\"color:green\">#{t}: #{s}"
        end
      end
    end
  end

  Autotest.add_hook :red do |at|
    STATUS[Time.now] = at.files_to_test.size
    update
  end

  Autotest.add_hook :green do |at|
    STATUS[Time.now] = 0
    update
  end
end
