namespace Shared

open System

type AddDTO =
    { Id : Guid
      Title : string }

type Command =
    | AddCommand of AddDTO

type Todo =
    { Id : Guid
      Title : string
      Completed : bool }

type Event =
    | TodoAdded of Todo

type Error =
    | TodoIdAlreadyExists

module Todos =

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

    let apply (event: Event) (todos: Todo list) =
        match event with
        | TodoAdded todo ->
            todos @ [ todo ]
