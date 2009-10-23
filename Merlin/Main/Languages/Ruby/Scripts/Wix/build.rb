class WixBuilder
  def initialize
    @start_dir = ARGV[0]
    @wix_loc = File.join(ENV["MERLIN_ROOT"], "External", "Wix")
    @msi_file = ARGV[1] || "IronRuby.msi"
    @rowan_bin = ENV['ROWAN_BIN'] || ARGV[2] || File.join(ENV["MERLIN_ROOT"], "bin", "release")
    @lib = File.join(@st)
  end

  def self.run
    x = new
    x.tallow_lib
    x.tallow_samples
    x.transform_lib
    x.transform_samples
  end

  def tallow(src, dest)
    cmd =  "#{@wix_loc}\\tallow -nologo -d #{src} > #{dest}"
    system cmd
    unless $?.success
      puts "#{cmd} failed"
      exit 1
    end
  end
  private :tallow

  def tallow_lib
    tallow("#{@start_dir}\\lib", "LibTemp.wxs")
  end

  def tallow_samples
    tallow("#{@start_dir}\\samples", "SamplesTemp.wxs")
  end
end

