using System.Runtime.InteropServices;

namespace Scripts.FirebaseScripts
{
    public static class FirebaseAuth
    {
        /// <summary>
        /// Creates and signs in a user anonymous
        /// </summary>
        /// <param name="objectName"> Name of the gameobject to call the callback/fallback of </param>
        /// <param name="callback"> Name of the method to call when the operation was successful. Method must have signature: void Method(string output) </param>
        /// <param name="fallback"> Name of the method to call when the operation was unsuccessful. Method must have signature: void Method(string output). Will return a serialized FirebaseError object </param>
        [DllImport("__Internal")]
        public static extern void SignInAnonymously(string objectName, string callback, string fallback);
    }
}