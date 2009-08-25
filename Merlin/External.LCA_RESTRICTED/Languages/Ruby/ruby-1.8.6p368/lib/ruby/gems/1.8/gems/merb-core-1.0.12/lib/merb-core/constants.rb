# Most of this list is simply constants frozen for efficiency
# and lowered memory consumption. Every time Ruby VM comes
# across a string or a number or a regexp literal,
# new object is created.
#
# This means if you refer to the same string 6 times per request
# and your application takes 100 requests per second, there are
# 600 objects for weak MRI garbage collector to work on.
#
# GC cycles take up to 80% (!) time of request processing in
# some cases. Eventually Rubinius and maybe MRI 2.0 gonna
# improve this situation but at the moment, all commonly used
# strings, regexp and numbers used as constants so no extra
# objects created and VM just operates pointers.
module Merb
  module Const

    DEFAULT_SEND_FILE_OPTIONS = {
      :type         => 'application/octet-stream'.freeze,
      :disposition  => 'attachment'.freeze
    }.freeze

    RACK_INPUT               = 'rack.input'.freeze
    SET_COOKIE               = " %s=%s; path=/; expires=%s".freeze
    COOKIE_EXPIRATION_FORMAT = "%a, %d-%b-%Y %H:%M:%S GMT".freeze
    COOKIE_SPLIT             = /[;,] */n.freeze
    COOKIE_REGEXP            = /\s*(.+)=(.*)\s*/.freeze
    COOKIE_EXPIRED_TIME      = Time.at(0).freeze
    ACCEPT_SPLIT             = /,/.freeze
    SLASH_SPLIT              = %r{/}.freeze
    MEDIA_RANGE              = /\s*([^;\s]*)\s*(;\s*q=\s*(.*))?/.freeze
    HOUR                     = 60 * 60
    DAY                      = HOUR * 24
    WEEK                     = DAY * 7
    MULTIPART_REGEXP         = /\Amultipart\/form-data.*boundary=\"?([^\";,]+)/n.freeze
    HTTP_COOKIE              = 'HTTP_COOKIE'.freeze
    QUERY_STRING             = 'QUERY_STRING'.freeze
    JSON_MIME_TYPE_REGEXP    = %r{^application/json|^text/x-json}.freeze
    XML_MIME_TYPE_REGEXP     = %r{^application/xml|^text/xml}.freeze
    FORM_URL_ENCODED_REGEXP  = %r{^application/x-www-form-urlencoded}.freeze
    LOCAL_IP_REGEXP          = /^unknown$|^(127|10|172\.16|192\.168)\.|^(172\.(1[6-9]|2[0-9]|3[0-1]))\.|^(169\.254)\./i.freeze
    XML_HTTP_REQUEST_REGEXP  = /XMLHttpRequest/i.freeze
    UPCASE_CONTENT_TYPE      = 'CONTENT_TYPE'.freeze
    CONTENT_TYPE             = "Content-Type".freeze
    DATE                     = 'Date'.freeze
    UPCASE_HTTPS             = 'HTTPS'.freeze
    HTTPS                    = 'https'.freeze
    HTTP                     = 'http'.freeze
    ETAG                     = 'ETag'.freeze
    LAST_MODIFIED            = "Last-Modified".freeze
    GET                      = "GET".freeze
    POST                     = "POST".freeze
    HEAD                     = "HEAD".freeze
    CONTENT_LENGTH           = "CONTENT_LENGTH".freeze
    HTTP_CLIENT_IP           = "HTTP_CLIENT_IP".freeze
    HTTP_X_REQUESTED_WITH    = "HTTP_X_REQUESTED_WITH".freeze
    HTTP_X_FORWARDED_FOR     = "HTTP_X_FORWARDED_FOR".freeze
    HTTP_X_FORWARDED_PROTO   = "HTTP_X_FORWARDED_PROTO".freeze
    HTTP_X_FORWARDED_HOST    = "HTTP_X_FORWARDED_HOST".freeze
    HTTP_IF_MODIFIED_SINCE   = "HTTP_IF_MODIFIED_SINCE".freeze
    HTTP_IF_NONE_MATCH       = "HTTP_IF_NONE_MATCH".freeze
    HTTP_CONTENT_TYPE        = "HTTP_CONTENT_TYPE".freeze
    HTTP_CONTENT_LENGTH      = "HTTP_CONTENT_LENGTH".freeze
    HTTP_REFERER             = "HTTP_REFERER".freeze
    HTTP_USER_AGENT          = "HTTP_USER_AGENT".freeze
    HTTP_HOST                = "HTTP_HOST".freeze
    HTTP_CONNECTION          = "HTTP_CONNECTION".freeze
    HTTP_KEEP_ALIVE          = "HTTP_KEEP_ALIVE".freeze
    HTTP_ACCEPT              = "HTTP_ACCEPT".freeze
    HTTP_ACCEPT_ENCODING     = "HTTP_ACCEPT_ENCODING".freeze
    HTTP_ACCEPT_LANGUAGE     = "HTTP_ACCEPT_LANGUAGE".freeze
    HTTP_ACCEPT_CHARSET      = "HTTP_ACCEPT_CHARSET".freeze
    HTTP_CACHE_CONTROL       = "HTTP_CACHE_CONTROL".freeze
    UPLOAD_ID                = "upload_id".freeze
    PATH_INFO                = "PATH_INFO".freeze
    HTTP_VERSION             = "HTTP_VERSION".freeze
    GATEWAY_INTERFACE        = "GATEWAY_INTERFACE".freeze
    SCRIPT_NAME              = "SCRIPT_NAME".freeze
    SERVER_NAME              = "SERVER_NAME".freeze
    SERVER_SOFTWARE          = "SERVER_SOFTWARE".freeze
    SERVER_PROTOCOL          = "SERVER_PROTOCOL".freeze
    SERVER_PORT              = "SERVER_PORT".freeze
    REQUEST_URI              = "REQUEST_URI".freeze
    REQUEST_PATH             = "REQUEST_PATH".freeze
    REQUEST_METHOD           = "REQUEST_METHOD".freeze
    REMOTE_ADDR              = "REMOTE_ADDR".freeze
    BREAK_TAG                = "<br/>".freeze
    EMPTY_STRING             = "".freeze
    NEWLINE                  = "\n".freeze
    SLASH                    = "/".freeze
    DOT                      = ".".freeze
    QUESTION_MARK            = "?".freeze
    DOUBLE_NEWLINE           = "\n\n".freeze
    LOCATION                 = "Location".freeze
    TEXT_SLASH_HTML          = "text/html".freeze

    WIN_PLATFORM_REGEXP      = /(:?mswin|mingw)/.freeze
    JAVA_PLATFORM_REGEXP     = /java/.freeze
  end
end
