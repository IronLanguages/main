# encoding: utf-8

f = File.new("a.txt", "r")


class C
  def method_missing name
    puts name
    raise NoMethodError
  end
end

#1: to_io, to_path, to_str
#2: to_str

f.reopen(C.new) rescue p $!
f.reopen("b.txt", C.new) rescue p $!
f.reopen("b.txt", encoding: "utf-8") rescue p $!
f.reopen("xxxx.txt\x80", "r:utf-8") rescue p $!