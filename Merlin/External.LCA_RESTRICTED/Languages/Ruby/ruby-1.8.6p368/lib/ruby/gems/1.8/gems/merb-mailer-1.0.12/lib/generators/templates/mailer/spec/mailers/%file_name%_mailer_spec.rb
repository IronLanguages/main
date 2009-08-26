require File.join(File.dirname(__FILE__), "..", "spec_helper")

# Move this to your spec_helper.rb.
module MailControllerTestHelper
  # Helper to clear mail deliveries.
  def clear_mail_deliveries
    Merb::Mailer.deliveries.clear
  end

  # Helper to access last delivered mail.
  # In test mode merb-mailer puts email to
  # collection accessible as Merb::Mailer.deliveries.
  def last_delivered_mail
    Merb::Mailer.deliveries.last
  end

  # Helper to deliver
  def deliver(action, mail_params = {}, send_params = {})
    <%= class_name %>Mailer.dispatch_and_deliver(action, { :from => "no-reply@webapp.com", :to => "recepient@person.com" }.merge(mail_params), send_params)
    @delivery = last_delivered_mail
  end
end

describe <%= class_name %>Mailer, "#notify_on_event email template" do
  include MailControllerTestHelper
  
  before :each do
    clear_mail_deliveries
    
    # instantiate some fixture objects
  end
    
  it "includes welcome phrase in email text" do
    violated "Mailer controller deserves to have specs, too."
    
    # <%= class_name %>Mailer.dispatch_and_deliver(:notify_on_event, {}, { :name => "merb-mailer user" })
    # last_delivered_mail.text.should =~ /Hello, merb-mailer user!/
  end
end