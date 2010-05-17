var waitForTrue = function(condition, onTrue, onFalse) {
  stop()
  iterations = 20
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
