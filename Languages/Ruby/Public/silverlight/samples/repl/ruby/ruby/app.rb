include Microsoft::Scripting::Silverlight
include System::Windows::Browser

repl, replDiv = Repl.create
HtmlPage.document.body.append_child replDiv
repl.start
$stdout = repl.output_buffer
$stderr = repl.output_buffer
