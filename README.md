# Xavier Framework

The Xavier Framework is a powerful framework designed to facilitate the development of web applications by providing support for multiple programming languages, including C#, Python, and JavaScript. It allows you to define and process template strings in both C# and JavaScript, ensuring seamless integration and efficient development. Xavier also offers server-side rendering capabilities, enabling full-page rendering for enhanced performance and SEO.

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
  - [Template Syntax](#template-syntax)
  - [Creating a Component](#creating-a-component)
  - [Server-Side Rendering](#server-side-rendering)
- [Initialization](#initialization)
- [Examples](#examples)
- [Information](#information)
- [Contributing](#contributing)
- [License](#license)

## Installation

To use the Xavier Framework, follow these steps:

- Download the Xavier nuget package Devmachinist.Xavier and for the aot compiler Devmachinist.Xavier.AOT

## Usage

### Template Syntax

The Xavier Framework recognizes three types of languages:

- `x{ ... }x` - This syntax denotes a C# execution. Any code within these tags will be interpreted as C# code.
- `{{ ... }}` - This syntax represents as JavaScript in a script tag. Code within these tags will be interpreted as JavaScript code.
- `py{ ... }py` - This syntax represents as python 3.4 IronPython. Code within these tags will be interpreted as python code.

### Creating a Component

To create a component using the Xavier Framework, follow these steps:

1. Create a new file with the `.xavier` extension (e.g., `MyComponent.xavier`).
2. Define your component within the `.xavier` file using the appropriate afformentioned syntax.
3. Implement the code behind for the component in a separate file with the same name as the component and a `.xavier.cs` extension (e.g., `MyComponent.xavier.cs`). Use the base class provided by Xavier Framework and match it with the component's code behind file.

### Server-Side Rendering

Xavier Framework supports server-side rendering, which provides benefits like improved performance and search engine optimization (SEO). To enable server-side rendering for Xavier pages, follow these steps:

1. Configure your web application to use Xavier Framework.
2. Use the `app.MapXavierNodes` method in your application's configuration to map the Xavier nodes to specific routes. This method specifies the URL pattern and the destination directory for Xavier pages.
3. Set the static fallback for the Xavier:

```csharp
Xavier.Memory.StaticFallback("c:/fallback/index.html");
```

## Initialization

To initialize Xavier in a .NET app, follow these steps:

1. Import the required namespace:

```csharp
using Xavier;
//or
using Xavier.AOT;
```

 Call the `Xavier.Memory.Init` method to initialize Xavier with the desired parameters. This method builds your assembly into the specified destination. The last part of the destination path should have a `.js` extension.

```csharp
var memory = new Memory();
await memory.Init(root, destination, assembly);
```

or with AOT

```csharp
var memory = new Memory();
var aot = new XAOT();
await aot.Init(memory,root,destination,assembly);
```

## Examples

Here's an example of a `.xavier` file that demonstrates the usage of template strings in C# and JavaScript within the Xavier Framework:

```html
<!--This file should be named MyComponent.xavier -->

<div id="${this.target}auth">
</div>

{{
let username = "";
var msalInstance = {};
var target = '${this.target}'



// More JavaScript code here
}}

// C# code here

x{
    var Items = new[]{"item1","item2","item3"};
    @foreach( var k in items){
      <div>
        @k
      </div>
    }
}x
```

Here is the code behind required for each component.

```csharp
//this file should be named MyComponent.xavier.cs
using Xavier;

namespace MyNamespace{
    public partial class MyComponent : XavierNode
    {
        new public bool? ShouldRender = true;
        public MyComponent(XavierNode xavier) : base(xavier){
        }
        public MyComponent(){
        }
    }
}
```

## Information

- Xavier is experimental and should be treated as such.
- Known to cause thread pool starvation while using Devmachinist.Xavier.AOT . Simply stop the app when done testing and use the Xavier.Memory.Init(root,destination,assembly) for production.

## Contributing

We welcome contributions from the community to enhance the Xavier Framework. If you find any issues, have suggestions for improvements, or would like to add new features, please submit a pull request.

## License

The Xavier Framework is released under the [MIT License](https://opensource.org/licenses/MIT). Feel free to use, modify, and distribute it according to the terms of this license.
