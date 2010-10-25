/*
 * stubs.js - A stubbing object for JavaScript to make unit testing and TDD less painful.
 * 
 * http://github.com/anglicangeek/stubs.js
 *
 * Copyright (c) 2010 Andrew Miller <ego@anglicangeek.com>
 * Licensed under the MIT (LICENSE.txt) license.
 */
 
(function() {
  var stubs = [];
  
  window.Stubs = {
  
    add: function(identifier, stub) {
      var real = eval(identifier);
      eval(identifier + " = stub");
      stubs.push({
        identifier: identifier,
        real: real
      });
    },
  
    removeAll: function() {
      for (var n = 0; n < stubs.length; n++) {
        var stub = stubs[n];
        eval(stub.identifier + " = stub.real");
      }
    }
  
  };
})();