require 'rake/tasklib'
class Object
  def heat(name, &blk)
    WixRake::HeatTask.new(name, &blk)
  end

  def candle(name, &blk)
    WixRake::CandleTask.new(name, &blk)
  end

  def light(hash, &blk)
    WixRake::LightTask.new(hash, &blk)
  end
end

module WixRake
  class Task < Rake::TaskLib
    def self.wix=(wix)
      @@wix = wix
    end
    attr_writer :wix
    def initialize(&blk)
      @wix = @@wix
      define &blk
    end
  end
  class HeatTask < Task
    attr_accessor :target
    attr_writer :root, :opts, :out
    def initialize(name, &blk)
      @name = name
      @opts = ''
      super(&blk)
    end

    def heat
      cmd = [File.join(@wix, "heat")]
      cmd << "dir" << File.join(@root, @target) 
      cmd << @opts
      cmd << "-out #{@out}"
      sh cmd.join(' ')
    end
    
    def define
      rule(@name) do |t|
        self.target = t.name.match(@name)[1]
        yield self if block_given?
        heat
      end
    end
  end

  class CandleTask < Task
    def self.global_ext=(var)
      @@global_ext = var 
    end
    attr_reader :ext
    def initialize(name, &blk)
      @name = name << ".wixobj"
      @target = @name.ext("wxs")
      @ext = @@global_ext || []
      @vars = {}
      super(&blk)
    end

    def vars=(hash)
      @vars = hash.merge(@vars)
    end

    def candle
      cmd = [File.join(@wix, "candle")]
      cmd << @target
      cmd << @ext.map {|ext| "-ext #{ext}"}.join(' ')
      cmd << @vars.map {|key,val| "-d#{key}=#{val}"}.join(" ")
      sh cmd.join(' ')
    end

    def define
      file(@name => [@target]) do
        yield self if block_given?
        candle
      end
    end
  end

  class LightTask < Task
    attr_writer :includes, :ext, :opts, :target, :files
    def initialize(hash, &blk)
      @hash = hash
      @includes = []
      @ext = []
      @opts = ""
      super(&blk)
    end

    def light
      cmd = [File.join(@wix, "light")]
      cmd << @includes.map {|b| "-b #{b}"}.join(" ")
      cmd << @ext.map {|ext| "-ext #{ext}"}.join(' ')
      cmd << "-cultures:en-us"
      cmd << @opts
      cmd << "-out #{@target}"
      cmd << @files.join(" ")
      sh cmd.join(' ')
    end

    def define
      rule(@hash) do |t|
        self.target = t
        yield self if block_given?
        light
      end
    end
  end
end
