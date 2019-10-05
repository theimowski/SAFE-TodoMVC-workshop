module FileStore

open System.IO

open Thoth.Json.Net

open Shared

type FileStoreMsg =
    | Get of AsyncReplyChannel<Todo list>
    | Save of Todo list

type FileStore () =
    let fileName = "filestore.json"
    let getTodos () : Todo list =
        try
            let rawJSON = File.ReadAllText(fileName)
            Decode.Auto.unsafeFromString(rawJSON)
        with _ -> []

    let saveTodos (todos: Todo list) =
        let rawJSON = Encode.Auto.toString(4, todos)
        File.WriteAllText(fileName, rawJSON)

    let mb = MailboxProcessor.Start(fun mb ->
        let rec loop () =
            async {
                let! msg = mb.Receive()
                match msg with
                | Get channel ->
                    let todos = getTodos()
                    channel.Reply todos
                    return! loop ()
                | Save todos ->
                    saveTodos todos
                    return! loop ()
            }
        loop ())

    member __.GetTodos() = mb.PostAndReply Get
    member __.SaveTodos(todos) = mb.Post (Save todos)
