namespace SkyVue.Error

// The user's token is not granted access to the protected channel - you must
// unlock the channel to access it. There is only one password for the channel
// and all subscribers will need to know it.
exception ProtectedChannel of message:string