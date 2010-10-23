require 'logger'

$:.unshift File.dirname(__FILE__)

require 'browser'

module CodePlex
  URL = "http://ironruby.codeplex.com"
  class Base
    def initialize(url)
      @log = Logger.new(STDOUT)
      @log.level = Logger::DEBUG
      @url = "#{URL}#{url}"
      @browser = Browser.new @url, @log
    end

    def done
      @browser.done
    end
  end

  class AdvancedIssueTracker < Base
    PROGNAME = "AdvancedIssueTracker"

    def initialize
      super('/WorkItem/AdvancedList.aspx')
    end

    def reset
      finish_loading
      @log.info(PROGNAME){ "Resetting filtering fields" }
      @browser.link(:id, /ResetButton/).click
      finish_loading
    end

    def type=(val)
      finish_loading
      @log.info(PROGNAME){ "Setting type to \"#{val}\"" }
      @browser.select_list(:id, /TypeListBox/).select_value(val.to_s.capitalize)
      finish_loading
    end

    def sort_by_update_date_dsc
      finish_loading
      if @browser.image(:id, /SortByUpdateDateImage/).href !~ /dsc/
        @log.info(PROGNAME){ "Sorting work-items by update-date, descending" }
        @browser.link(:id, /UpdateDateSortButton/).click
        finish_loading
      end
    end

    def show_fifty
      finish_loading
      fifty = @browser.link(:href, /ShowRangeControl','50'/)
      if fifty.locate
        @log.info(PROGNAME){ "Show 50 work-items on the page" }
        fifty.click
        finish_loading
      end
    end

    def fetch_workitems(options = {})
      defaults = {:type => :closed}
      options = defaults.merge(options)

      @log.info(PROGNAME){ "Getting \"#{options[:type]}\" work items" }
    
      require 'rubygems'
      require 'nokogiri'
      require 'activesupport'
      
      link_regex = /WorkItemId/
      finish_loading
      @browser.select_list(:id, /StatusListBox/).select_value(options[:type].to_s.capitalize)
      finish_loading

      count = 0
      @browser.links.each do |link|
        next if link.href !~ link_regex
        count += 1
        count = 0 if count > 5

        doc = Nokogiri::HTML(link.parent.parent.html)

        WorkItem.create :id => doc.css('tr td.ID').inner_text,
          :title => link.text,
          :date => doc.css('tr td.UpdateDate span').inner_text,
          :votes => doc.css('tr td.Votes').inner_text,
          :severity => doc.css('tr td.Severity').inner_text
      end
      nil
    end

    def finish_loading 
      @browser.finish_loading do |b|
        b.select_list(:id, /StatusListBox/).disabled
      end
    end

    def report(start_date = nil, out = STDOUT)
      WorkItem.order_by :date
      WorkItem.all.each_with_index do |item, i|
        next if !start_date || item.date < start_date
        out.print "#{item.date.to_time.strftime("%Y-%m-%d %H:%M")}\t"
        out.print "#{item.title}\t"
        out.print "(#{item.href})\t"
        out.print "\n"
      end
      nil
    end
  end

  class WorkItem
    class << self
      def all
        @workitems || []
      end

      def create(o)
        @workitems ||= []
        item = new(o[:id], o[:title], o[:date], o[:votes], o[:severity])
        @workitems << item
        item
      end

      def order_by(field = :date)
        @workitems.sort!{|x,y| x.send(field) <=> y.send(field)}
      end
    end

    attr_reader :cpid, :title, :date, :votes, :severity
      
    def initialize(id, title, date, votes, severity)
      @base_url = "/WorkItem/View.aspx?WorkItemId="
      @cpid = id.to_i
      @title = title
      date = DateTime.parse(date)
      date = date - 1.week if date > Time.now
      @date = date 
      @votes = votes.to_i
      @severity = severity.downcase.to_sym
    end
    
    def href
      @href ||= "#{URL}#{@base_url}#{@cpid}"
    end
  end
end
