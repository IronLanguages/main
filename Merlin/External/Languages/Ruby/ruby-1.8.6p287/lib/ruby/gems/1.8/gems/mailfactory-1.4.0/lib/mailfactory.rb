# = Overview:
# A simple to use module for generating RFC compliant MIME mail
# ---
# = License:
# Author:: David Powers
# Copyright:: May, 2005
# License:: Ruby License
# ---
# = Usage:
#   require 'net/smtp'
#   require 'rubygems'
#   require 'mailfactory'
#
#
#   mail = MailFactory.new()
#   mail.to = "test@test.com"
#   mail.from = "sender@sender.com"
#   mail.subject = "Here are some files for you!"
#   mail.text = "This is what people with plain text mail readers will see"
#   mail.html = "A little something <b>special</b> for people with HTML readers"
#   mail.attach("/etc/fstab")
#   mail.attach("/some/other/file")
#
#   Net::SMTP.start('smtp1.testmailer.com', 25, 'mail.from.domain', fromaddress, password, :cram_md5) { |smtp|
#     mail.to = toaddress
#     smtp.send_message(mail.to_s(), fromaddress, toaddress)
#   }

require 'pathname'

# try to bring in the mime/types module, make a dummy module if it can't be found
begin
  begin
    require 'rubygems'
  rescue LoadError
  end
  require 'mime/types'
rescue LoadError
  module MIME
    class Types
      def Types::type_for(filename)
        return('')
      end
    end
  end
end

# An easy class for creating a mail message
class MailFactory

  VERSION = '1.4.0'
  
  def initialize()
    @headers = Array.new()
    @attachments = Array.new()
    @attachmentboundary = generate_boundary()
    @bodyboundary = generate_boundary()
    @html = nil
    @text = nil
    @charset = 'utf-8'
  end
  
  
  # adds a header to the bottom of the headers
  def add_header(header, value)
    value = quoted_printable_with_instruction(value, @charset) if header == 'subject'
    value = quote_address_if_necessary(value, @charset) if %w[from to cc bcc reply-to].include?(header.downcase)
    @headers << "#{header}: #{value}"
  end
  
  
  # removes the named header - case insensitive
  def remove_header(header)
    @headers.each_index() { |i|
      if(@headers[i] =~ /^#{Regexp.escape(header)}:/i)
        @headers.delete_at(i)
      end
    }
  end
  
  
  # sets a header (removing any other versions of that header)
  def set_header(header, value)
    remove_header(header)
    add_header(header, value)
  end
  
  
  def replyto=(newreplyto)
    remove_header("Reply-To")
    add_header("Reply-To", newreplyto)
  end
  
  
  def replyto()
    return(get_header("Reply-To")[0])
  end
  
  
  # sets the plain text body of the message
  def text=(newtext)
    @text = newtext
  end
  
  
  # sets the HTML body of the message. Only the body of the
  # html should be provided
  def html=(newhtml)
    @html = "<html>\n<head>\n<meta content=\"text/html;charset=#{@charset}\" http-equiv=\"Content-Type\">\n</head>\n<body bgcolor=\"#ffffff\" text=\"#000000\">\n#{newhtml}\n</body>\n</html>"
  end
  
  
  # sets the HTML body of the message.  The entire HTML section should be provided
  def rawhtml=(newhtml)
    @html = newhtml
  end
  
  
  # implement method missing to provide helper methods for setting and getting headers.
  # Headers with '-' characters may be set/gotten as 'x_mailer' or 'XMailer' (splitting
  # will occur between capital letters or on '_' chracters)
  def method_missing(methId, *args)
    name = methId.id2name()
    
    # mangle the name if we have to
    if(name =~ /_/)
      name = name.gsub(/_/, '-')
    elsif(name =~ /[A-Z]/)
      name = name.gsub(/([a-zA-Z])([A-Z])/, '\1-\2')
    end
    
    # determine if it sets or gets, and do the right thing
    if(name =~ /=$/)
      if(args.length != 1)
        super(methId, args)
      end
      set_header(name[/^(.*)=$/, 1], args[0])     
    else
      if(args.length != 0)
        super(methId, args)
      end
      headers = get_header(name)
      return(get_header(name))
    end
  end

  
  # returns the value (or values) of the named header in an array
  def get_header(header)
    headers = Array.new()
    headerregex = /^#{Regexp.escape(header)}:/i
    @headers.each() { |h|
      if(headerregex.match(h))
        headers << h[/^[^:]+:(.*)/i, 1].strip()
      end
    }
    
    return(headers)
  end
  
  
  # returns true if the email is multipart
  def multipart?()
    if(@attachments.length > 0 or @html != nil)
      return(true)
    else
      return(false)
    end
  end
  

  # builds an email and returns it as a string.  Takes the following options:
  # <tt>:messageid</tt>:: Adds a message id to the message based on the from header (defaults to false)
  # <tt>:date</tt>:: Adds a date to the message if one is not present (defaults to true)
  def construct(options = Hash.new)
    if(options[:date] == nil)
      options[:date] = true
    end
    
    if(options[:messageid])
      # add a unique message-id
      remove_header("Message-ID")
      sendingdomain = get_header('from')[0].to_s()[/@([-a-zA-Z0-9._]+)/,1].to_s()
      add_header("Message-ID", "<#{Time.now.to_f()}.#{Process.euid()}.#{String.new.object_id()}@#{sendingdomain}>")
    end

    if(options[:date])
      if(get_header("Date").length == 0)
        add_header("Date", Time.now.strftime("%a, %d %b %Y %H:%M:%S %z"))
      end
    end

    # Add a mime header if we don't already have one and we have multiple parts
    if(multipart?())
      if(get_header("MIME-Version").length == 0)
        add_header("MIME-Version", "1.0")
      end
      
      if(get_header("Content-Type").length == 0)
        if(@attachments.length == 0)
          add_header("Content-Type", "multipart/alternative;boundary=\"#{@bodyboundary}\"")
        else
          add_header("Content-Type", "multipart/mixed; boundary=\"#{@attachmentboundary}\"")
        end
      end
    end
    
    return("#{headers_to_s()}#{body_to_s()}")
  end

  
  # returns a formatted email - equivalent to construct(:messageid => true) 
  def to_s()
    return(construct(:messageid => true))
  end
  
  
  # generates a unique boundary string
  def generate_boundary()
    randomstring = Array.new()
    1.upto(25) {
      whichglyph = rand(100)
      if(whichglyph < 40)
        randomstring << (rand(25) + 65).chr()
      elsif(whichglyph < 70)
        randomstring << (rand(25) + 97).chr()
      elsif(whichglyph < 90)
        randomstring << (rand(10) + 48).chr()
      elsif(whichglyph < 95)
        randomstring << '.'
      else
        randomstring << '_'
      end
    }
    return("----=_NextPart_#{randomstring.join()}")
  end
  
  
  # adds an attachment to the mail.  Type may be given as a mime type.  If it
  # is left off and the MIME::Types module is available it will be determined automagically.
  # If the optional attachemntheaders is given, then they will be added to the attachment
  # boundary in the email, which can be used to produce Content-ID markers.  attachmentheaders
  # can be given as an Array or a String.
  def add_attachment(filename, type=nil, attachmentheaders = nil)
    attachment = Hash.new()
    attachment['filename'] = Pathname.new(filename).basename
    if(type == nil)
      attachment['mimetype'] = MIME::Types.type_for(filename).to_s
    else
      attachment['mimetype'] = type
    end 
    
    # Open in rb mode to handle Windows, which mangles binary files opened in a text mode
    File.open(filename, "rb") { |fp|
      attachment['attachment'] = file_encode(fp.read())
    }

    if(attachmentheaders != nil)
      if(!attachmentheaders.kind_of?(Array))
        attachmentheaders = attachmentheaders.split(/\r?\n/)
      end
      attachment['headers'] = attachmentheaders
    end

    @attachments << attachment
  end
  
  
  # adds an attachment to the mail as emailfilename.  Type may be given as a mime type.  If it
  # is left off and the MIME::Types module is available it will be determined automagically.
  # file may be given as an IO stream (which will be read until the end) or as a filename.
  # If the optional attachemntheaders is given, then they will be added to the attachment
  # boundary in the email, which can be used to produce Content-ID markers.  attachmentheaders
  # can be given as an Array of a String.
  def add_attachment_as(file, emailfilename, type=nil, attachmentheaders = nil)
    attachment = Hash.new()
    attachment['filename'] = emailfilename

    if(type != nil)
      attachment['mimetype'] = type.to_s()
    elsif(file.kind_of?(String) or file.kind_of?(Pathname))
      attachment['mimetype'] = MIME::Types.type_for(file.to_s()).to_s
    else
      attachment['mimetype'] = ''
    end
    
    if(file.kind_of?(String) or file.kind_of?(Pathname))    
      # Open in rb mode to handle Windows, which mangles binary files opened in a text mode
      File.open(file.to_s(), "rb") { |fp|
        attachment['attachment'] = file_encode(fp.read())
      }
    elsif(file.respond_to?(:read))
      attachment['attachment'] = file_encode(file.read())
    else
      raise(Exception, "file is not a supported type (must be a String, Pathnamem, or support read method)")
    end
    
    if(attachmentheaders != nil)
      if(!attachmentheaders.kind_of?(Array))
        attachmentheaders = attachmentheaders.split(/\r?\n/)
      end
      attachment['headers'] = attachmentheaders
    end
    
    @attachments << attachment
  end
  
  
  alias attach add_attachment
  alias attach_as add_attachment_as
  
protected
  
  # returns the @headers as a properly formatted string
  def headers_to_s()
    return("#{@headers.join("\r\n")}\r\n\r\n")
  end
  
  
  # returns the body as a properly formatted string
  def body_to_s()
    body = Array.new()
    
    # simple message with one part
    if(!multipart?())
      return(@text)
    else
      body << "This is a multi-part message in MIME format.\r\n\r\n--#{@attachmentboundary}\r\nContent-Type: multipart/alternative; boundary=\"#{@bodyboundary}\""
      
      if(@attachments.length > 0)
        # text part
        body << "#{buildbodyboundary("text/plain; charset=#{@charset}; format=flowed", 'quoted-printable')}\r\n\r\n#{quote_if_necessary(@text, @charset)}"
        
        # html part if one is provided
        if @html
          body << "#{buildbodyboundary("text/html; charset=#{@charset}", 'quoted-printable')}\r\n\r\n#{quote_if_necessary(@html, @charset)}"
        end
        
        body << "--#{@bodyboundary}--"
        
        # and, the attachments
        if(@attachments.length > 0)
          @attachments.each() { |attachment|
            body << "#{buildattachmentboundary(attachment)}\r\n\r\n#{attachment['attachment']}"
          }
          body << "\r\n--#{@attachmentboundary}--"
        end
      else
        # text part
        body << "#{buildbodyboundary("text/plain; charset=#{@charset}; format=flowed", 'quoted-printable')}\r\n\r\n#{quote_if_necessary(@text, @charset)}"
        
        # html part
        body << "#{buildbodyboundary("text/html; charset=#{@charset}", 'quoted-printable')}\r\n\r\n#{quote_if_necessary(@html, @charset)}"
        
        body << "--#{@bodyboundary}--"
      end
      
      return(body.join("\r\n\r\n"))
    end
  end
  
  
  # builds a boundary string for including attachments in the body, expects an attachment hash as built by
  # add_attachment and add_attachment_as
  def buildattachmentboundary(attachment)
    disposition = "Content-Disposition: inline; filename=\"#{attachment['filename']}\""
    boundary = "--#{@attachmentboundary}\r\nContent-Type: #{attachment['mimetype']}; name=\"#{attachment['filename']}\"\r\nContent-Transfer-Encoding: base64\r\n#{disposition}"
    if(attachment['headers'])
      boundary = boundary + "\r\n#{attachment['headers'].join("\r\n")}"
    end
    
    return(boundary)
  end
  
  
  # builds a boundary string for inclusion in the body of a message
  def buildbodyboundary(type, encoding)
    return("--#{@bodyboundary}\r\nContent-Type: #{type}\r\nContent-Transfer-Encoding: #{encoding}")
  end


  # returns a base64 encoded version of the contents of str
  def file_encode(str)
    collection = Array.new()
    enc = [str].pack('m')
    #  while(enc.length > 60)
    #    collection << enc.slice!(0..59)
    #  end
    #  collection << enc
    #  return(collection.join("\n"))
    return(enc)
  end
  
  
  # Convert the given text into quoted printable format, with an instruction
  # that the text be eventually interpreted in the given charset.
  
  def quoted_printable_with_instruction(text, charset)
    text = quoted_printable_encode_header(text)
    "=?#{charset}?Q?#{text}?="
  end

  # rfc2045 compatible. use rfc2047 for headers (such as the Subject line) instead
  def quoted_printable_encode(text)
    [text].pack('M').gsub(/\n/, "\r\n").chomp.gsub(/=$/, '')
  end

  # Convert the given character to quoted printable format
  # see http://tools.ietf.org/html/rfc2047
  
  require 'enumerator' unless ''.respond_to? :enum_for

  def quoted_printable_encode_header(text)
    text.enum_for(:each_byte).map do |ord|
      if ord < 128 and ord != 61 # 61 is ascii '='
        ord.chr
      else
        '=%X' % ord
      end
    end.join('').
        chomp.
        gsub(/=$/,'').
        gsub('?', '=3F').
        gsub('_', '=5F').
        gsub(/ /, '_')
  end


  # A quick-and-dirty regexp for determining whether a string contains any
  # characters that need escaping.
  #--
  # Jun18-08: deprecated, since all multipart blocks are marked quoted-printable, quoting is required

  # if !defined?(CHARS_NEEDING_QUOTING)
  #   CHARS_NEEDING_QUOTING = /[\000-\011\013\014\016-\037\177-\377]/
  # end


  # Quote the given text if it contains any "illegal" characters
  def quote_if_necessary(text, charset, instruction = false)
    return unless text
    text = text.dup.force_encoding(Encoding::ASCII_8BIT) if text.respond_to?(:force_encoding)
    #(text =~ CHARS_NEEDING_QUOTING) ? (instruction ? quoted_printable_with_instruction(text, charset) : quoted_printable_encode(text)) : text
    instruction ? quoted_printable_with_instruction(text, charset) : quoted_printable_encode(text)
  end


  # Quote the given address if it needs to be. The address may be a
  # regular email address, or it can be a phrase followed by an address in
  # brackets. The phrase is the only part that will be quoted, and only if
  # it needs to be. This allows extended characters to be used in the
  # "to", "from", "cc", and "bcc" headers.
  def quote_address_if_necessary(address, charset)
    if Array === address
      address.map { |a| quote_address_if_necessary(a, charset) }
    elsif address =~ /^(\S.*)\s+(<.*>)$/
      address = $2
      phrase = quote_if_necessary($1.gsub(/^['"](.*)['"]$/, '\1'), charset, true)
      "\"#{phrase}\" #{address}"
    else
      address
    end
  end
  
end
