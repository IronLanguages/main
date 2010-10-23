var BROWSERS = [
  'explorer',
  'firefox',
  'chrome',
  'safari',
  'opera'
]

function reportTestResults(failures, total) {
  $.get("http://localhost:9090/complete", 
       getTestResults(failures, total), 
       function (data) { }, 'jsonp'
  );
}

function getTestResults(failures, total) {
  var results = { failures: failures, total: total };
  return {
    results: JSON.stringify(results),
    url: document.URL,
    output: getOutput(),
    browser: getBrowser(),
    pass: String(hasTestsPassed(results))
  };
}

function hasTestsPassed(results) {
  return results.failures == 0 && results.total > 0;
}

function getBrowser() {
  index = getBrowserIndex();
  if (index == NaN)
    return null;
  return BROWSERS[index];
}

function getBrowserIndex() {
  index = NaN;
  ua = navigator.userAgent;
  if (ua.match(/MSIE/)) index = 0;
  else if (ua.match(/Firefox/)) index = 1;
  else if (ua.match(/Chrome/)) index = 2;
  else if (ua.match(/Safari/)) index = 3;
  else if (ua.match(/Opera/)) index = 4;
  return index;
}

function getOutput() {
  // only get HTML in non IE browsers, as it needs to be sent over a GET HTTP
  // request, and only IE won't handle that.  
  if (getBrowserIndex() != 0) {
    return $('#tests').html();
  }
  return "";
}

/* 
 * waitForTrue(condition, onTrue, onFalse, timeout)
 *
 * polls every <timeout> milliseconds for the <condition> to be true. Runs 
 * <onTrue> if it is true within 20 iterations, otherwise runs <onFalse>
 */
var waitForTrue = function(condition, onTrue, onFalse, timeout) {
  stop()
  iterations = 20
  count = 0
  timeout = typeof timeout == 'undefined' ? 10 : timeout
  var doo = function() {
    count++;
    if(count < iterations) {
      if(!condition()) {
        setTimeout(doo, timeout)
      } else {
        start()
        onTrue()
      }
    } else {
      start()
      onFalse()
    }
  }
  setTimeout(doo, timeout)
}

function script_tag(attrs) {
  $(document.createElement('script')).attr(attrs).appendTo($(document.body))
}

var currentID = null;

function execute(id, script_tags) {
  currentID = id;
  for(i = 0; i < script_tags.length; i++) {
    opts = {'class': id};
    $.extend(opts, script_tags[i]);
    script_tag(opts);
  }
  DLR.createSilverlightObject({'id': id, xamlid: id});
}

function test_dlr_exception(attrs) {
  waitForTrue(function() {
    return ($('#' + currentID).length > 0) && ($('#silverlightDlrWindow').length > 0)
  }, function() {
    dlr_exception_window(attrs);
    $('#silverlightDlrWindowContainer').remove();
    currentID = null;
  }, function() {
    ok(false, 'Silverlight control failed to load')
  }, 500);
}

if(typeof(String.prototype.trim) === "undefined") {
  /*
   * String.trim()
   *
   * Removes the whitespace from the beginning and end of a string
   */
  String.prototype.trim = function() {
    return String(this).replace(/^\s+|\s+$/g, '');
  };
}

/*
 * dlr_exception_window(opts)
 *
 * Asserts that a DLR exception window has been created with the various
 * properties in <opts>:
 * - name: The exception name
 * - message: the exception message
 * - file: the file that threw the exception
 * - line: the line the exception was thrown on
 * - stack: the dynamic stacktrace for the exception
 */
function dlr_exception_window(opts) {
  __dlr_verify_error_message(opts);
  __dlr_verify_error_type(opts);
  __dlr_verify_error_file(opts);
  __dlr_verify_error_line(opts);
  __dlr_verify_error_stack(opts);
}

/*
 * htmlEncode(value)
 *
 * Encodes the <value> as HTML entities
 */
function htmlEncode(value) {
  return $('<div/>').text(value).html();
}

/*
 * htmlDecode(value)
 *
 * Decodes the <value> as plain text
 */
function htmlDecode(value) {
  return $('<div/>').html(value).text();
}

/*
 * stringToArray(str)
 *
 * Converts a HTML string to an array, splitting on break-row tags and
 * trimming each line's whitespace
 */
function stringToArray(str) {
  lines = str.split(/<br>/gi)
  for(i = 0; i < lines.length; i++) {
    lines[i] = lines[i].trim();
  }
  return lines;
}

/*
 * cmpstr_nospace(s1, s2)
 *
 * Compares two strings, ignoring whitespace.
 *
 * BUG: For some reason the error message string's white-space is not comparing
 * correctly ...  e = '012 45'; e[3] == ' ' # => false.
 */
function cmpstr_nospace(s1, s2) {
  if(s1.length != s2.length) return false;
  for(i = 0; i < s1.length; i++) {
    if(s1[i] != s2[i] && (s1[i] != ' ' && s2[i] != ' '))
      return false;
  }
  return true;
}

/*
 * Private assertions
 */

function __dlr_verify_error_message(opts) {
  if (opts.message) {
    actual = __dlr_get_error_message();
    ok(cmpstr_nospace(actual, opts.message), 'Error message: ' + actual);
  }
}

function __dlr_verify_error_type(opts) {
  if (opts.type) {
    equals(__dlr_get_error_type(), opts.type, 'Error type');
  }
}

function __dlr_verify_error_file(opts) {
  if (opts.file) {
    equals(__dlr_get_error_file(), opts.file, 'Error file');
  }
}

function __dlr_verify_error_line(opts) {
  if (opts.line) {
    equals(__dlr_get_error_line(), opts.line, 'Error line');
  }
}

function __dlr_verify_error_stack(opts) {
  if (opts.stack) {
    actual = __dlr_get_error_stack();
    expected = opts.stack;
    equals(actual.length, expected.length, 'Error stack length');
    for (i = 0; i < expected.length; i++) {
      equals(actual[i], expected[i], 'Error stack');
    }
  }
}

// HTML ID constants
var _htmlErrorReport = "#silverlightDlrErrorReport"
var _htmlErrorMessage = "#silverlightDlrErrorMessage"
var _htmlErrorSourceFile = "#silverlightDlrErrorSourceFile"
var _htmlErrorSourceCode = "#silverlightDlrErrorSourceCode"
var _htmlErrorSourceLine = "#silverlightDlrErrorLine"
var _htmlErrorType = "#silverlightDlrErrorType"
var _htmlErrorStackTrace = "#silverlightDlrErrorStackTrace"

// get the number of errors reported
function __dlr_get_error_count() {
  return $(_htmlErrorReport).length;
}

// look for error message of the exception
function __dlr_get_error_message() {
  return $(_htmlErrorMessage).text();
}

// look for error type
function __dlr_get_error_type() {
  return $(_htmlErrorType).text();
}

// look for error source file
function __dlr_get_error_file() {
  return $(_htmlErrorSourceFile).text();
}

// return error lines
function __dlr_get_error_lines() {
  error_lines = $(_htmlErrorSourceCode).html();
  if (error_lines) {
    return stringToArray(error_lines);
  }
  return [];
}

// look for the highlighted line in error stack
function __dlr_get_error_line() {
  lines = __dlr_get_error_lines();
  for(i = 0; i < lines.length; i++) {
    if (lines[i].indexOf(_htmlErrorSourceLine.slice(1)) != -1) {
      return lines[i].split(':')[0].split('Line ')[1];
    }
  }
  return 0;
}

// get the stack trace
function __dlr_get_error_stack() {
  var trace = $(_htmlErrorStackTrace).html();
  if (trace) {
    trace = stringToArray(trace);
    for(i = 0; i < trace.length; i++) {
      trace[i] = htmlDecode(trace[i]);
    }
    return trace;
  }
  return [];
}

