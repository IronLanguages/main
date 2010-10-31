dlr_root = ENV["DLR_ROOT"]
X = File.dirname(__FILE__) + "\\X"

sr = ENV["SystemRoot"]
cm = ENV["COMSPEC"]
ENV.clear
ENV["SystemRoot"] = sr
ENV["PATH"] = "#{dlr_root}\\bin\\debug"
ENV["COMSPEC"] = X + "\\inspect.exe"

puts '-1-'
p `x/a b/x`
p `"x/a" b/x`
puts '-2-'
p `"x/a b/x"`
puts '-3-'
p `ir -e "abort('hello')"`
puts '-4-'
p `ir -e "abort('hello>')"`
puts '-5-'
p `ir -e "abort('hello') > 0"`
puts '-6-'
p `ir -e "abort('hello')" >'`
puts '-7-'
# if the command contains unquoted (single or double) < | > it is executed using comspec
# only if it is not executed via comspec we need to search for exe
p `ir -e abort() >`
p `ir -e abort() >'`
p `ir -e abort() ''>''`
p `ir -e abort(') ''>''`
p `ir -e abort() '>'`
p `ir -e abort() ''>`
p `ir -e abort() '>`
p `ir -e abort() "''> "`
puts '-'
p `ir -e abort() |`
p `ir -e abort() <`
p `ir -e abort() ?`
p `ir -e abort() *`
p `ir -e abort() &`
p `ir -e abort() %`
p `ir -e abort() #`
puts '-'
p `ir -e abort() ~`
p `ir -e abort() !`
p `ir -e abort() @`
p `ir -e abort() ^`
p `ir -e abort() -`
p `ir -e abort() =`
puts '-'
p `ir -e abort() +`
p `ir -e abort() .`
p `ir -e abort() \``
p `ir -e abort() (`
p `ir -e abort() )`
p `ir -e abort() '>' > '>' '>'`
p `foo < bar baz`
puts '-8-'
p `ir -e "abort('hello')" '>'`
puts '-9-'
p `ir -e "abort('hello')" '>' '>`
puts '-a-'
p `ir -e "abort('hello')" >`
puts '-b-'
p `CD` rescue p $!
puts '-c-'
p `MKLINK` rescue p $!
puts '-d-'
ENV["PATH"] = X
p `attrib /?`
puts '-e-'

# copy zzz.exe to c:\windows and a different one to c:\windows\system32
#ENV["PATH"] = ""
#p `zzz /?`
