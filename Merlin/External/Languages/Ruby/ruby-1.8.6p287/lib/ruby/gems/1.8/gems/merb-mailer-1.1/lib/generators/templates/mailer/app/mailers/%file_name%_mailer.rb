<% with_modules(modules) do -%>
class <%= class_name %>Mailer < Merb::MailController

  def notify_on_event
    # use params[] passed to this controller to get data
    # read more at http://wiki.merbivore.com/pages/mailers
    render_mail
  end
  
end
<% end -%>