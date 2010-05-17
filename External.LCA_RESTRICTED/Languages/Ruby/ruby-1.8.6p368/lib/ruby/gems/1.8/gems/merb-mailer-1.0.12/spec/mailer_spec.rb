require File.dirname(__FILE__) + '/spec_helper'

class TestMailer < Merb::Mailer
  self.delivery_method = :test_send
end

class TestSMTPMailer < Merb::Mailer
  self.delivery_method = :net_smtp
  self.config = { :host     => 'smtp.yourserver.com',
                  :port     => '25',              
                  :user     => 'user',
                  :pass     => 'pass',
                  :auth     => :plain }
                  
end

class TestSendmailMailer < Merb::Mailer
  self.delivery_method = :sendmail
end

def setup_test_mailer klass = TestMailer
  @m = klass.new :to      => "test@test.com",
                 :from    => "foo@bar.com",
                 :subject => "Test Subject",
                 :body    => "Test"    
end

describe "a merb mailer" do

  before(:each) do
    TestMailer.deliveries.clear
    TestSMTPMailer.deliveries.clear
    TestSendmailMailer.deliveries.clear
  end
  
  it "should be able to send test emails" do
    setup_test_mailer
    @m.deliver!
    TestMailer.deliveries.size.should == 1
    delivery = TestMailer.deliveries.last
    delivery.to.should include("test@test.com")
    delivery.from.should include("foo@bar.com")
    delivery.subject.first.should =~ /utf-8/
    delivery.subject.first.should =~ /Test_Subject/
    delivery.body.should include("Test")
  end
  
  it "should be able to accept attachments" do
    setup_test_mailer
    @m.attach File.open("README.textile")
    @m.deliver!
    delivery = TestMailer.deliveries.last
    delivery.instance_variable_get("@attachments").size.should == 1
  end
  
  it "should be able to accept multiple attachments" do
    setup_test_mailer
    @m.attach [[File.open("README.textile")], [File.open("LICENSE")]]
    @m.deliver!
    delivery = TestMailer.deliveries.last
    delivery.instance_variable_get("@attachments").size.should == 2    
  end

  it "should be able to accept custom options for attachments" do
    setup_test_mailer
    @m.attach File.open("README.textile"), 'readme', 'text/plain', 'Content-ID: <readme.txt>'
    @m.deliver!
    delivery = TestMailer.deliveries.last
    attachments = delivery.instance_variable_get("@attachments")
    attachments.size.should == 1
    attachments.first["mimetype"].should eql("text/plain")
    attachments.first["filename"].should eql("readme")
    attachments.first["headers"].should include("Content-ID: <readme.txt>")
  end

  it "should be able to accept custom options for multiple attachments" do
    setup_test_mailer
    @m.attach [[File.open("README.textile"), 'readme', 'text/plain', 'Content-ID: <readme.txt>'],
               [File.open("LICENSE"), 'license', 'text/plain', 'Content-ID: <license.txt>']]
    @m.deliver!
    delivery = TestMailer.deliveries.last
    attachments = delivery.instance_variable_get("@attachments")
    attachments.size.should == 2
    attachments.first["mimetype"].should eql("text/plain")
    attachments.first["filename"].should eql("readme")
    attachments.first["headers"].should include("Content-ID: <readme.txt>")
    attachments.last["mimetype"].should eql("text/plain")
    attachments.last["filename"].should eql("license")
    attachments.last["headers"].should include("Content-ID: <license.txt>")
  end

  it "should be able to send mails via SMTP" do
    setup_test_mailer TestSMTPMailer
    Net::SMTP.stub!(:start).and_return(true)
    Net::SMTP.should_receive(:start).with("smtp.yourserver.com", 25, nil, "user", "pass", :plain)
    @m.deliver!
  end
  
  it "should send mails via SMTP with no auth" do
    setup_test_mailer TestSMTPMailer
    @m.config[:auth] = nil
    Net::SMTP.stub!(:start).and_return(true)
    Net::SMTP.should_receive(:start).with("smtp.yourserver.com", 25, nil, "user", "pass", nil)
    @m.deliver!
  end
  
  it "should be able to send mails via sendmail" do
    sendmail = mock("/usr/sbin/sendmail", :null_object => true) 
    setup_test_mailer TestSendmailMailer
    IO.should_receive(:popen).with("/usr/sbin/sendmail #{@m.mail.to}", "w+").and_return(sendmail) 
    @m.deliver!
  end  
  
  it "should be able to use a different sendmail path" do 
 	  sendmail = mock("/somewhere/sendmail", :null_object => true) 
 	  setup_test_mailer TestSendmailMailer 
 	  @m.config[:sendmail_path] = '/somewhere/sendmail' 
 	  IO.should_receive(:popen).with("/somewhere/sendmail #{@m.mail.to}", "w+").and_return(sendmail) 
 	  @m.deliver! 
 	end
  
end
