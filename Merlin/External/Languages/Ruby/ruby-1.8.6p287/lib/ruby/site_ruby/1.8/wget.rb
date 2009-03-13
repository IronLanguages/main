## -*- Ruby -*-
## URLopen
## 1999 by yoshidam
##
## TODO: This module should be writen by Ruby instead of wget/lynx.

module WGET
  PARAM = {
    'wget' => nil,
    'opts' => nil,
    'http_proxy' => nil,
    'ftp_proxy' => nil
  }

  def open(url, *rest)
      raise TypeError.new("wrong argument type #{url.inspect}" +
                          " (expected String)") if url.class != String

    if url =~ /^\/|^\./ || (url !~ /^http:|^ftp:/ && FileTest.exist?(url))
      File::open(url, *rest)
    else
      ENV['http_proxy'] = PARAM['http_proxy'] if PARAM['http_proxy']
      ENV['ftp_proxy'] = PARAM['ftp_proxy'] if PARAM['ftp_proxy']
      IO::popen(PARAM['wget'] + ' ' + PARAM['opts'] + ' ' + url)
    end
  end
  module_function :open
end

[ '/usr/local/bin/wget', '/usr/bin/wget',
  '/usr/local/bin/lynx', '/usr/bin/lynx',
  '/usr/local/bin/lwp-request', '/usr/bin/lwp-request' ].each do |p|
  if FileTest.executable?(p)
    WGET::PARAM['wget'] = p
    case p
    when /wget$/
      WGET::PARAM['opts'] = '-q -O -'
    when /lynx$/
      WGET::PARAM['opts'] = '-source'
    when /lwp-request$/
      WGET::PARAM['opts'] = '-m GET'
    end
    break
  end
end

raise "wget not found" if !WGET::PARAM['wget']
