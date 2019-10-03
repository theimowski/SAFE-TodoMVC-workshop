module Client

open System

open Browser
open Browser.Types
open Elmish
open Elmish.React
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Fetch
open Thoth.Fetch
open Thoth.Json

open Shared

// Model

/// This type keeps track of the state on client side.
/// In addition to list of Todos, we also keep track of
/// the value of text input field (for adding new Todo).
type Model =
    { Todos : Todo list
      Input : string }

// Messages

/// This types denotes all possible actions on Client side.
type Msg =
    // When app starts, we call GET /todos.
    // The TodosFetched Msg comes when a response is received
    | TodosFetched of Todo list
    // Msg to execute a given command
    | ExecuteCommand of Command
    // Msg to signal that an event has been applied
    | EventApplied of Todo list
    // Following Msgs are triggered from the UI itself
    // We're mostly interested in those
    | UpdateInput of string
    | Add

// Fetch

/// endpoint for actions related to all Todos
let todos = "/api/todos"
/// endpoint for actions related to a single Todo based on its Id
let todo (id: Guid) = sprintf "/api/todo/%O" id

/// helper function to call HTTP request
/// with given Method, url and optional body
let fetch method url (body: 'a option) =
    let properties =
        [ yield Method method
          match body with
          | Some body ->
            yield requestHeaders [ ContentType "application/json" ]
            yield Body (body |> Encode.toString 0 |> (!^))
          | None -> () ]
    Fetch.fetchAs<Todo list>(url, properties)

/// fetch all Todos - triggered at start of application
let fetchTodos () = fetch HttpMethod.GET todos None

/// function mapping a Command to corresponding HTTP request
let request (command: Command) =
    match command with
    | AddCommand addDTO -> fetch HttpMethod.POST todos (Some addDTO)

// Initial Model and Elmish Cmd

// !! Elmish Cmd is a different thing than our Command
// Don't confuse those two
// Our Command is for the domain logic
// And Elmish Cmd allows i.a. for asynchronous HTTP communication
// For the course of workshop we don't really need to worry about Elmish Cmd

/// This function defines what is the initial Model (state)
/// of the application and also calls fetchTodos
/// so that we can display all Todos in a list.
/// The fetchTodos call is translated to an Elmish Cmd
let init () : Model * Cmd<Msg> =
    let cmd = Cmd.OfPromise.perform fetchTodos () TodosFetched
    let model =
        { Todos = []
          Input = "" }
    model, cmd

// Update

/// function to push ExecuteCommand Msg for a given Command
/// into the Elmish loop
let execute = ExecuteCommand >> Cmd.ofMsg

/// update function takes current state (Model),
/// next incoming Msg and computes next state (Model) and
/// optionally an Elmish Cmd
let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    match msg with
    // when we get back Todos from Server, assign it to our Model
    | TodosFetched todos ->
        { model with Todos = todos }, Cmd.none
    // execute a command Client side
    // don't wait for server to create an event
    // but rather invoke the domain in Client application
    // this is possible as the domain logic is in Shared
    | ExecuteCommand command ->
        match Todos.handle command model.Todos with
        | Ok event ->
            let todos = Todos.apply event model.Todos
            let cmd =
                Cmd.OfPromise.perform (fun _ -> request command) () EventApplied
            { model with Todos = todos }, cmd
        | Error e ->
            console.log (sprintf "Domain error: %A" e)
            model, Cmd.none
    // After calling an HTTP request, we get back a response
    // with Todo list. Here we just check whether it's in sync
    // with Client model (whether Todos are equal)
    | EventApplied todos ->
        console.log (sprintf "Todos in sync: %b" (todos = model.Todos))
        model, Cmd.none
    // Msgs triggered from the UI
    | UpdateInput value ->
        { model with Input = value }, Cmd.none
    | Add ->
        let addDTO : AddDTO =
            { Id = Guid.NewGuid()
              Title = model.Input }
        let cmd = execute (AddCommand addDTO)
        { model with Input = "" }, cmd

// View

/// Keycodes for keys used in application
module Key =
    let enter = 13.
    let esc = 27.

/// displays main header and text input for adding new Todo
let viewInput (model: Model) dispatch =
    header [ ClassName "header" ]
        [ h1 [ ] [ str "todos" ]
          input
              [ ClassName "new-todo"
                Placeholder "What needs to be done?"
                OnChange (fun e -> e.target?value |> UpdateInput |> dispatch)
                OnKeyDown
                    (fun e -> if e.keyCode = Key.enter then dispatch Add)
                valueOrDefault model.Input
                AutoFocus true ] ]

/// displays a single Todo
let viewTodo (todo: Todo) dispatch =
  li
    [ classList
        [ "completed", todo.Completed ] ]
    [ div
        [ ClassName "view" ]
        [ label
            [ ]
            [ str todo.Title ]
          button
            [ ClassName "destroy" ]
            [ ] ] ]

/// displays whole list of Todos
let viewTodos model dispatch =
    let todos = model.Todos
    let cssVisibility =
        if List.isEmpty todos then "hidden" else "visible"

    section
      [ ClassName "main"
        Style [ Visibility cssVisibility ]]
      [ ul
          [ ClassName "todo-list" ]
          [ for todo in todos ->
                viewTodo todo dispatch ] ]

/// displays a footer under list of Todos
let viewControls model dispatch =
    let todosCompleted =
        model.Todos
        |> List.filter (fun t -> t.Completed)
        |> List.length

    let todosLeft = model.Todos.Length - todosCompleted

    let item = if todosLeft = 1 then " item" else " items"

    footer
        [ ClassName "footer"
          Hidden (List.isEmpty model.Todos) ]
        [ span
              [ ClassName "todo-count" ]
              [ strong [] [ str (string todosLeft) ]
                str (item + " left") ] ]

/// combines all parts into whole
let view model dispatch =
    div
      [ ClassName "todomvc-wrapper"]
      [ section
          [ ClassName "todoapp" ]
          [ viewInput model dispatch
            viewTodos model dispatch
            viewControls model dispatch ] ]

// Main

open Elmish.Debug
open Elmish.HMR

// create Elmish program using init update and view
Program.mkProgram init update view
|> Program.withConsoleTrace
|> Program.withReactBatched "elmish-app"
|> Program.withDebugger
|> Program.run