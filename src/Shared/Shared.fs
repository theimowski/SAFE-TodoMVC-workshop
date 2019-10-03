namespace Shared

open System

/// DTO stands for Data Transfer Object
/// and represents data transferred on wire.
/// We'll use DTO as suffix for corresponding types, e.g. `AddDTO`
type AddDTO =
    { Id : Guid
      Title : string }

/// Command is a part of our domain
/// It can be executed by performing certain action.
/// Executing the Command results in producing an Event or an Error.
/// To distinguish Command from Cmd (Elmish type in Client),
/// we'll follow a suffix convention, e.g. `AddCommand`
type Command =
    | AddCommand of AddDTO
    | DeleteCommand of Guid

/// Todo is the main type in our domain.
/// We'll use `Todo list` type to keep track of all Todos.
type Todo =
    { Id : Guid
      Title : string
      Completed : bool }

/// Event is part of our domain.
/// It represents something that has been validated and already happened.
/// We use past tense for naming those, e.g. `TodoAdded`.
type Event =
    | TodoAdded of Todo
    | TodoDeleted of Todo

/// Error is part of our domain.
/// It can be a result of executing a Command from invalid state.
/// E.g. `TodoIdAlreadyExists` when trying to add a Todo with duplicate Id.
type Error =
    | TodoIdAlreadyExists

/// Todos module is there for our domain logic
module Todos =

    /// handle takes current state (Todo list) and a Command
    /// and returns Result<Event, Error>
    /// We'll get an Event back if state is valid
    /// and Error otherwise
    let handle (command: Command) (todos: Todo list) : Result<Event, Error> =
        match command with
        | AddCommand addDTO ->
            if todos |> List.exists (fun t -> t.Id = addDTO.Id) then
                Error TodoIdAlreadyExists
            else
                let todo : Todo =
                    { Id = addDTO.Id
                      Title = addDTO.Title
                      Completed = false }
                TodoAdded todo |> Ok

    /// apply takes current state (Todo list) and an Event
    /// and returns next state (Todo list)
    /// Given a proper Event, we compute next state
    /// by manipulating the list of Todos
    let apply (event: Event) (todos: Todo list) : Todo list =
        match event with
        | TodoAdded todo ->
            todos @ [ todo ]
