# Hands-on with SAFE Stack

## Prerequisites

### Source code

Clone or download this repository:

[https://github.com/theimowski/SAFE-TodoMVC-workshop](https://github.com/theimowski/SAFE-TodoMVC-workshop
)

### Visual Studio Code

For this workshop it's recommended to use [Visual Studio Code](https://code.visualstudio.com/download). You **should** be fine using different F# IDE (Visual Studio or Jetbrains Rider) but I **might** not be able to help in case of any problems with these.

Then you have two options of installing prerequisites: **Installing locally** or using **Remote Container**.

### Installing locally

This is the standard way - installing all dependencies on your development box. Follow [quickstart](https://safe-stack.github.io/docs/quickstart/) section of SAFE docs. Make sure to install Ionide extension for VS Code.

### Remote Container

Alternatively, you can use Docker and an extension for Visual Studio Code - [Remote Containers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers). This allows your local VS Code to communicate with a remote Docker container that will have all other dependencies installed for you.

## Get the app running

If you choose to install prerequisites locally:

1. Open terminal of your choice
1. Change working directory to repository root
1. Invoke `fake build --target run`
1. After a short delay, http://localhost:8080/ should open in your browser automatically

> For unix you might need to add dotnet tools dir to path: `export PATH="$HOME/.dotnet/tools:$PATH"`

If using Remote Container:

1. Open repository directory in VS Code
1. Click "Reopen in container" in a pop-up window
1. Open VS Code terminal, it should attach to the container
1. Invoke `fake build --target run`
1. Open http://localhost:8080/

## 0. display Todos + add new Todo

> These features are already implemented on master branch.

## 1. delete a Todo

1. (Client) in `viewTodo`, just after the `label` add a `button` with `destroy` class
1. (Shared) add `DeleteCommand` with Id of a Todo (`Guid` type) and `TodoDeleted` event with a `Todo`
1. (Shared) in `handle` function add case for new command - use `List.find` to grab a Todo with given id
1. (Shared) in `apply` function add case for new event - use `List.filter` to remove Todo that has the given Id
1. (Client) add `Destroy` Msg with Id of a Todo (`Guid`)
1. (Client) in `update` function handle new Msg - execute `DeleteCommand`
1. (Client) add `OnClick` event handler to the button and `dispatch` a `Destroy` msg with todo's Id
1. (Client) in `request` function handle `DeleteCommand` - call DELETE /todo/{id} without body
1. (Server) add handler for DELETE to `todoRouter` - execute `DeleteCommand`
1. (Shared) add `TodoNotFound` error and in `handle` function replace `List.find` with `List.tryFind` to properly handle missing todo error
1. (Server) in `execute` function for `TodoNotFound` return HTTP 404 `notFound`

## 2. toggle completed for a Todo

1. (Client) in `viewTodo`, just before `label` add an `input` with `checkbox` type and `toggle` class - note `input` tag can't have children so use only list for properties!
1. (Client) use `Checked` property to mark the input when corresponding Todo is completed
1. (Shared) add `PatchDTO` type with `Completed` field and add `PatchCommand` with `Guid` and `PatchDTO`
1. (Shared) add `TodoPatched` event, implement case for `PatchCommand` in `handle` (remember to check if todo exists), cover `TodoPatched` in `apply` - use `List.map` and check if Id matches
1. (Client) add `SetCompleted` Msg with Id and flag (`bool`), in `update` function handle this case and `execute` the `PathCommand`
1. (Client) add `OnChange` handler for the checkbox `input` and `dispatch` `SetCompleted` Msg
1. (Client) handle `PatchCommand` in `request` function - call PATCH /todo/{id} with `PatchDTO` as body
1. (Server) add handler for PATCH to `todoRouter` - read `PatchDTO` from the request (`ctx.BindModelAsync`) and execute `PatchCommand`

## 3. delete completed Todos

1. (Client) in `viewControls`, just after `span` with "X items left" add a `button` with `clear-completed` class and "Clear completed" inner text child node (use `str "text"` function)
1. (Client) hide the `button` when none of Todos is Completed - use `Hidden` property and `todosCompleted` counter
1. (Shared) add `DeleteCompletedCommand` and `CompletedTodosDeleted` event, cover cases in `handle` and `apply`
1. (Client) add `ClearCompleted` Msg, execute `DeleteCompletedCommand` in `update` for the Msg, call DELETE /todos for the command in `request`
1. (Client) add `OnClick` handler to the "Clear completed" `button`, `dispatch ClearCompleted`
1. (Server) add handler for DELETE to `todosRouter` - execute `DeleteCompletedCommand`

## 4. toggle completed for all Todos

## 5. (*) edit title of a Todo

## 6. (**) extras

Following are left as an optional exercises, they are possible improvements on what we already have, and are part of the original TodoMVC project specifications.
They might be bit harder to do as I haven't prepared sample code for those (yet).

* add validation that Todo's title should never be empty
* implement Routing as per [TodoMVC specs](https://github.com/tastejs/todomvc/blob/master/app-spec.md#routing) - use [Fable.Elmish.Browser](https://elmish.github.io/browser/index.html) package
* make the edit input focused when entering editing mode - one way of doing that is using [React Refs](https://pl.reactjs.org/docs/refs-and-the-dom.html) - you'll need an advanced usage of `Fable.React` as described e.g. [here](https://fable.io/blog/Announcing-Fable-React-5.html)
