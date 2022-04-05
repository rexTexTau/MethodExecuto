# MethodExecuto
.Net Core console app to quickly execute a particular method of a particular type in a particular assembly

This project has three primary purposes:

* To supersede multiple test console apps and [Explicit] unit-tests in your solution;
* To explore .Net core assemblies without source code provided;
* To consume .Net core assemblies' particular method execution within your own app.

# Usage

## From console

```
MethodExecuto.exe -d C:\Path\Where\Dir\Is\" -a Assembly.Name -t SomeType -m SomeMethod -p one -p 2.22 -p 3
```
- returns result (converted to a string) of calling `SomeMethod(one, 2.22, 3)` of type `SomeType` from assembly `Assembly.Name`.

| Param (long)  | Param (short) | Meaning                                                 | If not specified                                 |
| :---          |     :---:     | :---                                                    | :---                                             |
| assembly      | a             | Assembly name or full path. You can omit .dll extension | Lists all available assemblies from working dir  |
| directory     | d             | Working directory that is used to desolve dependencies  | MethodExecuto.exe location is used for that      |
| type          | t             | Name of type to use method from.                        | List all types present in assembly (not autogen) |
| method        | m             | Name of method to call.                                 | List all methods present in type (not autogen)   |
| parameter     | p             | Parameters' values to pass to method. Order matters!    | Calls method with no parameters or using default |

If method's visibility is private, internal, protected or protected internal - it'll just be executed no matter what.

If method you've selected to call is not staic, parameterless type ctor will be executed (if any).

Multiple parameters' values allowed, i.e. -p one -p 2.22 -p 3 will lead to call ("one", 2.22, 3).

If there are several methods with one name and different signatures, the method with the signature that fits most list of given parameters will be called.

Run `MethodExecuto.exe -h` or `MethodExecuto.exe -help` to get the most actual command line usage instructions.

## From code

1. Add **MethodExecuto.Core** to your project references.
1. Use static **Invoker.Invoke** method to run a particular method from a particular assembly. Parameters:

* `assemblyPath` - fully qualified path to the assembly;
* `typeName` - name of the type to use;
* `methodName` - name of the method to execute;
* `parameters` - string array of parameters; elements will be converted to method's input parameter types, if needed; order matters;
* `additionalAssemblyResolveDir` - if assembly has dependencies, you could specify additional dir to resolve dependencies from.

Raw output result will be returned (not converted to string).

## Beware of .Net version compatibility issues

If target framework of the assembly you're exploring and/or consuming differs from the target framework of MethodExecuto (.Net 5 at the moment), you could get an empty types and/or methods list (althpugh they do present in the assembly), and an attempt to call a particular method will fail.

Recompile MethodExecuto using target framework of the assembly you're dealing with to avoid that behavior.


# Legal Notice

Before using this app against any assembly *ensure* its license allows you to
* explore it (in all cases);
* create derivative work (in case you're chaining assembly methods' calls in your software).

# Discalimer

USE AT YOUR OWN RISK

The author is not responsible for any kind of usage by third parties of this application (as well as its derivatives), which causes violation of license agreements and/or copyrights of third-party software assemblies, as well as any indirect and/or direct losses of third-party software copyright holders.

# If you benefit from my work

You're welcome to share a tiny part of your success with me:

[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/rextextaucom)
