require 'net/smtp'

module Autotest::EmailNotify
  @@smtp_settings = ['localhost']
  @@from = nil
  @@recipients = []
  @@use_svn = true
  @@report_every_run = false

  @@last_rev = nil

  def self.smtp_settings= o
    @@smtp_settings = o
  end

  def self.from= o
    @@from = o
  end

  def self.recipients= o
    @@recipients = o
  end

  def self.use_svn= o
    @@use_svn = o
  end

  def self.report_every_run= o
    @@report_every_run = o
  end

  def self.notify title, msg
    @@recipients.each do |to|
      body = ["From: autotest <#{@@from}>"]
      body << "To: <#{to}>"
      body << "Subject: #{title}"
      body << "\n"
      body << msg
      Net::SMTP.start(*@@smtp_settings) do |smtp|
        smtp.send_message body.join("\n"), @@from, to
      end
    end
  end

  def self.svn_release
    if @@use_svn
      rev = `svn info`.match(/Revision: (\d+)/)[1]
      return "r#{rev} "
    end
  end

  Autotest.add_hook :ran_command do |at|
    rev = self.svn_release
    if @@report_every_run or rev != @@last_rev
      @@last_rev = rev
      output = at.results.join
      failed = output.scan(/^\s+\d+\) (?:Failure|Error):\n(.*?)\((.*?)\)/)
      if failed.size == 0 then
        notify "#{rev}Tests Passed", output
      else
        f,e = failed.partition { |s| s =~ /Failure/ }
        notify "#{rev}#{failed.size} Tests Failed", output
      end
    end
  end
end
