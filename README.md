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

1. (Client) add UI elements - in `viewTodos` function, before `ul` add 2 elements: 1) `input` with `checkbox` type and `toggle-all` class, 2) `label` with `HtmlFor` property set to `toggle-all`
1. (Shared) add `PatchAllCommand` with `PatchDTO` and `AllTodosMarkedAs` event with bool flag, cover the cases in `handle` and `apply`
1. (Client) add `SetAllCompleted` Msg with bool flag, execute `PatchAllCommand` for the Msg, call PATCH /todos with `PatchDTO` body for the command
1. (Client) add `OnClick` handler to the "toggle-all" **label (!)** and `dispatch SetAllCompleted`
1. (Client) make the "toggle-all" checkbox checked when all Todos are completed, and add a dummy `OnChange` handler to checkbox (can use `ignore` function) - this is so that we overcome React warnings on uncontrolled inputs
1. (Server) add handler for PATCH /todos - read `PatchDTO` from request and call `PatchAllCommand`

## 5. (*) edit title of a Todo

The specifications for this task are as follows ([reference](https://github.com/tastejs/todomvc/blob/master/app-spec.md#editing)):

* After double clicking on a label, the Todo should go into editing mode
* Editing mode means that instead of the label, a text input is displayed
* You can edit the Todo's title by changing value of the input and using Enter key
* When editing the title and clicking Esc, changes should be aborted
* It should call PATCH /todo/{id} (see `todos.http` file)

This one is a bit harder and requires bit more work on the Client side.
Steps described here will not be as precise as before, and they are not necessarily in the order, rather just general tips, so treat this task as a kind of challenge!

* (Client) You'll need to model the editing mode - one way for doing that is adding `Editing` field to our `Model` and keeping track of the Id and temporary editing value
* (Client) More than one Msg is needed to implement this feature - you might create Msgs for following:
  * start editing mode,
  * abort editing mode,
  * set editing value,
  * save changes.
* (Client) in `viewTodo`:
  * `editing` class should be present on `li` when a Todo is in editing mode
  * the double-click handler should be attached to `label`
  * the edit `input` should be child of `li` element
  * the input should have `edit` class, `valueOrDefault` set to the temporary value, and subscribe to `OnChange` and `OnKeyDown` events
* (Shared) to reuse the `PatchCommand` for both toggling Completed and editing Title, you might make the `PatchDTO` have two `option` fields for Completed and Title - the serialization will simply set the value to `None` if it's missing in JSON. This means that it's probably a good idea to extract a separate type for `PatchAllCommand` - e.g. `PatchAllDTO` with single "Completed" field

## 6. (**) extras

Following are left as an optional exercises, they are possible improvements on what we already have, and are part of the original TodoMVC project specifications.
They might be bit harder to do as I haven't prepared sample code for those (yet).

* add validation that Todo's title should never be empty
* when editing a Todo, respect also `Blur` event to save changes
* implement Routing as per [TodoMVC specs](https://github.com/tastejs/todomvc/blob/master/app-spec.md#routing) - use [Fable.Elmish.Browser](https://elmish.github.io/browser/index.html) package
* make the edit input focused when entering editing mode - one way of doing that is using [React Refs](https://pl.reactjs.org/docs/refs-and-the-dom.html) - you'll need an advanced usage of `Fable.React` as described e.g. [here](https://fable.io/blog/Announcing-Fable-React-5.html)
