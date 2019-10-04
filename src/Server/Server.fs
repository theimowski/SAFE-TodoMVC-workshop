open System
open System.IO

open FSharp.Control.Tasks.V2
open Giraffe
open Saturn

open Shared

/// our file database
let store = FileStore.FileStore()

/// Execute a Command Server-side.
/// If the result is an Event, apply it to get new Todo list,
/// save new Todos in our store and return them as JSON.
/// If the result is an Error, map it to a proper HTTP status code and reponse body
let execute (command: Command) next ctx =
    task {
        let todos = store.GetTodos()
        match Todos.handle command todos with
        | Ok event ->
            let todos = store.GetTodos()
            let todos' = Todos.apply event todos
            store.SaveTodos todos'
            return! json todos' next ctx
        | Error TodoIdAlreadyExists ->
            return! Response.conflict ctx "Todo with same Id already exists!"
        | Error TodoNotFound ->
            return! Response.notFound ctx "Todo not found!"
    }

/// handles HTTP requests with a given method to /todos endpoint
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
    delete "" (fun next ctx ->
        task {
            return! execute DeleteCompletedCommand next ctx
        })
}

/// handles HTTP requests with a given method to /todo/{id} endpoint
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
    delete "" (fun next ctx ->
        task {
            return! execute (DeleteCommand id) next ctx
        })
    patch "" (fun next ctx ->
        task {
            let! patchDTO = ctx.BindModelAsync<PatchDTO>()
            return! execute (PatchCommand (id, patchDTO)) next ctx
        })
}

/// forward endpoints to proper routers
let webApp = router {
    forward "/api/todos" todosRouter
    forwardf "/api/todo/%O" todoRouter
}

/// creates the application
let app = application {
    url "http://0.0.0.0:8085/"
    use_router webApp
    memory_cache
    use_static (Path.GetFullPath "../Client/public")
    use_json_serializer(Thoth.Json.Giraffe.ThothSerializer())
    use_gzip
}

run app