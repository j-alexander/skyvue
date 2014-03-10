namespace SkyVue.Data

type Permission = Add | Delete | View

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Permission =
    
    let reduce (permission : Permission) (value : int16) (set : Set<Permission>) =
        if DataStore.yes = value then Set.remove permission set else set

    let augment (permission : Permission) (value : int16) (set : Set<Permission>) =
        if DataStore.yes = value then Set.add permission set else set