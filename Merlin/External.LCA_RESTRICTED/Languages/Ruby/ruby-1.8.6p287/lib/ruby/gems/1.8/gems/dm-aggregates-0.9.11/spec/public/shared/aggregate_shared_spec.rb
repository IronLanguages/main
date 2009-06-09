shared_examples_for 'It Has Setup Resources' do
  before :all do
    # A simplistic example, using with an Integer property
    class ::Knight
      include DataMapper::Resource

      property :id,   Serial
      property :name, String
    end

    class ::Dragon
      include DataMapper::Resource

      property :id,                Serial
      property :name,              String
      property :is_fire_breathing, Boolean
      property :toes_on_claw,      Integer
      property :birth_at,          DateTime
      property :birth_on,          Date
      property :birth_time,        Time

      belongs_to :knight
    end

    # A more complex example, with BigDecimal and Float properties
    # Statistics taken from CIA World Factbook:
    # https://www.cia.gov/library/publications/the-world-factbook/
    class ::Country
      include DataMapper::Resource

      property :id,                  Serial
      property :name,                String,     :nullable => false
      property :population,          Integer
      property :birth_rate,          Float,      :precision => 4,  :scale => 2
      property :gold_reserve_tonnes, Float,      :precision => 6,  :scale => 2
      property :gold_reserve_value,  BigDecimal, :precision => 15, :scale => 1  # approx. value in USD
    end

    DataMapper.auto_migrate!

    @birth_at   = DateTime.now
    @birth_on   = Date.parse(@birth_at.to_s)
    @birth_time = Time.parse(@birth_at.to_s)

    @chuck = Knight.create(:name => 'Chuck')
    @larry = Knight.create(:name => 'Larry')

    Dragon.create(:name => 'George', :is_fire_breathing => false, :toes_on_claw => 3, :birth_at => @birth_at, :birth_on => @birth_on, :birth_time => @birth_time, :knight => @chuck )
    Dragon.create(:name => 'Puff',   :is_fire_breathing => true,  :toes_on_claw => 4, :birth_at => @birth_at, :birth_on => @birth_on, :birth_time => @birth_time, :knight => @larry )
    Dragon.create(:name => nil,      :is_fire_breathing => true,  :toes_on_claw => 5, :birth_at => nil,       :birth_on => nil,       :birth_time => nil)

    gold_kilo_price  = 277738.70
    @gold_tonne_price = gold_kilo_price * 10000

    Country.create(:name => 'China',
                    :population => 1330044605,
                    :birth_rate => 13.71,
                    :gold_reserve_tonnes => 600.0,
                    :gold_reserve_value  => 600.0 * @gold_tonne_price) #  32150000
    Country.create(:name => 'United States',
                    :population => 303824646,
                    :birth_rate => 14.18,
                    :gold_reserve_tonnes => 8133.5,
                    :gold_reserve_value  => 8133.5 * @gold_tonne_price)
    Country.create(:name => 'Brazil',
                    :population => 191908598,
                    :birth_rate => 16.04,
                    :gold_reserve_tonnes => nil) # example of no stats available
    Country.create(:name => 'Russia',
                    :population => 140702094,
                    :birth_rate => 11.03,
                    :gold_reserve_tonnes => 438.2,
                    :gold_reserve_value  => 438.2 * @gold_tonne_price)
    Country.create(:name => 'Japan',
                    :population => 127288419,
                    :birth_rate => 7.87,
                    :gold_reserve_tonnes => 765.2,
                    :gold_reserve_value  => 765.2 * @gold_tonne_price)
    Country.create(:name => 'Mexico',
                    :population => 109955400,
                    :birth_rate => 20.04,
                    :gold_reserve_tonnes => nil) # example of no stats available
    Country.create(:name => 'Germany',
                    :population => 82369548,
                    :birth_rate => 8.18,
                    :gold_reserve_tonnes => 3417.4,
                    :gold_reserve_value  => 3417.4 * @gold_tonne_price)

    @approx_by = 0.000001
  end
end

shared_examples_for 'An Aggregatable Class' do
  describe '#size' do
    it_should_behave_like 'count with no arguments'
  end

  describe '#count' do
    it_should_behave_like 'count with no arguments'

    describe 'with a property name' do
      it 'should count the results' do
        @dragons.count(:name).should == 2
      end

      it 'should count the results with conditions having operators' do
        @dragons.count(:name, :toes_on_claw.gt => 3).should == 1
      end

      it 'should count the results with raw conditions' do
        statement = 'is_fire_breathing = ?'
        @dragons.count(:name, :conditions => [ statement, false ]).should == 1
        @dragons.count(:name, :conditions => [ statement, true  ]).should == 1
      end
    end
  end

  describe '#min' do
    describe 'with no arguments' do
      it 'should raise an error' do
        lambda { @dragons.min }.should raise_error(ArgumentError)
      end
    end

    describe 'with a property name' do
      it 'should provide the lowest value of an Integer property' do
        @dragons.min(:toes_on_claw).should == 3
        @countries.min(:population).should == 82369548
      end

      it 'should provide the lowest value of a Float property' do
        @countries.min(:birth_rate).should be_kind_of(Float)
        @countries.min(:birth_rate).should >= 7.87 - @approx_by  # approx match
        @countries.min(:birth_rate).should <= 7.87 + @approx_by  # approx match
      end

      it 'should provide the lowest value of a BigDecimal property' do
        @countries.min(:gold_reserve_value).should be_kind_of(BigDecimal)
        @countries.min(:gold_reserve_value).should == BigDecimal('1217050983400.0')
      end

      it 'should provide the lowest value of a DateTime property' do
        @dragons.min(:birth_at).should be_kind_of(DateTime)
        @dragons.min(:birth_at).to_s.should == @birth_at.to_s
      end

      it 'should provide the lowest value of a Date property' do
        @dragons.min(:birth_on).should be_kind_of(Date)
        @dragons.min(:birth_on).to_s.should == @birth_on.to_s
      end

      it 'should provide the lowest value of a Time property' do
        @dragons.min(:birth_time).should be_kind_of(Time)
        @dragons.min(:birth_time).to_s.should == @birth_time.to_s
      end

      it 'should provide the lowest value when conditions provided' do
        @dragons.min(:toes_on_claw, :is_fire_breathing => true).should  == 4
        @dragons.min(:toes_on_claw, :is_fire_breathing => false).should == 3
      end
    end
  end

  describe '#max' do
    describe 'with no arguments' do
      it 'should raise an error' do
        lambda { @dragons.max }.should raise_error(ArgumentError)
      end
    end

    describe 'with a property name' do
      it 'should provide the highest value of an Integer property' do
        @dragons.max(:toes_on_claw).should == 5
        @countries.max(:population).should == 1330044605
      end

      it 'should provide the highest value of a Float property' do
        @countries.max(:birth_rate).should be_kind_of(Float)
        @countries.max(:birth_rate).should >= 20.04 - @approx_by  # approx match
        @countries.max(:birth_rate).should <= 20.04 + @approx_by  # approx match
      end

      it 'should provide the highest value of a BigDecimal property' do
        @countries.max(:gold_reserve_value).should == BigDecimal('22589877164500.0')
      end

      it 'should provide the highest value of a DateTime property' do
        @dragons.min(:birth_at).should be_kind_of(DateTime)
        @dragons.min(:birth_at).to_s.should == @birth_at.to_s
      end

      it 'should provide the highest value of a Date property' do
        @dragons.min(:birth_on).should be_kind_of(Date)
        @dragons.min(:birth_on).to_s.should == @birth_on.to_s
      end

      it 'should provide the highest value of a Time property' do
        @dragons.min(:birth_time).should be_kind_of(Time)
        @dragons.min(:birth_time).to_s.should == @birth_time.to_s
      end

      it 'should provide the highest value when conditions provided' do
        @dragons.max(:toes_on_claw, :is_fire_breathing => true).should  == 5
        @dragons.max(:toes_on_claw, :is_fire_breathing => false).should == 3
      end
    end
  end

  describe '#avg' do
    describe 'with no arguments' do
      it 'should raise an error' do
        lambda { @dragons.avg }.should raise_error(ArgumentError)
      end
    end

    describe 'with a property name' do
      it 'should provide the average value of an Integer property' do
        @dragons.avg(:toes_on_claw).should be_kind_of(Float)
        @dragons.avg(:toes_on_claw).should == 4.0
      end

      it 'should provide the average value of a Float property' do
        mean_birth_rate = (13.71 + 14.18 + 16.04 + 11.03 + 7.87 + 20.04 + 8.18) / 7
        @countries.avg(:birth_rate).should be_kind_of(Float)
        @countries.avg(:birth_rate).should >= mean_birth_rate - @approx_by  # approx match
        @countries.avg(:birth_rate).should <= mean_birth_rate + @approx_by  # approx match
      end

      it 'should provide the average value of a BigDecimal property' do
        mean_gold_reserve_value = ((600.0 + 8133.50 + 438.20 + 765.20 + 3417.40) * @gold_tonne_price) / 5
        @countries.avg(:gold_reserve_value).should be_kind_of(BigDecimal)
        @countries.avg(:gold_reserve_value).should == BigDecimal(mean_gold_reserve_value.to_s)
      end

      it 'should provide the average value when conditions provided' do
        @dragons.avg(:toes_on_claw, :is_fire_breathing => true).should  == 4.5
        @dragons.avg(:toes_on_claw, :is_fire_breathing => false).should == 3
      end
    end
  end

  describe '#sum' do
    describe 'with no arguments' do
      it 'should raise an error' do
        lambda { @dragons.sum }.should raise_error(ArgumentError)
      end
    end

    describe 'with a property name' do
      it 'should provide the sum of values for an Integer property' do
        @dragons.sum(:toes_on_claw).should == 12

        total_population = 1330044605 + 303824646 + 191908598 + 140702094 +
                           127288419 + 109955400 + 82369548
        @countries.sum(:population).should == total_population
      end

      it 'should provide the sum of values for a Float property' do
        total_tonnes = 600.0 + 8133.5 + 438.2 + 765.2 + 3417.4
        @countries.sum(:gold_reserve_tonnes).should be_kind_of(Float)
        @countries.sum(:gold_reserve_tonnes).should >= total_tonnes - @approx_by  # approx match
        @countries.sum(:gold_reserve_tonnes).should <= total_tonnes + @approx_by  # approx match
      end

      it 'should provide the sum of values for a BigDecimal property' do
        @countries.sum(:gold_reserve_value).should == BigDecimal('37090059214100.0')
      end

      it 'should provide the average value when conditions provided' do
        @dragons.sum(:toes_on_claw, :is_fire_breathing => true).should  == 9
        @dragons.sum(:toes_on_claw, :is_fire_breathing => false).should == 3
      end
    end
  end

  describe '#aggregate' do
    describe 'with no arguments' do
      it 'should raise an error' do
        lambda { @dragons.aggregate }.should raise_error(ArgumentError)
      end
    end

    describe 'with only aggregate fields specified' do
      it 'should provide aggregate results' do
        results = @dragons.aggregate(:all.count, :name.count, :toes_on_claw.min, :toes_on_claw.max, :toes_on_claw.avg, :toes_on_claw.sum)
        results.should == [ 3, 2, 3, 5, 4.0, 12 ]
      end
    end

    describe 'with aggregate fields and a property to group by' do
      it 'should provide aggregate results' do
        results = @dragons.aggregate(:all.count, :name.count, :toes_on_claw.min, :toes_on_claw.max, :toes_on_claw.avg, :toes_on_claw.sum, :is_fire_breathing)
        results.should == [ [ 1, 1, 3, 3, 3.0, 3, false ], [ 2, 1, 4, 5, 4.5, 9, true ] ]
      end
    end
  end

  describe 'query path issue' do
    it "should not break when a query path is specified" do
      dragon = @dragons.first(Dragon.knight.name => 'Chuck')
      dragon.name.should == 'George'
    end
  end
end

shared_examples_for 'count with no arguments' do
  it 'should count the results' do
    @dragons.count.should  == 3

    @countries.count.should == 7
  end

  it 'should count the results with conditions having operators' do
    @dragons.count(:toes_on_claw.gt => 3).should == 2

    @countries.count(:birth_rate.lt => 12).should == 3
    @countries.count(:population.gt => 1000000000).should == 1
    @countries.count(:population.gt => 2000000000).should == 0
    @countries.count(:population.lt => 10).should == 0
  end

  it 'should count the results with raw conditions' do
    dragon_statement = 'is_fire_breathing = ?'
    @dragons.count(:conditions => [ dragon_statement, false ]).should == 1
    @dragons.count(:conditions => [ dragon_statement, true  ]).should == 2
  end
end
