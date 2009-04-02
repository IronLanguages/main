require File.dirname(__FILE__) + '/spec_helper'

describe "relative_date" do
  include Merb::Helpers::DateAndTime

  before :each do
    Time.stub!(:now).and_return(Time.utc(2007, 6, 1, 11))
    @controller = RelativeDateSpecs.new(Merb::Request.new({}))
  end

  it "Should show today" do
    relative_date(Time.now.utc).should == "today"
    result = @controller.render :relative_today
    result.should == "today"
  end

  it "Should show yesterday" do
    relative_date(1.day.ago.utc).should == 'yesterday'
    result = @controller.render :relative_yesterday
    result.should == "yesterday"
  end

  it "Should show tomorrow" do
    relative_date(1.day.from_now.utc).should == 'tomorrow'
    @controller.render(:relative_tomorrow).should == 'tomorrow'
  end

  it "Should show date with year" do
    relative_date(Time.utc(2005, 11, 15)).should == 'Nov 15th, 2005'
    @controller.render(:relative_date_with_year).should == 'Nov 15th, 2005'
  end

  it "Should show date" do
    relative_date(Time.utc(2007, 11, 15)).should == 'Nov 15th'
    @controller.render(:relative_date_without_year).should == 'Nov 15th'
  end
end

describe "relative_date_span" do
  include Merb::Helpers::DateAndTime

  before :each do
    Time.stub!(:now).and_return(Time.utc(2007, 6, 1, 11))
    @controller = RelativeDateSpanSpecs.new(Merb::Request.new({}))
  end

  it "Should show date span on the same day" do
    relative_date_span([Time.utc(2007, 11, 15), Time.utc(2007, 11, 15)]).should == 'Nov 15th'
    @controller.render(:date_span_on_same_day).should == 'Nov 15th'
  end

  it "Should show date span on the same day on different year" do
    relative_date_span([Time.utc(2006, 11, 15), Time.utc(2006, 11, 15)]).should == 'Nov 15th, 2006'
    @controller.render(:date_span_on_same_day_on_different_year).should == 'Nov 15th, 2006'
  end

  it "Should show date span on the same month" do
    relative_date_span([Time.utc(2007, 11, 15), Time.utc(2007, 11, 16)]).should == 'Nov 15th - 16th'
    relative_date_span([Time.utc(2007, 11, 16), Time.utc(2007, 11, 15)]).should == 'Nov 15th - 16th'
  end

  it "Should show date span on the same month on different year" do
    relative_date_span([Time.utc(2006, 11, 15), Time.utc(2006, 11, 16)]).should == 'Nov 15th - 16th, 2006'
    relative_date_span([Time.utc(2006, 11, 16), Time.utc(2006, 11, 15)]).should == 'Nov 15th - 16th, 2006'
  end

  it "Should show date span on the different month" do
    relative_date_span([Time.utc(2007, 11, 15), Time.utc(2007, 12, 16)]).should == 'Nov 15th - Dec 16th'
    relative_date_span([Time.utc(2007, 12, 16), Time.utc(2007, 11, 15)]).should == 'Nov 15th - Dec 16th'
  end

  it "Should show date span on the different month on different year" do
    relative_date_span([Time.utc(2006, 11, 15), Time.utc(2006, 12, 16)]).should == 'Nov 15th - Dec 16th, 2006'
    relative_date_span([Time.utc(2006, 12, 16), Time.utc(2006, 11, 15)]).should == 'Nov 15th - Dec 16th, 2006'
  end

  it "Should show date span on the different year" do
    relative_date_span([Time.utc(2006, 11, 15), Time.utc(2007, 12, 16)]).should == 'Nov 15th, 2006 - Dec 16th, 2007'
    relative_date_span([Time.utc(2007, 12, 16), Time.utc(2006, 11, 15)]).should == 'Nov 15th, 2006 - Dec 16th, 2007'
  end
end

describe "relative_time_span" do
  include Merb::Helpers::DateAndTime

  before :each do
    Time.stub!(:now).and_return(Time.utc(2007, 6, 1, 11))
  end

  # Time, Single Date
  it "Should show time span on the same day with same time" do
    relative_time_span([Time.utc(2007, 11, 15, 17, 00, 00)]).should == '5:00 PM Nov 15th'
  end

  it "Should show time span on the same day with same time on different year" do
    relative_time_span([Time.utc(2006, 11, 15, 17, 0), Time.utc(2006, 11, 15, 17, 0)]).should == '5:00 PM Nov 15th, 2006'
  end

  it "Should show time span on the same day with different times in same half of day" do
    relative_time_span([Time.utc(2007, 11, 15, 10), Time.utc(2007, 11, 15, 11, 0)]).should == '10:00 - 11:00 AM Nov 15th'
  end

  it "Should show time span on the same day with different times in different half of day" do
    relative_time_span([Time.utc(2007, 11, 15, 10, 0), Time.utc(2007, 11, 15, 14, 0)]).should == '10:00 AM - 2:00 PM Nov 15th'
  end

  it "Should show time span on the same day with different times in different half of day in different year" do
    relative_time_span([Time.utc(2006, 11, 15, 10, 0), Time.utc(2006, 11, 15, 14, 0)]).should == '10:00 AM - 2:00 PM Nov 15th, 2006'
  end

  it "Should show time span on different days in same year" do
    relative_time_span([Time.utc(2006, 11, 15, 10, 0), Time.utc(2006, 12, 16, 14, 0)]).should == '10:00 AM Nov 15th - 2:00 PM Dec 16th, 2006'
  end

  it "Should show time span on different days in different years" do
    relative_time_span([Time.utc(2006, 11, 15, 10, 0), Time.utc(2007, 12, 16, 14, 0)]).should == '10:00 AM Nov 15th, 2006 - 2:00 PM Dec 16th, 2007'
  end

  it "Should show time span on different days in current year" do
    relative_time_span([Time.utc(2007, 11, 15, 10, 0), Time.utc(2007, 12, 16, 14, 0)]).should == '10:00 AM Nov 15th - 2:00 PM Dec 16th'
  end
end

describe "time_lost_in_words" do
  include Merb::Helpers::DateAndTime

  it "Should show seconds" do
    time_lost_in_words(Time.now, Time.now, true).should == "less than 5 seconds"
  end

  it "Should not show seconds" do
    time_lost_in_words(Time.now).should == "less than a minute"
  end

  it "Should do minutes" do
    time_lost_in_words(2.minutes.ago).should == "2 minutes"
  end

  it "Should do hour" do
    time_lost_in_words(50.minutes.ago).should == "about 1 hour"
  end

  it "Should do hours" do
    time_lost_in_words(2.hours.ago).should == "about 2 hours"
  end

  it "Should do day" do
    time_lost_in_words(1.day.ago).should == "1 day"
  end

  it "Should do days" do
    time_lost_in_words(5.days.ago).should == "5 days"
  end

  it "Should do month" do
    time_lost_in_words(1.month.ago).should == "about 1 month"
  end

  it "Should do months" do
    time_lost_in_words(5.months.ago).should == "5 months"
  end

  it "Should do year" do
    time_lost_in_words(1.2.years.ago).should == "about 1 year"
  end

  it "Should do years" do
    time_lost_in_words(5.5.years.ago).should == "over 5 years"
  end
end

describe "prettier_time" do
  include Merb::Helpers::DateAndTime

  # prettier time"
  it "Should not show leading zero in hour" do
    prettier_time(Time.utc(2007, 11, 15, 14, 0)).should == '2:00 PM'
  end

  it "Should convert to 12 hour time" do
    prettier_time(Time.utc(2007, 11, 15, 2, 0)).should == '2:00 AM'
  end

  it "Should handle midnight correctly" do
    prettier_time(Time.utc(2007, 11, 15, 0, 0)).should == '12:00 AM'
  end
end

shared_examples_for "Date, DateTime, Time formatting" do

  before(:each) do
    Date.reset_formats
  end

  it "should list the available formats" do
    Date.formats.should be_an_instance_of(Hash)
    Date.formats.keys.length.should > 1
  end

  it "should support to be db formatted" do
    @date.formatted(:db).should =~ /^2007-11-02 \d{2}:\d{2}:\d{2}$/
  end

  it "should support to be time formatted" do
    @date.formatted(:time).should == "00:00"
  end

  it "should support to be short formatted" do
    @date.formatted(:short).should == "02 Nov 00:00"
  end

  it "should support to be date formatted" do
    @date.formatted(:date).should == "2007-11-02"
  end

  it "should support to be long formatted" do
    @date.formatted(:long).should == "November 02, 2007 00:00"
  end

  it "should support a new date format" do
    @date.formatted(:matt).should == @date.to_s
    Date.add_format(:matt, "%H:%M:%S %Y-%m-%d")
    @date.formatted(:matt).should == "00:00:00 2007-11-02"
  end

end


describe "Date" do
  before :each do
    @date = Date.new(2007, 11, 02)
  end

  it "Should do to_time conversion and return a Time class" do
    @date.is_a?(Date)
    @date.to_time.is_a?(Time)
  end

  it "Should do to_time conversion to utc by default" do
    @date.to_time.to_s.should == 'Fri Nov 02 00:00:00 UTC 2007'
  end

  it "Should do to_time conversion to utc when param :utc is given" do
    @date.to_time(:utc).to_s.should == 'Fri Nov 02 00:00:00 UTC 2007'
  end

  it "Should do to_time conversion to local time when param :local is given" do
    pending("Needs to have the call to figure out the local time stubbed so this test will work no matter what your local TZ is.")
    @date.to_time(:local).to_s.should == 'Fri Nov 02 00:00:00 -0500 2007'
  end

  it "Should return itself when to_date is called" do
    @date.to_date.should == @date
  end

  it_should_behave_like "Date, DateTime, Time formatting"

end

describe "DateTime" do

  before(:each) do
    @date = DateTime.new(2007, 11, 02)
  end

  it_should_behave_like "Date, DateTime, Time formatting"

end
