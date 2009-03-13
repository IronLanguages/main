# -*- ruby -*-

module Autotest::Heckle
  @@flags  = []
  @@klasses = []
  def self.flags; @@flags; end
  def self.klasses; @@klasses; end

  Autotest.add_hook :all_good do |at|
    heckle = "heckle" + (@@flags.empty? ? '' : " #{flags.join(' ')}")
    cmd = @@klasses.map { |klass| "#{heckle} #{klass}" }.join(" && ")
    system cmd
  end
end
