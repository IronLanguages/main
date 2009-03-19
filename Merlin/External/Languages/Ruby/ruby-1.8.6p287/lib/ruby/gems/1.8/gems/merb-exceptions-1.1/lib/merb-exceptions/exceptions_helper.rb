module MerbExceptions
  module ExceptionsHelper
    protected
    # if you need to handle the render yourself for some reason, you can call
    # this method directly. It sends notifications without any rendering logic.
    # Note though that if you are sending lots of notifications this could
    # delay sending a response back to the user so try to avoid using it
    # where possible.
    def notify_of_exceptions
      if Merb::Plugins.config[:exceptions][:environments].include?(Merb.env)
        begin
          request = self.request

          details = {}
          details['exceptions']      = request.exceptions
          details['params']          = params
          details['environment']     = request.env.merge( 'process' => $$ )
          details['url']             = "#{request.protocol}#{request.env["HTTP_HOST"]}#{request.uri}"
          MerbExceptions::Notification.new(details).deliver!
        rescue Exception => e
          exceptions = request.exceptions << e
          Merb.logger.fatal!("Exception Notification Failed:\n" + (exceptions).inspect)
          File.open(Merb.root / 'log' / 'notification_errors.log', 'a') do |log|
            log.puts("Exception Notification Failed:")
            exceptions.each do |e|
              log.puts(Merb.exception(e))
            end
          end
        end
      end
    end

  end
end