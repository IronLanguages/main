class TestFile
  
  DATASIZE = 100*1024

  def self.filename
    root = Dir.pwd
    file = "test"
    $i ||= 0
    $i += 1
    f = File.join(root,file + $i.to_s)
    File.delete(f) if File.exists?(f)
    f
  end

  def self.data
    s = ""
    DATASIZE.times { s << rand(255).to_i }
    s
  end
end
#setup
def setup
  data = {}
  10.times { data[TestFile.filename] = TestFile.data }
  data
end

def test(name)
  data = setup
  p name
  start = Time.now
  data.each {|k,v| File.open(k,name) {|file| file.write v}}
  ending = Time.now - start
  p ending
end

test("wb") 
test("w") 

