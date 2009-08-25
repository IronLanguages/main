module LoggingHelper
  def logger
    class << DataMapper.logger
      attr_writer :log
    end

    old_log = DataMapper.logger.log

    begin
      StringIO.new('') do |io|
        DataMapper.logger.log = io
        yield io
      end
    ensure
      DataMapper.logger.log = old_log
    end
  end
end
