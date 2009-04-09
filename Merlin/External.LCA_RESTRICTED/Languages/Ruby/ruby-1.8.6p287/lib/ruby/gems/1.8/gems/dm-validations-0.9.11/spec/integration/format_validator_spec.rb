require 'pathname'
require Pathname(__FILE__).dirname.expand_path.parent + 'spec_helper'

describe DataMapper::Validate::FormatValidator do
  before :all do
    class ::BillOfLading
      include DataMapper::Resource

      property :id,       Serial
      property :doc_no,   String, :auto_validation => false
      property :email,    String, :auto_validation => false
      property :username, String, :auto_validation => false
      property :url,      String, :auto_validation => false

      # this is a trivial example
      validates_format :doc_no, :with => lambda { |code|
        code =~ /\AA\d{4}\z/ || code =~ /\A[B-Z]\d{6}X12\z/
      }

      validates_format :email, :as => :email_address
      validates_format :url, :as => :url

      validates_format :username, :with => /[a-z]/, :message => 'Username must have at least one letter', :allow_nil => true
    end
  end

  def valid_attributes
    { :id => 1, :doc_no => 'A1234', :email => 'user@example.com', :url => 'http://example.com' }
  end

  it 'should validate the format of a value on an instance of a resource' do
    bol = BillOfLading.new(valid_attributes)
    bol.should be_valid

    bol.doc_no = 'BAD CODE :)'
    bol.should_not be_valid
    bol.errors.on(:doc_no).should include('Doc no has an invalid format')

    bol.doc_no = 'A1234'
    bol.should be_valid

    bol.doc_no = 'B123456X12'
    bol.should be_valid
  end

  describe "RFC2822 compatible email addresses" do
    before do
      @bol = BillOfLading.new(valid_attributes.except(:email))

      @valid_email_addresses = [
        '+1~1+@example.com',
        '{_dave_}@example.com',
        '"[[ dave ]]"@example.com',
        'dave."dave"@example.com',
        'test@localhost',
        'test@example.com',
        'test@example.co.uk',
        'test@example.com.br',
        '"J. P. \'s-Gravezande, a.k.a. The Hacker!"@example.com',
        'me@[187.223.45.119]',
        'someone@123.com',
        'simon&garfunkel@songs.com'
      ]

      @invalid_email_addresses = [
        '-- dave --@example.com',
        '[dave]@example.com',
        '.dave@example.com',
        'Max@Job 3:14',
        'Job@Book of Job',
        'J. P. \'s-Gravezande, a.k.a. The Hacker!@example.com'
      ]
    end

    it "should match the RFC reference addresses" do
      @valid_email_addresses.each do |email|
        email.should =~ DataMapper::Validate::Format::Email::EmailAddress
      end
    end

    it "should not be valid" do
      @invalid_email_addresses.each do |email|
        email.should_not =~ DataMapper::Validate::Format::Email::EmailAddress
      end
    end

  end

  it 'should have a pre-defined URL format' do
    bad = [ 'http:// example.com',
            'ftp://example.com',
            'http://.com',
            'http://',
            'test',
            '...'
          ]

    good = [
            'http://example.com',
            'http://www.example.com',
           ]

    bol = BillOfLading.new(valid_attributes.except(:url))
    bol.should_not be_valid
    bol.errors.on(:url).should include('Url has an invalid format')

    bad.map do |e|
      bol.url = e
      bol.valid?
      bol.errors.on(:url).should include('Url has an invalid format')
    end

    good.map do |e|
      bol.url = e
      bol.valid?
      bol.errors.on(:url).should be_nil
    end

  end

  describe 'with a regexp' do
    before do
      @bol = BillOfLading.new(valid_attributes)
      @bol.should be_valid
    end

    describe 'if matched' do
      before do
        @bol.username = 'a12345'
      end

      it 'should validate' do
        @bol.should be_valid
      end
    end

    describe 'if not matched' do
      before do
        @bol.username = '12345'
      end

      it 'should not validate' do
        @bol.should_not be_valid
      end

      it 'should set an error message' do
        @bol.valid?
        @bol.errors.on(:username).should include('Username must have at least one letter')
      end
    end
  end
end

=begin
addresses = [
  '-- dave --@example.com', # (spaces are invalid unless enclosed in quotation marks)
  '[dave]@example.com', # (square brackets are invalid, unless contained within quotation marks)
  '.dave@example.com', # (the local part of a domain name cannot start with a period)
  'Max@Job 3:14',
  'Job@Book of Job',
  'J. P. \'s-Gravezande, a.k.a. The Hacker!@example.com',
  ]
addresses.each do |address|
  if address =~ RFC2822::EmailAddress
    puts "#{address} deveria ter sido rejeitado, ERRO"
  else
    puts "#{address} rejeitado, OK"
  end
end


addresses = [
  '+1~1+@example.com',
  '{_dave_}@example.com',
  '"[[ dave ]]"@example.com',
  'dave."dave"@example.com',
  'test@localhost',
  'test@example.com',
  'test@example.co.uk',
  'test@example.com.br',
  '"J. P. \'s-Gravezande, a.k.a. The Hacker!"@example.com',
  'me@[187.223.45.119]',
  'someone@123.com',
  'simon&garfunkel@songs.com'
  ]
addresses.each do |address|
  if address =~ RFC2822::EmailAddress
    puts "#{address} aceito, OK"
  else
    puts "#{address} deveria ser aceito, ERRO"
  end
end
=end
