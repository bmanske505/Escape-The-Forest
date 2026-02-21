mergeInto(LibraryManager.library, {
    SetUserProperties: function(properties) {
        var props = JSON.parse(UTF8ToString(properties));
		
        firebase.analytics().setUserProperties(props);
    },
	
    LogEvent: function(eventName) {
        var event_name = UTF8ToString(eventName);
		
        firebase.analytics().logEvent(event_name);
    },
	
    LogEventParameter: function(eventName, eventParameter) {
        var event_name = UTF8ToString(eventName);
        var event_param = UTF8ToString(eventParameter);
		
        firebase.analytics().logEvent(event_name, event_param);
    }
});