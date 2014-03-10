namespace SkyVue.Interface

open SkyVue.Data

// The logon service provides authentication and access control facilities.
// Users intending to use the system will need to logon first to acquire a
// token.
type ILogon =

    // Verify a user's credentials with the system, but do not change the logon
    // or logoff dates. Access dates continue to be updated.
    abstract Authenticate : identity:string -> password:string -> string
    
    // Change the user's password by verifying the old password and setting the
    // new one. The user must be logged in to do this.
    abstract ChangePassword : token:string -> oldPassword:string -> newPassword:string ->  User

    // Log a user into the system to use its services. This will set various
    // dates used to determine if content is new. External programs may prefer
    // to use authenticate() if they don't intend to use the full array of
    // system services.
    abstract Logon : identity:string -> password:string -> string

    // Verify an existing authentication token by returning the user it applies
    // to.
    abstract Verify : token:string -> User