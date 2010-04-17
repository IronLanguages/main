param(
  [string] $old,
  [string] $new
)

$config = "$env:MERLIN_ROOT\Config"
$files = @("$config\Signed\App.config", "$config\Unsigned\App.config", "$env:MERLIN_ROOT\Languages\Ruby\Ruby\Runtime\RubyContext.cs", "$env:MERLIN_ROOT\Languages\Ruby\Scripts\Wix\IronRuby.wxs", "$env:MERLIN_ROOT\Languages\Ruby\Scripts\Wix\config.wxi", "$env:MERLIN_ROOT\Runtime\Tests\HostingTest\LangSetup.cs")
$cold = $old -replace "\.", ", "
$cnew = $new -replace "\.", ", "
$files | foreach {
  tf edit $_
  (get-content $_) | foreach-object {$_ -replace $old, $new} | set-content $_
  (get-content $_) | foreach-object {$_ -replace $cold, $cnew} | set-content $_
}
