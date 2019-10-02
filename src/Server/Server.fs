open System
open System.IO

open FSharp.Control.Tasks.V2
open Giraffe
open Saturn

open Shared

let store = FileStore.FileStore()

let execute (command: Command) next ctx =
    task {
        let todos = store.GetTodos()
        match Todos.handle command todos with
        | Ok event ->
            let todos' = store.Apply event
            return! json todos' next ctx
        | Error TodoIdAlreadyExists ->
            return! Response.conflict ctx "Todo with same Id already exists!"
    }

let todosRouter = router {
    get "" (fun next ctx ->
        task {
            let todos = store.GetTodos()
            return! json todos next ctx
        })
    post "" (fun next ctx ->
        task {
            let! addDTO = ctx.BindModelAsync<AddDTO>()
            return! execute (AddCommand addDTO) next ctx
        })
}

let todoRouter (id: Guid) = router {
    get "" (fun next ctx ->
        task {
            let todo = store.GetTodos() |> List.tryFind (fun t -> t.Id = id)
            match todo with
            | Some todo ->
                return! json todo next ctx
            | None ->
                return! Response.notFound ctx "Todo not found!"
        })
}

let webApp = router {
    forward "/api/todos" todosRouter
    forwardf "/api/todo/%O" todoRouter
}

let app = application {
    url "http://0.0.0.0:8085/"
    use_router webApp
    memory_cache
    use_static (Path.GetFullPath "../Client/public")
    use_json_serializer(Thoth.Json.Giraffe.ThothSerializer())
    use_gzip
}

run app