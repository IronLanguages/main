var waitForTrue = function(condition, onTrue, onFalse) {
  stop()
  iterations = 10
  count = 0
  timeout = 10
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

var waitForElement = function(element, onFound, notFound) {
  waitForTrue(function() { return $(element).length > 0 }, onFound, notFound);
}
