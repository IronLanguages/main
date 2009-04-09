# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{windows-pr}
  s.version = "0.9.3"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel J. Berger", "Park Heesob"]
  s.date = %q{2008-09-12}
  s.description = %q{Windows functions and constants bundled via Win32::API}
  s.email = %q{djberg96@gmail.com}
  s.extra_rdoc_files = ["MANIFEST", "README", "CHANGES"]
  s.files = ["doc/conversion_guide.txt", "lib/windows/clipboard.rb", "lib/windows/com/automation.rb", "lib/windows/com.rb", "lib/windows/console.rb", "lib/windows/debug.rb", "lib/windows/device_io.rb", "lib/windows/directory.rb", "lib/windows/error.rb", "lib/windows/eventlog.rb", "lib/windows/file.rb", "lib/windows/filesystem.rb", "lib/windows/file_mapping.rb", "lib/windows/gdi/bitmap.rb", "lib/windows/gdi/device_context.rb", "lib/windows/gdi/painting_drawing.rb", "lib/windows/handle.rb", "lib/windows/library.rb", "lib/windows/limits.rb", "lib/windows/memory.rb", "lib/windows/msvcrt/buffer.rb", "lib/windows/msvcrt/directory.rb", "lib/windows/msvcrt/file.rb", "lib/windows/msvcrt/io.rb", "lib/windows/msvcrt/string.rb", "lib/windows/msvcrt/time.rb", "lib/windows/national.rb", "lib/windows/network/management.rb", "lib/windows/network/snmp.rb", "lib/windows/network/winsock.rb", "lib/windows/nio.rb", "lib/windows/ntfs/file.rb", "lib/windows/path.rb", "lib/windows/pipe.rb", "lib/windows/process.rb", "lib/windows/registry.rb", "lib/windows/security.rb", "lib/windows/service.rb", "lib/windows/shell.rb", "lib/windows/sound.rb", "lib/windows/synchronize.rb", "lib/windows/system_info.rb", "lib/windows/thread.rb", "lib/windows/time.rb", "lib/windows/tool_helper.rb", "lib/windows/unicode.rb", "lib/windows/volume.rb", "lib/windows/window/dialog.rb", "lib/windows/window/message.rb", "lib/windows/window/properties.rb", "lib/windows/window/timer.rb", "lib/windows/window.rb", "test/tc_clipboard.rb", "test/tc_com.rb", "test/tc_console.rb", "test/tc_debug.rb", "test/tc_device_io.rb", "test/tc_directory.rb", "test/tc_error.rb", "test/tc_eventlog.rb", "test/tc_file.rb", "test/tc_filesystem.rb", "test/tc_file_mapping.rb", "test/tc_handle.rb", "test/tc_library.rb", "test/tc_limits.rb", "test/tc_memory.rb", "test/tc_msvcrt_buffer.rb", "test/tc_msvcrt_directory.rb", "test/tc_msvcrt_file.rb", "test/tc_msvcrt_io.rb", "test/tc_msvcrt_string.rb", "test/tc_msvcrt_time.rb", "test/tc_national.rb", "test/tc_network_management.rb", "test/tc_network_snmp.rb", "test/tc_nio.rb", "test/tc_path.rb", "test/tc_pipe.rb", "test/tc_process.rb", "test/tc_registry.rb", "test/tc_security.rb", "test/tc_service.rb", "test/tc_shell.rb", "test/tc_sound.rb", "test/tc_synchronize.rb", "test/tc_system_info.rb", "test/tc_thread.rb", "test/tc_time.rb", "test/tc_tool_helper.rb", "test/tc_unicode.rb", "test/tc_volume.rb", "test/tc_window.rb", "CHANGES", "doc", "lib", "MANIFEST", "Rakefile", "README", "test", "windows-pr.gemspec"]
  s.has_rdoc = true
  s.homepage = %q{http://www.rubyforge.org/projects/win32utils}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{win32utils}
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{Windows functions and constants bundled via Win32::API}
  s.test_files = ["test/tc_clipboard.rb", "test/tc_com.rb", "test/tc_console.rb", "test/tc_debug.rb", "test/tc_device_io.rb", "test/tc_directory.rb", "test/tc_error.rb", "test/tc_eventlog.rb", "test/tc_file.rb", "test/tc_filesystem.rb", "test/tc_file_mapping.rb", "test/tc_handle.rb", "test/tc_library.rb", "test/tc_limits.rb", "test/tc_memory.rb", "test/tc_msvcrt_buffer.rb", "test/tc_msvcrt_directory.rb", "test/tc_msvcrt_file.rb", "test/tc_msvcrt_io.rb", "test/tc_msvcrt_string.rb", "test/tc_msvcrt_time.rb", "test/tc_national.rb", "test/tc_network_management.rb", "test/tc_network_snmp.rb", "test/tc_nio.rb", "test/tc_path.rb", "test/tc_pipe.rb", "test/tc_process.rb", "test/tc_registry.rb", "test/tc_security.rb", "test/tc_service.rb", "test/tc_shell.rb", "test/tc_sound.rb", "test/tc_synchronize.rb", "test/tc_system_info.rb", "test/tc_thread.rb", "test/tc_time.rb", "test/tc_tool_helper.rb", "test/tc_unicode.rb", "test/tc_volume.rb", "test/tc_window.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<windows-api>, [">= 0.2.4"])
      s.add_runtime_dependency(%q<win32-api>, [">= 1.2.0"])
    else
      s.add_dependency(%q<windows-api>, [">= 0.2.4"])
      s.add_dependency(%q<win32-api>, [">= 1.2.0"])
    end
  else
    s.add_dependency(%q<windows-api>, [">= 0.2.4"])
    s.add_dependency(%q<win32-api>, [">= 1.2.0"])
  end
end
