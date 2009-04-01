begin
  require 'mailfactory'
  require 'net/smtp'
rescue LoadError
  Merb.logger.warn "You need to install the mailfactory gem to use Merb::Mailer"
end

class MailFactory
  attr_reader :html, :text
end

module Merb

  # You'll need a simple config like this in init.rb if you want
  # to actually send mail:
  #
  #   Merb::Mailer.config = {
  #     :host   => 'smtp.yourserver.com',
  #     :port   => '25',
  #     :user   => 'user',
  #     :pass   => 'pass',
  #     :auth   => :plain # :plain, :login, :cram_md5, the default is no auth
  #     :domain => "localhost.localdomain" # the HELO domain provided by the client to the server
  #   }
  #
  #   or
  #
  #   Merb::Mailer.config = {:sendmail_path => '/somewhere/odd'}
  #   Merb::Mailer.delivery_method = :sendmail
  #
  # You could send mail manually like this (but it's better to use
  # a MailController instead).
  #
  #   m = Merb::Mailer.new :to => 'foo@bar.com',
  #                        :from => 'bar@foo.com',
  #                        :subject => 'Welcome to whatever!',
  #                        :html => partial(:sometemplate)
  #   m.deliver!
  #
  # You can use :text option to specify plain text email body
  # and :html for HTML email body.
  class Mailer

    class_inheritable_accessor :config, :delivery_method, :deliveries
    attr_accessor :mail
    self.deliveries = []

    # Sends the mail using sendmail.
    def sendmail
      sendmail = IO.popen("#{config[:sendmail_path]} #{@mail.to}", 'w+')
      sendmail.puts @mail.to_s
      sendmail.close
    end

    # Sends the mail using SMTP.
    def net_smtp
      Net::SMTP.start(config[:host], config[:port].to_i, config[:domain],
                      config[:user], config[:pass], config[:auth]) { |smtp|
        smtp.send_message(@mail.to_s, @mail.from.first, @mail.to.to_s.split(/[,;]/))
      }
    end

    # Tests mail sending by adding the mail to deliveries.
    def test_send
      deliveries << @mail
    end

    # Delivers the mail with the specified delivery method, defaulting to
    # net_smtp.
    def deliver!
      send(delivery_method || :net_smtp)
    end

    # ==== Parameters
    # file_or_files<File, Array[File]>:: File(s) to attach.
    # filename<String>::
    # type<~to_s>::
    #   The attachment MIME type. If left out, it will be determined from
    #   file_or_files.
    # headers<String, Array>:: Additional attachment headers.
    #
    # ==== Raises
    # ArgumentError::
    #   file_or_files was not a File or an Array of File instances.
    def attach(file_or_files, filename = file_or_files.is_a?(File) ? File.basename(file_or_files.path) : nil,
      type = nil, headers = nil)
      if file_or_files.is_a?(Array)
        file_or_files.each do |v|
      	  if v.length < 2
      	    v << v.first.is_a?(File) ? File.basename(v.first.path) : nil
      	  end
      	  @mail.add_attachment_as *v
      	end
      else
        raise ArgumentError, "You did not pass in a file. Instead, you sent a #{file_or_files.class}" if !file_or_files.is_a?(File)
        @mail.add_attachment_as(file_or_files, filename, type, headers)
      end
    end

    # ==== Parameters
    # o<Hash{~to_s => Object}>:: Configuration commands to send to MailFactory.
    def initialize(o={})
      self.config = { :sendmail_path => '/usr/sbin/sendmail' } if config.nil?
      o[:rawhtml] = o.delete(:html)
      m = MailFactory.new()
      o.each { |k,v| m.send "#{k}=", v }
      @mail = m
    end

  end
end
