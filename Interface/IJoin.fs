namespace SkyVue.Interface

open SkyVue.Data

// The join service provides the facility to create new user accounts. For
// security reasons, this interface is optional and may not be supported by all
// systems running this software. Depending on the server, an alternate method
// may be in place for the creation of user accounts.
type IJoin =

    // People can join by supplying an identity and password along with their
    // name and e-mail for verification.
    abstract Join : identity:string -> password:string -> name:string -> mail:string -> User