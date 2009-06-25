require 'lib/eggs'

Eggs.config(:tests => %w(sample))
Eggs.run

include Microsoft::Scripting::Silverlight
include System::Windows::Controls
DynamicApplication.current.root_visual = Canvas.new
