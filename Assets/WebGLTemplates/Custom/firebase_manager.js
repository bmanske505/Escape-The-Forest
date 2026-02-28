import { initializeApp } from 'https://www.gstatic.com/firebasejs/12.9.0/firebase-app.js'

// Add Firebase products that you want to use
import { getAuth } from 'https://www.gstatic.com/firebasejs/12.9.0/firebase-auth.js'
import { getFirestore, collection, addDoc } from 'https://www.gstatic.com/firebasejs/12.9.0/firebase-firestore.js'

// Your web app's Firebase configuration
const firebaseConfig = {
    apiKey: "AIzaSyAwCZkNPd5wM3EJvOVVU4r_OYHx0qzSHoQ",
    authDomain: "escape-the-forest-8fe9b.firebaseapp.com",
    projectId: "escape-the-forest-8fe9b",
    storageBucket: "escape-the-forest-8fe9b.firebasestorage.app",
    messagingSenderId: "437342000393",
    appId: "1:437342000393:web:d231ad8bd6400bba702fd8",
    measurementId: "G-5CRCW35DMV",
}

// Initialize Firebase
const app = initializeApp(firebaseConfig);

// get Cloud Firestore instance
const db = getFirestore(app);
const auth = getAuth(app);

window.firebaseManager = {
    LogDocumentToFirestore: async function (collectionName, jsonData) {
        const data = JSON.parse(jsonData);
        await addDoc(collection(db, collectionName), {
					...data,
					timestamp: Date.now(), // ms since epoch
				});
    }
};