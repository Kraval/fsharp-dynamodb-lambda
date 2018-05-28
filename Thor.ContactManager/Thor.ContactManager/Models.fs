module Models

open Amazon.DynamoDBv2.DataModel
open System.Collections
open System.Collections.Generic

type Address() =
   
    [<DefaultValue>]
    val mutable City : string

    [<DefaultValue>]
    val mutable State : string

    [<DefaultValue>]
    val mutable Street : string

    [<DefaultValue>]
    val mutable ZipCode : string

type Phone() =

    [<DefaultValue>]
    val mutable Type : string

    [<DefaultValue>]
    val mutable Number : string

type THContact() =

    let Phones = new Dictionary<string,string>()
    //let SecondaryEmails = Array.create

    [<DefaultValue>]
    val mutable ContactID : string

    [<DefaultValue>]
    val mutable FirstName : string

    [<DefaultValue>]
    val mutable LastName : string

    [<DefaultValue>]
    val mutable PrimaryEmail : string

    [<DefaultValue>]
    val mutable Address : Address
   
    [<DefaultValue>]
    val mutable Phones : Dictionary<string,string>
    
    [<DefaultValue>]
    val mutable SecondaryEmails : List<string>


type Error(message:string) =
    member this.Message = message

type Result(message:string) =
    member this.Message = message

