module FileStore

open System.IO

open Thoth.Json.Net

open Shared

type FileStoreMsg =
    | Get of AsyncReplyChannel<Todo list>
    | Apply of Event * AsyncReplyChannel<Todo list>

type FileStore () =
    let fileName = "filestore.json"
    let getTodos () =
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
                | Apply (event, channel) ->
                    let todos = getTodos()
                    let todos' = Todos.apply event todos
                    saveTodos todos'
                    channel.Reply todos'
                    return! loop ()
            }
        loop ())

    member __.GetTodos() = mb.PostAndReply Get
    member __.Apply(event) = mb.PostAndReply (fun ch -> Apply(event, ch))
