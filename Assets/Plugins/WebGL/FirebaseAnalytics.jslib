mergeInto(LibraryManager.library, {
	GetHostDomain: function () {
		var host = window.location.hostname || "unknown";
		console.log("FirebaseAnalytics: GetHostDomain called", host);
		return stringToNewUTF8(host);
	},

  LogDocumentToFirebase: function (collectionNamePtr, jsonDataPtr) {
      const collectionName = UTF8ToString(collectionNamePtr);
      const jsonData = UTF8ToString(jsonDataPtr);
      window.firebaseManager.LogDocumentToFirestore(collectionName, jsonData);
  },
  
});
