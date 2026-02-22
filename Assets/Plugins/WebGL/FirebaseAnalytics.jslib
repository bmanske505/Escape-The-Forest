mergeInto(LibraryManager.library, {
  
  SetUserId: function (idPtr) {
    const id = UTF8ToString(idPtr);
    firebase.analytics().setUserId(id);
  }

	SetUserProperties: function (properties) {
		var props = JSON.parse(UTF8ToString(properties));
		console.log("FirebaseAnalytics: SetUserProperties called", props);
		firebase.analytics().setUserProperties(props);
	},

	LogEvent: function (eventName) {
		var event_name = UTF8ToString(eventName);
		console.log("FirebaseAnalytics: LogEvent called", event_name);
		firebase.analytics().logEvent(event_name);
	},

	LogEventParameter: function (eventName, eventParameter) {
		var event_name = UTF8ToString(eventName);
		var event_param = JSON.parse(UTF8ToString(eventParameter));
		console.log("FirebaseAnalytics: LogEventParameter called", event_name, event_param);
		firebase.analytics().logEvent(event_name, event_param);
	},
});
