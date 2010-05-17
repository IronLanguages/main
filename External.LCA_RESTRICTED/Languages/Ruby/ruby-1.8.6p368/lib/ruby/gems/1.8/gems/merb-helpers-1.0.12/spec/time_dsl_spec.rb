require File.dirname(__FILE__) + '/spec_helper'

describe "TimeDSL" do
  it "Should do second/seconds" do
    10.seconds.should == 10
    1.second.should == 1
  end

  it "Should do minute/minutes" do
    22.minutes.should == 22 * 60
    1.minute.should == 60
  end

  it "Should do hour/hours" do
    24.hours.should == 24 * 3600
    1.hour.should == 3600
  end

  it "Should do day/days" do
    7.days.should == 7 * 24 * 3600
    1.day.should == 24 * 3600
  end

  it "Should do month/months" do
    9.months.should == 9 * 30 * 24 * 3600
    1.month.should == 30 * 24 * 3600
  end

  it "Should do year/years" do
    3.years.should == 3 * 364.25 * 24 * 3600
    1.year.should == 364.25 * 24 * 3600
  end

  it "Should do ago/until" do
    5.seconds.ago.should be_close(Time.now - 5, 0.5)
    8.minutes.until(3.minute.from_now).should be_close(3.minutes.from_now - 8 * 60, 0.5)
  end

  it "Should do from_now/since" do
    3.seconds.from_now.should be_close(Time.now + 3, 0.5)
    2.minutes.since(2.minutes.ago).should be_close(2.minutes.ago + 2 * 60, 0.5)
  end
end
