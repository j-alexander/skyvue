namespace SkyVue.Interface

open System
open SkyVue.Data

// The users service provides directory information for locating, listing and
// retrieving user accounts.
type IUsers =

    // List all users, with a page size, sorting by identity asc.
    abstract List : size:int -> page:int -> List<User>

    // List all users since the date specified, with a page size, sorting by
    // identity asc.
    abstract ListSince : size:int -> page:int -> since:DateTime -> List<User>

    // Find and page any users by name - sort results by name asc.
    abstract FindByName : name:string -> size:int -> page:int -> List<User>

    // Find and page any users accessing the system since the date specified -
    // sort results ascending by name.
    abstract FindByNameSince : name:string -> size:int -> page:int -> since:DateTime -> List<User>

    // Find and page a user by identity - sort results by identity asc.
    abstract FindByIdentity : identity:string -> size:int -> page:int -> List<User>

    // Find and page any users accessing the system since the date specified -
    // sort results ascending by identity.
    abstract FindByIdentitySince : identity:string -> size:int -> page:int -> since:DateTime -> List<User>

    // View a specific user based on their user Id.
    abstract View : userId:string -> User