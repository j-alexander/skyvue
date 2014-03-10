namespace SkyVue.Test

open SkyVue.Data
open System.Data.SqlClient
open System.IO

module Database =

    let path = "Server=localhost\\SQLEXPRESS;Integrated security=SSPI"

    let delete() =
        use connection = new SqlConnection(path)
        connection.Open()

        let instruction =
            "IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'skyvuetest') " +
            "BEGIN " +
            "   ALTER DATABASE skyvuetest SET SINGLE_USER WITH ROLLBACK IMMEDIATE " +
            "   DROP DATABASE skyvuetest "+
            "END "
        use command = new SqlCommand(instruction, connection)
        let result = command.ExecuteNonQuery()
        ()

    let create() =
        delete()

        use connection = new SqlConnection(path)
        connection.Open()

        // create the database
        use command = new SqlCommand("CREATE DATABASE skyvuetest", connection)
        let result = command.ExecuteNonQuery()

        // use the database
        use command = new SqlCommand("USE skyvuetest", connection)
        let result = command.ExecuteNonQuery()

        // create the tables
        use stream = typeof<Schema>.Assembly.GetManifestResourceStream "Schema.sql"
        use reader = new StreamReader(stream)
        use command = new SqlCommand(reader.ReadToEnd(), connection)
        let result = command.ExecuteNonQuery()
        ()

    let connect() =
        Schema.GetDataContext(path + ";Initial Catalog=skyvuetest")